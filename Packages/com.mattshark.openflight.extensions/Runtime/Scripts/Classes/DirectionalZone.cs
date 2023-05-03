using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//the point of this class is to extend the zone to be directional

public class DirectionalZone : Zone
{
    protected float _colliderSizeX = 1f;
    protected float _colliderSizeY = 1f;
    protected float _colliderSizeZ = 1f;

#if !COMPILER_UDONSHARP && UNITY_EDITOR

    protected override void handleColliderType()
    {
        base.handleColliderType();
        _colliderSizeX = colliderSizeX;
        _colliderSizeY = colliderSizeY;
        _colliderSizeZ = colliderSizeZ;
    }

    protected override void drawColliderGizmo()
    {
        base.drawColliderGizmo();

        //determine how many arrows we can fit along the x axis based on the size of the collider
        int arrowsX = Mathf.FloorToInt(colliderSizeX);
        arrowsX = Mathf.Max(arrowsX, 1); //make sure we have at least 1 arrow on the x axis
        //determine how many arrows we can fit along the y axis based on the size of the collider
        int arrowsY = Mathf.FloorToInt(colliderSizeY);
        arrowsY = Mathf.Max(arrowsY, 1); //make sure we have at least 1 arrow on the y axis

        //place an arrow at each point
        for (int x = 0; x < arrowsX + 1; x++)
        {
            for (int y = 0; y < arrowsY + 1; y++)
            {
                //get the position of the arrow
                Vector3 arrowPos = new Vector3(colliderSizeX / arrowsX * x, colliderSizeY / arrowsY * y, colliderSizeZ / 2);
                //shift the arrow pos so they get placed properly
                arrowPos -= new Vector3(colliderSizeX / 2, colliderSizeY / 2, 0);
                //draw the arrow
                DrawArrow.ForGizmo(arrowPos, Vector3.forward);
            }
        }
        //DrawArrow.ForGizmo(zoneCollider.center + new Vector3(0, 0, zoneCollider.size.z / 2), Vector3.forward, color: Color.white);
    }
#endif
}
