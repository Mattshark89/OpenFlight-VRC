
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class LocalPlayerFollower : UdonSharpBehaviour
{
    public Transform Target;
    private VRCPlayerApi player;

    public void Start()
    {
        if (Target == null)
        {
            Target = transform;
        }

        player = Networking.LocalPlayer;
    }
    public override void PostLateUpdate()
    {
        Target.position = player.GetPosition();
        Target.rotation = player.GetRotation();
    }
}
