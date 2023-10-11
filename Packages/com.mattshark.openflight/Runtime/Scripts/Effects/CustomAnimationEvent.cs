
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.Effects
{
    public class CustomAnimationEvent : UdonSharpBehaviour
    {
        void OnEnable()
        {
            GetComponent<AudioSource>().Play();
        }

        void OnDisable()
        {
            GetComponent<AudioSource>().Stop();
        }
    }
}
