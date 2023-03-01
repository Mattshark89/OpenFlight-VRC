
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
    bool isStringType = false;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        //determine if the target variable is a string
        if (target.GetProgramVariableType(targetVariable) == typeof(string))
        {
            isStringType = true;
        }
    }

    void Update()
    {
        var targetValue = target.GetProgramVariable(targetVariable);
        //determine if it is a number or a string
        if (!isStringType)
        {
            float rounded = Mathf.Round((float)targetValue * 100f) / 100f;
            text.text = prefix + rounded.ToString() + suffix;
        }
        else
        {
            text.text = prefix + targetValue.ToString() + suffix;
        }
    }
}
