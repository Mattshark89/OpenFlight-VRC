//mathf
using System.Collections;
//import TMP
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using Koyashiro.UdonJson;


public class AvatarDetection : UdonSharpBehaviour
{
    VRCPlayerApi localPlayer = null;
    public string debugInfo = ""; //Contains all the debug info about avatar detection
    /*
    Spine to Chest: XXX
    Head to Neck: XXX
    Chest to Neck: XXX
    Left Shoulder to Left Upper Arm: XXX
    Left Upper Arm to Left Lower Arm: XXX
    Left Lower Arm to Left Hand: XXX
    Combined Bone Info: XXX
    Hash: XXX
    Allowed to Fly: XXX
    Detected Avatar Info:
    Name: XXX
    Creator: XXX
    Introducer: XXX
    Weight: XXX
    Wingtip Offset: XXX
    */

    int scalingFactor = 1000; //this is used to essentially round and make the bone distances integers
    double d_spinetochest = 0; //used to calculate the avatar scale
    double previous_d_spinetochest = 0; //used to see if the avatar has changed
    
    //external JSON list stuff
    public AvatarListLoader JSONLoader; //this is the script that loads the JSON list
    public OpenFlight OpenFlight; 
    public WingFlightPlusGlide WingFlightPlusGlide;
    string jsonString = ""; //this is the JSON list in string form
    UdonJsonValue json; //this is the JSON list in a serialized form, allowing for JSON commands to be used on it
    public bool allowedToFly = false; //this is used to tell openflight if the avatar is allowed to fly
    public bool skipLoadingAvatar = true; //this is used to skip the loading avatar, as it is not a real avatar

    //gizmo related stuff
    public bool showWingTipGizmo = false;
    public GameObject wingtipGizmo; //this shows the wingtip offset as a sphere in game. Only works in VR due to implementation

    //information about the avatar that has been detected
    public float weight = 0;
    public float WingtipOffset = 0;
    public string name = ""; //this is the name of the avatar base
    public string creator = ""; //this is the person who created the avatar base
    public string introducer = ""; //this is the person who introduced the avatar to the JSON list itself

    //information about the json itself
    public string jsonVersion = "";
    public string jsonDate = "";

    void Start()
    {
        //get the local player
        localPlayer = Networking.LocalPlayer;

        debugInfo = "Loading JSON list...";
        JSONLoader.LoadURL(); //tell the JSON loader to try to load the JSON list from the github
    }

    void Update()
    {
        //if the JSON list is empty, then return
        if (jsonString == "" || jsonString == null)
        {
            jsonString = (string)JSONLoader.Output;
            LoadJSON();
            return;
        }

        //get spine and hips first, as they are used to calculate the avatar scale
        Vector3 spine = localPlayer.GetBonePosition(HumanBodyBones.Spine);
        Vector3 chest = localPlayer.GetBonePosition(HumanBodyBones.Chest);

        //calculate the avatar scale using the distance from hips to spine
        d_spinetochest = Vector3.Distance(chest, spine);

        WingFlightPlusGlide.wingtipOffset = WingtipOffset;
        WingFlightPlusGlide.weight = weight;

        //if the player has changed avatars, do the hashing and determine if the avatar is allowed to fly
        //avatar change is doen by checking if the distance from spine to chest has changed by a significant amount
        if (Mathf.Abs((float)d_spinetochest - (float)previous_d_spinetochest) > 0.0001f)
        {
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
            
            //this is a combined string of all the bone info
            //this string is then hashed to get a unique hash for each avatar
            string boneInfo = d_necktohead + "." + d_chesttoneck + "." + d_leftshouldertoleftupperarm + "." + d_leftupperarmtoleftlowerarm + "." + d_leftlowertolefthand;
        
            int hash = boneInfo.GetHashCode();

            //check if the hash is the loading avatar, and if it is then dont check if the avatar is allowed to fly
            if (hash == -1470672748 && skipLoadingAvatar)
            {
                debugInfo = "Loading Avatar Detected, ignoring...";
                name = "Loading Avatar";
                creator = "Loading Avatar";
                introducer = "Loading Avatar";
                weight = 0;
                WingtipOffset = 0;
                return;
            }

            //check if the avatar is allowed to fly
            allowedToFly = isAvatarAllowedToFly(hash);

            //tell openflight if the avatar is allowed to fly
            if (allowedToFly)
            {
                OpenFlight.CanFly();
            }
            else
            {
                OpenFlight.CannotFly();
                WingFlightPlusGlide.wingtipOffset = 0;
                WingFlightPlusGlide.weight = 1;
            }

            //print all the info to the text
            debugInfo =
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

        //gizmo stuff
        visualizeWingTips();
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
                var hashArray = variant.GetValue("Hash");
                for (int k = 0; k < hashArray.Count(); k++)
                {
                    string hash = hashArray.GetValue(k).AsString();
                    if (hash == in_hash.ToString())
                    {
                        name = variant.GetValue("Name").AsString();
                        creator = variant.GetValue("Creator").AsString();
                        introducer = variant.GetValue("Introducer").AsString();
                        weight = (float)variant.GetValue("Weight").AsNumber();
                        WingtipOffset = (float)variant.GetValue("WingtipOffset").AsNumber();
                        return true;
                    }
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
        debugInfo = "Loading JSON list...";
        //get the JSON list
        JSONLoader.LoadURL();

        jsonString = "";
        d_spinetochest = 0;
        previous_d_spinetochest = 1000f;
    }

    //TODO: Clean up this code so it isnt so segmented
    void visualizeWingTips()
    {
        //reset the wingtip gizmo rotation
        wingtipGizmo.transform.rotation = Quaternion.identity;

        //move a gameobject to the visualize the wingtips
        Vector3 rightHandPosition = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;
        Quaternion rightHandRotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation;

        //calculate the wingtip position by adding the offset to the right hand position in the direction of the right hand rotation
        Vector3 WingTipPosition = rightHandPosition + (rightHandRotation * Vector3.forward * new Vector3(0, 0, (float)WingtipOffset * (float)d_spinetochest).z);

        wingtipGizmo.transform.position = WingTipPosition;
        wingtipGizmo.transform.RotateAround(rightHandPosition, rightHandRotation * Vector3.up, 70);
    }

    public void showGizmo()
    {
        wingtipGizmo.SetActive(true);
    }

    public void hideGizmo()
    {
        wingtipGizmo.SetActive(false);
    }
}
