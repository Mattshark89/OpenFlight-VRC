
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.Gizmos
{
    public class GizmoController : UdonSharpBehaviour
    {
        [Header("Script References")]
        public AvatarDetection avatarDetection;

        [Header("Gizmo Objects")]
        public GameObject wingtipGizmo;
        public LineRenderer wingtipLine;
        void Start()
        {

        }

        public override void PostLateUpdate()
        {
            //scale self to match to the player 
            transform.localScale = new Vector3(Util.ScaleModifier(), Util.ScaleModifier(), Util.ScaleModifier());

            //move the gameobject this is on to the player's position and rotation
            transform.position = Networking.LocalPlayer.GetPosition();
            transform.rotation = Networking.LocalPlayer.GetRotation();

            //Wingtip gizmo
            Util.SetWingtipTransform(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand), wingtipGizmo, avatarDetection.WingtipOffset, avatarDetection.d_spinetochest);
            //set the line renderer start to the hand and end to the wingtip
            wingtipLine.SetPositions(new Vector3[] { Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position, wingtipGizmo.transform.position });
        }
    }
}
