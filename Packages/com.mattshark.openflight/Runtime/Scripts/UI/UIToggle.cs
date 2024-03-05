/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine.UI;

namespace OpenFlightVRC.UI
{
	/// <summary>
	/// A utility for the toggle to get and set values from a UdonBehaviour variable
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class UIToggle : UIBase
	{
		private Toggle _toggle; // The toggle component
		public bool invert = false; // If true, the toggle will be inverted
		private bool _value; // The value of variable

		void Start()
		{
			InitializeTargetInfo();
			_toggle = GetComponent<Toggle>();
			_value = (bool)target.GetProgramVariable(targetVariable) ^ invert;
			_toggle.isOn = _value;
		}

		//allow the toggle to be set both by the UI button and by the target variable
		void Update()
		{
			//check if the target variable has been changed
			if ((bool)target.GetProgramVariable(targetVariable) != _toggle.isOn ^ invert)
			{
				_toggle.isOn = (bool)target.GetProgramVariable(targetVariable) ^ invert;
			}
		}

		//called by the button
		public void Toggle()
		{
			target.SetProgramVariable(targetVariable, _toggle.isOn ^ invert);
		}
	}
}
