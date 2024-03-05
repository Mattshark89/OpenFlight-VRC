/**
 * @ Maintainer: Mattshark89
 */

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace OpenFlightVRC
{
	/// <summary>
	/// A safer respawn that resets the players velocity on respawn to avoid flinging them around if they were flying
	/// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SaferRespawn : LoggableUdonSharpBehaviour
	{
		public override void OnPlayerRespawn(VRCPlayerApi player)
		{
			base.OnPlayerRespawn(player);
			//the whole reason this even needs to exist is really fucking cursed,
			//but VRChat doesnt reset your velocity when you respawn for whatever reason, so we do it ourselves here
			if (player.isLocal)
			{
				player.SetVelocity(Vector3.zero);
			}
		}
	}
}
