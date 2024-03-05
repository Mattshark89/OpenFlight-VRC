/**
 * @ Maintainer: Happyrobot33
 */

using TMPro;
using UdonSharp;

namespace OpenFlightVRC.UI
{
	/// <summary>
	/// A endpoint for the loggable udon behaviour to send logs to. see <see cref="LoggableUdonSharpBehaviour"/>
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class LoggerProxy : LoggableUdonSharpBehaviour
	{
		//this literally just acts as a variable proxy
		//this is needed since this gameobject will always be on, while the actual log UI might not be
		public string log = "";
		public TextMeshProUGUI text;

		void Update()
		{
			text.text = log;
		}
	}
}
