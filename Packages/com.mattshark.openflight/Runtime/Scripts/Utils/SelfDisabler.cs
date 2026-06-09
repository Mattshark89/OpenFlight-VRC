
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SelfDisabler : UdonSharpBehaviour
{
    public GameObject ThisGameObject;
    public void SelfDisable()
    {
        ThisGameObject.SetActive(false);
    }
}
