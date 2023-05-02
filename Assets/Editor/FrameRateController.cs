using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//simple unity editor window to control the framerate of the editor
public class FrameRateController : EditorWindow
{
	[MenuItem("VRC Packages/OpenFlight/DEV/Frame Rate Controller")]
	public static void ShowWindow()
	{
		EditorWindow.GetWindow(typeof(FrameRateController));
	}

	int targetFrameRate = 60;

	void OnGUI()
	{
		GUILayout.Label("Frame Rate Controller", EditorStyles.boldLabel);
		GUILayout.Label("Set the target frame rate for the editor. This will not affect builds.", EditorStyles.wordWrappedLabel);
		GUILayout.Label("Current Frame Rate: " + Application.targetFrameRate, EditorStyles.wordWrappedLabel);
		targetFrameRate = EditorGUILayout.IntSlider("Target Frame Rate", targetFrameRate, 1, 120);
		if (GUILayout.Button("Set Frame Rate"))
		{
			Application.targetFrameRate = targetFrameRate;
		}
	}
}
