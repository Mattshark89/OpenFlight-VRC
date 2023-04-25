using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Gizmo drawing
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

/*
This is a base class for a zone. It is not meant to be used directly, but rather to be inherited from.
*/
public class Zone : UdonSharpBehaviour
{
	BoxCollider zoneCollider;
	protected ZoneNotifier zoneNotifier;
	public bool notifyPlayer = true; //whether or not to notify the player when they enter the zone
	protected VRCPlayerApi localPlayer = null;

	//This is here to allow sub classes to call it in their start function, grabbing the neccesary components
	protected void init()
	{
		//finds the local player
		localPlayer = Networking.LocalPlayer;

		//finds the zone notifier
		zoneNotifier = GameObject.Find("ZoneNotifier").GetComponent<ZoneNotifier>();

		//finds the collider
		zoneCollider = GetComponent<BoxCollider>();
	}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
	[System.NonSerialized]
	float selectionGizmoAlphaAdjust = 0.2f;
    void OnDrawGizmos()
    {
        Gizmos.color = GetGizmoColorWithAlpha();
        drawColliderGizmo();
    }


    //make it slightly more opaque when selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = GetGizmoColorWithAlpha() + new Color(0f, 0f, 0f, selectionGizmoAlphaAdjust);
        drawColliderGizmo();
    }

    //Override this to change the color of the gizmo
    protected virtual Color GetGizmoColor()
    {
        return new Color(1f, 0f, 0f, 0.2f);
    }

    Color GetGizmoColorWithAlpha()
    {
        Color color = GetGizmoColor();
        color.a = 0.2f;
        return color;
    }

    void drawColliderGizmo(){
        //handle rotation
        Quaternion rotation = transform.rotation;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, rotation, Vector3.one);

        //get the collider
        if (zoneCollider == null)
        {
            zoneCollider = GetComponent<BoxCollider>();
        }

        Gizmos.DrawCube(zoneCollider.center, zoneCollider.size);
    }
#endif
}
