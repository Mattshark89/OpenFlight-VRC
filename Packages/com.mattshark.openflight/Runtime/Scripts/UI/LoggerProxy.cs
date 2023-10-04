
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
    public class LoggerProxy : UdonSharpBehaviour
    {
        //this literally just acts as a variable proxy
        //this is needed since this gameobject will always be on, while the actual log UI might not be
        public string log = "";
        public TextMeshProUGUI text;
        void Update()
        {
            text.text = log;
        }
    }
}
