
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class WingFlightPlusGlide : UdonSharpBehaviour {
    [Tooltip("Flap Strength varies by wingsize. 0.3-0.5 include most half-sized birds, 1 is about the wingspan of a VRChat avatar of average height.")]
    public AnimationCurve flapStrength = new AnimationCurve(new Keyframe(0.1f,1000, 0, -120), new Keyframe(0.5f,400, -90, -90, 0, 0.2f), new Keyframe(1, 260, -90, -90, 0.3f, 0.08f), new Keyframe(8, 100, 0, 0, 0.1f, 0));
    [Tooltip("Modifier for horizontal flap strength. Makes flapping forwards easier (Default: 1.5)")]
    public float horizontalStrengthMod = 1.5f;
    // GravityMod Advanced Usage:
    // The curve exists to make larger avatars feel heavier; they will fall to the ground faster than smaller avatars.
    // To remove this distinction, make the line a horizontal one
    [Tooltip("Gravity multiplier while flying.\nFor basic adjustments, drag the middle two dots up/down to the desired y value, using the SHIFT key to lock x (Default: 0.2)")]
    public AnimationCurve gravityMod = new AnimationCurve(new Keyframe(0.1f, 0.1f, 0, 0, 0, 0), new Keyframe(0.2f, 0.2f, 0, 0, 0, 0), new Keyframe(1, 0.2f, 0, 0, 0, 0), new Keyframe(8, 1, 0, 0, 0, 0));
    [Tooltip("Allow locomotion (wasd/left joystick) while flying? (Default: false)")]
    public bool allowLoco;

    // Essential Variables
    private VRCPlayerApi LocalPlayer;
    private Vector3 RHPos;
    private Vector3 LHPos;
    private Vector3 RHPosLast = new Vector3(0f, float.NegativeInfinity, 0f);
    private Vector3 LHPosLast = new Vector3(0f, float.NegativeInfinity, 0f);
    private Quaternion RHRot;
    private Quaternion LHRot;
    private bool isFlapping = false; // Doing the arm motion
    private bool isFlying = false; // Currently in the air after/during a flap
    private bool isGliding = false; // Has arms out while flying

    // Variables related to Velocity
    private Vector3 finalVelocity; // Modify this value instead of the player's velocity directly, then run `setFinalVelocity = true`
    private bool setFinalVelocity;
    private Vector3 newVelocity; // tmp var
    private Vector3 targetVelocity; // tmp var, usually associated with slerping/lerping
    private float downThrust = 0f;

    // Variables related to gliding
    private Vector3 wingPlaneNormal;
    private Vector3 wingDirection;
    private float steering;

    // "old" values are the world's defaults
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
    }
    
    public void Update() {
        if (LocalPlayer.IsValid()) {
            CalculateStats();
            setFinalVelocity = false;
            // Check if hands are being moved downward while above a certain Y threshold
            // We're using LocalPlayer.GetPosition() to turn these global coordinates into local ones
            RHPos = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
            LHPos = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
            if ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y) > 0) {
                downThrust = ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y)) * Time.deltaTime / wingspan;
            } else {
                downThrust = 0;
            }
            // Legacy code: check for triggers being held
            //if ((Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger") > 0.5f) & (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") > 0.5f)) {
            
            // -- STATE: Flapping
            if (isFlapping) {
                if (downThrust > 0) {
                    // Calculate force to apply based on the flap
                    targetVelocity = ((RHPos - RHPosLast) + (LHPos - LHPosLast)) * Time.deltaTime * flapStrength.Evaluate(wingspan);
                    float ley = targetVelocity.y;
                    targetVelocity = targetVelocity * horizontalStrengthMod;
                    targetVelocity.y = ley;
                    newVelocity = targetVelocity + LocalPlayer.GetVelocity();
                    if (LocalPlayer.IsPlayerGrounded()) {newVelocity = new Vector3(0, newVelocity.y, 0);} // Removes sliding along the ground
                    finalVelocity = Vector3.ClampMagnitude(newVelocity, Time.deltaTime * wingspan * flapStrength.Evaluate(wingspan));
                    setFinalVelocity = true;
                } else { 
                    isFlapping = false;
                }
            } else {
                // Check for the beginning of a flap
                
                // So this one's a bit complicated.
                // First it checks the right hand, then the left, for two conditions.
                // Condition one: Is the hand higher than shoulder height?
                // Condition two (the super long lines of code): if the player isn't flying yet, check to see if their hand is held away from their body. (This check makes it much less likely to accidentially initiate flight)
                if (RHPos.y < LocalPlayer.GetPosition().y - LocalPlayer.GetBonePosition(rightUpperArmBone).y
                    && (isFlying ? true : Vector2.Distance(new Vector2(LocalPlayer.GetBonePosition(rightUpperArmBone).x, LocalPlayer.GetBonePosition(rightUpperArmBone).z), new Vector2(LocalPlayer.GetBonePosition(rightHandBone).x, LocalPlayer.GetBonePosition(rightHandBone).z)) > wingspan / 3.2f)
                    && LHPos.y < LocalPlayer.GetPosition().y - LocalPlayer.GetBonePosition(leftUpperArmBone).y
                    && (isFlying ? true : Vector2.Distance(new Vector2(LocalPlayer.GetBonePosition(leftUpperArmBone).x, LocalPlayer.GetBonePosition(leftUpperArmBone).z), new Vector2(LocalPlayer.GetBonePosition(leftHandBone).x, LocalPlayer.GetBonePosition(leftHandBone).z)) > wingspan / 3.2f)
                    && downThrust > 0.0002) {

                    isFlapping = true;
                    newVelocity = LocalPlayer.GetVelocity();
                    if (!isFlying) { // First flap of the flight (ie grounded)
                        isFlying = true;
                        CalculateStats();
                        oldGravityStrength = LocalPlayer.GetGravityStrength();
                        LocalPlayer.SetGravityStrength(oldGravityStrength * gravityMod.Evaluate(wingspan));
                        if (!allowLoco) {
                            ImmobilizePart(true);
                        }
                    }
                    RHPosLast = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                    LHPosLast = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                    // (pseudocode `LocalPlayer.SetPlayerGrounded(false)`)
                    if (LocalPlayer.IsPlayerGrounded()) {newVelocity = new Vector3(0, newVelocity.y, 0);} // Removes sliding along the ground
                    finalVelocity = Vector3.ClampMagnitude(newVelocity, Time.deltaTime * wingspan * flapStrength.Evaluate(wingspan));
                    setFinalVelocity = true;
                }
            }

            // -- STATE: Flying
            // `isFlying` is set within STATE: Flapping's `else` block
            if (isFlying) {
                if (LocalPlayer.IsPlayerGrounded()) {
                    // Script to run when landing
                    isFlying = false;
                    isGliding = false;
                    LocalPlayer.SetGravityStrength(oldGravityStrength);
                    if (!allowLoco) {
                        ImmobilizePart(false);
                    }
                } else {
                    // ---=== Run every frame while the player is "flying" ===---

                    if (LocalPlayer.GetGravityStrength() != (oldGravityStrength * gravityMod.Evaluate(wingspan)) && LocalPlayer.GetVelocity().y < 0) {
                        LocalPlayer.SetGravityStrength(oldGravityStrength * gravityMod.Evaluate(wingspan));
                    }
                    LHRot = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation;
                    RHRot = LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;
                    if (Vector2.Distance(new Vector2(LocalPlayer.GetBonePosition(rightUpperArmBone).x, LocalPlayer.GetBonePosition(rightUpperArmBone).z), new Vector2(LocalPlayer.GetBonePosition(rightHandBone).x, LocalPlayer.GetBonePosition(rightHandBone).z)) > wingspan / 3.2f
                        && Vector2.Distance(new Vector2(LocalPlayer.GetBonePosition(leftUpperArmBone).x, LocalPlayer.GetBonePosition(leftUpperArmBone).z), new Vector2(LocalPlayer.GetBonePosition(leftHandBone).x, LocalPlayer.GetBonePosition(leftHandBone).z)) > wingspan / 3.2f) {
                        // Gliding logic
                        isGliding = true;
                        newVelocity = setFinalVelocity ? finalVelocity : LocalPlayer.GetVelocity();
                        wingPlaneNormal = Vector3.Normalize(Quaternion.Slerp(LHRot, RHRot, 0.5f) * Vector3.right); // A plane normal is a vector perpendicular to a plane's surface. For example, if the player's wing is horizontal and flat like a table, its plane normal will point straight up.
                        wingDirection = Vector3.Normalize(Quaternion.Slerp(LHRot, RHRot, 0.5f) * Vector3.forward); // The direction the player should go based on how they've angled their wings
                        // Uncomment next line to flip the "wing direction" while moving backwards (Probably more realistic physics-wise but feels awkward in VR)
                        //if (Vector2.Angle(new Vector2(wingDirection.x, wingDirection.z), new Vector2(newVelocity.x, newVelocity.z)) > 90) {wingDirection = wingDirection * -1;}
                        steering = (RHPos.y - LHPos.y) * 80 / wingspan;
                        if (steering > 30) {steering = 30;} else if (steering < -30) {steering = -30;}
                        wingDirection = Quaternion.Euler(0, steering, 0) * wingDirection;
                        Vector3 counterForce = Vector3.Reflect(newVelocity, wingPlaneNormal); // The force pushing off of the wings
                        
                        // X and Z are purely based on which way the wings are pointed ("forward") for ease of VR control
                        targetVelocity = Vector3.ClampMagnitude(newVelocity + (Vector3.Normalize(new Vector3(wingDirection.x, counterForce.y, wingDirection.z)) * counterForce.magnitude), newVelocity.magnitude);
                        finalVelocity = Vector3.Slerp(newVelocity, targetVelocity, Time.deltaTime * 2);
                        setFinalVelocity = true;
                        // Legacy code: the amount of velocity added by gravity every frame = (new Vector3(0,LocalPlayer.GetGravityStrength(), 0) * Time.deltaTime * 10)
                    } else {isGliding = false;}
                }
            }
            RHPosLast = RHPos;
            LHPosLast = LHPos;
            if (setFinalVelocity) {LocalPlayer.SetVelocity(finalVelocity);}
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
        leftLowerArmBone = HumanBodyBones.LeftLowerArm;
        rightLowerArmBone = HumanBodyBones.RightLowerArm;
        leftUpperArmBone = HumanBodyBones.LeftUpperArm;
        rightUpperArmBone = HumanBodyBones.RightUpperArm;
        leftHandBone = HumanBodyBones.LeftHand;
        rightHandBone = HumanBodyBones.RightHand;
        // `wingspan` does not include the distance between shoulders
        wingspan = Vector3.Distance(LocalPlayer.GetBonePosition(leftUpperArmBone),LocalPlayer.GetBonePosition(leftLowerArmBone)) + Vector3.Distance(LocalPlayer.GetBonePosition(leftLowerArmBone),LocalPlayer.GetBonePosition(leftHandBone)) + Vector3.Distance(LocalPlayer.GetBonePosition(rightUpperArmBone),LocalPlayer.GetBonePosition(rightLowerArmBone)) + Vector3.Distance(LocalPlayer.GetBonePosition(rightLowerArmBone),LocalPlayer.GetBonePosition(rightHandBone));
    }
}