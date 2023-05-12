using UnityEditor;
using UnityEngine;

namespace OpenFlightVRC.Editor
{
	public class CameraScreenshot : EditorWindow
	{
		[MenuItem("VRC Packages/OpenFlight/Editor/Camera Screenshot")]
		public static void TakeScreenshot()
		{
			// ask the user where to save the screenshot
			string path = EditorUtility.SaveFilePanel("Save Screenshot", "Assets/Screenshots", "Screenshot.png", "png");
			if (path.Length != 0)
			{
				// Take a screenshot of the scene view
				ScreenCapture.CaptureScreenshot(path);
			}
		}
	}
}
