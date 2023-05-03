using UnityEngine;
using PrimitiveGenerator;

namespace OpenFlightVRC.Extensions
{
	public static class DrawCapsule
	{
		public static void ForGizmo(Vector3 pos, float height, float radius, int direction)
		{
			ForGizmo(pos, height, radius, direction, Gizmos.color);
		}

		public static void ForGizmo(Vector3 pos, float height, float radius, int direction, Color color)
		{
			//account for the fact that the height is used differently in our mesh builder
			//this means we need to remove the radius from the height
			height -= radius * 2;

			Gizmos.color = color;
			Mesh capsule = CapsuleBuilder.Build(height, radius, 10, 12, 3);
			Quaternion rotation = Quaternion.identity;
			//0 is x
			//1 is y
			//2 is z
			switch (direction)
			{
				case 0:
					rotation = Quaternion.Euler(0, 0, 90);
					break;
				case 1:
					rotation = Quaternion.Euler(0, 0, 0);
					break;
				case 2:
					rotation = Quaternion.Euler(90, 0, 0);
					break;
			}
			Gizmos.DrawMesh(capsule, pos, rotation);
		}
	}
}
