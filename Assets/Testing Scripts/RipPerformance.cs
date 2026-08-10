
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class RipPerformance : UdonSharpBehaviour
{
    [Tooltip("Make this 50000 for terrible performance")]
    //public float reduction = 0;
    private int number = 0;
    public Slider inputSlider;
    public Text textOutput;
    public void Update()
    {
        textOutput.text = inputSlider.value.ToString();
        for (int i = 0; i < inputSlider.value; i++)
        {
            number++;
        }
    }
}
