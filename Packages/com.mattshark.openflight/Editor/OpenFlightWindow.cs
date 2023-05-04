using UnityEditor;
using UnityEngine;

/*
The point of this script is to create a dropdown menu in the Unity Editor top bar

The top bar entry is Packages, which inside of that dropdown it contains a entry for each package
(Future goal, package creators would need to add their package to the dropdown menu. Hopefully this can get standardized in the future)

an example of the dropdown structure
VRC Packages
    OpenFlight
        Prefabs
            Lite
            Full
    AnotherPackagename
*/
namespace OpenFlightVRC.Editor
{
	public class OpenFlightWindow : EditorWindow
	{
		[MenuItem("VRC Packages/OpenFlight/Prefabs/Lite")]
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
		}
	}
}
