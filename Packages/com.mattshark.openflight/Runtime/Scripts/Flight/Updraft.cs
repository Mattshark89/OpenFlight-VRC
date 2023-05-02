
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Updraft : UdonSharpBehaviour
{
    [Range(0,800)]
    public int updraftStrength;

    public GameObject wingedFlight;
    private WingFlightPlusGlide flightCore;
    private bool Enabled = false;

    void Start()
    {
        if (wingedFlight != null)
        {
            flightCore = wingedFlight.GetComponent<WingFlightPlusGlide>();
            Enabled = true;
        }
        else
        {
            Enabled = false;
            Debug.LogError("Disabling Updraft - Script missing WingedFlight GameObject");
        }
    }

    public override void OnPlayerTriggerEnter(VRCPlayerApi player)
    {
        if (Enabled && player.IsValid() && player.isLocal)
        {
            if (flightCore != null)
            {
                flightCore.EnterUpdraft(updraftStrength);
            }
        }
        base.OnPlayerTriggerEnter(player);
    }

    public override void OnPlayerTriggerExit(VRCPlayerApi player)
    {
        if (Enabled && player.IsValid() && player.isLocal)
        {
            if (flightCore != null)
            {
                flightCore.ExitUpdraft();
            }
        }
        base.OnPlayerTriggerExit(player);
    }
}
