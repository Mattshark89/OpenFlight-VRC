/**
 * @ Maintainer: Happyrobot33
 */

using UdonSharp;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

namespace OpenFlightVRC.UI
{
	/// <summary>
	/// A utility for the slider to get and set values from a UdonBehaviour variable
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class UISliderUtility : UIBase
	{
		private Slider _slider;
		private InputField _inputField;

		void Start()
		{
			InitializeTargetInfo();
			_slider = GetComponent<Slider>();
			_inputField = GetComponentInChildren<InputField>();
		}

		void Update()
		{
			var value = _slider.value;

			switch (targetType)
			{
				case System.Type floatType when floatType == typeof(float):
					if ((float)target.GetProgramVariable(targetVariable) != value)
					{
						_slider.value = (float)target.GetProgramVariable(targetVariable);
					}
					break;
				case System.Type intType when intType == typeof(int):
					if ((int)target.GetProgramVariable(targetVariable) != value)
					{
						_slider.value = (int)target.GetProgramVariable(targetVariable);
					}
					break;
				case System.Type doubleType when doubleType == typeof(double):
					if ((double)target.GetProgramVariable(targetVariable) != value)
					{
						_slider.value = (float)(double)target.GetProgramVariable(targetVariable);
					}
					break;
				default:
					if ((int)target.GetProgramVariable(targetVariable) != value)
					{
						_slider.value = (int)target.GetProgramVariable(targetVariable);
					}
					break;
			}
		}

		public void Changed()
		{
			var value = _slider.value;
			switch (targetType.ToString())
			{
				case "System.Single":
					target.SetProgramVariable(targetVariable, (float)value);
					break;
				case "System.Int32":
					target.SetProgramVariable(targetVariable, (int)value);
					break;
				case "System.Double":
					target.SetProgramVariable(targetVariable, (double)value);
					break;
				default:
					Logger.Log("Unknown type: " + targetType.ToString(), this);
					break;
			}

			// Update the input field. This is done as the slider will automatically bound the value to the min and max values
			_inputField.text = value.ToString();
		}

		public void PreciseInputChanged()
		{
			if (float.TryParse(_inputField.text, out float value))
			{
				_slider.value = value;
				Changed();
			}
		}
	}
}
