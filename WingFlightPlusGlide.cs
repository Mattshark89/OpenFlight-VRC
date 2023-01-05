
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class WingFlightPlusGlide : UdonSharpBehaviour {
    private VRCPlayerApi LocalPlayer;
    [Tooltip("Flap Strength varies by wingsize. 0.3-0.5 include most half-sized birds, 1 is about the wingspan of a VRChat avatar of average height.")]
    public AnimationCurve flapStrength = new AnimationCurve(new Keyframe(0.1f,1000, 0, -120), new Keyframe(0.5f,400, -90, -90, 0, 0.2f), new Keyframe(1, 260, -90, -90, 0.3f, 0.08f), new Keyframe(8, 100, 0, 0, 0.1f, 0));
    [Tooltip("Modifier for horizontal flap strength (Default: 1.5)")]
    public float horizontalStrengthMod = 1.5f;
    private Vector3 RHPos;
    private Vector3 LHPos;
    private Vector3 RHPosLast = new Vector3(0f, float.NegativeInfinity, 0f);
    private Vector3 LHPosLast = new Vector3(0f, float.NegativeInfinity, 0f);
    private Quaternion RHRot;
    private Quaternion LHRot;
    private bool isFlapping = false; // Doing the arm motion
    private bool isFlying = false; // Currently in the air after/during a flap
    private bool isGliding = false; // Has arms out while flying
    // GravityMod Advanced Usage:
    // The curve exists to make larger avatars feel heavier; they will fall to the ground faster than smaller avatars.
    // To remove this distinction, make the line a horizontal one
    [Tooltip("Gravity multiplier while flying.\nFor basic adjustments, drag the middle two dots up/down to the desired y value, using the SHIFT key to lock x (Default: 0.2)")]
    public AnimationCurve gravityMod = new AnimationCurve(new Keyframe(0.1f, 0.1f, 0, 0, 0, 0), new Keyframe(0.2f, 0.2f, 0, 0, 0, 0), new Keyframe(1, 0.2f, 0, 0, 0, 0), new Keyframe(8, 1, 0, 0, 0, 0));
    [Tooltip("Allow locomotion (wasd/left joystick) while flying? (Default: false)")]
    public bool allowLoco;
    // TODO: clean up these four vars
    private Vector3 targetVelocity;
    private Vector3 newVelocity;
    private bool setFinalVelocity;
    private Vector3 finalVelocity;
    
    private Vector3 wingPlaneNormal;
    private Vector3 wingDirection;
    private Vector3 counterForce;
    // "old" values are the world's defaults
    private float oldGravityStrength;
    private float oldWalkSpeed;
    private float oldRunSpeed;
    private float oldStrafeSpeed;
    // Bones used for calculating arm/wingspan (defined later)
    private HumanBodyBones rightUpperArmBone;
    private HumanBodyBones leftUpperArmBone;
    private HumanBodyBones rightLowerArmBone;
    private HumanBodyBones leftLowerArmBone;
    private HumanBodyBones rightHandBone;
    private HumanBodyBones leftHandBone;
    private float wingspan = 99999f; // You'll never be able to fly with a wingspan this wide... Unless your avatar is just stupid oversized
    private float downThrust = 0f;
    
    public void Start() {
        isFlapping = false;
        isFlying = false;
        LocalPlayer = Networking.LocalPlayer;
    }
    
    public void Update() {
        if (LocalPlayer.IsValid()) {
            CalculateStats();
            setFinalVelocity = false;
            // Check if both triggers are being held
            // TODO: Check instead if hands are being moved downward while above a certain Y threshold
            // We're using LocalPlayer.GetPosition() to turn these coordinates into relative ones
            RHPos = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
            LHPos = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
            if ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y) > 0) {
                downThrust = ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y)) * Time.deltaTime / wingspan;
            } else {
                downThrust = 0;
            }
            //if ((Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger") > 0.5f) & (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") > 0.5f)) {
            if (isFlapping) {
                if (downThrust > 0) {
                    // Calculate Force to apply
                    targetVelocity = ((RHPos - RHPosLast) + (LHPos - LHPosLast)) * Time.deltaTime * flapStrength.Evaluate(wingspan);
                    float ley = targetVelocity.y;
                    targetVelocity = targetVelocity * horizontalStrengthMod;
                    targetVelocity.y = ley;
                    newVelocity = targetVelocity + LocalPlayer.GetVelocity();
                    if (LocalPlayer.IsPlayerGrounded()) {newVelocity = new Vector3(0, newVelocity.y, 0);} // Removes sliding along the ground
                    setFinalVelocity = true;
                    finalVelocity = Vector3.ClampMagnitude(newVelocity, Time.deltaTime * wingspan * flapStrength.Evaluate(wingspan));
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
                    // First frame of flapping (setting necessary variables)
                    isFlapping = true;
                    newVelocity = LocalPlayer.GetVelocity();
                    if (!isFlying) { // First flap of the flight (ie grounded)
                        isFlying = true;
                        CalculateStats();
                        oldGravityStrength = LocalPlayer.GetGravityStrength();
                        LocalPlayer.SetGravityStrength(oldGravityStrength * gravityMod.Evaluate(wingspan));
                        // Workaround to get the player off the ground
                        // newVelocity.y = LocalPlayer.GetGravityStrength() * Time.deltaTime * 500;
                        if (!allowLoco) {
                            ImmobilizePart(true);
                        }
                    }
                    RHPosLast = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                    LHPosLast = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                    // (pseudocode `LocalPlayer.SetPlayerGrounded(false)`)
                    if (LocalPlayer.IsPlayerGrounded()) {newVelocity = new Vector3(0, newVelocity.y, 0);} // Removes sliding along the ground
                    setFinalVelocity = true;
                    finalVelocity = Vector3.ClampMagnitude(newVelocity, Time.deltaTime * wingspan * flapStrength.Evaluate(wingspan));
                }
            }
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
                        wingPlaneNormal = Vector3.Normalize(Quaternion.Slerp(LHRot, RHRot, 0.5f) * Vector3.right);
                        wingDirection = Vector3.Normalize(Quaternion.Slerp(LHRot, RHRot, 0.5f) * Vector3.forward);
                        // Flip the "wing direction" if flapping backwards
                        if (Vector2.Angle(new Vector2(wingDirection.x, wingDirection.z), new Vector2(newVelocity.x, newVelocity.z)) > 90) {wingDirection = wingDirection * -1;}
                        counterForce = Vector3.Reflect(newVelocity, wingPlaneNormal); // The force pushing off of the wings
                        
                        // X and Z are purely based on which way the wings are pointed ("forward") for ease of VR control
                        targetVelocity = Vector3.ClampMagnitude(newVelocity + (Vector3.Normalize(new Vector3(wingDirection.x, counterForce.y, wingDirection.z)) * counterForce.magnitude), newVelocity.magnitude);
                        finalVelocity = Vector3.Slerp(newVelocity, targetVelocity, Time.deltaTime * 2);
                        setFinalVelocity = true;
                        // Note to self: the amount of velocity added by gravity every frame = (new Vector3(0,LocalPlayer.GetGravityStrength(), 0) * Time.deltaTime * 10)
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