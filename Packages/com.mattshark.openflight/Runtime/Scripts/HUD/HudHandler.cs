
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
        private VRCPlayerApi localplayer;
        private UnityEngine.Quaternion DefaultRotation;
        private bool lastflightstate;
        void Start()
        {
            localplayer = Networking.LocalPlayer;
            DefaultRotation = HudNotificationObject.transform.rotation;
            lastflightstate = false;
        }

        public override void OnAvatarEyeHeightChanged(VRCPlayerApi player, float eyeHeight)
        {
            if (player.isLocal) {
                float scale = Mathf.Lerp(0.1f, 5f, player.GetAvatarEyeHeightAsMeters()/5.0f);
                HudNotificationObject.transform.localScale = new UnityEngine.Vector3(scale, scale, scale);
                HudNotificationObject.transform.localPosition = new UnityEngine.Vector3(HudNotificationObject.transform.localPosition.x, HudNotificationObject.transform.localPosition.y, 1.2f * eyeHeight);
            }
        }



        public void NotifyFlightCapable()
        {
            if (!lastflightstate) {
                HudNotificationObject.SetActive(false);
                HudNotificationObject.transform.rotation = DefaultRotation;
                HudNotificationObject.SetActive(true);
                lastflightstate = true;
            }
        }
        
        public void NotifyNotFlightCapable()
        {
            if (lastflightstate) {
                HudNotificationObject.SetActive(false);
                HudNotificationObject.transform.rotation = new UnityEngine.Quaternion(0.707106829f,0f,0f,0.707106829f);
                HudNotificationObject.transform.position = localplayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                HudNotificationObject.SetActive(true);
                lastflightstate = false;
            }
        }
    }
}
