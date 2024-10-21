/**
 * @ Maintainer: Happyrobot33
 */

using UnityEditor;
using UnityEngine;
namespace OpenFlightVRC.Editor
{
	/// <summary>
	/// The point of this script is to create a dropdown menu in the Unity Editor top bar for adding in the OpenFlight prefabs
	/// </summary>
	public class OpenFlightWindow : EditorWindow
	{
		[MenuItem("VRC Packages/OpenFlight/Prefabs/Lite (Deprecated)")]
		[System.Obsolete("This prefab is deprecated, please use the Full prefab instead")]
		public static void OpenFlightBasic()
		{
			// Open the Lite prefab
			GameObject prefab = AssetDatabase.LoadAssetAtPath("Packages/com.mattshark.openflight/Runtime/OpenFlight (Lite).prefab", typeof(GameObject)) as GameObject;
			AddPrefabToScene(prefab);
		}

		[MenuItem("VRC Packages/OpenFlight/Prefabs/Full")]
		public static void OpenFlightFull()
		{
			// Open the full prefab
			GameObject prefab = AssetDatabase.LoadAssetAtPath("Packages/com.mattshark.openflight/Runtime/OpenFlight.prefab", typeof(GameObject)) as GameObject;
			AddPrefabToScene(prefab);
		}

		static void AddPrefabToScene(GameObject prefab)
		{
			// Check to see if the prefab is already in the scene
			if (GameObject.Find(prefab.name))
			{
				EditorUtility.DisplayDialog("Prefab already in scene", "The prefab is already in the scene, so it wont be added", "OK");
				return;
			}
			GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

			//place the prefab in the center of the scene camera
			instance.transform.position = SceneView.lastActiveSceneView.camera.transform.position + SceneView.lastActiveSceneView.camera.transform.forward * 2;
		}
	}
}
