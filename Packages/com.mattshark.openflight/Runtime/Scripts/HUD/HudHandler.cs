
using System;
using System.Numerics;

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.Hud
{
    public class HudHandler : UdonSharpBehaviour
    {
        public GameObject HudNotificationObject;
        private Transform HudNotificationTransform;
        private VRCPlayerApi localplayer;
        private UnityEngine.Quaternion DefaultRotation;
        private bool lastflightstate;
        void Start()
        {
            localplayer = Networking.LocalPlayer;
            HudNotificationTransform = HudNotificationObject.transform;
            DefaultRotation = HudNotificationTransform.rotation;
            lastflightstate = false;
        }

        public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float eyeHeight)
        {
            if (player.isLocal) {
                float scale = Mathf.Lerp(0.1f, 5f, player.GetAvatarEyeHeightAsMeters()/5.0f);
                HudNotificationTransform.localScale = new UnityEngine.Vector3(scale, scale, scale);
                HudNotificationTransform.localPosition = new UnityEngine.Vector3(HudNotificationTransform.localPosition.x, HudNotificationTransform.localPosition.y, 1.2f * eyeHeight);
            }
        }



        public void NotifyFlightCapable()
        {
            if (!lastflightstate) {
                HudNotificationObject.SetActive(false);
                HudNotificationTransform.rotation = DefaultRotation;
                HudNotificationTransform.position = localplayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                HudNotificationObject.SetActive(true);
                lastflightstate = true;
            }
        }
        
        
        public void NotifyNotFlightCapable()
        {
            if (lastflightstate) {
                HudNotificationObject.SetActive(false);
                HudNotificationTransform.rotation = new UnityEngine.Quaternion(0.707106829f,0f,0f,0.707106829f);
                HudNotificationTransform.position = localplayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                HudNotificationObject.SetActive(true);
                lastflightstate = false;
            }
        }
    }
}
