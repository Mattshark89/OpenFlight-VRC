# ProxyUdonScript
*class in OpenFlightVRC.UI | Inherits from: UdonSharpBehaviour*

## Description
The purpose of this script is to be a hub script for many other scripts to reference, so that the target on this script can change and all the other scripts will still work. This is useful for when you want to have a single script that can be used for multiple different objects, but you don't want to have to change the target on all the other scripts. Scripts referencing this script must have support for it.

## Properties
| Property | Type | Description |
|-|-|-|
| target | UdonBehaviour | The UdonBehaviour to proxy |
| targetGameObject | GameObject | The GameObject to proxy |

## Public Methods
For the purpose of these docs, there is no unique public methods on this script. However, there are a few public methods pulled into this script to allow for UI elements to reference this script and call methods on the target script. Check the source for the full list of methods that are pulled in.
