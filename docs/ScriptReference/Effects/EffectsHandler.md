# EffectsHandler
*class in OpenFlightVRC.Effects | Inherits from: UdonSharpBehaviour*

## Description
This is used to handle renderering and control of individual players effects both locally and remotely.

## Properties
| Property | Type | Description |
|-|-|-|
| playerInfoStore | [PlayerInfoStore](/ScriptReference/Networking/PlayerInfoStore) | The information store for the player that this effects handler is related to |
| VFX | bool | Determines if the Visual Effects should be rendered or not |
| SFX | bool | Determines if the Sound Effects should be played or not |
| minGlidePitch | float | The minimum pitch to set the glide sound effect at |
| maxGlidePitch | float | The maximum pitch to set the glide sound effect at |
| minGlideVelocity | float | The minimum velocity to start lerping the glide sound effect at |
| maxGlideVelocity | float | The maximum velocity to start lerping the glide sound effect at |
