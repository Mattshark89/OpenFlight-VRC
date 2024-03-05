/**
 * @ Maintainer: Happyrobot33
 */

using TMPro;
using UdonSharp;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
	/// <summary>
	/// Handles the contributer in world text on the tablet
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class ContributerText : LoggableUdonSharpBehaviour
	{
		public UdonBehaviour ContributerDetection;
		private TextMeshProUGUI _text;

		void Start()
		{
			//get the real target from the proxy, if it exists
			if (ContributerDetection.GetProgramVariable("target") != null)
				ContributerDetection = (UdonBehaviour)ContributerDetection.GetProgramVariable("target");

			_text = GetComponent<TextMeshProUGUI>();
		}

		void Update()
		{
			if ((bool)ContributerDetection.GetProgramVariable("contributerInWorld"))
			{
				//turn the TMP on
				_text.enabled = true;
			}
			else
			{
				//turn the TMP off
				_text.enabled = false;
			}
		}
	}
}
