using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OpenFlightVRC.Editor
{
    public class ClientSimTestSetup : EditorWindow
    {
        //add a item to the menu that will setup client sim objects properly
        [MenuItem("VRC Packages/OpenFlight/Editor/Modify Client Sim Objects")]
        public static void ModifyClientSimObjects()
        {
            //make sure we are in play mode
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

            //get the local player
            GameObject localPlayer = GameObject.Find("[1] Local Player");
            if (localPlayer == null)
            {
                EditorUtility.DisplayDialog("Error", "Local player object not found", "Ok");
                return;
            }

            //get both hands, they should be under the DestkopTrackingData object
            GameObject DestkopTrackingData = GameObject.Find("DestkopTrackingData");
            GameObject leftHand = DestkopTrackingData.FindObjectInChilds("LeftHand");
            GameObject rightHand = DestkopTrackingData.FindObjectInChilds("RightHand");

            //rotate by 90 degrees around their Z axis
            leftHand.transform.Rotate(0, 0, -90);
            rightHand.transform.Rotate(0, 0, 90);

            //get both armature hands
            GameObject leftArmatureHand = GameObject.Find("LeftArm");
            GameObject rightArmatureHand = GameObject.Find("RightArm");

            //rotate around their X by 50 degrees
            leftArmatureHand.transform.Rotate(-80, 0, 0);
            rightArmatureHand.transform.Rotate(-80, 0, 0);
        }

        //add a item to the menu that will make the scene camera follow the local player
        [MenuItem("VRC Packages/OpenFlight/Editor/Make Scene Camera Follow Local Player")]
        public static void MakeSceneCameraFollowLocalPlayer()
        {
            //make sure we are in play mode
            if (!EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "You must be in play mode to use this feature", "Ok");
                return;
            }

            //invert the FollowPlayerWithCamera bool
            FollowPlayerWithCamera = !FollowPlayerWithCamera;
        }

        static bool FollowPlayerWithCamera = false;

        //run during the update loop, so we can move the camera actively
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            EditorApplication.update += Update;

            //if exiting play mode
            EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    FollowPlayerWithCamera = false;
                }
            };
        }

        //update loop
        static void Update()
        {
            //make sure we are in play mode
            if (!EditorApplication.isPlaying || !FollowPlayerWithCamera)
            {
                return;
            }

            //make sure the client sim objects exist
            GameObject clientSim = GameObject.Find("__ClientSimSystem");
            if (clientSim == null)
            {
                EditorUtility.DisplayDialog("Error", "You must have a ClientSimSystem active in your scene to use this feature", "Ok");
                return;
            }

            //get the local player
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
                followRotation *= Quaternion.Euler(20, 0, 0);
                //gently lerp the camera to the player
                sceneView.rotation = Quaternion.Lerp(sceneView.rotation, followRotation, 0.1f);
            }
        }
    }

    //extension method for finding a child object
    public static class GameObjectExtensions
    {
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
