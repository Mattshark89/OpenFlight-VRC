using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//the point of this class is to extend the zone to be directional

public class DirectionalZone : Zone
{
#if !COMPILER_UDONSHARP && UNITY_EDITOR
    protected override void drawColliderGizmo()
    {
        base.drawColliderGizmo();
        
        //determine how many arrows we can fit along the x axis based on the size of the collider
        int arrowsX = Mathf.FloorToInt(zoneCollider.size.x);
        arrowsX = Mathf.Max(arrowsX, 1); //make sure we have at least 1 arrow on the x axis
        //determine how many arrows we can fit along the y axis based on the size of the collider
        int arrowsY = Mathf.FloorToInt(zoneCollider.size.y);
        arrowsY = Mathf.Max(arrowsY, 1); //make sure we have at least 1 arrow on the y axis

        //place an arrow at each point
        for (int x = 0; x < arrowsX + 1; x++)
        {
            for (int y = 0; y < arrowsY + 1; y++)
            {
                //get the position of the arrow
                Vector3 arrowPos = zoneCollider.center + new Vector3(zoneCollider.size.x / arrowsX * x, zoneCollider.size.y / arrowsY * y, zoneCollider.size.z / 2);
                //shift the arrow pos so they get placed properly
                arrowPos -= new Vector3(zoneCollider.size.x / 2, zoneCollider.size.y / 2, 0);
                //draw the arrow
                DrawArrow.ForGizmo(arrowPos, Vector3.forward);
            }
        }
        //DrawArrow.ForGizmo(zoneCollider.center + new Vector3(0, 0, zoneCollider.size.z / 2), Vector3.forward, color: Color.white);
    }
#endif
}
