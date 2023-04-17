
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//the point of this script is to teleport the player to incrementally far away locations, every time making sure the hash stays the same as it was at 0,0,0
public class HashDistanceTest : UdonSharpBehaviour
{
    public AvatarDetection avatarDetection;
    int startHashV1;
    int startHashV2;
    int currentHashV1;
    int currentHashV2;
    int testingDistance = 0;
    int testingDistanceIncrement = 1;
    void Start()
    {
        avatarDetection.skipLoadingAvatar = false;
    }
    void Update()
    {
        //wait till the two hashes are generated
        if (startHashV1 == 0 || startHashV2 == 0)
        {
            if (avatarDetection.hashV1 != 0)
            {
                startHashV1 = avatarDetection.hashV1;
            }
            if (avatarDetection.hashV2 != 0)
            {
                startHashV2 = avatarDetection.hashV2;
            }
            return;
        }

        Debug.Log("Testing distance: " + testingDistance + "\n" + avatarDetection.debugInfo);
    }

    public void testNext(){
        //exponentially move the player away from 0,0,0
        testingDistance += testingDistanceIncrement;
        testingDistanceIncrement *= 2;

        //teleport the player to the new location
        Networking.LocalPlayer.TeleportTo(new Vector3(testingDistance, 0, 0), Quaternion.identity);

        //tell it to update the hash
        avatarDetection.ReevaluateFlight();
    }
}
