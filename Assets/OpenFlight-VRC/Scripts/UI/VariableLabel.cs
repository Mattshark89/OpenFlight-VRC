
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
        //round the variable to 2 decimal places
        float rounded = Mathf.Round((float)target.GetProgramVariable(targetVariable) * 100f) / 100f;
        text.text = prefix + rounded + suffix;
    }
}
