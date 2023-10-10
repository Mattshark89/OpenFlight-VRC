using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace OpenFlightVRC.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class UISliderUtility : UdonSharpBehaviour
	{
		public UdonBehaviour target;
		public string targetVariable;
		Slider slider;
        System.Type targetType;

		void Start()
		{
			//get the real target from the proxy, if it exists
			if (target.GetProgramVariable("target") != null)
				target = (UdonBehaviour)target.GetProgramVariable("target");

			slider = GetComponent<Slider>();
            //determine if the target variable is a float or an double
            targetType = target.GetProgramVariableType(targetVariable);
		}

		void Update()
		{
            var value = slider.value;

            switch (targetType)
			{
                case System.Type floatType when floatType == typeof(float):
                    if ((float)target.GetProgramVariable(targetVariable) != value)
                    {
                        slider.value = (float)target.GetProgramVariable(targetVariable);
                    }
                    break;
                case System.Type intType when intType == typeof(int):
                    if ((int)target.GetProgramVariable(targetVariable) != value)
                    {
                        slider.value = (int)target.GetProgramVariable(targetVariable);
                    }
                    break;
                case System.Type doubleType when doubleType == typeof(double):
                    if ((double)target.GetProgramVariable(targetVariable) != value)
                    {
                        slider.value = (float)(double)target.GetProgramVariable(targetVariable);
                    }
                    break;
                default:
                    if ((int)target.GetProgramVariable(targetVariable) != value)
                    {
                        slider.value = (int)target.GetProgramVariable(targetVariable);
                    }
                    break;
            }
        }

        public void Changed()
        {
            var value = slider.value;
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
		}
	}
}
