
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
    bool isBoolType = false;

    void Start()
    {
        //get the real target from the proxy, if it exists
        if (target.GetProgramVariable("target") != null)
            target = (UdonBehaviour)target.GetProgramVariable("target");

        text = GetComponent<TextMeshProUGUI>();
        //determine if the target variable is a string
        if (target.GetProgramVariableType(targetVariable) == typeof(string))
        {
            isStringType = true;
        }
        //determine if the target variable is a bool
        if (target.GetProgramVariableType(targetVariable) == typeof(bool))
        {
            isBoolType = true;
        }
    }

    void Update()
    {
        var targetValue = target.GetProgramVariable(targetVariable);
        //determine if it is a bool
        if (isBoolType)
        {
            if ((bool)targetValue)
            {
                text.text = prefix + "True" + suffix;
            }
            else
            {
                text.text = prefix + "False" + suffix;
            }
        }
        else if (!isStringType)
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
