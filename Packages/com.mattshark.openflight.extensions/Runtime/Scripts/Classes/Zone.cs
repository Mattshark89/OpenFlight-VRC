using UdonSharp;
using UnityEngine;
using UnityEditor;
using VRC.SDKBase;
using VRC.Udon;

//Gizmo drawing
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

/*
This is a base class for a zone. It is not meant to be used directly, but rather to be inherited from.
*/
namespace OpenFlightVRC.Extensions
{
	public class Zone : UdonSharpBehaviour
	{
		protected Collider zoneCollider;
		protected ZoneNotifier zoneNotifier;
		protected VRCPlayerApi localPlayer = null;

		//This is here to allow sub classes to call it in their start function, grabbing the neccesary components
		protected virtual void init()
		{
			//finds the local player
			localPlayer = Networking.LocalPlayer;

			//finds the zone notifier
			zoneNotifier = GameObject.Find("ZoneNotifier").GetComponent<ZoneNotifier>();

			//finds the collider
			zoneCollider = GetComponent<Collider>();

			//calculates the size of the collider
			calcColliderSize();
		}

		protected float colliderSizeX = 1f;
		protected float colliderSizeY = 1f;
		protected float colliderSizeZ = 1f;

		protected virtual void calcColliderSize()
		{
			//i HATE this, but since the udonsharp compiler doesnt seem to recognize the 'is' or 'as' keywords, i have to do this
			//essentially, try to find all types of colliders on this object, and if one is found, use it to calculate the size
			BoxCollider boxCollider = GetComponent<BoxCollider>();
			SphereCollider sphereCollider = GetComponent<SphereCollider>();
			CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
			MeshCollider meshCollider = GetComponent<MeshCollider>();

			if (boxCollider != null)
			{
				colliderSizeX = boxCollider.size.x * transform.lossyScale.x;
				colliderSizeY = boxCollider.size.y * transform.lossyScale.y;
				colliderSizeZ = boxCollider.size.z * transform.lossyScale.z;
			}
			else if (sphereCollider != null)
			{
				float radius = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
				colliderSizeX = sphereCollider.radius * radius;
				colliderSizeY = sphereCollider.radius * radius;
				colliderSizeZ = sphereCollider.radius * radius;
			}
			else if (capsuleCollider != null)
			{
				//determine the two points based on the axis selected
				int axis = capsuleCollider.direction;
				Vector3 point1 = new Vector3(0f, 0f, 0f);
				Vector3 point2 = new Vector3(0f, 0f, 0f);
				switch (axis)
				{
					case 0:
						point1 = new Vector3(capsuleCollider.height / 2f, 0f, 0f);
						point2 = new Vector3(-capsuleCollider.height / 2f, 0f, 0f);
						colliderSizeX = capsuleCollider.height;
						colliderSizeY = capsuleCollider.radius * 2f;
						colliderSizeZ = capsuleCollider.radius * 2f;
						break;
					case 1:
						point1 = new Vector3(0f, capsuleCollider.height / 2f, 0f);
						point2 = new Vector3(0f, -capsuleCollider.height / 2f, 0f);
						colliderSizeX = capsuleCollider.radius * 2f;
						colliderSizeY = capsuleCollider.height;
						colliderSizeZ = capsuleCollider.radius * 2f;
						break;
					case 2:
						point1 = new Vector3(0f, 0f, capsuleCollider.height / 2f);
						point2 = new Vector3(0f, 0f, -capsuleCollider.height / 2f);
						colliderSizeX = capsuleCollider.radius * 2f;
						colliderSizeY = capsuleCollider.radius * 2f;
						colliderSizeZ = capsuleCollider.height;
						break;
				}
			}
			else if (meshCollider != null)
			{
				//get the bounds of the mesh
				Bounds bounds = meshCollider.bounds;
				colliderSizeX = bounds.size.x;
				colliderSizeY = bounds.size.y;
				colliderSizeZ = bounds.size.z;
			}
			else
			{
				//if no collider is found, use the default values
				colliderSizeX = 1f;
				colliderSizeY = 1f;
				colliderSizeZ = 1f;
			}
		}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
	//[System.NonSerialized]
	static float selectionGizmoAlphaAdjust = 0.2f;
    void OnDrawGizmos()
    {
        if(EditorPrefs.GetBool("OpenFlightShowZoneGizmos"))
        {
            Gizmos.color = GetGizmoColorWithAlpha();
            drawColliderGizmo();
        }
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

    protected virtual void drawColliderGizmo(){
        //handle rotation
        Quaternion rotation = transform.rotation;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, rotation, Vector3.one);

        handleColliderType();
    }


