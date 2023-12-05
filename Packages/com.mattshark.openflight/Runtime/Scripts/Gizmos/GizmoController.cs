
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using static OpenFlightVRC.Util;

namespace OpenFlightVRC.Gizmos
{
    public class GizmoController : LoggableUdonSharpBehaviour
    {
        [Header("Script References")]
        public AvatarDetection avatarDetection;
        public WingFlightPlusGlide wingFlightPlusGlide;

        [Header("Gizmo Objects")]
        public GameObject wingtipGizmo;
        public LineRenderer wingtipLine;

        public TextMeshProUGUI NeckDistanceText;
        public TextMeshProUGUI ChestDistanceText;
        public TextMeshProUGUI LeftShoulderDistanceText;
        public TextMeshProUGUI LeftElbowDistanceText;
        public TextMeshProUGUI LeftWristDistanceText;
        [Header("Velocity Arrows")]
        public GameObject wingDirectionGizmo;
        public GameObject playerVelocityGizmo;
        void Start()
        {

        }

        public override void PostLateUpdate()
        {
            //scale self to match to the player 
            transform.localScale = new Vector3(ScaleModifier(), ScaleModifier(), ScaleModifier());

            //move the gameobject this is on to the player's position and rotation
            transform.position = Networking.LocalPlayer.GetPosition();
            transform.rotation = Networking.LocalPlayer.GetRotation();

            //Wingtip gizmo
            SetWingtipTransform(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand), wingtipGizmo, avatarDetection.WingtipOffset, avatarDetection.d_spinetochest);
            //set the line renderer start to the hand and end to the wingtip
            wingtipLine.SetPositions(new Vector3[] { Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position, wingtipGizmo.transform.position });

            #region Bone Debug Info
            NeckDistanceText.text = avatarDetection.hashDistances[0].ToString();
            ChestDistanceText.text = avatarDetection.hashDistances[1].ToString();
            LeftShoulderDistanceText.text = avatarDetection.hashDistances[2].ToString();
            LeftElbowDistanceText.text = avatarDetection.hashDistances[3].ToString();
            LeftWristDistanceText.text = avatarDetection.hashDistances[4].ToString();
            #endregion

            //Wing direction gizmo
            ScaleArrow(wingDirectionGizmo, wingFlightPlusGlide.wingDirection, Color.red);
            ScaleArrow(playerVelocityGizmo, Networking.LocalPlayer.GetVelocity(), Color.blue);
        }

        private void ScaleArrow(GameObject arrow, Vector3 velocity, Color color)
        {
            if (velocity.magnitude < 0.01f)
            {
                arrow.SetActive(false);
                return;
            }
            arrow.SetActive(true);
            arrow.transform.rotation = Quaternion.LookRotation(velocity);
            //arrow.transform.localScale = new Vector3(velocity.magnitude, velocity.magnitude, velocity.magnitude);
            arrow.transform.localScale = new Vector3(1, 1, velocity.magnitude);
            arrow.GetComponentInChildren<MeshRenderer>().material.color = color;
        }
    }
}
