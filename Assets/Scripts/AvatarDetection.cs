//mathf
using System.Collections;
//import TMP
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Koyashiro.UdonJson;

//TODO: Make avatar scaling not recalculate hash, by checking if the lodaing avatar has been changed into first
//TODO: Fix lag spikes due to long json list

public class AvatarDetection : UdonSharpBehaviour
{
    VRCPlayerApi localPlayer = null;
	[Tooltip("optional")]
    public TextMeshProUGUI text;
    int scalingFactor = 1000;
    //this is used as the base for the avatar scale compenstation
    double d_spinetochest = 0;
    //used to see if the player has changed avatars
    double previous_d_spinetochest = 0;
    //external JSON list stuff
    public UdonBehaviour JSONLoader;
	public OpenFlight openFlight;
    string jsonString = "";
    UdonJsonValue json;
	[HideInInspector]
    public bool bypassDetection = false;
	[HideInInspector]
    public bool allowedToFly = false;
    public bool skipLoadingAvatar = true;

    //information about the avatar that has been detected
	[HideInInspector]
    public double weight = 0;
	[HideInInspector]
    public double WingtipOffset = 0;
	[HideInInspector]
    public string name = "";
	[HideInInspector]
    public string creator = "";
	[HideInInspector]
    public string introducer = "";

    //information about the json itself
	[HideInInspector]
    public string jsonVersion = "";
	[HideInInspector]
    public string jsonDate = "";

    void Start()
    {
        //get the local player
        localPlayer = Networking.LocalPlayer;

		if (text != null) {
			text.text = "Loading JSON list...";
		}
        //get the JSON list
        JSONLoader.SendCustomEvent("LoadUrl");
    }

    void Update()
    {
        //if the JSON list is empty, then return
        if (jsonString == "" || jsonString == null)
        {
            jsonString = (string)JSONLoader.GetProgramVariable("Output");
            LoadJSON();
            return;
        }

        //get spine and hips first, as they are used to calculate the avatar scale
        Vector3 spine = localPlayer.GetBonePosition(HumanBodyBones.Spine);
        Vector3 chest = localPlayer.GetBonePosition(HumanBodyBones.Chest);

        //calculate the avatar scale using the distance from hips to spine
        d_spinetochest = Vector3.Distance(chest, spine);

        //if the player has changed avatars, do the hashing and determine if the avatar is allowed to fly
        if (Mathf.Abs((float)d_spinetochest - (float)previous_d_spinetochest) > 0.0001f)
        {
            bypassDetection = false;
            previous_d_spinetochest = d_spinetochest;

            //get all the bones now
            Vector3 head = localPlayer.GetBonePosition(HumanBodyBones.Head);
            Vector3 neck = localPlayer.GetBonePosition(HumanBodyBones.Neck);
            Vector3 leftShoulder =
                localPlayer.GetBonePosition(HumanBodyBones.LeftShoulder);
            Vector3 LeftUpperArm =
                localPlayer.GetBonePosition(HumanBodyBones.LeftUpperArm);
            Vector3 LeftLowerArm =
                localPlayer.GetBonePosition(HumanBodyBones.LeftLowerArm);
            Vector3 LeftHand = localPlayer.GetBonePosition(HumanBodyBones.LeftHand);

            int d_necktohead = getBoneDistance(neck, head);
            int d_chesttoneck = getBoneDistance(chest, neck);
            int d_leftshouldertoleftupperarm =
                getBoneDistance(leftShoulder, LeftUpperArm);
            int d_leftupperarmtoleftlowerarm =
                getBoneDistance(LeftUpperArm, LeftLowerArm);
            int d_leftlowertolefthand = getBoneDistance(LeftLowerArm, LeftHand);
            
            //this is a combined string of all the bone info, this is used for the avatar detection
            string boneInfo = d_necktohead + "." + d_chesttoneck + "." + d_leftshouldertoleftupperarm + "." + d_leftupperarmtoleftlowerarm + "." + d_leftlowertolefthand;
        
            int hash = boneInfo.GetHashCode();

            //check if the hash is the loading avatar, and if it is then dont check if the avatar is allowed to fly
            if (hash == -1470672748 && skipLoadingAvatar)
            {
				if (text != null) {
					text.text = "Loading Avatar Detected, ignoring...";
				}
                return;
            }

            //check if the avatar is allowed to fly
            allowedToFly = isAvatarAllowedToFly(hash);
			if (allowedToFly) {
				openFlight.EnableWingedFlight();
			} else {
				openFlight.DisableFlight();
			}

            //print all the info to the text
			if (text != null) {
				text.text =
					"Spine to Chest: " +
					d_spinetochest +
					"\nHead to Neck: " +
					d_necktohead +
					"\nChest to Neck: " +
					d_chesttoneck +
					"\nLeft Shoulder to Left Upper Arm: " +
					d_leftshouldertoleftupperarm +
					"\nLeft Upper Arm to Left Lower Arm: " +
					d_leftupperarmtoleftlowerarm +
					"\nLeft Lower Arm to Left Hand: " +
					d_leftlowertolefthand +
					"\nCombined Bone Info: " +
					boneInfo +
					"\nHash: " +
					hash + 
					"\nAllowed to Fly: " +
					allowedToFly +
					"\n\nDetected Avatar Info: " +
					"\nName: " +
					name +
					"\nCreator: " +
					creator +
					"\nIntroduced by: " +
					introducer +
					"\nWeight: " +
					weight +
					"\nWingtip Offset: " +
					WingtipOffset;
			}
        }
        else if (bypassDetection && !allowedToFly)
        {
            if (text != null) {
				text.text = "Bypassing Avatar Detection\nThe avatar you are currently in is able to fly no matter what. If you change avatars, you will need to re-enable the bypass";
            }
			allowedToFly = true;
			openFlight.EnableWingedFlight();
        }
    }