    protected virtual void handleColliderType()
    {
        //get the collider
        if (zoneCollider == null)
        {
            zoneCollider = GetComponent<Collider>();
			//if a collider still isnt found, add a box collider
			if(zoneCollider == null)
			{
				zoneCollider = gameObject.AddComponent<BoxCollider>();
				Debug.LogWarning("No collider found on " + gameObject.name + ", adding a box collider");
			}
        }

        calcColliderSize();
        
        //handle the collider type
        if (zoneCollider is BoxCollider)
        {
            BoxCollider boxCollider = zoneCollider as BoxCollider;

			if (!boxCollider.isTrigger)
			{
				boxCollider.isTrigger = true;
				Debug.LogWarning("BoxCollider on " + gameObject.name + " was not set to trigger. Automatically fixed");
			}

            Gizmos.DrawCube(boxCollider.center, Vector3.Scale(boxCollider.size, transform.lossyScale));
        }
        else if (zoneCollider is SphereCollider)
        {
            SphereCollider sphereCollider = zoneCollider as SphereCollider;

			if (!sphereCollider.isTrigger)
			{
				sphereCollider.isTrigger = true;
				Debug.LogWarning("SphereCollider on " + gameObject.name + " was not set to trigger. Automatically fixed");
			}

			//collider radius is whatever the largest scale is
			float radius = Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

            Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius * radius);
        }
        else if (zoneCollider is CapsuleCollider)
        {
            CapsuleCollider capsuleCollider = zoneCollider as CapsuleCollider;

			if (!capsuleCollider.isTrigger)
			{
				capsuleCollider.isTrigger = true;
				Debug.LogWarning("CapsuleCollider on " + gameObject.name + " was not set to trigger. Automatically fixed");
			}

            DrawCapsule.ForGizmo(capsuleCollider.center, capsuleCollider.height, capsuleCollider.radius, capsuleCollider.direction);
        }
        else if (zoneCollider is MeshCollider)
        {
			MeshCollider meshCollider = zoneCollider as MeshCollider;

			//make sure the collider is set to convex and trigger
			if (!meshCollider.convex || !meshCollider.isTrigger)
			{
				meshCollider.convex = true;
				meshCollider.isTrigger = true;
				Debug.LogWarning("MeshCollider on " + gameObject.name + " was not set to convex and trigger. Automatically fixed.");
			}

			//use the bounds and place points in a grid on all 6 faces of the bounds
			Bounds bounds = meshCollider.bounds;
			//get the distance between two opposite corners
			float distance = Vector3.Distance(bounds.min, bounds.max);
			//distribute points right outside of the bounds
			int numPoints = 1000;
			//static random seed based on the object's name
			Random.InitState(gameObject.name.GetHashCode());
			for (int i = 0; i < numPoints; i++)
			{
				//create points randomly on a sphere that is slightly larger than the bounds
				Vector3 point = Random.onUnitSphere * (distance / 2);
				point = Vector3.Scale(point, bounds.size.normalized);
				point += bounds.center;
				//raycast to the bound center
				RaycastHit hit;
				if (meshCollider.Raycast(new Ray(point, bounds.center - point), out hit, distance))
				{
					point = hit.point;
				}
				//offset points by our transform
				point = transform.InverseTransformPoint(point);
				point = Vector3.Scale(point, transform.lossyScale);
				Gizmos.DrawSphere(point, 0.1f);
			}
        }
    }
#endif
	}
}
