# Util
*class in OpenFlightVRC | Inherits from: UdonSharpBehaviour*

## Description
This is a collection of useful functions that are used throughout the project. They are all static, so you can call them from anywhere in the namespace.

## Public Methods
| Method | Return Type | Description |
|-|-|-|
| GetBoneDistance(Vector3 bone1, Vector3 bone2, int scalingFactor, float d_spinetochest) | int | Returns the distance between two bones, modified by the scaling factor and spine |
| SetWingtipTransform(VRCPlayerApi.TrackingData bone, GameObject objectToMove, float WingtipOffset, double d_spinetochest) | void | Moves a gameobject to a wingtip position |
| TotalVectorDistance(Vector3[] vectors) | float | Adds up all of the distances to each position in order |
| ScaleModifier | float | Gets a scale modifier based on the player's scale in order to scale things uniformly |
| GetRainbowGradient | Gradient | Creates and returns a rainbow gradient for use in particle systems |
| SetParticleSystemEmission(ParticleSystem ps, bool enabled) | void | Sets the emission of a particle system |
| ControlSound(AudioSource source, bool enabled) | bool | Controls the sound of an audio source. Returns if it changed anything |
