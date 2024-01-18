using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ToggleWingedFlight : LoggableUdonSharpBehaviour
	{
		// Settings:
		// 0- Flight Disabled
		// 1- Flight Enabled
		// 2- Flight Automatic
		public int setting = 2;
		const float OFFTEXTUREOFFSET = 0f;
		const float AUTOTEXTUREOFFSET = 0.5f;
		const float ONTEXTUREOFFSET = 0.75f;
		public OpenFlight openFlight;
		public GameObject wingedFlight;

		void Start()
		{
			this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0, 0.75f));
		}

        public override void Interact()
		{
			setting++;
			if (setting > 2)
			{
				setting = 0;
			}

			switch (setting)
			{
				case 0:
					openFlight.FlightOff();
					this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, OFFTEXTUREOFFSET));
					break;
				case 1:
					openFlight.FlightOn();
					this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, AUTOTEXTUREOFFSET));
					break;
				case 2:
					openFlight.FlightAuto();
					this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, ONTEXTUREOFFSET));
					break;
				default:
					//default to auto if something has gone wrong here, but this should never happen
					openFlight.FlightAuto();
					this.GetComponent<MeshRenderer>().material.SetTextureOffset("_MainTex", new Vector2(0f, AUTOTEXTUREOFFSET));
					Debug.LogError("Invalid flight setting: " + setting);
					break;
			}
		}
	}
}
