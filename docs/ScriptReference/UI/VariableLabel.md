# VariableLabel
*class in OpenFlightVRC.UI | Inherits from: UdonSharpBehaviour*

## Description
This script links a [TextMeshProUGUI](https://docs.unity3d.com/Packages/com.unity.textmeshpro@1.1/api/TMPro.TextMeshProUGUI.html) with a variable from a target UdonBehaviour

## Properties
| Property | Type | Description |
|-|-|-|
| target | UdonBehaviour | The UdonBehaviour to get the variable from. This can also be a [ProxyUdonScript](/ScriptReference/UI/ProxyUdonScript.md) |
| targetVariable | string | The name of the variable to get/set |
| decimalPlaces | int | The number of decimal places to round to |
| prefix | string | The prefix to add to the input field displayed |
| suffix | string | The suffix to add to the input field displayed |
