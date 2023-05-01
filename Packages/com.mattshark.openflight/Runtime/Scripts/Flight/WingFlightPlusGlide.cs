using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

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
	private bool isFlapping = false; // Doing the arm motion
	private bool isFlying = false; // Currently in the air after/during a flap
	private bool isGliding = false; // Has arms out while flying
	private int cannotFlyTick = 0; // If >0, disables flight then decreases itself by one
	private int fallingTick = 0; // Increased by one every tick one's y velocity > 0
	private float tmpFloat;
	private float dtFake = 0;
	private int intUI = 0;
	private bool menuOpen = false;

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
		LocalPlayer = Networking.LocalPlayer;
		//oldGravityStrength = LocalPlayer.GetGravityStrength();
		//oldWalkSpeed = LocalPlayer.GetWalkSpeed();
		//oldRunSpeed = LocalPlayer.GetRunSpeed();
		//oldStrafeSpeed = LocalPlayer.GetStrafeSpeed();
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
			spine = HumanBodyBones.Spine;
			chest = HumanBodyBones.Chest;
			CalculateStats();
		}
		setFinalVelocity = false;
		// Check if hands are being moved downward while above a certain Y threshold
		// We're using LocalPlayer.GetPosition() to turn these global coordinates into local ones
		RHPos = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
		LHPos = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
		if ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y) > 0)
		{
			downThrust = ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y)) * dt / armspan;
		}
		else
		{
			downThrust = 0;
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
		// Check if hands are held out
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
		else
		{
			handsOut = false;
		}

		if (!isFlapping)
		{
			intUI = Physics.OverlapSphere(LocalPlayer.GetPosition(), 10f, 524288).Length;
			menuOpen = (intUI == 8 || intUI == 9 || intUI == 10);
			// Check for the beginning of a flap
			if (
				!menuOpen
				&& (isFlying ? true : handsOut)
				&& (requireJump ? !LocalPlayer.IsPlayerGrounded() : true)
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
		if (fallToGlide && fallingTick >= 20 && handsOut && canGlide)
		{
			TakeOff();
		}

		// -- STATE: Flying
		// (Flying starts when a player first flaps and ends when they become grounded)
		if (isFlying)
		{
			if ((!isFlapping) && LocalPlayer.IsPlayerGrounded())
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
				LHRot = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation;
				RHRot = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
				if ((!isFlapping) && handsOut && canGlide)
				{
					// Gliding, banking, and steering logic
					isGliding = true;
					newVelocity = setFinalVelocity ? finalVelocity : LocalPlayer.GetVelocity();
					wingDirection = Vector3.Normalize(Quaternion.Slerp(LHRot, RHRot, 0.5f) * Vector3.forward); // The direction the player should go based on how they've angled their wings
					// Hotfix: Always have some form of horizontal velocity while falling. In rare cases (more common with extremely small avatars) a player's velocity is perfectly straight up/down, which breaks gliding
					if (newVelocity.y < 0.3f && newVelocity.x == 0 && newVelocity.z == 0)
					{
						Vector2 tmpV2 = new Vector2(wingDirection.x, wingDirection.z).normalized * 0.145f;
						newVelocity = new Vector3(Mathf.Round(tmpV2.x * 10) / 10, newVelocity.y, Mathf.Round(tmpV2.y * 10) / 10);
					}
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
						// Default "banking" which is just midair strafing
						wingDirection = Quaternion.Euler(0, steering, 0) * wingDirection;
					}
					// X and Z are purely based on which way the wings are pointed ("forward") for ease of VR control
					targetVelocity = Vector3.ClampMagnitude(newVelocity + (Vector3.Normalize(wingDirection) * newVelocity.magnitude), newVelocity.magnitude);
					// tmpFloat == glideControl (Except if weight > 1, glideControl temporarily decreases)
					tmpFloat = (useAvatarModifiers && weight > 1) ? glideControl - ((weight - 1) * 0.6f) : glideControl;
					finalVelocity = Vector3.Slerp(newVelocity, targetVelocity, dt * tmpFloat);
					// Apply Air Friction
					finalVelocity = finalVelocity * (1 - (airFriction * dt));
					setFinalVelocity = true;
				}
				else
				{
					isGliding = false;
					rotSpeedGoal = 0;
				}
			}
		}
		RHPosLast = RHPos;
		LHPosLast = LHPos;

		// Bug check: if avatar has been swapped, sometimes the player will be launched straight up
		spineToChest = Vector3.Distance(LocalPlayer.GetBonePosition(chest), LocalPlayer.GetBonePosition(spine));
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
			LocalPlayer.SetVelocity(finalVelocity);
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
						+ string.Concat("\nGrounded: ", LocalPlayer.IsPlayerGrounded().ToString())
						+ string.Concat("\nCannotFly: ", (cannotFlyTick > 0).ToString());
				}
			}
		}
	}

	// Immobilize Locomotion but still allow body rotation
	private void ImmobilizePart(bool b)
	{
		if (b)
		{
			oldWalkSpeed = LocalPlayer.GetWalkSpeed();
			oldRunSpeed = LocalPlayer.GetRunSpeed();
			oldStrafeSpeed = LocalPlayer.GetStrafeSpeed();
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
	}

	// Set necessary values for beginning flight
	public void TakeOff()
	{
		if (!isFlying)
		{
			isFlying = true;
			CalculateStats();
			oldGravityStrength = LocalPlayer.GetGravityStrength();
			LocalPlayer.SetGravityStrength(flightGravity());
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
		LocalPlayer.SetGravityStrength(oldGravityStrength);
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
