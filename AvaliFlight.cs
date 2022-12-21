
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AvaliFlight : UdonSharpBehaviour {
    private VRCPlayerApi LocalPlayer;
    [Tooltip("Strength of each flap. Recommended values for default gravity: 300-700 (Default: 400)")]
    public float flightStrength = 400f;
    private Vector3 RHPos;
    private Vector3 LHPos;
    private Vector3 RHPosLast;
    private Vector3 LHPosLast;
    private bool isFlapping;
    private bool isFlying;
    [Tooltip("Change gravity while flying? (Default: true)")]
    public bool setGravity = true;
    [Tooltip("Value of gravity while flying (Default: 0.22)")]
    public float gravity = 0.22f;
    [Tooltip("Allow locomotion (wasd/left joystick) while flying? (Default: false)")]
    public bool allowLoco;
    private Vector3 velMod;
    // "old" values are the world's defaults
    private float oldGravityStrength;
    private float oldWalkSpeed;
    private float oldRunSpeed;
    private float oldStrafeSpeed;
    
    public void Start() {
        isFlapping = false;
        isFlying = false;
        LocalPlayer = Networking.LocalPlayer;
    }
    
    public void Update() {
        // Check if both triggers are being held
        // TODO: Check instead if hands are being moved downward while above a certain Y threshold
        if ((Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger") > 0.5f) & (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") > 0.5f)) {
            if (isFlapping) {
                // We're using LocalPlayer.GetPosition() to turn these coordinates into local ones
                RHPos = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                LHPos = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                // Calculate Force to apply
                velMod = ((RHPos - RHPosLast) + (LHPos - LHPosLast)) * (Time.deltaTime * flightStrength);
                RHPosLast = RHPos;
                LHPosLast = LHPos;

                LocalPlayer.SetVelocity(LocalPlayer.GetVelocity() + velMod);
            } else { // First frame of flapping (setting necessary variables)
                isFlapping = true;
                velMod = LocalPlayer.GetVelocity();
                if (!isFlying) { // First flap of the flight (ie grounded)
                    isFlying = true;
                    if (setGravity) {
                        oldGravityStrength = LocalPlayer.GetGravityStrength();
                        LocalPlayer.SetGravityStrength(gravity);
                    }
                    if (!allowLoco) {
                        ImmobilizePart(true);
                    }
                    // Workaround to get the player off the ground
                    velMod.y = LocalPlayer.GetGravityStrength() * Time.deltaTime * 500;
                }
                RHPosLast = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                LHPosLast = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
                // (pseudocode `LocalPlayer.PlayerGrounded(false)`)
                LocalPlayer.SetVelocity(velMod);
            }
        } else { // Stopped flapping
            isFlapping = false;
        }
        if (isFlying && LocalPlayer.IsPlayerGrounded()) { // Script to run when landing
            isFlying = false;
            if (setGravity) {
                LocalPlayer.SetGravityStrength(oldGravityStrength);
            }
            if (!allowLoco) {
                ImmobilizePart(false);
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
}