
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace OpenFlightVRC.UI
{
    public class ContributerScroller : UdonSharpBehaviour
    {
        public GameObject T1;
        public GameObject ScrollRect;
        private float time = 0;
        public float speed = 1;

        void Update()
        {
            T1.GetComponent<RectTransform>().anchoredPosition = new Vector2(CalculatePosition(T1.GetComponent<RectTransform>(), time), 0);
            //scale the speed with the text length. We want it to slow down as the text gets longer so words move at the same speed
            float scaledSpeed = speed / T1.GetComponent<TextMeshProUGUI>().text.Length;

            //make sure time does not become NAN
            if (float.IsNaN(time) || float.IsInfinity(time))
            {
                time = 0;
            }
            time += Time.deltaTime * scaledSpeed;
        }

        /// <summary>
        /// Calculates the position of a RectTransform based on a percentage of the scroll rects width
        /// </summary>
        /// <param name="t">The RectTransform to calculate the position of</param>
        /// <param name="percentage">The percentage of the scroll rects width to use</param>
        /// <returns></returns>
        private float CalculatePosition(RectTransform t, float percentage)
        {
            //make sure it wraps around if it goes too far
            percentage = percentage % 1;

            //calculate the position
            float position = Mathf.Lerp(-t.rect.width, 0, percentage);

            //prevent NAN
            if (float.IsNaN(position))
            {
                position = 0;
            }

            return position;
        }
    }
}
