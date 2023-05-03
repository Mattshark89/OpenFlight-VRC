using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//the point of this class is to extend the zone to be directional
namespace OpenFlightVRC.Extensions
{
	public enum Direction
	{
		X,
		Y,
		Z
	}

	public class DirectionalZone : Zone
	{
		protected float _colliderSizeX = 1f;
		protected float _colliderSizeY = 1f;
		protected float _colliderSizeZ = 1f;

		public Direction direction = Direction.Z;

		public bool flipDirection = false;

		protected override void init()
		{
			base.init();
			calcDirectionAlignedSizes();
		}

		protected override void calcColliderSize()
		{
			base.calcColliderSize();
			_colliderSizeX = colliderSizeX;
			_colliderSizeY = colliderSizeY;
			_colliderSizeZ = colliderSizeZ;
		}

		protected float colliderWidth = 0;
		protected float colliderHeight = 0;
		protected float colliderDepth = 0;
		protected Vector3 colliderCenter = Vector3.zero;
		protected Vector3 directionVector = Vector3.forward;

		protected Vector3 getDirectionVector()
		{
			calcDirectionAlignedSizes();
			return directionVector;
		}

		//these are collider sizes that are aligned with the direction of the zone
		protected void calcDirectionAlignedSizes()
		{
			calcColliderSize();
			switch (direction)
			{
				case Direction.X:
					colliderWidth = colliderSizeZ;
					colliderHeight = colliderSizeY;
					colliderDepth = colliderSizeX;
					break;
				case Direction.Y:
					colliderWidth = colliderSizeX;
					colliderHeight = colliderSizeZ;
					colliderDepth = colliderSizeY;
					break;
				case Direction.Z:
					colliderWidth = colliderSizeX;
					colliderHeight = colliderSizeY;
					colliderDepth = colliderSizeZ;
					break;
			}

			//direction vector
			switch (direction)
			{
				case Direction.X:
					directionVector = Vector3.right;
					break;
				case Direction.Y:
					directionVector = Vector3.up;
					break;
				case Direction.Z:
					directionVector = Vector3.forward;
					break;
			}

			//handle direction flipping
			if (flipDirection)
			{
				colliderDepth *= -1;
				directionVector *= -1;
			}

			//collider center is the center of the bounds
			colliderCenter = transform.InverseTransformPoint(zoneCollider.bounds.center);
			colliderCenter = Vector3.Scale(colliderCenter, transform.lossyScale);
		}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    protected override void drawColliderGizmo()
    {
        base.drawColliderGizmo();
        calcDirectionAlignedSizes();

        //determine how many arrows we can fit along the x axis based on the size of the collider
        int arrowsX = Mathf.FloorToInt(colliderWidth);
        arrowsX = Mathf.Max(arrowsX, 1); //make sure we have at least 1 arrow on the x axis
        //determine how many arrows we can fit along the y axis based on the size of the collider
        int arrowsY = Mathf.FloorToInt(colliderHeight);
        arrowsY = Mathf.Max(arrowsY, 1); //make sure we have at least 1 arrow on the y axis

        //place an arrow at each point
        for (int x = 0; x < arrowsX + 1; x++)
        {
            for (int y = 0; y < arrowsY + 1; y++)
            {
                //get the position of the arrow
                //Vector3 arrowPos = new Vector3(colliderWidth / arrowsX * x, colliderHeight / arrowsY * y, colliderDepth / 2);
                Vector3 arrowPos = Vector3.zero;
                switch (direction)
                {
                    case Direction.X:
                        arrowPos += new Vector3(colliderDepth / 2, colliderHeight / arrowsY * y, colliderWidth / arrowsX * x);
                        arrowPos -= new Vector3(0, colliderHeight / 2, colliderWidth / 2);
                        break;
                    case Direction.Y:
                        arrowPos += new Vector3(colliderWidth / arrowsX * x, colliderDepth / 2, colliderHeight / arrowsY * y);
                        arrowPos -= new Vector3(colliderWidth / 2, 0, colliderHeight / 2);
                        break;
                    case Direction.Z:
                        arrowPos += new Vector3(colliderWidth / arrowsX * x, colliderHeight / arrowsY * y, colliderDepth / 2);
                        arrowPos -= new Vector3(colliderWidth / 2, colliderHeight / 2, 0);
                        break;
                }

				//arrowPos = transform.TransformPoint(arrowPos);
				arrowPos += colliderCenter;
				arrowPos = Vector3.Scale(arrowPos, new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1 / transform.lossyScale.z));

				arrowPos = transform.TransformPoint(arrowPos);


                //for visual sake, make the arrows base start on the collider properly
                arrowPos = zoneCollider.ClosestPoint(arrowPos);

				//transform the arrow position to local space
				arrowPos = transform.InverseTransformPoint(arrowPos);
				arrowPos = Vector3.Scale(arrowPos, transform.lossyScale);
				//arrowPos -= transform.position;
				//arrowPos = Quaternion.Inverse(transform.rotation) * arrowPos;

                //draw the arrow
                DrawArrow.ForGizmo(arrowPos, directionVector);
            }
        }
        //DrawArrow.ForGizmo(zoneCollider.center + new Vector3(0, 0, zoneCollider.size.z / 2), Vector3.forward, color: Color.white);
    }
#endif
	}
}
