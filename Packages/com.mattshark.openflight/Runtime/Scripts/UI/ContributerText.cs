
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ContributerText : LoggableUdonSharpBehaviour
    {
        public UdonBehaviour ContributerDetection;
        private TextMeshProUGUI text;
        void Start()
        {
            //get the real target from the proxy, if it exists
            if (ContributerDetection.GetProgramVariable("target") != null)
                ContributerDetection = (UdonBehaviour)ContributerDetection.GetProgramVariable("target");

            text = GetComponent<TextMeshProUGUI>();
        }

        void Update()
        {
            if ((bool)ContributerDetection.GetProgramVariable("contributerInWorld"))
            {
                //turn the TMP on
                text.enabled = true;
            }
            else
            {
                //turn the TMP off
                text.enabled = false;
            }
        }
    }
}
