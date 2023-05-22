# OpenFlightTablet
*class in OpenFlightVRC.UI | Inherits from: UdonSharpBehaviour*

## Description
Controls the Tablet scale based on the players height, handles the fading logic for the tablet, and controls the menu tab colors

## Properties
| Property | Type | Description |
|-|-|-|
| fadeDistance | int | The distance from the player to start fading the tablet |
| allowFade | bool | Whether or not to allow the tablet to fade |
| objectsToHideOnFade | GameObject[] | The objects to hide when the tablet is faded |
| OpenFlight | [OpenFlight](/ScriptReference/Flight/OpenFlight.md) | The OpenFlight script, used to get the OpenFlight version |
| AvatarDetection | [AvatarDetection](/ScriptReference/Detection/AvatarDetection.md) | The AvatarDetection script, used to get the JSON version and date |
| VersionInfo | [TextMeshProUGUI](https://docs.unity3d.com/Packages/com.unity.textmeshpro@1.1/api/TMPro.TextMeshProUGUI.html) | This is the TMP component that will display the version info |
| tabs | [Button](https://docs.unity3d.com/2018.2/Documentation/ScriptReference/UI.Button.html)[] | The tabs on the tablet |

## Public Methods
| Method | Return Type | Description |
|-|-|-|
| SetActiveTab | void | Sets the active tab on the tablet |
| SetActiveTabMain | void | Sets the active tab on the tablet to the main tab |
| SetActiveTabSettings | void | Sets the active tab on the tablet to the settings tab |
| SetActiveTabDebug | void | Sets the active tab on the tablet to the debug tab |
