
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC
{
    /// <summary>
    /// A collection of useful functions that I cant find a better place for
    /// </summary>
    public class Util : UdonSharpBehaviour
    {
        /// <summary>
        /// Returns the distance between two bones, modified by the scaling factor and spine
        /// </summary>
        /// <param name="bone1">The first bone</param>
        /// <param name="bone2">The second bone</param>
        /// <param name="scalingFactor">The scaling factor</param>
        /// <param name="d_spinetochest">The distance between the spine and the chest</param>
        /// <returns></returns>
        public static int GetBoneDistance(Vector3 bone1, Vector3 bone2, int scalingFactor, float d_spinetochest)
        {
            return Mathf.FloorToInt(Vector3.Distance(bone1, bone2) / d_spinetochest * scalingFactor);
        }

        /// <summary>
        /// Moves a gameobject to a wingtip position
        /// </summary>
        /// <param name="bone">The tracking data of the player</param>
        /// <param name="objectToMove">The wingtip gameobject</param>
        /// <param name="WingtipOffset">The offset of the wingtip</param>
        /// <param name="d_spinetochest">The distance between the spine and the chest</param>
        public static void SetWingtipTransform(VRCPlayerApi.TrackingData bone, GameObject objectToMove, float WingtipOffset, double d_spinetochest)
        {
            objectToMove.transform.rotation = Quaternion.identity;
            Vector3 position = bone.position;
            Quaternion rotation = bone.rotation;

            Vector3 WingTipPosition = position + (rotation * Vector3.forward * new Vector3(0, 0, WingtipOffset * (float)d_spinetochest).z);

            objectToMove.transform.position = WingTipPosition;

            //rotate so it goes in the correct direction
            objectToMove.transform.RotateAround(position, rotation * Vector3.up, 70);
        }


        /// <summary>
        /// Helper function to get the total distance of a vector array.
        /// this adds up all of the distances between each vector in the array in order, then returns the total distance
        /// </summary>
        /// <param name="vectors">The vector array to get the total distance of</param>
        /// <returns>The total distance of the vector array</returns>
        public static float TotalVectorDistance(Vector3[] vectors)
        {
            float totalDistance = 0;
            for (int i = 0; i < vectors.Length; i++)
            {
                if (i != 0)
                {
                    totalDistance += Vector3.Distance(vectors[i], vectors[i - 1]);
                }
            }
            return totalDistance;
        }

        /// <summary>
        /// Gets a scale modifier based on the player's scale in order to scale things uniformly
        /// </summary>
        /// <remarks>
        /// This really should be replaced with the newer GetAvatarEyeHeightAsMeters function from VRC, but testing to make sure it behaves the same is needed and is difficult to do in unity since clientsim doesnt emulate scaling as of now
        /// </remarks>
        /// <returns>A float to scale by. 1 is the base scale of the loading avatar</returns>
        public static float ScaleModifier()
        {
            VRCPlayerApi localPlayer = Networking.LocalPlayer;

            //change the scale of the gameobject based on the players scale
            //add up all of the bone distances from the foot to the head
            Vector3 footR = localPlayer.GetBonePosition(HumanBodyBones.RightFoot);
            Vector3 LowerLegR = localPlayer.GetBonePosition(HumanBodyBones.RightLowerLeg);
            Vector3 UpperLegR = localPlayer.GetBonePosition(HumanBodyBones.RightUpperLeg);
            Vector3 Hips = localPlayer.GetBonePosition(HumanBodyBones.Hips);
            Vector3 spine = localPlayer.GetBonePosition(HumanBodyBones.Spine);
            Vector3 chest = localPlayer.GetBonePosition(HumanBodyBones.Chest);
            Vector3 Neck = localPlayer.GetBonePosition(HumanBodyBones.Neck);
            Vector3 Head = localPlayer.GetBonePosition(HumanBodyBones.Head);
            float PlayerScale = TotalVectorDistance(new Vector3[] { footR, LowerLegR, UpperLegR, Hips, spine, chest, Neck, Head });

            //if the player is too small, set the scale to 0.1
            PlayerScale = Mathf.Max(PlayerScale, 0.1f);

            //this is determined experimentally, and makes the loading avatar scale 1
            const float scalingOffset = 0.6183768f;

            return PlayerScale * scalingOffset;
        }

        /// <summary>
        /// Creates and returns a rainbow gradient for use in particle systems
        /// </summary>
        /// <returns>A rainbow gradient</returns>
        public static Gradient GetRainbowGradient()
        {
            GradientColorKey[] rainbow = new GradientColorKey[8];

            for (int i = 0; i < 8; i++)
            {
                var color = Color.HSVToRGB(i / 7f, 1, 1);
                rainbow[i] = new GradientColorKey(color, i / 7f);
            }

            Gradient gradient = new Gradient();

            gradient.colorKeys = rainbow;

            return gradient;
        }
    }
}
