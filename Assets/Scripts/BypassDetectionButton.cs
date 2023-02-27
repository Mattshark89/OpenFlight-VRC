
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class BypassDetectionButton : UdonSharpBehaviour
{
    public UdonBehaviour AvatarDetection;
    public GameObject WingedFlight;
    void Update()
    {
        WingedFlight.SetActive((bool)AvatarDetection.GetProgramVariable("allowedToFly"));

        if ((bool)AvatarDetection.GetProgramVariable("allowedToFly"))
        {
            //change our mesh renderer to have a 0.5 y offset
            GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0, 0.5f));
        }
        else
        {
            //change our mesh renderer to have a 0 y offset
            GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0, 0));
        }   
    }

    void Interact()
    {
        AvatarDetection.SetProgramVariable("bypassDetection", !(bool)AvatarDetection.GetProgramVariable("bypassDetection"));
    }
}
