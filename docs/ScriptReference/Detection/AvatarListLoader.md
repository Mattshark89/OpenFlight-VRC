# AvatarListLoader
*class in OpenFlightVRC | Inherits from: UdonSharpBehaviour*

## Description
This is used to query the Github data.json file for the list of avatars. It supports falling back to the in-world list if the Github list fails to load.

## Properties
| Property | Type | Description |
|-|-|-|
| URL | [VRCUrl](https://udonsharp.docs.vrchat.com/vrchat-api/#vrcurl) ||
| Output | String ||
| OfflineJSON | [TextAsset](https://docs.unity3d.com/ScriptReference/TextAsset.html) ||
| useOfflineJSON |  ||

## Public Methods
| Method | Return Type | Description |
|-|-|-|
| LoadURL | void | Loads the URL and sets the Output property. This is done asynchronously, so make sure your script waits for output to be set. See [VRCStringDownloader](https://docs.vrchat.com/docs/string-loading) for more information|
