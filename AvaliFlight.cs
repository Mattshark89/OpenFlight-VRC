
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AvaliFlight : UdonSharpBehaviour {
    private VRCPlayerApi LocalPlayer;
    [Tooltip("Make this stronk")]
    public float flightStrength = 300;
    private Vector3 RHPos;
    private Vector3 LHPos;
    private Vector3 RHPosLast;
    private Vector3 LHPosLast;
    private bool isFlapping;
    private bool isFlying;
    public bool setGravity;
    private float oldGravity;
    public float gravity;
    private Vector3 speedMod;
    
    public void Start() {
        isFlapping = false;
        LocalPlayer = Networking.LocalPlayer;
        // Avoid errors with RHPosLast not existing
        RHPosLast = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
        LHPosLast = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
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
                speedMod = ((RHPos - RHPosLast) + (LHPos - LHPosLast)) * (Time.deltaTime * flightStrength);
                RHPosLast = RHPos;
                LHPosLast = LHPos;

                LocalPlayer.SetVelocity(LocalPlayer.GetVelocity() + speedMod);
            } else { // First frame of flapping (setting necessary variables)
                isFlapping = true;
                if (!isFlying) {
                    isFlying = true;
                    if (setGravity) {
                        oldGravity = LocalPlayer.GetGravityStrength();
                        LocalPlayer.SetGravityStrength(gravity);
                    }
                }
                RHPosLast = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
                LHPosLast = LocalPlayer.GetPosition() - LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position;
            }
        } else { // Stopped flapping
            isFlapping = false;
        }
        if (isFlying && LocalPlayer.IsPlayerGrounded()) {
            isFlying = false;
            if (setGravity) {
                LocalPlayer.SetGravityStrength(oldGravity);
            }
        }
    }
}