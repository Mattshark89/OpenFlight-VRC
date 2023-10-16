# GithubReleaseInterpreter
*class in OpenFlightVRC | Inherits from: UdonSharpBehaviour*

## Description
Loads the last 20 releases from the github API and formats them to be displayed in the UI. See [API Endpoint](https://api.github.com/repos/Mattshark89/OpenFlight-VRC/releases?per_page=20)

## Properties
| Property | Type | Description |
|-|-|-|
| outputText | string | The formatted text to be displayed in the UI |
| onLatestRelease | bool | Is true if the latest release is installed in this world |
| releasesBehind | int | The number of releases behind the latest release this world is |
| latestReleaseVersion | string | The latest release version |
