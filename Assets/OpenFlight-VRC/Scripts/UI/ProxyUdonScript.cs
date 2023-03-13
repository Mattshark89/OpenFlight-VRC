
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ProxyUdonScript : UdonSharpBehaviour
{
    public UdonBehaviour target;
    public GameObject targetGameObject;

    public void FlightOn()
    {
        target.SendCustomEvent("FlightOn");
    }

    public void FlightOff()
    {
        target.SendCustomEvent("FlightOff");
    }

    public void FlightAuto()
    {
        target.SendCustomEvent("FlightAuto");
    }

    public void reloadJSON()
    {
        target.SendCustomEvent("reloadJSON");
    }

    public void showGizmo()
    {
        target.SendCustomEvent("showGizmo");
    }

    public void hideGizmo()
    {
        target.SendCustomEvent("hideGizmo");
    }

    public void OnDisable()
    {
        if (targetGameObject != null)
            targetGameObject.SetActive(false);
    }

    public void OnEnable()
    {
        if (targetGameObject != null)
            targetGameObject.SetActive(true);
    }
}
