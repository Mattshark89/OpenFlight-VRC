/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
	/// <summary>
	/// A dynamic input field that gets its value from a UdonBehaviour variable, or a proxy
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class DynamicInputField : LoggableUdonSharpBehaviour
	{
		/// <summary>
		/// The target UdonBehaviour to get the variable from
		/// </summary>
		public UdonBehaviour target;
		/// <summary>
		/// The ammount of decimal places to round the number to
		/// </summary>
		public int decimalPlaces = 2;
		/// <summary>
		/// The name of the variable to get from the target
		/// </summary>
		public string targetVariable;
		/// <summary>
		/// The prefix to add to the variable
		/// </summary>
		public string prefix = "";
		/// <summary>
		/// The suffix to add to the variable
		/// </summary>
		public string suffix = "";
		private InputField field;
		private bool _isStringType = false;
		private bool _isBoolType = false;

		void Start()
		{
			//get the real target from the proxy, if it exists
			if (target.GetProgramVariable("target") != null)
				target = (UdonBehaviour)target.GetProgramVariable("target");

			//text = GetComponent<TextMeshProUGUI>();
			field = GetComponent<InputField>();
			//determine if the target variable is a string
			var targetType = target.GetProgramVariableType(targetVariable);
			if (targetType == typeof(string))
			{
				_isStringType = true;
			}
			//determine if the target variable is a bool
			if (targetType == typeof(bool))
			{
				_isBoolType = true;
			}
		}

		void Update()
		{
			var targetValue = target.GetProgramVariable(targetVariable);
			//determine if it is a bool
			if (_isBoolType)
			{
				if ((bool)targetValue)
				{
					field.text = prefix + "True" + suffix;
				}
				else
				{
					field.text = prefix + "False" + suffix;
				}
			}
			else if (!_isStringType)
			{
				float roundingModifier = Mathf.Pow(10, decimalPlaces);
				float rounded = Mathf.Round((float)targetValue * roundingModifier) / roundingModifier;
				field.text = prefix + rounded.ToString() + suffix;
			}
			else
			{
				field.text = prefix + targetValue.ToString() + suffix;
			}
		}
	}
}
