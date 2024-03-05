/**
 * @ Maintainer: Happyrobot33
 */

using TMPro;
using UdonSharp;
using UnityEngine;

namespace OpenFlightVRC.UI
{
	/// <summary>
	/// Scrolls the contributer text on the tablet. Implemented in U# for various TMPro limitation reasons
	/// </summary>
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class ContributerScroller : LoggableUdonSharpBehaviour
	{
		public GameObject T1;
		public GameObject ScrollRect;
		private float _time = 0;
		public float speed = 1;

		void Update()
		{
			T1.GetComponent<RectTransform>().anchoredPosition = new Vector2(CalculatePosition(T1.GetComponent<RectTransform>(), _time), 0);
			//scale the speed with the text length. We want it to slow down as the text gets longer so words move at the same speed
			float scaledSpeed = speed / T1.GetComponent<TextMeshProUGUI>().text.Length;

			//make sure time does not become NAN
			if (float.IsNaN(_time) || float.IsInfinity(_time))
			{
				_time = 0;
			}
			_time += Time.deltaTime * scaledSpeed;

			//wrap time so it doesnt overflow
			_time %= 1;
		}

		/// <summary>
		/// Calculates the position of a RectTransform based on a percentage of the scroll rects width
		/// </summary>
		/// <param name="t">The RectTransform to calculate the position of</param>
		/// <param name="percentage">The percentage of the scroll rects width to use</param>
		/// <returns></returns>
		private float CalculatePosition(RectTransform t, float percentage)
		{
			//calculate the position
			float position = Mathf.Lerp(t.rect.width, 0, percentage);

			//prevent NAN
			if (float.IsNaN(position))
			{
				position = 0;
			}

			return position;
		}
	}
}
