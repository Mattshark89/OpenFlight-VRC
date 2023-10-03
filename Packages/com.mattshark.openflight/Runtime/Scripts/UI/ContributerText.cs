
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
    public class ContributerText : UdonSharpBehaviour
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
                //set the text to rainbow
                text.color = TimedRainbow(1);
            }
            else
            {
                //turn the TMP off
                text.enabled = false;
            }
        }

        private Color TimedRainbow(float speed)
        {
            float t = Time.time * speed;
            return new Color(Mathf.Sin(t) * 0.5f + 0.5f,
                             Mathf.Sin(t + 2 * Mathf.PI / 3) * 0.5f + 0.5f,
                             Mathf.Sin(t + 4 * Mathf.PI / 3) * 0.5f + 0.5f);
        }
    }
}