    int getBoneDistance(Vector3 bone1, Vector3 bone2)
    {
        return Mathf
            .FloorToInt(Vector3.Distance(bone1, bone2) /
            (float)d_spinetochest *
            scalingFactor);
    }

    bool isAvatarAllowedToFly(int in_hash)
    {
        var avi_bases = json.GetValue("Bases"); //array of all the bases
        for (int i = 0; i < avi_bases.Count(); i++)
        {
            var avi_base_keys = avi_bases.Keys();
            var avi_base = avi_bases.GetValue(avi_base_keys[i]);
            for (int j = 0; j < avi_base.Count(); j++)
            {
                var avi_varaint_keys = avi_base.Keys();
                var variant = avi_base.GetValue(avi_varaint_keys[j]);
                //Debug.Log(variant.GetValue("Hash").AsString());
                string hash = variant.GetValue("Hash").AsString();
                if (hash == in_hash.ToString())
                {
                    name = variant.GetValue("Name").AsString();
                    creator = variant.GetValue("Creator").AsString();
                    introducer = variant.GetValue("Introducer").AsString();
                    weight = variant.GetValue("Weight").AsNumber();
                    WingtipOffset = variant.GetValue("WingtipOffset").AsNumber();
                    return true;
                }
            }
        }

        name = "Unknown";
        creator = "Unknown";
        introducer = "Unknown";
        weight = 0;
        WingtipOffset = 0;
        return false;
    }

    void LoadJSON()
    {
        if (jsonString != "" && jsonString != null)
        {
            var result = UdonJsonDeserializer.TryDeserialize(jsonString, out json);
            jsonVersion = json.GetValue("JSON Version").AsString();
            jsonDate = json.GetValue("JSON Date").AsString();
        }
    }

    public void reloadJSON()
    {
		if (text != null) {
			text.text = "Loading JSON list...";
		}
        //get the JSON list
        JSONLoader.SendCustomEvent("LoadUrl");

        jsonString = "";
        d_spinetochest = 0;
        previous_d_spinetochest = 1000f;
    }
}
