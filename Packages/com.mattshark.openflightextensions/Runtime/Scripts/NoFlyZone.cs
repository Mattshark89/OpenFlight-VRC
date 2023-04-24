using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//Gizmo drawing
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

public class NoFlyZone : UdonSharpBehaviour
{
	OpenFlight openFlight;
	VRCPlayerApi localPlayer = null;
	BoxCollider noFlyZoneCollider;

	void Start()
	{
		//finds the OpenFlight script in the scene
		openFlight = GameObject.Find("OpenFlight").GetComponent<OpenFlight>();

		//finds the local player
		localPlayer = Networking.LocalPlayer;
	}

	public void OnPlayerTriggerEnter()
	{
		//turns off flight when the player enters the no fly zone
		openFlight.ForceDisableFlight();
	}

	public void OnPlayerTriggerExit()
	{
		//turns flight back on when the player leaves the no fly zone
		openFlight.ResetForcedFlight();
	}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [System.NonSerialized]
    static Color gizmoColor = new Color(1f, 0f, 0f, 0.2f);
    [System.NonSerialized]
    static float selectionGizmoAlphaAdjust = 0.2f;

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        drawColliderGizmo();
    }

    //make it slightly more opaque when selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, gizmoColor.a + selectionGizmoAlphaAdjust);
        drawColliderGizmo();
    }

    void drawColliderGizmo(){
        //handle rotation
        Quaternion rotation = transform.rotation;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, rotation, Vector3.one);

        //get the collider
        if (noFlyZoneCollider == null)
        {
            noFlyZoneCollider = GetComponent<BoxCollider>();
        }

        Gizmos.DrawCube(noFlyZoneCollider.center, noFlyZoneCollider.size);
    }
#endif
}
