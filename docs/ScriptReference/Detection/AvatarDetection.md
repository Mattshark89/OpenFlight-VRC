# AvatarDetection
*class in OpenFlightVRC | Inherits from: UdonSharpBehaviour*

## Description
This is the script that is responsible for the automatic avatar detection. It handles navigating the loaded JSON, and setting the flight properties accordingly, along with communicating to the OpenFlight script to enable/disable flight accordingly.

## Properties
| Property | Type | Description |
|-|-|-|
| debugInfo | string | Contains debug information from the script, you should only read from this property |
| JSONLoader | [AvatarListLoader](/ScriptReference/Detection/AvatarListLoader.md) | The AvatarListLoader script that is used to load the avatar list |
| OpenFlight | [OpenFlight](/ScriptReference/Flight/OpenFlight.md) | The OpenFlight script, used to enable/disable flight |
| WingFlightPlusGlide | [WingFlightPlusGlide](/ScriptReference/Flight/WingFlightPlusGlide.md) | The WingFlightPlusGlide script, needed to set the flight properties |
| allowedToFly | bool | Whether or not the user is allowed to fly |
| skipLoadingAvatar | bool | Whether or not to skip loading the avatar entirely |
| showWingTipGizmo | bool | Toggles displaying the wing tip gizmo |
| hashV1 | string | The V1 hash of the currently worn avatar |
| hashV2 | string | The V2 hash of the currently worn avatar |
| weight | float | The weight of the currently worn avatar |
| WingTipOffset | float | The offset of the wing tip for the current avatar |
| name | string | The name of the currently worn avatar. Defaults to `Unknown` if not in the list, or `Loading Avatar` if you are in the loading avatar / mecanim default rig |
| creator | string | The creator of the currently worn avatar. Defaults to `Unknown` if not in the list, or `Loading Avatar` if you are in the loading avatar / mecanim default rig |
| introducer | string | The introducer of the currently worn avatar. Defaults to `Unknown` if not in the list, or `Loading Avatar` if you are in the loading avatar / mecanim default rig |
| jsonVersion | string | The version of the JSON file that was loaded |
| jsonDate | string | The date of the JSON file that was loaded |

## Public Methods
| Method | Return Type | Description |
|-|-|-|
| reloadJSON | void | Tells the script to reload the JSON file and then recheck your worn avatar for flight |
| ReevaluateFlight | void | Reevaluates whether or not you should be able to fly |
