using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace OpenFlightVRC
{
#if !COMPILER_UDONSHARP && UNITY_EDITOR // These using statements must be wrapped in this check to prevent issues on builds
using UnityEditor;
using UdonSharpEditor;
#endif

	// This is a custom inspector for the WingFlightPlusGlide script. It currently just adds a reset to defaults button
#if !COMPILER_UDONSHARP && UNITY_EDITOR
[CustomEditor(typeof(WingFlightPlusGlide))]
public class WingFlightPlusGlideEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WingFlightPlusGlide script = (WingFlightPlusGlide)target;

        if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

        if (GUILayout.Button("Reset to Prefab Defaults"))
        {
            // Reset all values to the default in the prefab
            PrefabUtility.RevertObjectOverride(script, InteractionMode.AutomatedAction);
        }

        DrawDefaultInspector();
    }
}
#endif

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WingFlightPlusGlide : LoggableUdonSharpBehaviour
	{
		[Header("Basic Settings")]
		// Both of these "base" values are by default affected by the avatar's armspan. See sizeCurve.
		[Tooltip("Want flaps to be stronger or weaker? Change this value first. (Default: 285)")]
		[Range(100, 800)]
		public int flapStrengthBase = 285;
		int flapStrengthBase_DEFAULT = 285;

		[Tooltip("Base gravity while flying (Default: 0.5)")]
		public float flightGravityBase = 0.5f;
		float flightGravityBase_DEFAULT = 0.5f;

		[Tooltip("Require the player to jump before flapping can occur? Makes it less likely to trigger a flap by accident. (Default: true) CURRENTLY HAS NO EFFECT.")]
		public bool requireJump = true;
		bool requireJump_DEFAULT = true;

		[Tooltip("Allow locomotion (wasd/left joystick) while flying? (Default: false)")]
		public bool allowLoco = false;
		bool allowLoco_DEFAULT = false;

		[Tooltip(
			"Avatars using the avatar detection system may have wingtip, weight, etc. modifiers intended to personalize how they feel in the air. Set this value to true to use these modifiers or false if you want them disregarded for consistency. (Note: avatar size detection is not an Avatar Modifier; size-related calculations will always apply even if this setting is set to false.) (Default: true)"
		)]
		public bool useAvatarModifiers = true;
		bool useAvatarModifiers_DEFAULT = true;

		[Tooltip("Allow gliding. (Default: true)")]
		public bool canGlide = true;
		bool canGlide_DEFAULT = true;

		[Tooltip(
			"Avatars can glide directly from a fall without having to flap first. This behavior is more intuitive for gliding off cliffs, but may cause players to trigger gliding on accident more often when they just want to fall. (Default: false)"
		)]
		public bool fallToGlide = false;
		bool fallToGlide_DEFAULT = false;

		[Header("Advanced Settings (Only for specialized use!)")]
        [Tooltip("Angle to offset the gliding direction by from your hands. (Default: 0)")]
        public float glideAngleOffset = 0f;
        float glideAngleOffset_DEFAULT = 0f;
		[Tooltip(
			"How much Flap Strength and Flight Gravity are affected by an avatar's armspan. Default values will make smaller avis feel lighter and larger avis heavier."
		)]
		public AnimationCurve sizeCurve = new AnimationCurve(new Keyframe(0.05f, 2), new Keyframe(1, 1), new Keyframe(20, 0.00195f));

		[Tooltip("Modifier for horizontal flap strength. Makes flapping forwards easier. (Default: 1.5)")]
		public float horizontalStrengthMod = 1.5f;
		float horizontalStrengthMod_DEFAULT = 1.5f;

		[Tooltip("How tight you want your turns while gliding. May be dynamically decreased by Avatar Modifier: weight. (Default: 1.5)")]
		[Range(1f, 5f)]
		public float glideControl = 2.3f; // Do not reduce this below 1; it will break under some weight values if you do
		float glideControl_DEFAULT = 2.3f;

		[Tooltip("Slows gliding down over time. (Default: 0.02)")]
		[Range(0f, 0.2f)]
		public float airFriction = 0.02f;
		float airFriction_DEFAULT = 0.02f;

		[Tooltip("If enabled, flight gravity will use Gravity Curve's curve instead of Size Curve's curve multiplied by Flight Gravity Base. (Default: false)")]
		public bool useGravityCurve = false;
		bool useGravityCurve_DEFAULT = false;

		[Tooltip(
			"Similar to Size Curve, but instead of modifying Flap Strength, it only affects Gravity. This value is ignored (Size Curve will be used instead) unless Use Gravity Curve is enabled."
		)]
		public AnimationCurve gravityCurve = new AnimationCurve(new Keyframe(0.05f, 0.4f), new Keyframe(1, 0.2f), new Keyframe(20, 0.00039f));

		[Tooltip("If a GameObject with a TextMeshPro component is attached here, debug some basic info into it. (Default: unset)")]
		public TextMeshProUGUI debugOutput;

		[Tooltip("Banking to the left or right will force the player to rotate. May cause network lag? (Default: true)")]
		public bool bankingTurns = true;
		bool bankingTurns_DEFAULT = true;

		[Tooltip("If enabled, gravity and movement will be saved each time the user takes off, instead of just at the start of the world. (Default: false)")]
		public bool dynamicPlayerPhysics = false;

		// Essential Variables
		private VRCPlayerApi LocalPlayer;
		private double debugTemp;
		private int timeTick = -1; // -1 until the player is valid, then this value cycles from 0-99 at 50 ticks per second
		private Vector3 RHPos;
		private Vector3 LHPos;
		private Vector3 RHPosLast = new Vector3(0f, float.NegativeInfinity, 0f);
		private Vector3 LHPosLast = new Vector3(0f, float.NegativeInfinity, 0f);
		private Quaternion RHRot;
		private Quaternion LHRot;
		private Quaternion playerRot;
		private bool handsOut = false; // Are the controllers held outside of an imaginary cylinder?
		private bool handsOpposite = false;

        [HideInInspector]
        /// <summary> If true, the player is currently in the process of flapping. </summary>
        public bool isFlapping = false; // Doing the arm motion
        [HideInInspector]
        /// <summary> If true, the player is currently flying. </summary>
        public bool isFlying = false; // Currently in the air after/during a flap
        [HideInInspector]
        /// <summary> If true, the player is currently gliding. </summary>
        public bool isGliding = false; // Has arms out while flying


        private int cannotFlyTick = 0; // If >0, disables flight then decreases itself by one
        private int fallingTick = 0; // Increased by one every tick one's y velocity < 0
		private float tmpFloat;
		private float dtFake = 0;

		// Variables related to Velocity
		private Vector3 finalVelocity; // Modify this value instead of the player's velocity directly, then run `setFinalVelocity = true`
		private bool setFinalVelocity;
		private Vector3 newVelocity; // tmp var
		private Vector3 targetVelocity; // tmp var, usually associated with slerping/lerping
		private float downThrust = 0f;
		private float flapAirFriction = 0.04f; // Prevents the gain of infinite speed while flapping. Set to 0 to remove this feature. THIS IS NOT A MAX SPEED

        // Variables related to gliding
        internal Vector3 wingDirection;
		private float steering;
		private bool spinningRightRound = false; // Can't get that Protogen animation out of my head
		private float rotSpeed = 0;
		private float rotSpeedGoal = 0;
		private float glideDelay = 0; // Minus one per tick, upon hitting ten gliding will gradually come into effect. Zero means gliding functions fully

		// "old" values are the world's defaults (recorded immediately before they are modified)
		private float oldGravityStrength;
		private float oldWalkSpeed;
		private float oldRunSpeed;
		private float oldStrafeSpeed;

		// Avatar-specific properties
		private HumanBodyBones rightUpperArmBone; // Bones won't be given a value until LocalPlayer.IsValid()
		private HumanBodyBones leftUpperArmBone;
		private HumanBodyBones rightLowerArmBone;
		private HumanBodyBones leftLowerArmBone;
		private HumanBodyBones rightHandBone;
		private HumanBodyBones leftHandBone;
		private float shoulderDistance = 0; // Distance between the two shoulders

		[HideInInspector]
		public float armspan = 1f;

		[Tooltip("Default avatar wingtipOffset. (Default: 0)")]
		public float wingtipOffset = 0;
		float wingtipOffset_DEFAULT = 0;

		[Tooltip("Default avatar weight. (Default: 1)")]
		[Range(0f, 2f)]
		public float weight = 1.0f;

		//Banking variables
		private Vector3 playerHolder;

		[Header("Helper property (Do not touch, unless empty, then set to empty game object)")]
		[Tooltip("Do not remove, if empty add the 'Load Bearing' game object, script will fail at runtime without this setup.")]
		public Transform loadBearingTransform; //Transforms cannot be created, they can only be gotten from game objects, it isn't possible to create either in code.

		public void Start()
		{
			LocalPlayer = Networking.LocalPlayer;
			//save the user gravity if dynamic gravity is disabled
			if (!dynamicPlayerPhysics)
			{
				oldGravityStrength = LocalPlayer.GetGravityStrength();
				oldWalkSpeed = LocalPlayer.GetWalkSpeed();
				oldRunSpeed = LocalPlayer.GetRunSpeed();
				oldStrafeSpeed = LocalPlayer.GetStrafeSpeed();
                Logger.Log("Player Physics saved.", this);
			}
		}

		public void OnEnable()
		{
			timeTick = -20;
			isFlapping = false;
			isFlying = false;
			isGliding = false;
			spinningRightRound = false;
		}

		public void OnDisable()
		{
			if (isFlying)
			{
				Land();
			}
            Logger.Log("Disabled.", this);
		}

        public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float eyeHeight) // According to the docs, this also runs upon changing avatars
        {
			if (player == LocalPlayer)
			{
				// Bug check: if avatar has been swapped, sometimes the player will be launched straight up
				cannotFlyTick = 20;
				setFinalVelocity = false;
			}

			CalculateStats();
		}

		public void Update()
		{
			if ((LocalPlayer != null) && LocalPlayer.IsValid())
			{
				dtFake = dtFake + Time.deltaTime;
				if (dtFake >= 0.011f)
				{
					dtFake = dtFake - 0.011f;
					FlightTick(0.011f);
				}
			}
			if (spinningRightRound)
			{
				// Rotate the player (only if the Banking Turns beta setting is enabled)
				if (useAvatarModifiers)
				{
					rotSpeed = rotSpeed + ((rotSpeedGoal - rotSpeed) * Time.deltaTime * 6 * (1 - (weight - 1)));
				}
				else
				{
					rotSpeed = rotSpeed + ((rotSpeedGoal - rotSpeed) * Time.deltaTime * 6);
				}

				//Playspace origin and actual player position seems to work as parent and child objects,
				//therefore the conclusion is that we must make the playspace origin orbit the player.
				//
				//Caching positional data and modifying a virtual origin to be translated.
				loadBearingTransform.position = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin).position;
				loadBearingTransform.rotation = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin).rotation;
				playerHolder = LocalPlayer.GetPosition();

				//This function is strange.
				//I am in awe of the Unity engineers that had to fix the edge case of someone wanting to rotate the parent around a child.
				//Sure is useful in this case though.
				loadBearingTransform.RotateAround(playerHolder, Vector3.up, rotSpeed * Time.deltaTime);

				//Teleport based on playspace position, with an offset to place the player at the teleport location instead of the playspace origin.
				LocalPlayer.TeleportTo(
					playerHolder + (loadBearingTransform.position - playerHolder),
					loadBearingTransform.rotation,
					VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint,
					true
				);
			}
		}

		private void FlightTick(float dt)
		{
			// Variable `dt` is Delta Time
			if (timeTick < 0)
			{
				// This block only runs once shortly after joining the world
				timeTick = 0;
				leftLowerArmBone = HumanBodyBones.LeftLowerArm;
				rightLowerArmBone = HumanBodyBones.RightLowerArm;
				leftUpperArmBone = HumanBodyBones.LeftUpperArm;
				rightUpperArmBone = HumanBodyBones.RightUpperArm;
				leftHandBone = HumanBodyBones.LeftHand;
				rightHandBone = HumanBodyBones.RightHand;
				CalculateStats();
			}
			setFinalVelocity = false;
			// Check if hands are being moved downward while above a certain Y threshold
			// We're using LocalPlayer.GetPosition() to turn these global coordinates into local ones
			RHPos = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
			LHPos = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
			LHRot = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation;
			RHRot = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;

			downThrust = 0;
			if ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y) > 0)
			{
				downThrust = ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y)) * dt / armspan;
			}

			// Check if player is falling
			if ((!LocalPlayer.IsPlayerGrounded()) && LocalPlayer.GetVelocity().y < 0)
			{
				fallingTick++;
			}
			else
			{
				fallingTick = 0;
			}

			// Check if hands are held out (ie are a certain distance from the central body)
			handsOut = false;
			if (
				Vector2.Distance(
					new Vector2(LocalPlayer.GetBonePosition(rightUpperArmBone).x, LocalPlayer.GetBonePosition(rightUpperArmBone).z),
					new Vector2(LocalPlayer.GetBonePosition(rightHandBone).x, LocalPlayer.GetBonePosition(rightHandBone).z)
				)
					> armspan / 3.3f
				&& Vector2.Distance(
					new Vector2(LocalPlayer.GetBonePosition(leftUpperArmBone).x, LocalPlayer.GetBonePosition(leftUpperArmBone).z),
					new Vector2(LocalPlayer.GetBonePosition(leftHandBone).x, LocalPlayer.GetBonePosition(leftHandBone).z)
				)
					> armspan / 3.3f
			)
			{
				handsOut = true;
			}

			//if (Vector3.Angle(LHRot * Vector3.right, RHRot * Vector3.right) > 90)
			handsOpposite = (
				Vector3.Distance(LocalPlayer.GetBonePosition(leftHandBone), LocalPlayer.GetBonePosition(rightHandBone)) > ((armspan / 3.3) * 2) + shoulderDistance
			);

			if (!isFlapping)
			{
				// Check for the beginning of a flap
				if (
					(isFlying ? true : handsOut)
					// && (requireJump ? !LocalPlayer.IsPlayerGrounded() : true)
					&& (!LocalPlayer.IsPlayerGrounded())
					&& RHPos.y < LocalPlayer.GetPosition().y - LocalPlayer.GetBonePosition(rightUpperArmBone).y
					&& LHPos.y < LocalPlayer.GetPosition().y - LocalPlayer.GetBonePosition(leftUpperArmBone).y
					&& downThrust > 0.0002
				)
				{
					isFlapping = true;
					// TakeOff() will only take effect if !isFlying
					TakeOff();
				}
			}

			// -- STATE: Flapping
			if (isFlapping)
			{
				if (downThrust > 0)
				{
					// Calculate force to apply based on the flap
					newVelocity = ((RHPos - RHPosLast) + (LHPos - LHPosLast)) * dt * flapStrength();
					if (LocalPlayer.IsPlayerGrounded())
					{
						// Prevents skiing along the ground
						newVelocity = new Vector3(0, newVelocity.y, 0);
					}
					else
					{
						// apply horizontalStrengthMod
						tmpFloat = newVelocity.y;
						newVelocity = newVelocity * horizontalStrengthMod;
						newVelocity.y = tmpFloat;
					}
					finalVelocity = LocalPlayer.GetVelocity() + newVelocity;
					// Speed cap (check, then apply flapping air friction)
					if (finalVelocity.magnitude > 0.02f * flapStrength())
					{
						finalVelocity = finalVelocity.normalized * (finalVelocity.magnitude - (flapAirFriction * flapStrength() * 0.01f));
					}
					setFinalVelocity = true;
				}
				else
				{
					isFlapping = false;
				}
			}

			// See fallToGlide tooltip
			if (fallToGlide && fallingTick >= 20 && handsOut && handsOpposite && canGlide)
			{
				TakeOff();
			}

			// -- STATE: Flying
			// (Flying starts when a player first flaps and ends when they become grounded)
			if (isFlying)
			{
				if (IsMainMenuOpen() || ((!isFlapping) && LocalPlayer.IsPlayerGrounded()))
				{
					Land();
				}
				else
				{
					// ---=== Run every frame while the player is "flying" ===---
					if (LocalPlayer.GetGravityStrength() != (flightGravity()) && LocalPlayer.GetVelocity().y < 0)
					{
						LocalPlayer.SetGravityStrength(flightGravity());
					}
					if ((!isFlapping) && (isGliding ? true : handsOut) && handsOpposite && canGlide)
					{
						// Gliding, banking, and steering logic
						if (LocalPlayer.GetVelocity().y > -1f && (!isGliding))
						{
							glideDelay = 3;
						}

						isGliding = true;
						newVelocity = setFinalVelocity ? finalVelocity : LocalPlayer.GetVelocity();

						if (glideDelay <= 1)
						{
                            //rotate Vector3.forward by the gliding angle offset
                            Vector3 newForwardRight = Quaternion.Euler(glideAngleOffset, 0, 0) * Vector3.forward;
                            Vector3 newForwardLeft = Quaternion.Euler(-glideAngleOffset, 0, 0) * Vector3.forward;
                            wingDirection = Vector3.Normalize(Vector3.Slerp(RHRot * newForwardRight, LHRot * newForwardLeft, 0.5f)); // The direction the player should go based on how they've angled their wings
                        }
						else
						{
							wingDirection = newVelocity.normalized;
							glideDelay = glideDelay - (5 * dt);
						}

						// Hotfix: Always have some form of horizontal velocity while falling. In rare cases (more common with extremely small avatars) a player's velocity is perfectly straight up/down, which breaks gliding
						if (newVelocity.y < 0.3f && newVelocity.x == 0 && newVelocity.z == 0)
						{
							Vector2 tmpV2 = new Vector2(wingDirection.x, wingDirection.z).normalized * 0.145f;
							newVelocity = new Vector3(Mathf.Round(tmpV2.x * 10) / 10, newVelocity.y, Mathf.Round(tmpV2.y * 10) / 10);
						}

						steering = (RHPos.y - LHPos.y) * 80 / armspan;
						//clamp steering to 45 degrees
						steering = Mathf.Clamp(steering, -45, 45);

						if (bankingTurns)
						{
							spinningRightRound = true;
							rotSpeedGoal = steering;
						}
						else
						{
							// Default "banking" which is just midair strafing
							wingDirection = Quaternion.Euler(0, steering, 0) * wingDirection;
						}

						// X and Z are purely based on which way the wings are pointed ("forward") for ease of VR control
						targetVelocity = Vector3.ClampMagnitude(newVelocity + (Vector3.Normalize(wingDirection) * newVelocity.magnitude), newVelocity.magnitude);

						// tmpFloat == glideControl (Except if weight > 1, glideControl temporarily decreases)
						tmpFloat = (useAvatarModifiers && weight > 1) ? glideControl - ((weight - 1) * 0.6f) : glideControl;
						if (glideDelay > 0)
						{
							glideDelay = glideDelay - (5 * dt);
						}
						tmpFloat = tmpFloat * ((1 - glideDelay) / 1);
						finalVelocity = Vector3.Slerp(newVelocity, targetVelocity, dt * tmpFloat);

						// Apply Air Friction
						finalVelocity = finalVelocity * (1 - (airFriction * dt));
						setFinalVelocity = true;
					}
					else
					{
						isGliding = false;
						rotSpeedGoal = 0;
						glideDelay = 0;
					}
				}
			}
			RHPosLast = RHPos;
			LHPosLast = LHPos;

			if (cannotFlyTick > 0)
			{
				setFinalVelocity = false;
				cannotFlyTick--;
			}

			if (setFinalVelocity)
			{
				LocalPlayer.SetVelocity(finalVelocity);
			}
		}

		public void FixedUpdate()
		{
			if (timeTick >= 0)
			{
				timeTick = timeTick + 1;
				// Automatically update the debug output every 0.2 seconds (assuming VRChat uses the Unity default of 50 ticks per second)
				if (timeTick > 9)
				{
					timeTick = 0;
					if (debugOutput != null)
					{
						Vector3 ye = (RHRot.eulerAngles.normalized) + (LHRot.eulerAngles.normalized);
						debugOutput.text =
							string.Concat("\nIsFlying: ", isFlying.ToString())
							+ string.Concat("\nIsFlapping: ", isFlapping.ToString())
							+ string.Concat("\nIsGliding: ", isGliding.ToString())
							+ string.Concat("\nHandsOut: ", handsOut.ToString())
							+ string.Concat("\nDownThrust: ", downThrust.ToString())
							+ string.Concat("\nCannotFly: ", (cannotFlyTick > 0).ToString())
							+ string.Concat("\nGlideDelay: ", glideDelay.ToString())
							+ string.Concat("\neulerR: ", RHRot.eulerAngles.normalized.ToString())
							+ string.Concat("\nmag: ", LocalPlayer.GetVelocity().y.ToString())
							//+ string.Concat("\nLeAngle: ", Quaternion.Angle(mehl, mehr).ToString())
							+ string.Concat(
								"\nCanDoIt: ",
								(
									(
										Vector3.Distance(LocalPlayer.GetBonePosition(leftHandBone), LocalPlayer.GetBonePosition(rightHandBone))
										> ((armspan / 3.3) * 2)
											+ Vector3.Distance(LocalPlayer.GetBonePosition(leftUpperArmBone), LocalPlayer.GetBonePosition(rightUpperArmBone))
									).ToString()
								)
							);
					}
				}
			}
		}

		// Immobilize Locomotion but still allow body rotation
		private void ImmobilizePart(bool b)
		{
			if (b)
			{
				if(dynamicPlayerPhysics)
				{
					oldWalkSpeed = LocalPlayer.GetWalkSpeed();
					oldRunSpeed = LocalPlayer.GetRunSpeed();
					oldStrafeSpeed = LocalPlayer.GetStrafeSpeed();
				}
				LocalPlayer.SetWalkSpeed(0f);
				LocalPlayer.SetRunSpeed(0f);
				LocalPlayer.SetStrafeSpeed(0f);
			}
			else
			{
				LocalPlayer.SetWalkSpeed(oldWalkSpeed);
				LocalPlayer.SetRunSpeed(oldRunSpeed);
				LocalPlayer.SetStrafeSpeed(oldStrafeSpeed);
			}
		}

		// Flight Strength, etc. are based on armspan and whatnot.
		// This function can be re-run to recalculate these values at any time (upon switching avatars for example)
		private void CalculateStats()
		{
			// `armspan` does not include the distance between shoulders
			armspan =
				Vector3.Distance(LocalPlayer.GetBonePosition(leftUpperArmBone), LocalPlayer.GetBonePosition(leftLowerArmBone))
				+ Vector3.Distance(LocalPlayer.GetBonePosition(leftLowerArmBone), LocalPlayer.GetBonePosition(leftHandBone))
				+ Vector3.Distance(LocalPlayer.GetBonePosition(rightUpperArmBone), LocalPlayer.GetBonePosition(rightLowerArmBone))
				+ Vector3.Distance(LocalPlayer.GetBonePosition(rightLowerArmBone), LocalPlayer.GetBonePosition(rightHandBone));
			shoulderDistance = Vector3.Distance(LocalPlayer.GetBonePosition(leftUpperArmBone), LocalPlayer.GetBonePosition(rightUpperArmBone));
            Logger.Log("Armspan: " + armspan.ToString() + " Shoulder Distance: " + shoulderDistance.ToString(), this);
		}

		// Set necessary values for beginning flight
		public void TakeOff()
		{
			if (!isFlying)
			{
				isFlying = true;
				if (dynamicPlayerPhysics)
				{
					oldGravityStrength = LocalPlayer.GetGravityStrength();
				}
				else
                {
                    CheckPhysicsUnChanged();
                }
                LocalPlayer.SetGravityStrength(flightGravity());
				if (!allowLoco)
				{
					ImmobilizePart(true);
				}
                Logger.Log("Took off.", this);
			}
		}

        /// <summary>
        /// Checks if the world gravity or player movement has changed from the saved values and throws a warning if so.
        /// </summary>
        private void CheckPhysicsUnChanged()
        {
            //if the world gravity is different than what we have saved, throw a warning
            if (LocalPlayer.GetGravityStrength() != oldGravityStrength)
            {
                Logger.LogWarning("World gravity is different than the saved gravity, this may cause issues. If you want to avoid this, edit scripts to inform OpenFlight of the new world gravity using UpdatePlayerPhysics().", this);
                Logger.LogWarning("Saved Gravity: " + oldGravityStrength.ToString(), this);
            }

            //if the player movement is different than what we have saved, throw a warning
            if (LocalPlayer.GetWalkSpeed() != oldWalkSpeed || LocalPlayer.GetRunSpeed() != oldRunSpeed || LocalPlayer.GetStrafeSpeed() != oldStrafeSpeed)
            {
                Logger.LogWarning("Player movement is different than the saved movement, this may cause issues. If you want to avoid this, edit scripts to inform OpenFlight of the new player movement using UpdatePlayerPhysics().", this);
                Logger.LogWarning("Saved Walk Speed: " + oldWalkSpeed.ToString() + " Saved Run Speed: " + oldRunSpeed.ToString() + " Saved Strafe Speed: " + oldStrafeSpeed.ToString(), this);
            }
        }

        /// <summary>
        /// Utility method to detect main menu status. Technique pulled from <see href="https://github.com/Superbstingray/UdonPlayerPlatformHook">UdonPlayerPlatformHook</see>
        /// </summary>
        /// <returns>True if the main menu is open, false otherwise</returns>
        private bool IsMainMenuOpen()
		{
			int uiColliderCount = Physics.OverlapSphere(LocalPlayer.GetPosition(), 10f, 524288).Length;
			return (uiColliderCount == 8 || uiColliderCount == 9 || uiColliderCount == 10);
		}

		/// <summary>
		/// Effectually disables all flight-related variables
		/// </summary>
		public void Land()
		{
			isFlying = false;
			isFlapping = false;
			isGliding = false;
			spinningRightRound = false;
			rotSpeed = 0;
			rotSpeedGoal = 0;
            LocalPlayer.SetGravityStrength(oldGravityStrength);
			if (!allowLoco)
			{
				ImmobilizePart(false);
			}
            if (!dynamicPlayerPhysics)
            {
                CheckPhysicsUnChanged();
            }
            Logger.Log("Landed.", this);
		}

		private float flapStrength()
		{
			if (useAvatarModifiers)
			{
				// default settings
				return sizeCurve.Evaluate(armspan) * (flapStrengthBase + (wingtipOffset * 8));
			}
			else
			{
				return sizeCurve.Evaluate(armspan) * flapStrengthBase + 10;
			}
		}

		private float flightGravity()
		{
			if (useGravityCurve)
			{
				tmpFloat = gravityCurve.Evaluate(armspan) * armspan;
			}
			else
			{
				// default settings
				tmpFloat = sizeCurve.Evaluate(armspan) * flightGravityBase * armspan;
			}
			if (useAvatarModifiers)
			{
				// default settings
				return tmpFloat * weight;
			}
			else
			{
				return tmpFloat;
			}
		}

		// --- Helper Functions ---

		public void EnableBetaFeatures()
		{
			bankingTurns = true;
            Logger.Log("Beta Features enabled.", this);
		}

		public void DisableBetaFeatures()
		{
			bankingTurns = false;
            Logger.Log("Beta Features disabled.", this);
		}

		/// <summary>
		/// Calling this function tells the script to pull in the worlds values for player physics. This is useful if you have a world that changes gravity or movement often, but still want water systems to work.
		/// </summary>
		/// <remarks>
		/// This function is only useful if dynamic player physics is disabled. Otherwise, it will do nothing.
		/// </remarks>
		public void UpdatePlayerPhysics()
		{
			if (!dynamicPlayerPhysics)
			{
				oldGravityStrength = LocalPlayer.GetGravityStrength();
				oldWalkSpeed = LocalPlayer.GetWalkSpeed();
				oldRunSpeed = LocalPlayer.GetRunSpeed();
				oldStrafeSpeed = LocalPlayer.GetStrafeSpeed();
                Logger.Log("Player Physics updated.", this);
			}
			else {
                Logger.Log("Dynamic Player Physics is enabled. Player Physics will be updated automatically.", this);
			}
		}
		/// <summary>
		/// Initializes all default values. This should not be called by end users in most cases.
		/// </summary>
		public void InitializeDefaults()
		{
			flapStrengthBase_DEFAULT = flapStrengthBase;
			flightGravityBase_DEFAULT = flightGravityBase;
			requireJump_DEFAULT = requireJump;
			allowLoco_DEFAULT = allowLoco;
			useAvatarModifiers_DEFAULT = useAvatarModifiers;
			wingtipOffset_DEFAULT = wingtipOffset;
			canGlide_DEFAULT = canGlide;
			fallToGlide_DEFAULT = fallToGlide;
			horizontalStrengthMod_DEFAULT = horizontalStrengthMod;
			glideControl_DEFAULT = glideControl;
			airFriction_DEFAULT = airFriction;
			useGravityCurve_DEFAULT = useGravityCurve;
			bankingTurns_DEFAULT = bankingTurns;
            glideAngleOffset_DEFAULT = glideAngleOffset;
            Logger.Log("Defaults initialized.", this);
		}

		/// <summary>
		/// Restores all values to their prefab defaults
		/// </summary>
		public void RestoreDefaults()
		{
			flapStrengthBase = flapStrengthBase_DEFAULT;
			flightGravityBase = flightGravityBase_DEFAULT;
			requireJump = requireJump_DEFAULT;
			allowLoco = allowLoco_DEFAULT;
			useAvatarModifiers = useAvatarModifiers_DEFAULT;
			wingtipOffset = wingtipOffset_DEFAULT;
			canGlide = canGlide_DEFAULT;
			fallToGlide = fallToGlide_DEFAULT;
			horizontalStrengthMod = horizontalStrengthMod_DEFAULT;
			glideControl = glideControl_DEFAULT;
			airFriction = airFriction_DEFAULT;
			useGravityCurve = useGravityCurve_DEFAULT;
			bankingTurns = bankingTurns_DEFAULT;
            glideAngleOffset = glideAngleOffset_DEFAULT;
            Logger.Log("Defaults restored.", this);
		}
	}
}
