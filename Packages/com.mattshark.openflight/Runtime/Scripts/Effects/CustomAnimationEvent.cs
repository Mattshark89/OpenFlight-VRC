
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CustomAnimationEvent : UdonSharpBehaviour
{
    private AudioSource audioSource;

    void OnEnable()
    {
        GetComponent<AudioSource>().Play();
    }

    void OnDisable()
    {
        GetComponent<AudioSource>().Stop();
    }
}
