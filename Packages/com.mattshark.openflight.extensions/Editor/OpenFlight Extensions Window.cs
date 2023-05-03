using UnityEditor;
using UnityEngine;

namespace OpenFlightVRC.Extensions
{
	public class OpenFlightExtensionsWindow : EditorWindow
	{
		[MenuItem("VRC Packages/OpenFlight/Prefabs/Zones/ZoneNotifier")]
		public static void ZoneNotifier()
		{
			GameObject prefab =
				AssetDatabase.LoadAssetAtPath("Packages/com.mattshark.openflight.extensions/Runtime/Prefabs/ZoneNotifier.prefab", typeof(GameObject)) as GameObject;
			AddPrefabToScene(prefab, true);
		}

		[MenuItem("VRC Packages/OpenFlight/Prefabs/Zones/No Fly Zone")]
		public static void NoFlyZone()
		{
			//check if a zone notifier is already in the scene
			//if it isnt, add it
			if (!GameObject.Find("ZoneNotifier"))
			{
				ZoneNotifier();
			}
			GameObject prefab =
				AssetDatabase.LoadAssetAtPath("Packages/com.mattshark.openflight.extensions/Runtime/Prefabs/NoFlyZone.prefab", typeof(GameObject)) as GameObject;
			AddPrefabToScene(prefab);
		}

		[MenuItem("VRC Packages/OpenFlight/Prefabs/Zones/Defaults Zone")]
		public static void DefaultsZone()
		{
			//check if a zone notifier is already in the scene
			//if it isnt, add it
			if (!GameObject.Find("ZoneNotifier"))
			{
				ZoneNotifier();
			}
			GameObject prefab =
				AssetDatabase.LoadAssetAtPath("Packages/com.mattshark.openflight.extensions/Runtime/Prefabs/DefaultsZone.prefab", typeof(GameObject)) as GameObject;
			AddPrefabToScene(prefab);
		}

		[MenuItem("VRC Packages/OpenFlight/Prefabs/Zones/Out Of Bounds Zone")]
		public static void OutOfBoundsZone()
		{
			//check if a zone notifier is already in the scene
			//if it isnt, add it
			if (!GameObject.Find("ZoneNotifier"))
			{
				ZoneNotifier();
			}
			GameObject prefab =
				AssetDatabase.LoadAssetAtPath("Packages/com.mattshark.openflight.extensions/Runtime/Prefabs/OutOfBoundsZone.prefab", typeof(GameObject)) as GameObject;
			AddPrefabToScene(prefab);
		}

		static void AddPrefabToScene(GameObject prefab, bool checkForExisting = false)
		{
			// Check to see if the prefab is already in the scene
			if (GameObject.Find(prefab.name) && checkForExisting)
			{
				EditorUtility.DisplayDialog("Prefab already in scene", "Only one of these prefabs is allowed per scene", "OK");
				return;
			}
			GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

			//place the prefab at the center of the editor viewport
			Camera sceneViewCamera = SceneView.lastActiveSceneView.camera;
			Vector3 center = sceneViewCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 4.0f));
			instance.transform.position = center;
		}

		//gizmo show/hide
		[MenuItem("VRC Packages/OpenFlight/Gizmos/Show Zone Gizmos")]
		public static void ShowZoneGizmos()
		{
			EditorPrefs.SetBool("OpenFlightShowZoneGizmos", true);
		}

		[MenuItem("VRC Packages/OpenFlight/Gizmos/Hide Zone Gizmos")]
		public static void HideZoneGizmos()
		{
			EditorPrefs.SetBool("OpenFlightShowZoneGizmos", false);
		}
	}
}
