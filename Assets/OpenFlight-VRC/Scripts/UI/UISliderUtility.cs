
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;


public class UISliderUtility : UdonSharpBehaviour
{
    public UdonBehaviour target;
    public string targetVariable;
    Slider slider;
    float value;
    float previousValue;
    bool TargetIsFloat = false;
    bool TargetIsInt = false;
    void Start()
    {
        slider = GetComponent<Slider>();
        //determine if the target variable is a float or an double
        if (target.GetProgramVariableType(targetVariable) == typeof(float))
        {
            TargetIsFloat = true;
        }
        else if (target.GetProgramVariableType(targetVariable) == typeof(int))
        {
            TargetIsFloat = false;
            TargetIsInt = true;
        }
        else if (target.GetProgramVariableType(targetVariable) == typeof(double))
        {
            TargetIsFloat = false;
        }
        value = slider.value;
        previousValue = value;
    }

    void Update()
    {
        value = slider.value;

        //check if the slider has been changed
        if (value != previousValue)
        {
            //if the target variable is a float, then set the target variable to the slider value
            if (TargetIsFloat)
            {
                target.SetProgramVariable(targetVariable, value);
            }
            else if (TargetIsInt)
            {
                target.SetProgramVariable(targetVariable, (int)value);
            }
            else
            {
                target.SetProgramVariable(targetVariable, (double)value);
            }
            previousValue = value;
        }

        //check if the target variable has been changed
        if (TargetIsFloat)
        {
            if ((float)target.GetProgramVariable(targetVariable) != value)
            {
                slider.value = (float)target.GetProgramVariable(targetVariable);
            }
        }
        else if (TargetIsInt)
        {
            if ((int)target.GetProgramVariable(targetVariable) != value)
            {
                slider.value = (int)target.GetProgramVariable(targetVariable);
            }
        }
        else
        {
            if ((float)target.GetProgramVariable(targetVariable) != value)
            {
                slider.value = (float)target.GetProgramVariable(targetVariable);
            }
        }
    }
}
