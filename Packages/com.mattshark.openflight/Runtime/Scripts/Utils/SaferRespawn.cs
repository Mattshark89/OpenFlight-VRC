using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC
{
	public class SaferRespawn : UdonSharpBehaviour
	{
		public override void OnPlayerRespawn(VRCPlayerApi player)
		{
			base.OnPlayerRespawn(player);
			if (player.isLocal)
			{
				player.SetVelocity(Vector3.zero);
			}
		}
	}
}
