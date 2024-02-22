/**
 * @ Maintainer: Happyrobot33
 */

using UnityEditor;
using UnityEngine;

namespace OpenFlightVRC.Editor
{
    /// <summary>
    /// A simple script to modify client sim at runtime for in-editor flight testing
    /// </summary>
    public class ClientSimTestSetup : EditorWindow
    {
        [MenuItem("VRC Packages/OpenFlight/Editor/Modify Client Sim Objects")]
        public static void ModifyClientSimObjects()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "You must be in play mode to use this feature", "Ok");
                return;
            }

            //make sure the client sim objects exist
            GameObject clientSim = GameObject.Find("__ClientSimSystem");
            if (clientSim == null)
            {
                EditorUtility.DisplayDialog("Error", "You must have a ClientSimSystem active in your scene to use this feature", "Ok");
                return;
            }

            GameObject localPlayer = GameObject.Find("[1] Local Player");
            if (localPlayer == null)
            {
                EditorUtility.DisplayDialog("Error", "Local player object not found", "Ok");
                return;
            }

            //get both hand objects associated with tracking inputs
            GameObject DestkopTrackingData = GameObject.Find("DestkopTrackingData");
            GameObject leftHand = DestkopTrackingData.FindObjectInChilds("LeftHand");
            GameObject rightHand = DestkopTrackingData.FindObjectInChilds("RightHand");

            //rotate so they are facing the right way
            leftHand.transform.Rotate(0, 0, -90);
            rightHand.transform.Rotate(0, 0, 90);

            //get both hand objects associated with the bones themselves
            GameObject leftArmatureHand = GameObject.Find("LeftArm");
            GameObject rightArmatureHand = GameObject.Find("RightArm");

            //rotate so they are facing the right way
            leftArmatureHand.transform.Rotate(-80, 0, 0);
            rightArmatureHand.transform.Rotate(-80, 0, 0);
        }

        [MenuItem("VRC Packages/OpenFlight/Editor/Make Scene Camera Follow Local Player")]
        public static void MakeSceneCameraFollowLocalPlayer()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "You must be in play mode to use this feature", "Ok");
                return;
            }

            //make sure we are keeping track of if we are following the player or not
            FollowPlayerWithCamera = !FollowPlayerWithCamera;
        }

        /// <summary>
        /// If true, the scene camera will follow the local player
        /// </summary>
        static bool FollowPlayerWithCamera = false;

        // Add ourselves to the editors update loop
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            EditorApplication.update += Update;

            EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    FollowPlayerWithCamera = false;
                }
            };
        }

        static void Update()
        {
            if (!EditorApplication.isPlaying || !FollowPlayerWithCamera)
            {
                return;
            }

            GameObject clientSim = GameObject.Find("__ClientSimSystem");
            if (clientSim == null)
            {
                EditorUtility.DisplayDialog("Error", "You must have a ClientSimSystem active in your scene to use this feature", "Ok");
                return;
            }

            GameObject localPlayer = GameObject.Find("PlayerController");
            if (localPlayer == null)
            {
                EditorUtility.DisplayDialog("Error", "Local player object not found", "Ok");
                return;
            }

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null)
            {
                sceneView.pivot = localPlayer.transform.position;
                Quaternion followRotation = localPlayer.transform.rotation;

                //give the camera a slight downward angle for visuals
                followRotation *= Quaternion.Euler(20, 0, 0);
                //gently lerp the camera to the player
                sceneView.rotation = Quaternion.Lerp(sceneView.rotation, followRotation, 0.1f);
            }
        }
    }

    //extension method for finding a child object
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Finds a child object by name
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="gameObjectName"></param>
        /// <returns></returns>
        public static GameObject FindObjectInChilds(this GameObject gameObject, string gameObjectName)
        {
            Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
            foreach (Transform item in children)
            {
                if (item.name == gameObjectName)
                {
                    return item.gameObject;
                }
            }

            return null;
        }
    }
}
