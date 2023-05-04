using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//TODO: Tie in the air friction, weight and wingspan values into this somehow
namespace OpenFlightVRC.Extensions
{
	public class WindEffectZone : DirectionalZone
	{
		[Tooltip("This is the speed of wind a player will experience. Players velocity will be altered by how much of their wing area is exposed to the wind")]
		public float windSpeed = 10f;
		public OpenFlight openFlight;
		WingFlightPlusGlide wingFlightPlusGlide;

		void Start()
		{
			wingFlightPlusGlide = openFlight.wingedFlight.GetComponent<WingFlightPlusGlide>();
			init();
		}

		public void OnPlayerTriggerEnter()
		{
			wingFlightPlusGlide.SetProgramVariable("windVector", Vector3.Normalize(transform.TransformDirection(getDirectionVector())) * windSpeed);
			wingFlightPlusGlide.SetProgramVariable("windy", true);
			Debug.Log("Entered Wind Zone");
		}

		public void OnPlayerTriggerExit()
		{
			wingFlightPlusGlide.SetProgramVariable("windy", false);
		}

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    protected override Color GetGizmoColor()
    {
        return Color.cyan;
    }
#endif
	}
}
