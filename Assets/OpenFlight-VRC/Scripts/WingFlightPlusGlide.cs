
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class WingFlightPlusGlide : UdonSharpBehaviour {
    [Header("Basic Settings")]
    // Both of these "base" values are by default affected by the avatar's wingspan. See sizeCurve.
    [Tooltip("Want flaps to be stronger or weaker? Change this value first. (Default: 170)")]
    public int flapStrengthBase = 170;
    [Tooltip("Base gravity while flying (Default: 0.2)")]
    public float flightGravityBase = 0.2f;
    [Tooltip("Require the player to jump before flapping can occur? Makes it less likely to trigger a flap by accident. (Default: false)")]
    public bool requireJump;
    [Tooltip("Allow locomotion (wasd/left joystick) while flying? (Default: false)")]
    public bool allowLoco;
    
    [Header("Advanced Settings (Only for specialized use!)")]
    [Tooltip("How much Flap Strength and Flight Gravity are affected by an avatar's wingspan. Default values will make smaller avis feel lighter and larger avis heavier.")]
    public AnimationCurve sizeCurve = new AnimationCurve(new Keyframe(0.05f, 2), new Keyframe(1, 1), new Keyframe(20, 0.00195f));
    [Tooltip("Modifier for horizontal flap strength. Makes flapping forwards easier (Default: 1.5)")]
    public float horizontalStrengthMod = 1.5f;
    [Tooltip("How loose you want your turns while gliding. Lower values mean tighter control/sharper turns. (Default: 2)")]
    [Range(0f, 8f)]
    public float glideLooseness = 2;
    [Tooltip("If enabled, flight gravity will use Gravity Curve's curve instead of Size Curve's curve multiplied by Flight Gravity Base. (Default: false)")]
    public bool useGravityCurve;
    [Tooltip("Similar to Size Curve, but instead of modifying Flap Strength, it only affects Gravity. This value is ignored (Size Curve will be used instead) unless Use Gravity Curve is enabled.")]
    public AnimationCurve gravityCurve = new AnimationCurve(new Keyframe(0.05f, 0.4f), new Keyframe(1, 0.2f), new Keyframe(20, 0.00039f));
    [Tooltip("If a GameObject with a Text component is attached here, debug some basic info into it. (Default: unset)")]
    public Text debugOutput;
    [HideInInspector]
    public bool bankingTurns = false;

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
    private float tmpFloat;

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
    private bool handsDown = false; // Currently doesn't function as intended
    private bool spinningRightRound = false; // Can't get that Protogen animation out of my head
    private float rotSpeed = 0;
    private float rotSpeedGoal = 0;

    // "old" values are the world's defaults (recorded immediately before they are modified)
    private float oldGravityStrength;
    private float oldWalkSpeed;
    private float oldRunSpeed;
    private float oldStrafeSpeed;

    // Player-specific properties
    private HumanBodyBones rightUpperArmBone; // Bones aren't given a value until the player is valid
    private HumanBodyBones leftUpperArmBone;
    private HumanBodyBones rightLowerArmBone;
    private HumanBodyBones leftLowerArmBone;
    private HumanBodyBones rightHandBone;
    private HumanBodyBones leftHandBone;
    private float wingspan = 1f;
    
    public void Start() {
        LocalPlayer = Networking.LocalPlayer;
        //oldGravityStrength = LocalPlayer.GetGravityStrength();
        //oldWalkSpeed = LocalPlayer.GetWalkSpeed();
        //oldRunSpeed = LocalPlayer.GetRunSpeed();
        //oldStrafeSpeed = LocalPlayer.GetStrafeSpeed();
    }
    
    public void OnEnable() {
        timeTick = -5;
        isFlapping = false;
        isFlying = false;
        isGliding = false;
        spinningRightRound = false;
    }
    
    public void OnDisable() {
        if (isFlying) {
            land();
        }
    }
    
    public void EnableBetaFeatures() {
        bankingTurns = true;
    }
    
    public void DisableBetaFeatures() {
        bankingTurns = false;
    }
    
    public void Update() {
        if ((LocalPlayer != null) && LocalPlayer.IsValid()) {
            FlightTick(Time.deltaTime);
        }
    }
    
    public void FlightTick(float dt) {
        // Variable `dt` is Delta Time
        if (timeTick < 0) {
            // Only runs once shortly after joining the world
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
        if ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y) > 0) {
            downThrust = ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y)) * dt / wingspan;
        } else {
            downThrust = 0;
        }
        if (Vector2.Distance(new Vector2(LocalPlayer.GetBonePosition(rightUpperArmBone).x, LocalPlayer.GetBonePosition(rightUpperArmBone).z), new Vector2(LocalPlayer.GetBonePosition(rightHandBone).x, LocalPlayer.GetBonePosition(rightHandBone).z)) > wingspan / 3.3f && Vector2.Distance(new Vector2(LocalPlayer.GetBonePosition(leftUpperArmBone).x, LocalPlayer.GetBonePosition(leftUpperArmBone).z), new Vector2(LocalPlayer.GetBonePosition(leftHandBone).x, LocalPlayer.GetBonePosition(leftHandBone).z)) > wingspan / 3.3f) {
            handsOut = true;
        } else {handsOut = false;}
        // `handsDown` currently doesn't work as intended. Do not use
        if ((RHPos.y > LocalPlayer.GetBonePosition(rightUpperArmBone).y + (wingspan * 0.4f)) && (LHPos.y > LocalPlayer.GetBonePosition(leftUpperArmBone).y + (wingspan * 0.4f))) {
            handsDown = true;
        } else {handsDown = false;}
        
        // Legacy code: check for triggers being held
        //if ((Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger") > 0.5f) & (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") > 0.5f)) {
        
        if (!isFlapping) {
            // Check for the beginning of a flap
            if ((isFlying ? true : handsOut)
                && (requireJump ? !LocalPlayer.IsPlayerGrounded() : true)
                && RHPos.y < LocalPlayer.GetPosition().y - LocalPlayer.GetBonePosition(rightUpperArmBone).y
                && LHPos.y < LocalPlayer.GetPosition().y - LocalPlayer.GetBonePosition(leftUpperArmBone).y
                && downThrust > 0.0002) {

                isFlapping = true;
                if (!isFlying) { // First flap of the flight (likely from grounded)
                    isFlying = true;
                    CalculateStats();
                    oldGravityStrength = LocalPlayer.GetGravityStrength();
                    LocalPlayer.SetGravityStrength(flightGravity());
                    if (!allowLoco) {
                        ImmobilizePart(true);
                    }
                }
            }
        }
        
        // -- STATE: Flapping
        if (isFlapping) {
            if (downThrust > 0) {
                // Calculate force to apply based on the flap
                newVelocity = ((RHPos - RHPosLast) + (LHPos - LHPosLast)) * dt * flapStrength();
                if (LocalPlayer.IsPlayerGrounded()) {
                    // Removes sliding along the ground
                    newVelocity = new Vector3(0, newVelocity.y, 0);
                } else {
                    // apply horizontalStrengthMod
                    tmpFloat = newVelocity.y;
                    newVelocity = newVelocity * horizontalStrengthMod;
                    newVelocity.y = tmpFloat;
                }
                finalVelocity = LocalPlayer.GetVelocity() + newVelocity;
                // Speed cap (check, then apply flapping air friction)
                if (finalVelocity.magnitude > 0.02f * flapStrength()) {
                    finalVelocity = finalVelocity.normalized * (finalVelocity.magnitude - (flapAirFriction * flapStrength() * dt));
                }
                setFinalVelocity = true;
            } else { 
                isFlapping = false;
            }
        }

        // -- STATE: Flying
        // (Flying starts when a player first flaps and ends when they become grounded)
        if (isFlying) {
            if ((!isFlapping) && LocalPlayer.IsPlayerGrounded()) {
                // Script to run when landing
                land();
            } else {
                // ---=== Run every frame while the player is "flying" ===---

                if (LocalPlayer.GetGravityStrength() != (flightGravity()) && LocalPlayer.GetVelocity().y < 0) {
                    LocalPlayer.SetGravityStrength(flightGravity());
                }
                LHRot = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation;
                RHRot = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
                if ((!isFlapping) && handsOut) {
                    // Gliding, banking, and steering logic
                    isGliding = true;
                    newVelocity = setFinalVelocity ? finalVelocity : LocalPlayer.GetVelocity();
                    wingDirection = Vector3.Normalize(Quaternion.Slerp(LHRot, RHRot, 0.5f) * Vector3.forward); // The direction the player should go based on how they've angled their wings
                    // Hotfix: Always have some form of horizontal velocity while falling. In rare cases (more common with extremely small avatars) a player's velocity is perfectly straight up/down, which breaks gliding
                    if (newVelocity.y < 0.3f && newVelocity.x == 0 && newVelocity.z == 0) {
                        Vector2 tmpV2 = new Vector2(wingDirection.x, wingDirection.z).normalized * 0.145f;
                        newVelocity = new Vector3(Mathf.Round(tmpV2.x * 10) / 10, newVelocity.y, Mathf.Round(tmpV2.y * 10) / 10);
                    }
                    // Uncomment next line to flip the "wing direction" while moving backwards (Probably more realistic physics-wise but feels awkward in VR)
                    //if (Vector2.Angle(new Vector2(wingDirection.x, wingDirection.z), new Vector2(newVelocity.x, newVelocity.z)) > 90) {wingDirection = wingDirection * -1;}
                    steering = (RHPos.y - LHPos.y) * 80 / wingspan;
                    if (steering > 35) {steering = 35;} else if (steering < -35) {steering = -35;}
                    if (bankingTurns) {
                        spinningRightRound = true;
                        rotSpeedGoal = steering;
                    } else {
                        // Default "banking," which is just midair strafing
                        wingDirection = Quaternion.Euler(0, steering, 0) * wingDirection;
                    }
                    // X and Z are purely based on which way the wings are pointed ("forward") for ease of VR control
                    targetVelocity = Vector3.ClampMagnitude(newVelocity + (Vector3.Normalize(wingDirection) * newVelocity.magnitude), newVelocity.magnitude);
                    finalVelocity = Vector3.Slerp(newVelocity, targetVelocity, dt * glideLooseness);
                    setFinalVelocity = true;
                    // Legacy code: the amount of velocity added by gravity every frame = (new Vector3(0,LocalPlayer.GetGravityStrength(), 0) * dt * 10)
                } else {isGliding = false; rotSpeedGoal = 0;}
            }
        }
        RHPosLast = RHPos;
        LHPosLast = LHPos;
        if (setFinalVelocity) {LocalPlayer.SetVelocity(finalVelocity);}
        if (spinningRightRound) {
            rotSpeed = rotSpeed + ((rotSpeedGoal - rotSpeed) * dt * 6);
            // Legacy code (banking methods that didn't work):
            //playerRot = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            //playerRot = Quaternion.Euler(0, playerRot.eulerAngles.y, 0);
            //playerRot = Quaternion.Euler(LocalPlayer.GetVelocity().x, 0, LocalPlayer.GetVelocity().z);
            //LocalPlayer.TeleportTo(LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin).position, Quaternion.Lerp(playerRot, playerRot * Quaternion.Euler(Vector3.up * (rotSpeed / 100)), dt * 200), VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint, true);
            playerRot = LocalPlayer.GetRotation();
            // As far as I know, TeleportTo() is the only way (without colliders nor stations) to force a player to rotate
            LocalPlayer.TeleportTo(LocalPlayer.GetPosition(), Quaternion.Lerp(playerRot, playerRot * Quaternion.Euler(Vector3.up * (rotSpeed / 100)), dt * 200), VRC_SceneDescriptor.SpawnOrientation.AlignPlayerWithSpawnPoint, true);
        }
    }

    public void FixedUpdate() {
        if (timeTick >= 0) {
            timeTick = timeTick + 1;
            // Automatically CalculateStats() every second (assuming VRChat uses the Unity default of 50 ticks per second)
            if (timeTick > 49) {
                timeTick = 0;
                CalculateStats();
                if (debugOutput != null) {
                    debugOutput.text = string.Concat("Wingspan:\n", wingspan.ToString()) + string.Concat("\nFlapStr:\n", flapStrength().ToString()) + string.Concat("\nGrav: ", (Mathf.Round(LocalPlayer.GetGravityStrength() * 1000) * 0.001f).ToString());
                }
            }
        }
    }

    // Immobilize Locomotion but still allow body rotation
    private void ImmobilizePart(bool b) {
        if (b) {
            oldWalkSpeed = LocalPlayer.GetWalkSpeed();
            oldRunSpeed = LocalPlayer.GetRunSpeed();
            oldStrafeSpeed = LocalPlayer.GetStrafeSpeed();
            LocalPlayer.SetWalkSpeed(0f);
            LocalPlayer.SetRunSpeed(0f);
            LocalPlayer.SetStrafeSpeed(0f);
        } else {
            LocalPlayer.SetWalkSpeed(oldWalkSpeed);
            LocalPlayer.SetRunSpeed(oldRunSpeed);
            LocalPlayer.SetStrafeSpeed(oldStrafeSpeed);
        }
    }
    
    // Determine Flight Strength, etc. based on wingspan and whatnot.
    // This function can be re-run to recalculate these values at any time (upon switching avatars for example)
    private void CalculateStats() {
        // `wingspan` does not include the distance between shoulders
        wingspan = Vector3.Distance(LocalPlayer.GetBonePosition(leftUpperArmBone),LocalPlayer.GetBonePosition(leftLowerArmBone)) + Vector3.Distance(LocalPlayer.GetBonePosition(leftLowerArmBone),LocalPlayer.GetBonePosition(leftHandBone)) + Vector3.Distance(LocalPlayer.GetBonePosition(rightUpperArmBone),LocalPlayer.GetBonePosition(rightLowerArmBone)) + Vector3.Distance(LocalPlayer.GetBonePosition(rightLowerArmBone),LocalPlayer.GetBonePosition(rightHandBone));
    }
    
    // Effectually disables all flight-related mechanics
    private void land() {
        isFlying = false;
        isFlapping = false;
        isGliding = false;
        spinningRightRound = false;
        rotSpeed = 0;
        rotSpeedGoal = 0;
        LocalPlayer.SetGravityStrength(oldGravityStrength);
        if (!allowLoco) {
            ImmobilizePart(false);
        }
    }
    
    private float flapStrength() {
        return sizeCurve.Evaluate(wingspan) * flapStrengthBase;
    }
    
    private float flightGravity() {
        if (useGravityCurve) {
            return gravityCurve.Evaluate(wingspan) * wingspan;
        } else {
            return sizeCurve.Evaluate(wingspan) * flightGravityBase * wingspan;
        }
    }
}