
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class AvaliFlight : UdonSharpBehaviour {
    private VRCPlayerApi LocalPlayer;
    [Tooltip("Recommended values for default gravity: 300-450 (Default: 400)")]
    public int flapStrength = 400;
    [Tooltip("It is recommended you set this higher than Flap Strength (Default: 700)")]
    public int velocityCap = 700;
    private Vector3 RHPos;
    private Vector3 LHPos;
    private Vector3 RHPosLast = new Vector3(0f, float.NegativeInfinity, 0f);
    private Vector3 LHPosLast = new Vector3(0f, float.NegativeInfinity, 0f);
    private bool isFlapping = false;
    private bool isFlying = false;
    [Tooltip("Gravity multiplier while flying upwards. Set to 1 to disable (Default: 0.3)")]
    public float upwardsGravityMod = 0.3f;
    [Tooltip("Gravity multiplier while floating downwards. Set to 1 to disable (Default: 0.22)")]
    public float downwardsGravityMod = 0.22f;
    [Tooltip("Allow locomotion (wasd/left joystick) while flying? (Default: false)")]
    public bool allowLoco;
    private Vector3 velMod;
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
        CalculateStats();
    }
    
    public void Update() {
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
                velMod = ((RHPos - RHPosLast) + (LHPos - LHPosLast)) * Time.deltaTime * flapStrength;
                LocalPlayer.SetVelocity(Vector3.ClampMagnitude(LocalPlayer.GetVelocity() + velMod, Time.deltaTime * wingspan * velocityCap));
            } else { 
                isFlapping = false;
            }
        } else {
            // check for the beginning of a flap
            if (downThrust > 0.0002 && RHPos.y < LocalPlayer.GetPosition().y - LocalPlayer.GetBonePosition(rightUpperArmBone).y && LHPos.y < LocalPlayer.GetPosition().y - LocalPlayer.GetBonePosition(leftUpperArmBone).y) {
                // First frame of flapping (setting necessary variables)
                isFlapping = true;
                velMod = LocalPlayer.GetVelocity();
                if (!isFlying) { // First flap of the flight (ie grounded)
                    isFlying = true;
                    CalculateStats();
                    oldGravityStrength = LocalPlayer.GetGravityStrength();
                    LocalPlayer.SetGravityStrength(oldGravityStrength * upwardsGravityMod);
                    // Workaround to get the player off the ground
                    // velMod.y = LocalPlayer.GetGravityStrength() * Time.deltaTime * 500;
                    if (!allowLoco) {
                        ImmobilizePart(true);
                    }
                }
                RHPosLast = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                LHPosLast = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                // (pseudocode `LocalPlayer.PlayerGrounded(false)`)
                LocalPlayer.SetVelocity(Vector3.ClampMagnitude(velMod, Time.deltaTime * flapStrength * velocityCap ));
            }
        }
        if (isFlying) {
            if (LocalPlayer.IsPlayerGrounded()) {
                // Script to run when landing
                isFlying = false;
                LocalPlayer.SetGravityStrength(oldGravityStrength);
                if (!allowLoco) {
                    ImmobilizePart(false);
                }
            } else {
                if (LocalPlayer.GetGravityStrength() != (oldGravityStrength * downwardsGravityMod) && LocalPlayer.GetVelocity().y < 0) {
                    LocalPlayer.SetGravityStrength(oldGravityStrength * downwardsGravityMod);
                }
            }
        }
        RHPosLast = RHPos;
        LHPosLast = LHPos;
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
        Debug.Log(wingspan);
    }
}