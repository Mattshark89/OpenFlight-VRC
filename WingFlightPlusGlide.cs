
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class WingFlightPlusGlide : UdonSharpBehaviour {
    [Tooltip("Flap Strength varies by wingsize. 0.3-0.5 include most half-sized birds, 1 is about the wingspan of a VRChat avatar of average height.")]
    public AnimationCurve flapStrength = new AnimationCurve(new Keyframe(0.05f, 800), new Keyframe(0.1f, 460), new Keyframe(0.5f, 245), new Keyframe(1, 180, -90, -90, 0.3f, 0.08f), new Keyframe(8, 80, 0, 0, 0.1f, 0));
    [Tooltip("Require the player to jump before flapping can occur. Makes it less likely to trigger a flap by accident when enabled. (Default: False)")]
    public bool requireJump;
    [Tooltip("Modifier for horizontal flap strength. Makes flapping forwards easier (Default: 1.5)")]
    public float horizontalStrengthMod = 1.5f;
    // GravityMod Advanced Usage:
    // The curve exists to make larger avatars feel heavier; they will fall to the ground faster than smaller avatars.
    // To remove this distinction, make the line a horizontal one (though you will have to adjust flapStrength to account for it)
    [Tooltip("Gravity multiplier while flying.\nFor basic adjustments, drag the middle two dots up/down to the desired y value, using the SHIFT key to lock x (Default: 0.2)")]
    public AnimationCurve gravityMod = new AnimationCurve(new Keyframe(0.1f, 0.09f, 0, 0, 0, 0), new Keyframe(0.2f, 0.2f, 0, 0, 0, 0), new Keyframe(1, 0.2f, 0, 0, 0, 0), new Keyframe(8, 1, 0, 0, 0, 0));
    [Tooltip("How loose you want your turns while gliding. Lower values mean tighter control/sharper turns. (Default: 2)")]
    public float glideLooseness = 2;
    [Tooltip("Allow locomotion (wasd/left joystick) while flying? (Default: false)")]
    public bool allowLoco;

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
    private bool handsOut = false; // Are the controllers held outside of an imaginary cylinder?
    private bool isFlapping = false; // Doing the arm motion
    private bool isFlying = false; // Currently in the air after/during a flap
    private bool isGliding = false; // Has arms out while flying

    // Variables related to Velocity
    private Vector3 finalVelocity; // Modify this value instead of the player's velocity directly, then run `setFinalVelocity = true`
    private bool setFinalVelocity;
    private Vector3 newVelocity; // tmp var
    private Vector3 targetVelocity; // tmp var, usually associated with slerping/lerping
    private float downThrust = 0f;
    private float flapAirFriction = 0.04f; // Prevents the gain of infinite speed while flapping. Set to 0 to remove this feature. THIS IS NOT A MAX SPEED

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
                downThrust = ((RHPos.y - RHPosLast.y) + (LHPos.y - LHPosLast.y)) * Time.deltaTime / wingspan;
            } else {
                downThrust = 0;
            }
            if (Vector2.Distance(new Vector2(LocalPlayer.GetBonePosition(rightUpperArmBone).x, LocalPlayer.GetBonePosition(rightUpperArmBone).z), new Vector2(LocalPlayer.GetBonePosition(rightHandBone).x, LocalPlayer.GetBonePosition(rightHandBone).z)) > wingspan / 3.2f && Vector2.Distance(new Vector2(LocalPlayer.GetBonePosition(leftUpperArmBone).x, LocalPlayer.GetBonePosition(leftUpperArmBone).z), new Vector2(LocalPlayer.GetBonePosition(leftHandBone).x, LocalPlayer.GetBonePosition(leftHandBone).z)) > wingspan / 3.2f) {
                handsOut = true;
            } else {handsOut = false;}
            
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
                        LocalPlayer.SetGravityStrength(oldGravityStrength * gravityMod.Evaluate(wingspan));
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
                    newVelocity = ((RHPos - RHPosLast) + (LHPos - LHPosLast)) * Time.deltaTime * flapStrength.Evaluate(wingspan);
                    float ley = newVelocity.y;
                    newVelocity = newVelocity * horizontalStrengthMod;
                    newVelocity.y = ley;
                    finalVelocity = LocalPlayer.GetVelocity() + newVelocity;
                    if (LocalPlayer.IsPlayerGrounded()) {finalVelocity = new Vector3(0, finalVelocity.y, 0);} // Removes sliding along the ground
                    // Speed cap (check, then apply flapping air friction)
                    if (finalVelocity.magnitude > 0.02f * flapStrength.Evaluate(wingspan)) {
                        finalVelocity = finalVelocity.normalized * (finalVelocity.magnitude - (flapAirFriction * flapStrength.Evaluate(wingspan) * Time.deltaTime));
                    }
                    setFinalVelocity = true;
                } else { 
                    isFlapping = false;
                }
            }

            // -- STATE: Flying
            if (isFlying) {
                if ((!isFlapping) && LocalPlayer.IsPlayerGrounded()) {
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
                    if ((!isFlapping) && handsOut) {
                        // Gliding logic
                        isGliding = true;
                        newVelocity = setFinalVelocity ? finalVelocity : LocalPlayer.GetVelocity();
                        wingPlaneNormal = Vector3.Normalize(Quaternion.Slerp(LHRot, RHRot, 0.5f) * Vector3.right); // A plane normal is a vector perpendicular to a plane's surface. For example, if the player's wing is horizontal and flat like a table, its plane normal will point straight up.
                        wingDirection = Vector3.Normalize(Quaternion.Slerp(LHRot, RHRot, 0.5f) * Vector3.forward); // The direction the player should go based on how they've angled their wings
                        // Hotfix: Always have some form of horizontal velocity while falling. In rare cases (more common with extremely small avatars) a player's velocity is perfectly straight up/down, which breaks gliding
                        if (newVelocity.y < 0.3f && newVelocity.x == 0 && newVelocity.z == 0) {
                            Vector2 tmpV2 = new Vector2(wingDirection.x, wingDirection.z).normalized * 0.145f;
                            newVelocity = new Vector3(Mathf.Round(tmpV2.x * 10) / 10, newVelocity.y, Mathf.Round(tmpV2.y * 10) / 10);
                        }
                        // Uncomment next line to flip the "wing direction" while moving backwards (Probably more realistic physics-wise but feels awkward in VR)
                        //if (Vector2.Angle(new Vector2(wingDirection.x, wingDirection.z), new Vector2(newVelocity.x, newVelocity.z)) > 90) {wingDirection = wingDirection * -1;}
                        steering = (RHPos.y - LHPos.y) * 80 / wingspan;
                        if (steering > 30) {steering = 30;} else if (steering < -30) {steering = -30;}
                        wingDirection = Quaternion.Euler(0, steering, 0) * wingDirection;
                        Vector3 counterForce = Vector3.Reflect(newVelocity, wingPlaneNormal); // The force pushing off of the wings
                        
                        // X and Z are purely based on which way the wings are pointed ("forward") for ease of VR control
                        targetVelocity = Vector3.ClampMagnitude(newVelocity + (Vector3.Normalize(new Vector3(wingDirection.x, counterForce.normalized.y, wingDirection.z)) * counterForce.magnitude), newVelocity.magnitude);
                        finalVelocity = Vector3.Slerp(newVelocity, targetVelocity, Time.deltaTime * glideLooseness);
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

    public void FixedUpdate() {
        if (timeTick >= 0) {
            timeTick = timeTick + 1;
            // Automatically CalculateStats() every second (assuming VRChat uses the Unity default of 50 ticks per second)
            if (timeTick > 49) {
                timeTick = 0;
                CalculateStats();
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
}