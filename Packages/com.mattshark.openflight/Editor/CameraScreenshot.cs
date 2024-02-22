/**
 * @ Maintainer: Happyrobot33
 */

using UnityEditor;
using UnityEngine;

namespace OpenFlightVRC.Editor
{
	/// <summary>
	/// A simple editor window that takes a screenshot of the scene view
	/// </summary>
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
