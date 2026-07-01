
using UdonSharp;
using UnityEngine;
using UnityEngine.PlayerLoop;

using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDKBase;
using VRC.Udon;

public class OpenFlightContactTester : UdonSharpBehaviour
{
    public GameObject contact;

    public Vector3 contacttransform;
    public Vector3 contactrotation;
    public bool togglecontact = false;

    public void Update()
    {
        contact.transform.localPosition = contacttransform;
        contact.transform.localRotation = Quaternion.Euler(contactrotation);
        contact.SetActive(togglecontact);
    }
}
