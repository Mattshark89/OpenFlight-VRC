using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace OpenFlightVRC.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class UIToggle : UIBase
	{
		Toggle toggle; // The toggle component
        public bool invert = false; // If true, the toggle will be inverted
        bool value; // The value of variable

		void Start()
		{
			InitializeTargetInfo();
			toggle = GetComponent<Toggle>();
            value = (bool)target.GetProgramVariable(targetVariable) ^ invert;
			toggle.isOn = value;
		}

		//allow the toggle to be set both by the UI button and by the target variable
		void Update()
        {
            //check if the target variable has been changed
            if ((bool)target.GetProgramVariable(targetVariable) != toggle.isOn ^ invert)
			{
                toggle.isOn = (bool)target.GetProgramVariable(targetVariable) ^ invert;
			}
		}

        //called by the button
        public void Toggle()
        {
            target.SetProgramVariable(targetVariable, toggle.isOn ^ invert);
        }
	}
}
