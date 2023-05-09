# OpenFlight
*class in OpenFlightVRC | Inherits from: UdonSharpBehaviour*

## Description
This script is responsible for the enabling and disabling of the system itself.

## Properties
| Property | Type | Description |
|-|-|-|
| OpenFlightVersion | string | The version of OpenFlight that is currently installed in the world. This should not be set, as this value is set upon scene load |
| wingedFlight | GameObject | The WingedFlight game object, used to enable/disable the WingedFlight script |
| avatarDetection | [AvatarDetection](/ScriptReference/Detection/AvatarDetection.md) | The AvatarDetection script, used to re-evaluate flight upon switching to auto |
| flightMode | string | The current flight mode |
| flightAllowed | bool | Whether or not flight is allowed |

## Public Methods
| Method | Return Type | Description |
|-|-|-|
| FlightOn | void | Enables flight |
| FlightAuto | void | Enables automatic flight detection |
| FlightOff | void | Disables flight entirely |
| CanFly | void | Enables flight if in auto |
| CannotFly | void | Disables flight if in auto |
