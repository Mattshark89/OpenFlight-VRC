/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;
using TMPro;

namespace OpenFlightVRC.UI
{
	/// <summary>
	/// A utility for a dynamic label to get and values from a UdonBehaviour variable
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class VariableLabel : UIBase
	{
		public int decimalPlaces = 2;
		public string prefix = "";
		public string suffix = "";
		TextMeshProUGUI text;
		private bool _isStringType = false;
		private bool _isBoolType = false;

		void Start()
		{
			InitializeTargetInfo();
			text = GetComponent<TextMeshProUGUI>();
			_isStringType = targetType == typeof(string);
			_isBoolType = targetType == typeof(bool);
		}

		void Update()
		{
			var targetValue = target.GetProgramVariable(targetVariable);
			//determine if it is a bool
			if (_isBoolType)
			{
				if ((bool)targetValue)
				{
					text.text = prefix + "True" + suffix;
				}
				else
				{
					text.text = prefix + "False" + suffix;
				}
			}
			else if (!_isStringType)
			{
				float roundingModifier = Mathf.Pow(10, decimalPlaces);
				float rounded = Mathf.Round((float)targetValue * roundingModifier) / roundingModifier;
				text.text = prefix + rounded.ToString() + suffix;
			}
			else
			{
				text.text = prefix + targetValue.ToString() + suffix;
			}
		}
	}
}
