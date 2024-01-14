using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace OpenFlightVRC.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class VariableLabel : UIBase
	{
		public int decimalPlaces = 2;
		public string prefix = "";
		public string suffix = "";
		TextMeshProUGUI text;
		bool isStringType = false;
		bool isBoolType = false;

		void Start()
		{
			InitializeTargetInfo();
			text = GetComponent<TextMeshProUGUI>();
			isStringType = targetType == typeof(string);
			isBoolType = targetType == typeof(bool);
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
