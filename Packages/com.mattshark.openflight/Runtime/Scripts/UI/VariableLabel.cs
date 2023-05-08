using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace OpenFlightVRC.UI
{
	public class VariableLabel : UdonSharpBehaviour
	{
		public UdonBehaviour target;
		public int decimalPlaces = 2;
		public string targetVariable;
		public string prefix = "";
		public string suffix = "";
		TextMeshProUGUI text;
		bool isStringType = false;
		bool isBoolType = false;

		void Start()
		{
			//get the real target from the proxy, if it exists
			if (target.GetProgramVariable("target") != null)
				target = (UdonBehaviour)target.GetProgramVariable("target");

			text = GetComponent<TextMeshProUGUI>();
			//determine if the target variable is a string
			var targetType = target.GetProgramVariableType(targetVariable);
			if (targetType == typeof(string))
			{
				isStringType = true;
			}
			//determine if the target variable is a bool
			if (targetType == typeof(bool))
			{
				isBoolType = true;
			}
		}

		void Update()
		{
			var targetValue = target.GetProgramVariable(targetVariable);
			//determine if it is a bool
			if (isBoolType)
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
			else if (!isStringType)
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
