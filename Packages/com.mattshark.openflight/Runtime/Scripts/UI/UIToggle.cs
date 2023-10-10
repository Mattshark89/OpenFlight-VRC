using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace OpenFlightVRC.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class UIToggle : UdonSharpBehaviour
	{
		Toggle toggle; // The toggle component
		public UdonBehaviour target; // The target UdonBehaviour
		public string targetVariable; // The target variable
        public bool invert = false; // If true, the toggle will be inverted
        bool value; // The value of variable

		void Start()
		{
			//get the real target from the proxy, if it exists
			if (target.GetProgramVariable("target") != null)
				target = (UdonBehaviour)target.GetProgramVariable("target");

			toggle = GetComponent<Toggle>();
            value = (bool)target.GetProgramVariable(targetVariable) ^ invert;
			toggle.isOn = value;
		}

		//allow the toggle to be set both by the UI button and by the target variable
		void Update()
        {
			//check if the target variable has been changed
			if ((bool)target.GetProgramVariable(targetVariable) != toggle.isOn)
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
