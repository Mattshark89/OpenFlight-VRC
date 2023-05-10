# AvatarListLoader
*class in OpenFlightVRC | Inherits from: UdonSharpBehaviour*

## Description
This is used to query the Github data.json file for the list of avatars. It supports falling back to the in-world list if the Github list fails to load.

## Properties
| Property | Type | Description |
|-|-|-|
| URL | [VRCUrl](https://udonsharp.docs.vrchat.com/vrchat-api/#vrcurl) | The URL to the json file |
| Output | String | The output of the json file. This is set by the LoadURL method, and is done asynchronously, so make sure your script waits for output to be set. See [VRCStringDownloader](https://docs.vrchat.com/docs/string-loading) for more information |
| OfflineJSON | [TextAsset](https://docs.unity3d.com/ScriptReference/TextAsset.html) | The in-world json file. This is used if the URL fails to load |
| useOfflineJSON | bool | If true, will use the in-world json file instead of fetching from the Github |

## Public Methods
| Method | Return Type | Description |
|-|-|-|
| LoadURL | void | Loads the URL and sets the Output property. This is done asynchronously, so make sure your script waits for output to be set. See [VRCStringDownloader](https://docs.vrchat.com/docs/string-loading) for more information|
