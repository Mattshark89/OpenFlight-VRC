using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//This script edits the OpenFlight.cs script to keep the version number up to date.
//get the version number from the package.json file
class ImportRunner : AssetPostprocessor
{
	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		//edit the OpenFlight.cs script to update the version number
		string[] lines = System.IO.File.ReadAllLines("Packages/com.mattshark.openflight/Runtime/Scripts/Flight/OpenFlight.cs");
		for (int i = 0; i < lines.Length; i++)
		{
			if (lines[i].Contains("public string OpenFlightVersion = "))
			{
				//find the version number from the package.json file
				string version = System.IO.File.ReadAllText("Packages/com.mattshark.openflight/package.json");
				version = version.Substring(version.IndexOf("version") + 10); //this makes version look like "1.0.0"
				//remove the first " from the version number
				version = version.Substring(1);
				version = version.Substring(0, version.IndexOf("\"")); //this removes the last " from the version number
				if (lines[i].Contains("OpenFlightVersion = \"" + version + "\";"))
					return; //if the version number is already correct, don't update it
				lines[i] = "    public string OpenFlightVersion = \"" + version + "\";";
				System.IO.File.WriteAllLines("Packages/com.mattshark.openflight/Runtime/Scripts/Flight/OpenFlight.cs", lines);
				//reimport the OpenFlight.cs script so that the changes take effect
				Debug.Log("OpenFlight version updated to " + version);
			}
		}
	}
}
