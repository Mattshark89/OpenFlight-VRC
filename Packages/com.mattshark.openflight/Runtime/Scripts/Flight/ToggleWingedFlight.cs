/**
 * @ Maintainer: Mattshark89
 */

using UdonSharp;
using UnityEngine;

namespace OpenFlightVRC
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class ToggleWingedFlight : LoggableUdonSharpBehaviour
	{
		const float OFFTEXTUREOFFSET = 0f;
		const float AUTOTEXTUREOFFSET = 0.5f;
		const float ONTEXTUREOFFSET = 0.75f;
        private const string ShaderTextureProperty = "_MainTex";
        public OpenFlight openFlight;

		void Start()
		{
			UpdateMaterial();
            SendCustomEventDelayedSeconds(nameof(DelayedStart), 0.5f);
		}

        public void DelayedStart()
        {
            UpdateMaterial();
        }

		private void UpdateMaterial()
        {
            switch (openFlight.flightMode)
			{
				case FlightMode.Off:
					GetComponent<MeshRenderer>().material.SetTextureOffset(ShaderTextureProperty, new Vector2(0f, OFFTEXTUREOFFSET));
                    InteractionText = "Flight: Off. Click to turn on.";
					break;
				case FlightMode.On:
					GetComponent<MeshRenderer>().material.SetTextureOffset(ShaderTextureProperty, new Vector2(0f, AUTOTEXTUREOFFSET));
                    InteractionText = "Flight: On. Click to set to Automatic.";
					break;
				case FlightMode.Auto:
					GetComponent<MeshRenderer>().material.SetTextureOffset(ShaderTextureProperty, new Vector2(0f, ONTEXTUREOFFSET));
                    InteractionText = "Flight: Automatic. Click to turn off.";
					break;
				default:
					GetComponent<MeshRenderer>().material.SetTextureOffset(ShaderTextureProperty, new Vector2(0f, AUTOTEXTUREOFFSET));
					break;
			}
        }

        private void CycleFlightMode()
        {
            int currentMode = (int)openFlight.flightMode;
            int nextMode = (currentMode + 1) % 3; // Cycle through
            openFlight.flightMode = (FlightMode)nextMode;
        }

		public override void Interact()
		{
            CycleFlightMode();
			UpdateMaterial();
		}
	}
}
