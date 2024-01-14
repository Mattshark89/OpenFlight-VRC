
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    /// <summary>
    /// The base class for all UI elements that need proxy interactions
    /// </summary>
    public class UIBase : LoggableUdonSharpBehaviour
    {
        /// <summary>
        /// The target UdonBehaviour
        /// </summary>
        public UdonBehaviour target;

        /// <summary>
        /// The target variable name
        /// </summary>
        public string targetVariable;
        /// <summary>
        /// The target type
        /// </summary>
        public System.Type targetType;

        //resolve the proxy
        internal void InitializeTargetInfo()
        {
            if (target.GetProgramVariable("target") != null)
				target = (UdonBehaviour)target.GetProgramVariable("target");
            
            targetType = target.GetProgramVariableType(targetVariable);
        }
    }
}
