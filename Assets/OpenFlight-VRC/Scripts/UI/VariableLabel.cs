
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

public class VariableLabel : UdonSharpBehaviour
{
    public UdonBehaviour target;
    public string targetVariable;
    public string prefix = "";
    public string suffix = "";
    TextMeshProUGUI text;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        //determine if it is a number or a string
        if (target.GetProgramVariableType(targetVariable) == typeof(float) || target.GetProgramVariableType(targetVariable) == typeof(double) || target.GetProgramVariableType(targetVariable) == typeof(int))
        {
            float rounded = Mathf.Round((float)target.GetProgramVariable(targetVariable) * 100f) / 100f;
            text.text = prefix + rounded + suffix;
        }
        else
        {
            text.text = prefix + (string)target.GetProgramVariable(targetVariable) + suffix;
        }
    }
}
