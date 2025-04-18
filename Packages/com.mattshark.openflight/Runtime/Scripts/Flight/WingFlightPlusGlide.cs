/**
 * @ Maintainer: Mattshark89
 */

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;
using VRC.SDK3.Data;

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

	/// <summary>
	/// This is the main script that controls all of the physics for the flight system.
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class WingFlightPlusGlide : LoggableUdonSharpBehaviour
	{
		#region Settings
		/// <summary>
		/// Base strength of a flap. This value is affected by the avatar's armspan. See sizeCurve.
		/// </summary>
		/// <seealso cref="sizeCurve"/>
		[Header("Basic Settings")]
		// Both of these "base" values are by default affected by the avatar's armspan. See sizeCurve.
		[Tooltip("Want flaps to be stronger or weaker? Change this value first. (Default: 285)")]
		[Range(100, 800)]
		public int flapStrengthBase = 285;

		/// <summary>
		/// Base gravity while flying.
		/// </summary>
		[Tooltip("Base gravity while flying (Default: 0.4)")]
		public float flightGravityBase = 0.4f;

		/// <summary>
		/// Require the player to jump before flapping can occur? Makes it less likely to trigger a flap by accident.
		/// </summary>
		[Tooltip("Require the player to jump before flapping can occur? Makes it less likely to trigger a flap by accident. (Default: true)")]
		public bool requireJump = true;

		/// <summary>
		/// Allow locomotion (wasd/left joystick) while flying?
		/// </summary>
		[Tooltip("Allow locomotion (wasd/left joystick) while flying? (Default: false)")]
		public bool allowLoco = false;

		/// <summary>
		/// Avatars using the avatar detection system may have wingtip, weight, etc. modifiers intended to personalize how they feel in the air. Set this value to true to use these modifiers or false if you want them disregarded for consistency. (Note: avatar size detection is not an Avatar Modifier; size-related calculations will always apply even if this setting is set to false. see <see cref="useAvatarScale"/>)
		/// </summary>
		[Tooltip(
			"Avatars using the avatar detection system may have wingtip, weight, etc. modifiers intended to personalize how they feel in the air. Set this value to true to use these modifiers or false if you want them disregarded for consistency. (Note: avatar size detection is not an Avatar Modifier; size-related calculations will always apply even if this setting is set to false.) (Default: true)"
		)]
		public bool useAvatarModifiers = true;

		/// <summary>
		/// Use the avatar's scale to affect the physics of flight. This is useful for making smaller avatars feel lighter and larger avatars feel heavier.
		/// </summary>
		[Tooltip("Use the avatar's scale to affect the physics of flight. This is useful for making smaller avatars feel lighter and larger avatars feel heavier. (Default: true)")]
		public bool useAvatarScale = true;

		/// <summary>
		/// Allow gliding?
		/// </summary>
		[Tooltip("Allow gliding. (Default: true)")]
		public bool canGlide = true;

		/// <summary>
		/// Avatars can glide directly from a fall without having to flap first. This behavior is more intuitive for gliding off cliffs, but may cause players to trigger gliding on accident more often when they just want to fall.
		/// </summary>
		[Tooltip(
			"Avatars can glide directly from a fall without having to flap first. This behavior is more intuitive for gliding off cliffs, but may cause players to trigger gliding on accident more often when they just want to fall. (Default: false)"
		)]
		public bool fallToGlide = true;
		/// <summary>
		/// The number of ticks the player must be falling before automatically gliding. This is only used if <see cref="fallToGlide"/> is true.
		/// </summary>
		[Tooltip("The number of ticks the player must be falling before automatically gliding. This is only used if Fall to Glide is enabled. (Default: 20)")]
		[Range(1, 100)]
		public int fallToGlideActivationDelay = 20;

		#region Advanced Settings
		/// <summary>
		/// Angle to offset the gliding direction by from your hands.
		/// </summary>
		[Header("Advanced Settings (Only for specialized use!)")]
		[Tooltip("Angle to offset the gliding direction by from your hands. (Default: 0)")]
		public float glideAngleOffset = 0f;

		/// <summary>
		/// How much Flap Strength and Flight Gravity are affected by an avatar's armspan. Default values will make smaller avis feel lighter and larger avis heavier.
		/// </summary>
		[Tooltip(
			"How much Flap Strength and Flight Gravity are affected by an avatar's armspan. Default values will make smaller avis feel lighter and larger avis heavier."
		)]
		public AnimationCurve sizeCurve = new AnimationCurve(new Keyframe(0.05f, 2), new Keyframe(1, 1), new Keyframe(20, 0.00195f));

		/// <summary>
		/// Modifier for horizontal flap strength. Makes flapping forwards easier.
		/// </summary>
		[Tooltip("Modifier for horizontal flap strength. Makes flapping forwards easier. (Default: 1.5)")]
		public float horizontalStrengthMod = 1.5f;

		/// <summary>
		/// How tight you want your turns while gliding. May be dynamically decreased by Avatar Modifier: weight.
		/// </summary>
		/// <remarks>
		/// Do not reduce this below 1; it will break under some weight values if you do
		/// </remarks>
		[Tooltip("How tight you want your turns while gliding. May be dynamically decreased by Avatar Modifier: weight. (Default: 2.5)")]
		[Range(1f, 5f)]
		public float glideControl = 2.5f; // Do not reduce this below 1; it will break under some weight values if you do

		/// <summary>
		/// Slows gliding down over time.
		/// </summary>
		[Tooltip("Slows gliding down over time. (Default: 0.02)")]
		[Range(0f, 0.2f)]
		public float airFriction = 0.02f;

		/// <summary>
		/// If enabled, flight gravity will use Gravity Curve's curve instead of Size Curve's curve multiplied by Flight Gravity Base.
		/// </summary>
		[Tooltip("If enabled, flight gravity will use Gravity Curve's curve instead of Size Curve's curve multiplied by Flight Gravity Base. (Default: false)")]
		public bool useGravityCurve = false;

		/// <summary>
		/// Similar to Size Curve, but instead of modifying Flap Strength, it only affects Gravity. This value is ignored (Size Curve will be used instead) unless Use Gravity Curve is enabled.
		/// </summary>
		[Tooltip(
			"Similar to Size Curve, but instead of modifying Flap Strength, it only affects Gravity. This value is ignored (Size Curve will be used instead) unless Use Gravity Curve is enabled."
		)]
		public AnimationCurve gravityCurve = new AnimationCurve(new Keyframe(0.05f, 0.4f), new Keyframe(1, 0.2f), new Keyframe(20, 0.00039f));

		/// <summary>
		/// If a GameObject with a TextMeshPro component is attached here, debug some basic info into it. (Default: unset)
		/// </summary>
		[Tooltip("If a GameObject with a TextMeshPro component is attached here, debug some basic info into it. (Default: unset)")]
		public TextMeshProUGUI debugOutput;

		/// <summary>
		/// If enabled, banking to the left or right will force the player to rotate.
		/// </summary>
		/// <remarks>
		/// Can possibly cause network lag, but in testing it doesnt seem to.
		/// </remarks>
		[Tooltip("Banking to the left or right will force the player to rotate. (Default: true)")]
		public bool bankingTurns = true;

		/// <summary>
		/// If enabled, gravity and movement will be saved each time the user takes off, instead of just at the start of the world.
		/// </summary>
		[Tooltip("If enabled, gravity and movement will be saved each time the user takes off, instead of just at the start of the world. (Default: false)")]
		public bool dynamicPlayerPhysics = false;

		/// <summary>
		/// Helper property. Do not remove the GameObject. If it somehow got unset, then set it to an empty game object (A Load Bearing Transform object should exist in the prefab for this purpose).
		/// </summary>
		[Header("Helper property (Do not touch, unless empty, then set to empty game object)")]
		[Tooltip("Do not remove, if empty add the 'Load Bearing' game object, script will fail at runtime without this setup.")]
		public Transform loadBearingTransform; //Transforms cannot be created, they can only be gotten from game objects, it isn't possible to create either in code.
		#endregion
		#endregion

		// State Control Variables
		/// <summary>
		/// Cached reference to the local player. This is set in Start() and should not be modified.
		/// </summary>
		private VRCPlayerApi LocalPlayer;

		/// <summary>
		/// The ticks per second in deltatime form.
		/// For example, a value of 0.02f would be 50 ticks per second, or 1/50.
		/// </summary>
		private const float DeltaTimeTicksPerSecond = 1f / 20f;

		/// <summary>
		/// The current time tick value.
		/// It cycles from 0 to 99 at a rate of 50 ticks per second.
		/// </summary>
		private int timeTick = -1; // -1 until the player is valid, then this value cycles from 0-99 at 50 ticks per second
		private Vector3 RHPos;
		private Vector3 LHPos;
		private Vector3 RHPosLast = Vector3.zero;
		private Vector3 LHPosLast = Vector3.zero;
		private Quaternion RHRot;
		private Quaternion LHRot;

		/// <summary>
		/// Determines whether the controllers are held outside of an imaginary cylinder.
		/// </summary>
		private bool handsOut = false; // Are the controllers held outside of an imaginary cylinder?

		/// <summary>
		/// Indicates whether the hands are in opposite positions.
		/// </summary>
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

		/// <summary>
		/// If >0, disables flight then decreases itself by one
		/// </summary>
		private int cannotFlyTick = 0;

		/// <summary>
		/// Increased by one every tick the local players y velocity is negative
		/// </summary>
		private int fallingTick = 0;
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
		private const HumanBodyBones RightUpperArmBone = HumanBodyBones.RightUpperArm;
		private const HumanBodyBones LeftUpperArmBone = HumanBodyBones.LeftUpperArm;
		private const HumanBodyBones RightLowerArmBone = HumanBodyBones.RightLowerArm;
		private const HumanBodyBones LeftLowerArmBone = HumanBodyBones.LeftLowerArm;
		private const HumanBodyBones RightHandBone = HumanBodyBones.RightHand;
		private const HumanBodyBones LeftHandBone = HumanBodyBones.LeftHand;
		private float shoulderDistance = 0; // Distance between the two shoulders

		[HideInInspector]
		public float armspan = 1f;

		[Tooltip("Default avatar wingtipOffset. (Default: 0)")]
		public float wingtipOffset = 0;

		[Tooltip("Default avatar weight. (Default: 1)")]
		[Range(0f, 2f)]
		public float weight = 1.0f;

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
				// Bug: if avatar has been swapped, sometimes the player will be launched straight up.
				// Fix: while cannotFlyTick > 0, do not allow flying. Decreases by one each tick.
				cannotFlyTick = 20;
				setFinalVelocity = false;

				CalculateStats();
			}
		}

		public void Update()
		{
			//do a sanity check due to https://feedback.vrchat.com/udon/p/update-is-executed-for-one-frame-after-the-script-is-disabled
            //we might run for 1 extra frame if we are turned off
            if (!gameObject.activeSelf || !gameObject.activeInHierarchy)
            {
                //return as we shouldnt actually be running any of this code!
                return;
            }
			
			// FixedUpdate()'s tick rate varies per VR headset.
			// Therefore, I am using Update() to create my own fake homebrew FixedUpdate()
			// It is called MainFlightTick()
			if ((LocalPlayer != null) && LocalPlayer.IsValid())
			{
				dtFake += Time.deltaTime;
				if (dtFake >= DeltaTimeTicksPerSecond)
				{
					dtFake -= DeltaTimeTicksPerSecond;
					MainFlightTick(DeltaTimeTicksPerSecond);
				}
			}
			// Banking turns should feel smooth since it's heavy on visuals. So this block exists in Update() instead of MainFlightTick()
			if (spinningRightRound)
			{
				// Avatar modifiers affect spin speed
				float weightMod = useAvatarModifiers ? (1 - (weight - 1)) : 1;
				rotSpeed += (rotSpeedGoal - rotSpeed) * Time.deltaTime * 6 * weightMod;

				// --- BEGIN MACKANDELIUS NO-JITTER BANKING TURNS FIX ---
				//Playspace origin and actual player position seems to work as parent and child objects,
				//therefore the conclusion is that we must make the playspace origin orbit the player.
				//
				//Caching positional data and modifying a virtual origin to be translated.
				VRCPlayerApi.TrackingData trackingData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);
				loadBearingTransform.position = trackingData.position;
				loadBearingTransform.rotation = trackingData.rotation;
				Vector3 playerPos = LocalPlayer.GetPosition();

				//This function is strange.
				//I am in awe of the Unity engineers that had to fix the edge case of someone wanting to rotate the parent around a child.
				//Sure is useful in this case though.
				loadBearingTransform.RotateAround(playerPos, Vector3.up, rotSpeed * Time.deltaTime);

				//Teleport based on playspace position, with an offset to place the player at the teleport location instead of the playspace origin.
				LocalPlayer.TeleportTo(
					playerPos + (loadBearingTransform.position - playerPos),
					loadBearingTransform.rotation,
					VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint,
					true
				);
				// --- END FIX ---
			}
		}

		private void MainFlightTick(float fixedDeltaTime)
		{
			if (timeTick < 0)
			{
				// This block only runs once shortly after joining the world. (1/2)
				CalculateStats();
			}

			// Only affect velocity this tick if setFinalVelocity == true by the end
			setFinalVelocity = false;

			Vector3 playerPos = LocalPlayer.GetPosition();

			// Check if hands are being moved downward while above a certain Y threshold
			// We're using LocalPlayer.GetPosition() to turn these global coordinates into local ones
			VRCPlayerApi.TrackingData leftHandData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);
			RHPos = playerPos - leftHandData.position;
			RHRot = leftHandData.rotation;

			VRCPlayerApi.TrackingData rightHandData = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
			LHPos = playerPos - rightHandData.position;
			LHRot = rightHandData.rotation;

			if (timeTick < 0)
			{
				// This block only runs once shortly after joining the world. (2/2)
				timeTick = 0;
				RHPosLast = RHPos;
				LHPosLast = LHPos;
			}

			downThrust = 0;
			if ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y) > 0)
			{
				downThrust = ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y)) * fixedDeltaTime / armspan;
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

			// Hands are out if they are a certain distance from the torso
			handsOut = (
				Vector2.Distance(
					new Vector2(LocalPlayer.GetBonePosition(RightUpperArmBone).x, LocalPlayer.GetBonePosition(RightUpperArmBone).z),
					new Vector2(LocalPlayer.GetBonePosition(RightHandBone).x, LocalPlayer.GetBonePosition(RightHandBone).z)
				)
					> armspan / 3.3f
				&& Vector2.Distance(
					new Vector2(LocalPlayer.GetBonePosition(LeftUpperArmBone).x, LocalPlayer.GetBonePosition(LeftUpperArmBone).z),
					new Vector2(LocalPlayer.GetBonePosition(LeftHandBone).x, LocalPlayer.GetBonePosition(LeftHandBone).z)
				)
					> armspan / 3.3f
			);

			//if (Vector3.Angle(LHRot * Vector3.right, RHRot * Vector3.right) > 90)
			handsOpposite = (
				Vector3.Distance(LocalPlayer.GetBonePosition(LeftHandBone), LocalPlayer.GetBonePosition(RightHandBone)) > (armspan / 3.3f * 2) + shoulderDistance
			);

			if (!isFlapping)
			{
				// Check for the beginning of a flap
				if (
					(isFlying || handsOut)
					&& (requireJump ? !LocalPlayer.IsPlayerGrounded() : true)
					&& !IsPlayerInStation()
					&& RHPos.y < playerPos.y - LocalPlayer.GetBonePosition(RightUpperArmBone).y
					&& LHPos.y < playerPos.y - LocalPlayer.GetBonePosition(LeftUpperArmBone).y
					&& downThrust > 0.002f
				)
				{
					isFlapping = true;
					// TakeOff() will check !isFlying
					TakeOff();
				}
			}

			// This should not be an else. It can trigger the same tick as "if (!isFlapping)"
			if (isFlapping)
			{
				FlapTick();
			}

			// See fallToGlide tooltip
			if (fallToGlide && fallingTick >= fallToGlideActivationDelay && handsOut && handsOpposite && canGlide)
			{
				TakeOff();
			}

			// Flying starts when a player first flaps and ends when they become grounded
			if (isFlying)
			{
				FlyTick(fixedDeltaTime);
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
				// Hard cap velocity to prevent lag abuse
				finalVelocity = Vector3.ClampMagnitude(finalVelocity, 2000);
				LocalPlayer.SetVelocity(finalVelocity);
			}
		}

		/// <summary>
		/// Flying starts when a player first flaps and ends when they become grounded
		/// </summary>
		/// <param name="dt"></param>
		private void FlyTick(float dt)
		{
			// Check if FlyTick should be skipped this tick
			if (IsMainMenuOpen() || ((!isFlapping) && LocalPlayer.IsPlayerGrounded()))
			{
				Land();
			}
			else
			{
				// Ensure Gravity is correct
				if (LocalPlayer.GetGravityStrength() != GetFlightGravity() && LocalPlayer.GetVelocity().y < 0)
				{
					LocalPlayer.SetGravityStrength(GetFlightGravity());
				}

				// Check for a gliding pose
				// Verbose explanation: (Ensure you're not flapping) && (check for handsOut frame one, ignore handsOut afterwards) && Self Explanatory && Ditto
				if ((!isFlapping) && (isGliding || handsOut) && handsOpposite && canGlide)
				{
					// Currently, glideDelay is being disabled to alleviate a VRChat issue where avatars may spazz out while moving at high velocities.
					// However, this may reintroduce an old bug so we're keeping this here.
					// If gliding is suddenly causing you to bank up and down rapidly, uncomment this:
					// if (LocalPlayer.GetVelocity().y > -1f && (!isGliding)) {glideDelay = 3;}

					isGliding = true;
					newVelocity = setFinalVelocity ? finalVelocity : LocalPlayer.GetVelocity();

					if (glideDelay <= 1)
					{
						Vector3 newForwardRight = Quaternion.Euler(glideAngleOffset, 0, 0) * Vector3.forward;
						Vector3 newForwardLeft = Quaternion.Euler(-glideAngleOffset, 0, 0) * Vector3.forward;
						// wingDirection is a normal vector pointing towards the forward direction, based on arm/wing angle
						wingDirection = Vector3.Normalize(Vector3.Slerp(RHRot * newForwardRight, LHRot * newForwardLeft, 0.5f));
					}
					else
					{
						wingDirection = newVelocity.normalized;
						glideDelay -= 5 * dt;
					}

					// Bug: In rare cases (more common with extremely small avatars) a player's velocity is perfectly straight up/down, which breaks gliding
					// Fix: Always have some form of horizontal velocity while falling.
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
						// "Where's the logic for banking turns?" See Update()
						spinningRightRound = true;
						rotSpeedGoal = steering;
					}
					else
					{
						// Fallback "banking" which is just midair strafing. Nobody likes how this feels, should depreciate it
						wingDirection = Quaternion.Euler(0, steering, 0) * wingDirection;
					}

					// Favoring Fun over Realism
					// Verbose: X and Z are purely based on which way the wings are pointed ("forward") instead of calculating how the wind would hit each wing, for ease of VR control
					targetVelocity = Vector3.ClampMagnitude(newVelocity + (Vector3.Normalize(wingDirection) * newVelocity.magnitude), newVelocity.magnitude);

					float newGlideControl = (useAvatarModifiers && weight > 1) ? glideControl - ((weight - 1) * 0.6f) : glideControl;
					if (glideDelay > 0)
					{
						glideDelay -= 5 * dt;
					}
					newGlideControl *= (1 - glideDelay) / 1;

					finalVelocity = Vector3.Slerp(newVelocity, targetVelocity, dt * newGlideControl);

					// Apply Air Friction
					finalVelocity *= 1 - (airFriction * 0.011f);
					setFinalVelocity = true;
				}
				else // Not in a gliding pose?
				{
					isGliding = false;
					rotSpeedGoal = 0;
					glideDelay = 0;
				}
			}
		}

		/// <summary>
		/// Flapping starts when a player first flaps and ends when they stop flapping. FlapTick will run every tick.
		/// </summary>
		private void FlapTick()
		{
			if (downThrust > 0)
			{
				// Calculate force to apply based on the flap
				newVelocity = 0.011f * GetFlapStrength() * ((RHPos - RHPosLast) + (LHPos - LHPosLast));

				if (!useAvatarScale)
				{
					//scale up the flap strength by the avatar's size inversely
					// 1 / 0.1 = 10 Smaller than normal Avatar
					// 1 / 1 = 1 Normal
					// 1 / 10 = 0.1 Larger than normal Avatar
					newVelocity = newVelocity / armspan;
				}

				if (LocalPlayer.IsPlayerGrounded())
				{
					// Prevents skiing along the ground
					newVelocity = new Vector3(0, newVelocity.y, 0);
				}
				else
				{
					newVelocity.Scale(new Vector3(horizontalStrengthMod, 1, horizontalStrengthMod));
				}
				finalVelocity = LocalPlayer.GetVelocity() + newVelocity;
				// Speed cap (check, then apply flapping air friction)
				if (finalVelocity.magnitude > 0.02f * GetFlapStrength())
				{
					finalVelocity = finalVelocity.normalized * (finalVelocity.magnitude - (flapAirFriction * GetFlapStrength() * 0.011f));
				}
				setFinalVelocity = true;
			}
			else
			{
				if (IsPlayerInStation())
				{
					finalVelocity = Vector3.zero;
					setFinalVelocity = true;
				}
				isFlapping = false;
			}
		}

		public void FixedUpdate()
		{
			if (timeTick >= 0)
			{
				timeTick++;
				// Automatically update the debug output every 0.2 seconds (sorta, since certain VR headsets affect FixedUpdate())
				if (timeTick > 9)
				{
					timeTick = 0;
					if (debugOutput != null)
					{
						//Don't add tabs back here, or else they will end up in the multiline string
						debugOutput.text = string.Format(
@"Is Player Flying: {0}
Is Player Flapping: {1}
Is Player Gliding: {2}
--Internal Vars--
Hands Out: {3}
Downward Thrust: {4}
Cannot Fly: {5}
Glide Delay: {6}
--Player Controller State--
Grounded: {7}
Velocity: {8}",
							isFlying,
							isFlapping,
							isGliding,
							handsOut,
							downThrust,
							cannotFlyTick > 0,
							glideDelay,
							LocalPlayer.IsPlayerGrounded(),
							LocalPlayer.GetVelocity()
						);
					}
				}
			}
		}

		/// <summary>
		/// Immobilizes the player's locomotion. This is useful for preventing the player from moving while flying. Still allows the player to rotate, unlike VRC's method of immobilization.
		/// </summary>
		/// <param name="immobilize"></param>
		private void ImmobilizePlayer(bool immobilize)
		{
			//This is non-zero as it allows the "forward" direction of a player to still update while flying, fixing some animation bugs.
			const float ImmobileSpeed = 0.001f;

			if (immobilize)
			{
				if (dynamicPlayerPhysics)
				{
					oldWalkSpeed = LocalPlayer.GetWalkSpeed();
					oldRunSpeed = LocalPlayer.GetRunSpeed();
					oldStrafeSpeed = LocalPlayer.GetStrafeSpeed();
				}

				LocalPlayer.SetWalkSpeed(ImmobileSpeed);
				LocalPlayer.SetRunSpeed(ImmobileSpeed);
				LocalPlayer.SetStrafeSpeed(ImmobileSpeed);
			}
			else
			{
				LocalPlayer.SetWalkSpeed(oldWalkSpeed);
				LocalPlayer.SetRunSpeed(oldRunSpeed);
				LocalPlayer.SetStrafeSpeed(oldStrafeSpeed);
			}
		}

		/// <summary>
		/// Running this function will recalculate important variables needed for Flap Strength.
		/// </summary>
		private void CalculateStats()
		{
			// `armspan` does not include the distance between shoulders. shoulderDistance stores this value by itself.
			armspan =
				Vector3.Distance(LocalPlayer.GetBonePosition(LeftUpperArmBone), LocalPlayer.GetBonePosition(LeftLowerArmBone))
				+ Vector3.Distance(LocalPlayer.GetBonePosition(LeftLowerArmBone), LocalPlayer.GetBonePosition(LeftHandBone))
				+ Vector3.Distance(LocalPlayer.GetBonePosition(RightUpperArmBone), LocalPlayer.GetBonePosition(RightLowerArmBone))
				+ Vector3.Distance(LocalPlayer.GetBonePosition(RightLowerArmBone), LocalPlayer.GetBonePosition(RightHandBone));
			shoulderDistance = Vector3.Distance(LocalPlayer.GetBonePosition(LeftUpperArmBone), LocalPlayer.GetBonePosition(RightUpperArmBone));
			Logger.Log("Armspan: " + armspan.ToString() + " Shoulder Distance: " + shoulderDistance.ToString(), this);
		}

		/// <summary>
		/// Set necessary values for beginning flight. Automatically ensures it only runs on the first tick of flight.
		/// </summary>
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
					CheckPhysicsUnchanged();
				}
				LocalPlayer.SetGravityStrength(GetFlightGravity());
				if (!allowLoco)
				{
					ImmobilizePlayer(true);
				}
				Logger.Log("Took off.", this);
			}
		}

		/// <summary>
		/// Checks if the world gravity or player movement has changed from the saved values and throws a warning if so.
		/// </summary>
		private void CheckPhysicsUnchanged()
		{
			// Log a warning if gravity values differ from what we have saved
			if (LocalPlayer.GetGravityStrength() != oldGravityStrength)
			{
				Logger.LogWarning(
					"World gravity is different than the saved gravity, this may cause issues. If you want to avoid this, edit scripts to inform OpenFlight of the new world gravity using UpdatePlayerPhysics().",
					this
				);
				Logger.LogWarning("Saved Gravity: " + oldGravityStrength.ToString(), this);
			}

			// Log a warning if movement values differ from what we have saved
			if (LocalPlayer.GetWalkSpeed() != oldWalkSpeed || LocalPlayer.GetRunSpeed() != oldRunSpeed || LocalPlayer.GetStrafeSpeed() != oldStrafeSpeed)
			{
				Logger.LogWarning(
					"Player movement is different than the saved movement, this may cause issues. If you want to avoid this, edit scripts to inform OpenFlight of the new player movement using UpdatePlayerPhysics().",
					this
				);
				Logger.LogWarning(
					"Saved Walk Speed: " + oldWalkSpeed.ToString() + " Saved Run Speed: " + oldRunSpeed.ToString() + " Saved Strafe Speed: " + oldStrafeSpeed.ToString(),
					this
				);
			}
		}

		private readonly Collider[] _colliders = new Collider[50];
		/// <summary>
		/// Utility method to detect main menu status. Technique pulled from <see href="https://github.com/Superbstingray/UdonPlayerPlatformHook">UdonPlayerPlatformHook</see>
		/// </summary>
		/// <returns>True if the main menu is open, false otherwise</returns>
		private bool IsMainMenuOpen()
		{
			const int layer = 2 << 18;
			//swapped from OverlapSphere to OverlapSphereNonAlloc, as it does not allocate memory each time it is called,
			//saving on garbage collection. Also doesnt require a .Length check, as it returns the number of colliders it found inherently.
			//Second note, instead of using localplayer position, we use the head position, as the player position can desync depending on Holoport and VRC changes.
			int uiColliderCount = Physics.OverlapSphereNonAlloc(LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position, 10f, _colliders, layer);
			//commented out due to extern count, this uses 3
			//return uiColliderCount == 8 || uiColliderCount == 9 || uiColliderCount == 10;

			//this uses 2 externs
			return 8 <= uiColliderCount && uiColliderCount <= 10;
		}

		/// <summary>
		/// Utility method to detect if the player is in a station.
		/// </summary>
		/// <returns>True if the player is in a station, false otherwise</returns>
		private bool IsPlayerInStation()
		{
			//player local layer, which is layer 9
			const int layer = 2 << 9;
			int colliderCount = Physics.OverlapSphereNonAlloc(LocalPlayer.GetPosition(), 50f, _colliders, layer);

			//if the count is 0, then we can garuntee the player is in a station
			if (colliderCount == 0)
			{
				return true;
			}

			//loop over the colliders and if there is colliders that are null, then the player is not in a station
			for (int i = 0; i < colliderCount; i++)
			{
				if (_colliders[i] == null)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Effectually disables all flight-related variables and functions. This does not permanently disable flight (the player can just flap again); disable the GameObject instead.
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
				ImmobilizePlayer(false);
			}

			if (!dynamicPlayerPhysics)
			{
				CheckPhysicsUnchanged();
			}

			Logger.Log("Landed.", this);
		}

		private float GetFlapStrength()
		{
			float flapStrengthMod = useAvatarModifiers ? wingtipOffset * 8 : 10;

			return sizeCurve.Evaluate(GetArmspanValue()) * (flapStrengthBase + flapStrengthMod);
		}

		/// <summary>
		/// Returns the current armspan value, or a value of 1 if <see cref="useAvatarScale"/> is false.
		/// </summary>
		/// <returns></returns>
		private float GetArmspanValue()
		{
			return useAvatarScale ? armspan : 1.0f;
		}

		private float GetFlightGravity()
		{
			float gravity = useGravityCurve
				? gravityCurve.Evaluate(GetArmspanValue()) * GetArmspanValue()
				: sizeCurve.Evaluate(GetArmspanValue()) * flightGravityBase * GetArmspanValue();

			if (useAvatarModifiers)
			{
				// default settings
				return gravity * weight;
			}
			return gravity;
		}

		/// <summary>
		/// Calling this function tells the script to pull in the worlds values for player physics. This is useful if you have a world that changes gravity or movement often, but still want water systems to work.
		/// </summary>
		/// <remarks>
		/// This function is only useful if dynamic player physics is disabled. Otherwise, it will do nothing.
		/// </remarks>
		public void UpdatePlayerPhysics()
		{
			if (dynamicPlayerPhysics)
			{
				Logger.Log("Dynamic Player Physics is enabled. Player Physics will be updated automatically.", this);
				return;
			}

			oldGravityStrength = LocalPlayer.GetGravityStrength();
			oldWalkSpeed = LocalPlayer.GetWalkSpeed();
			oldRunSpeed = LocalPlayer.GetRunSpeed();
			oldStrafeSpeed = LocalPlayer.GetStrafeSpeed();
			Logger.Log("Player Physics updated.", this);
		}

		/// <summary>
		/// Stores the default values of all settings fields in the script.
		/// </summary>
		private DataDictionary defaultsStore;

		/// <summary>
		/// Initializes all default values. This should not be called by end users in most cases.
		/// </summary>
		public void InitializeDefaults()
		{
			defaultsStore = new DataDictionary();
			defaultsStore.SetValue((DataToken)nameof(flapStrengthBase), flapStrengthBase);
			defaultsStore.SetValue((DataToken)nameof(flightGravityBase), flightGravityBase);
			defaultsStore.SetValue((DataToken)nameof(requireJump), requireJump);
			defaultsStore.SetValue((DataToken)nameof(allowLoco), allowLoco);
			defaultsStore.SetValue((DataToken)nameof(useAvatarModifiers), useAvatarModifiers);
			defaultsStore.SetValue((DataToken)nameof(wingtipOffset), wingtipOffset);
			defaultsStore.SetValue((DataToken)nameof(canGlide), canGlide);
			defaultsStore.SetValue((DataToken)nameof(fallToGlide), fallToGlide);
			defaultsStore.SetValue((DataToken)nameof(horizontalStrengthMod), horizontalStrengthMod);
			defaultsStore.SetValue((DataToken)nameof(glideControl), glideControl);
			defaultsStore.SetValue((DataToken)nameof(airFriction), airFriction);
			defaultsStore.SetValue((DataToken)nameof(useGravityCurve), useGravityCurve);
			defaultsStore.SetValue((DataToken)nameof(bankingTurns), bankingTurns);
			defaultsStore.SetValue((DataToken)nameof(glideAngleOffset), glideAngleOffset);
			defaultsStore.SetValue((DataToken)nameof(useAvatarScale), useAvatarScale);
			defaultsStore.SetValue((DataToken)nameof(fallToGlideActivationDelay), fallToGlideActivationDelay);
			Logger.Log(string.Format("Defaults initialized ({0} values).", defaultsStore.Count), this);
		}

		/// <summary>
		/// Restores all values to their prefab defaults
		/// </summary>
		public void RestoreDefaults()
		{
			flapStrengthBase = GetDefaultValue(nameof(flapStrengthBase)).Int;
			flightGravityBase = GetDefaultValue(nameof(flightGravityBase)).Float;
			requireJump = GetDefaultValue(nameof(requireJump)).Boolean;
			allowLoco = GetDefaultValue(nameof(allowLoco)).Boolean;
			useAvatarModifiers = GetDefaultValue(nameof(useAvatarModifiers)).Boolean;
			wingtipOffset = GetDefaultValue(nameof(wingtipOffset)).Float;
			canGlide = GetDefaultValue(nameof(canGlide)).Boolean;
			fallToGlide = GetDefaultValue(nameof(fallToGlide)).Boolean;
			horizontalStrengthMod = GetDefaultValue(nameof(horizontalStrengthMod)).Float;
			glideControl = GetDefaultValue(nameof(glideControl)).Float;
			airFriction = GetDefaultValue(nameof(airFriction)).Float;
			useGravityCurve = GetDefaultValue(nameof(useGravityCurve)).Boolean;
			bankingTurns = GetDefaultValue(nameof(bankingTurns)).Boolean;
			glideAngleOffset = GetDefaultValue(nameof(glideAngleOffset)).Float;
			useAvatarScale = GetDefaultValue(nameof(useAvatarScale)).Boolean;
			fallToGlideActivationDelay = GetDefaultValue(nameof(fallToGlideActivationDelay)).Int;
			Logger.Log(string.Format("Defaults restored ({0} values).", defaultsStore.Count), this);
		}

		/// <summary>
		/// Gets the default value for a given key. If the key is not found, a warning is logged and a default value of 0 is returned.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private DataToken GetDefaultValue(string key)
		{
			if (defaultsStore.TryGetValue(key, out DataToken value))
			{
				return value;
			}
			else
			{
				Logger.LogError("Key not found in defaults store: " + key, this);
				return new DataToken(0);
			}
		}
	}
}
