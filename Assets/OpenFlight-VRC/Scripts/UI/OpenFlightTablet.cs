
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using TMPro;

public class OpenFlightTablet : UdonSharpBehaviour
{
    VRCPlayerApi localPlayer = null;
    public string PanelVersion = "0.0.1";
    public float scalingOffset = 0.1f;
    public OpenFlight OpenFlight;
    public AvatarDetection AvatarDetection;

    public TextMeshProUGUI VersionInfo;

    public Color activeTabColor;
    public Color inactiveTabColor;

    public Button[] tabs;
    void Start()
    {
        //get the local player
        localPlayer = Networking.LocalPlayer;

        //initialize the tabs
        SetActiveTabMain();
    }

    void Update()
    {
        //change the scale of the gameobject based on the players scale
        Vector3 spine = localPlayer.GetBonePosition(HumanBodyBones.Spine);
        Vector3 chest = localPlayer.GetBonePosition(HumanBodyBones.Chest);
        float d_spinetochest = Vector3.Distance(chest, spine);

        //set this gameobjects scale to the players scale
        transform.localScale = new Vector3((float)d_spinetochest * scalingOffset, (float)d_spinetochest * scalingOffset, (float)d_spinetochest * scalingOffset);

        //set the version info text
        VersionInfo.text = "Open-Flight Ver " + OpenFlight.OpenFlightVersion + "\nPanel Ver " + PanelVersion + "\nJSON Ver " + AvatarDetection.jsonVersion;
    }

    public void SetActiveTab(int tab)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            if (i == tab)
            {
                tabs[i].GetComponent<Image>().color = activeTabColor;
            }
            else
            {
                tabs[i].GetComponent<Image>().color = inactiveTabColor;
            }
        }
    }

    public void SetActiveTabMain()
    {
        SetActiveTab(0);
    }

    public void SetActiveTabSettings()
    {
        SetActiveTab(1);
    }

    public void SetActiveTabDebug()
    {
        SetActiveTab(2);
    }
}
