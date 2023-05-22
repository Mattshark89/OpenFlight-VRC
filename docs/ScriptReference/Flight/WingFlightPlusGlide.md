# WingFlightPlusGlide
*class in OpenFlightVRC | Inherits from: UdonSharpBehaviour*

## Description
This is the script that dictates the velocity and movement of flight itself.

## Properties
| Property | Type | Description |
|-|-|-|
| flapStrengthBase | int | The base strength of the flap. Affected by players armspan |
| flightGravityBase | float | The base gravity when in flight |
| requireJump | bool | Whether or not the player must jump to start flying |
| allowLoco | bool | Whether or not the player can use locomotion while flying |
| useAvatarModifiers | bool | Whether or not to use the avatar modifiers such as weight or wing tip offset |
| canGlide | bool | Whether or not the player can glide |
| fallToGlide | bool | Whether the player falling will automatically glide without flapping first |
| sizeCurve | [AnimationCurve](https://docs.unity3d.com/ScriptReference/AnimationCurve.html) | How much Flap Strength and Flight Gravity are affected by an avatar's armspan |
| horizontalStrengthMod | float | Modifier for horizontal flap strength. Makes flapping forward easier |
| glideControl | float | How tightly the player can turn while gliding |
| airFriction | float | How much friction is applied to the player while gliding |
| useGravityCurve | bool | Whether or not to use the gravity curve |
| gravityCurve | [AnimationCurve](https://docs.unity3d.com/ScriptReference/AnimationCurve.html) | Similar to Size Curve, but instead of modifying Flap Strength, it only affects Gravity. This value is ignored (Size Curve will be used instead) unless Use Gravity Curve is enabled |
| debugOutput | [TextMeshProUGUI](https://docs.unity3d.com/Packages/com.unity.textmeshpro@1.1/api/TMPro.TextMeshProUGUI.html) | If a GameObject with a TextMeshPro component is attached here, debug some basic info into it |
| bankingTurns | bool | Allows for players to tilt their arms to turn left or right while gliding |
| armspan | float | The armspan of the player |
| wingtipOffset | float | The wingtip offset of the player. Usually set by [AvatarDetection](/ScriptReference/Detection/AvatarDetection.md) |
| weight | float | The weight of the player. Usually set by [AvatarDetection](/ScriptReference/Detection/AvatarDetection.md) |
| loadBearingTransform | [Transform](https://docs.unity3d.com/ScriptReference/Transform.html) | This needs to be set to a static empty gameobject somewhere in the world. This is purely used as a helper |

## Public Methods
| Method | Return Type | Description |
|-|-|-|
| EnableBetaFeatures | void | Enables beta features such as banking turns |
| DisableBetaFeatures | void | Disables beta features such as banking turns |
| InitializeDefaults | void | Tells the script to store its default values internally. This should only be done once upon world load |
| RestoreDefaults | void | Restores the default values of the script into itself. This will reset the script to the defaults that were set in unity itself, not the packages defaults |
