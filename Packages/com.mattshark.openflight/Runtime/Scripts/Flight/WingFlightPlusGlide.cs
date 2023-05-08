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

	public class WingFlightPlusGlide : UdonSharpBehaviour
	{
		[Header("Basic Settings")]
		// Both of these "base" values are by default affected by the avatar's armspan. See sizeCurve.
		[Tooltip("Want flaps to be stronger or weaker? Change this value first. (Default: 225)")]
		[Range(100, 800)]
		public int flapStrengthBase = 225;
		int flapStrengthBase_DEFAULT = 225;

		[Tooltip("Base gravity while flying (Default: 0.3)")]
		public float flightGravityBase = 0.3f;
		float flightGravityBase_DEFAULT = 0.3f;

		[Tooltip("Require the player to jump before flapping can occur? Makes it less likely to trigger a flap by accident. (Default: false)")]
		public bool requireJump = false;
		bool requireJump_DEFAULT = false;

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

		// Essential Variables
		private VRCPlayerApi localPlayer;
		private double debugTemp;
		private int timeTick = -1; // -1 until the player is valid, then this value cycles from 0-99 at 50 ticks per second
		private Vector3 RHPos;
		private Vector3 LHPos;
		private Vector3 RHPosLast = new Vector3(0f, float.NegativeInfinity, 0f);
		private Vector3 LHPosLast = new Vector3(0f, float.NegativeInfinity, 0f);
		private Quaternion RHRot;
		private Quaternion LHRot;
		private Quaternion playerRot;
		private float handsOutAmount = 0.01f; // How far apart are your hands from your central body? 0-1
		private bool handsOut = false; // Are the controllers held outside of an imaginary cylinder?
		private bool isFlapping = false; // Doing the arm motion
		private bool isFlying = false; // Currently in the air after/during a flap
		private bool isGliding = false; // Has arms out while flying
		private int cannotFlyTick = 0; // If >0, disables flight then decreases itself by one
		private int fallingTick = 0; // Increased by one every tick one's y velocity > 0
		private float tmpFloat;
		private float tmpFloatB;
		private float tmpFloatH; // Reserved for use in helper functions
		private Vector3 tmpV3;
		private float dtFake = 0;

		// Variables related to Velocity
		private Vector3 finalVelocity; // Modify this value instead of the player's velocity directly, then run `setFinalVelocity = true`
		private bool setFinalVelocity;
		private Vector3 newVelocity; // tmp var
		private Vector3 targetVelocity; // tmp var, usually associated with slerping/lerping
		private float downThrust = 0f;
		private float flapAirFriction = 0.04f; // Prevents the gain of infinite speed while flapping. Set to 0 to remove this feature. THIS IS NOT A MAX SPEED

		// Variables related to gliding
		private Vector3 wingDirection;
		private float steering;
		private bool spinningRightRound = false; // Can't get that Protogen animation out of my head
		private float rotSpeed = 0;
		private float rotSpeedGoal = 0;
		
		// Variables related to wind
		private Vector3 wingPlaneNormalL;
		private Vector3 wingPlaneNormalR;
		[HideInInspector]
		public bool windy = false;
		[HideInInspector]
		public Vector3 windVector = new Vector3(0,0,0);

		// "old" values are the world's defaults (recorded immediately before they are modified)
		private float oldGravityStrength;
		private float oldWalkSpeed;
		private float oldRunSpeed;
		private float oldStrafeSpeed;

		// Avatar-specific properties
		private HumanBodyBones rightUpperArmBone; // Bones won't be given a value until localPlayer.IsValid()
		private HumanBodyBones leftUpperArmBone;
		private HumanBodyBones rightLowerArmBone;
		private HumanBodyBones leftLowerArmBone;
		private HumanBodyBones rightHandBone;
		private HumanBodyBones leftHandBone;
		private HumanBodyBones spine;
		private HumanBodyBones chest;
		private float spineToChest = 0; // These two vars are only used to check if an avi has been recently swapped/scaled
		private float spineToChest_last = 0;

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
			localPlayer = Networking.LocalPlayer;
			//oldGravityStrength = localPlayer.GetGravityStrength();
			//oldWalkSpeed = localPlayer.GetWalkSpeed();
			//oldRunSpeed = localPlayer.GetRunSpeed();
			//oldStrafeSpeed = localPlayer.GetStrafeSpeed();
		}

		public void OnEnable()
		{
			timeTick = -5;
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
		}

		public void Update()
		{
			if ((localPlayer != null) && localPlayer.IsValid())
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
				loadBearingTransform.position = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin).position;
				loadBearingTransform.rotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin).rotation;
				playerHolder = localPlayer.GetPosition();

				//This function is strange.
				//I am in awe of the Unity engineers that had to fix the edge case of someone wanting to rotate the parent around a child.
				//Sure is useful in this case though.
				loadBearingTransform.RotateAround(playerHolder, Vector3.up, rotSpeed * Time.deltaTime);

				//Teleport based on playspace position, with an offset to place the player at the teleport location instead of the playspace origin.
				localPlayer.TeleportTo(
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
				spine = HumanBodyBones.Spine;
				chest = HumanBodyBones.Chest;
				CalculateStats();
			}
			setFinalVelocity = false;
			// Check if hands are being moved downward while above a certain Y threshold
			// We're using localPlayer.GetPosition() to turn these global coordinates into local ones
			RHPos = localPlayer.GetPosition() - localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
			LHPos = localPlayer.GetPosition() - localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
			if ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y) > 0)
			{
				downThrust = ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y)) * dt / armspan;
			}
			else
			{
				downThrust = 0;
			}
			// Check if player is falling
			if ((!localPlayer.IsPlayerGrounded()) && localPlayer.GetVelocity().y < 0)
			{
				fallingTick++;
			}
			else
			{
				fallingTick = 0;
			}
			// Check if hands are held out
			tmpFloat = Vector2.Distance(
					new Vector2(localPlayer.GetBonePosition(rightUpperArmBone).x, localPlayer.GetBonePosition(rightUpperArmBone).z),
					new Vector2(localPlayer.GetBonePosition(rightHandBone).x, localPlayer.GetBonePosition(rightHandBone).z)
				);
			tmpFloatB = Vector2.Distance(
					new Vector2(localPlayer.GetBonePosition(leftUpperArmBone).x, localPlayer.GetBonePosition(leftUpperArmBone).z),
					new Vector2(localPlayer.GetBonePosition(leftHandBone).x, localPlayer.GetBonePosition(leftHandBone).z)
				);
			// handsOutAmount chooses the closest hand to the body
			handsOutAmount = (tmpFloat < tmpFloatB ? tmpFloat : tmpFloatB) / (armspan / 2);
			if (handsOutAmount > 0.6f)
			{
				handsOut = true;
			}
			else
			{
				handsOut = false;
			}

			if (!isFlapping)
			{
				// Check for the beginning of a flap
				if (
					(isFlying ? true : handsOut)
					&& (requireJump ? !localPlayer.IsPlayerGrounded() : true)
					&& RHPos.y < localPlayer.GetPosition().y - localPlayer.GetBonePosition(rightUpperArmBone).y
					&& LHPos.y < localPlayer.GetPosition().y - localPlayer.GetBonePosition(leftUpperArmBone).y
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
					if (localPlayer.IsPlayerGrounded())
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
					finalVelocity = localPlayer.GetVelocity() + newVelocity;
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
			if (fallToGlide && fallingTick >= 20 && handsOut && canGlide)
			{
				TakeOff();
			}

			// -- STATE: Flying
			// (Flying starts when a player first flaps and ends when they become grounded)
			if (isFlying)
			{
				if ((!isFlapping) && localPlayer.IsPlayerGrounded())
				{
					Land();
				}
				else
				{
					// ---=== Run every frame while the player is "flying" ===---
					if (localPlayer.GetGravityStrength() != (flightGravity()) && localPlayer.GetVelocity().y < 0)
					{
						localPlayer.SetGravityStrength(flightGravity());
					}
					LHRot = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation;
					RHRot = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
					// Calculate the plane normal of each wing. IE if the player is T-Posing, both wing plane normals will point upwards
					wingPlaneNormalR = Vector3.Normalize(RHRot * Vector3.up);
					wingPlaneNormalL = Vector3.Normalize(LHRot * Vector3.down); // Why is it down? I don't know, it's just how it turned up in testing
					if ((!isFlapping) && handsOut && canGlide)
					{
						// Gliding, banking, and steering logic
						isGliding = true;
						newVelocity = setFinalVelocity ? finalVelocity : localPlayer.GetVelocity();
						wingDirection = Vector3.Normalize(Vector3.Slerp(RHRot * Vector3.forward, LHRot * Vector3.forward, 0.5f)); // The direction the player should go based on how they have angled their wings

						// Hotfix: Always have some form of horizontal velocity while falling, except when windy. In rare cases (more common with extremely small avatars) a player's velocity is perfectly straight up/down, which breaks gliding
						if ((!windy) && newVelocity.y < 0.3f && newVelocity.x == 0 && newVelocity.z == 0)
						{
							Vector2 tmpV2 = new Vector2(wingDirection.x, wingDirection.z).normalized * 0.145f;
							newVelocity = new Vector3(Mathf.Round(tmpV2.x * 10) / 10, newVelocity.y, Mathf.Round(tmpV2.y * 10) / 10);
						}

						// Steering logic
						steering = (RHPos.y - LHPos.y) * 80 / armspan;
						if (steering > 35)
						{
							steering = 35;
						}
						else if (steering < -35)
						{
							steering = -35;
						}
						
						if (bankingTurns)
						{
							spinningRightRound = true;
							rotSpeedGoal = steering;
						}
						else
						{
							if (!windy)
							{
								// Default "banking" which is just midair strafing
								wingDirection = Quaternion.Euler(0, steering, 0) * wingDirection;
							}
						}
						if (!windy)
						{
							// Apply wingDirection
							// X and Z are purely based on which way the wings are pointed ("forward") for ease of VR control
							targetVelocity = Vector3.ClampMagnitude(newVelocity + (Vector3.Normalize(wingDirection) * newVelocity.magnitude), newVelocity.magnitude);
							// tmpFloat == glideControl (Except if weight > 1, glideControl temporarily decreases)
							tmpFloat = (useAvatarModifiers && weight > 1) ? glideControl - ((weight - 1) * 0.6f) : glideControl;
							finalVelocity = Vector3.Slerp(newVelocity, targetVelocity, dt * tmpFloat);
							// Apply Air Friction
							finalVelocity = finalVelocity * (1 - (airFriction * dt));
							setFinalVelocity = true;
						}
					}
					else
					{
						isGliding = false;
						rotSpeedGoal = 0;
					}
					if (windy)
					{
						finalVelocity = setFinalVelocity ? finalVelocity : localPlayer.GetVelocity();
						// Handle wind
						// How much speed should be added
						tmpFloat = windVector.magnitude * GetWingArea(windVector.normalized) * dt;
						// Player's current speed in the same direction as the wind
						tmpFloatB = Vector3.Dot(finalVelocity, windVector.normalized);
						
						if (tmpFloatB < windVector.magnitude && tmpFloat > 0.03f)
						{
							tmpV3 = -1 * finalVelocity * GetWingArea(-1 * finalVelocity) + windVector; // The force pushing on the player's wings from simply moving combined with the force pushing on the player's wings from the wind zone
							newVelocity = -1 * Vector3.Reflect(tmpV3, Vector3.Slerp(wingPlaneNormalL, wingPlaneNormalR, 0.5f)); // The force pushing on the wings based on its angle
							finalVelocity = finalVelocity + (newVelocity.normalized * tmpFloat);
							setFinalVelocity = true;
						}
					}
				}
			}
			RHPosLast = RHPos;
			LHPosLast = LHPos;

			// Bug check: if avatar has been swapped, sometimes the player will be launched straight up
			spineToChest = Vector3.Distance(localPlayer.GetBonePosition(chest), localPlayer.GetBonePosition(spine));
			if (Mathf.Abs(spineToChest - spineToChest_last) > 0.001f)
			{
				cannotFlyTick = 20;
			}
			if (cannotFlyTick > 0)
			{
				setFinalVelocity = false;
				cannotFlyTick--;
			}
			spineToChest_last = spineToChest;
			// end Bug Check

			if (setFinalVelocity)
			{
				localPlayer.SetVelocity(finalVelocity);
			}
		}

		public void FixedUpdate()
		{
			if (timeTick >= 0)
			{
				timeTick = timeTick + 1;
				// Automatically CalculateStats() every second (assuming VRChat uses the Unity default of 50 ticks per second)
				if (timeTick > 9)
				{
					timeTick = 0;
					CalculateStats();
					if (debugOutput != null)
					{
						debugOutput.text =
							string.Concat("\nIsFlying: ", isFlying.ToString())
							+ string.Concat("\nIsFlapping: ", isFlapping.ToString())
							+ string.Concat("\nIsGliding: ", isGliding.ToString())
							+ string.Concat("\nHandsOut: ", handsOut.ToString())
							+ string.Concat("\nDownThrust: ", downThrust.ToString())
							+ string.Concat("\nGrounded: ", localPlayer.IsPlayerGrounded().ToString())
							+ string.Concat("\nCannotFly: ", (cannotFlyTick > 0).ToString())
							+ string.Concat("\nThing: ", GetWingArea(windVector.normalized).ToString());
					}
				}
			}
		}

		// Immobilize Locomotion but still allow body rotation
		private void ImmobilizePart(bool b)
		{
			if (b)
			{
				oldWalkSpeed = localPlayer.GetWalkSpeed();
				oldRunSpeed = localPlayer.GetRunSpeed();
				oldStrafeSpeed = localPlayer.GetStrafeSpeed();
				localPlayer.SetWalkSpeed(0f);
				localPlayer.SetRunSpeed(0f);
				localPlayer.SetStrafeSpeed(0f);
			}
			else
			{
				localPlayer.SetWalkSpeed(oldWalkSpeed);
				localPlayer.SetRunSpeed(oldRunSpeed);
				localPlayer.SetStrafeSpeed(oldStrafeSpeed);
			}
		}

		// Flight Strength, etc. are based on armspan and whatnot.
		// This function can be re-run to recalculate these values at any time (upon switching avatars for example)
		private void CalculateStats()
		{
			// `armspan` does not include the distance between shoulders
			armspan =
				Vector3.Distance(localPlayer.GetBonePosition(leftUpperArmBone), localPlayer.GetBonePosition(leftLowerArmBone))
				+ Vector3.Distance(localPlayer.GetBonePosition(leftLowerArmBone), localPlayer.GetBonePosition(leftHandBone))
				+ Vector3.Distance(localPlayer.GetBonePosition(rightUpperArmBone), localPlayer.GetBonePosition(rightLowerArmBone))
				+ Vector3.Distance(localPlayer.GetBonePosition(rightLowerArmBone), localPlayer.GetBonePosition(rightHandBone));
		}

		// Set necessary values for beginning flight
		public void TakeOff()
		{
			if (!isFlying)
			{
				isFlying = true;
				CalculateStats();
				oldGravityStrength = localPlayer.GetGravityStrength();
				localPlayer.SetGravityStrength(flightGravity());
				if (!allowLoco)
				{
					ImmobilizePart(true);
				}
			}
		}

		// Effectually disables all flight-related variables
		public void Land()
		{
			isFlying = false;
			isFlapping = false;
			isGliding = false;
			spinningRightRound = false;
			rotSpeed = 0;
			rotSpeedGoal = 0;
			localPlayer.SetGravityStrength(oldGravityStrength);
			if (!allowLoco)
			{
				ImmobilizePart(false);
			}
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
				tmpFloatH = gravityCurve.Evaluate(armspan) * armspan;
			}
			else
			{
				// default settings
				tmpFloatH = sizeCurve.Evaluate(armspan) * flightGravityBase * armspan;
			}
			if (useAvatarModifiers)
			{
				// default settings
				return tmpFloatH * weight;
			}
			else
			{
				return tmpFloatH;
			}
		}
		
		public float GetWingArea(Vector3 windDirection)
		{
			// Basically, how much wing area is the wind impacting?
			tmpFloatH = ((Mathf.Abs(Vector3.Angle(wingPlaneNormalL, windVector) - 90) / 100) + 0.1f) / 2;
			tmpFloatH += ((Mathf.Abs(Vector3.Angle(wingPlaneNormalR, windVector) - 90) / 100) + 0.1f) / 2;
			return tmpFloatH * handsOutAmount;
		}

		// --- Helper Functions ---

		public void EnableBetaFeatures()
		{
			bankingTurns = true;
		}

		public void DisableBetaFeatures()
		{
			bankingTurns = false;
		}

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
		}

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
		}
	}
}
