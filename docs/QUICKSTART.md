!> This documentation is incomplete! Help us improve it by contributing to the [OpenFlight Repository](https://github.com/Mattshark89/OpenFlight-VRC/tree/main/docs) or by joining the [OpenFlight Discord](https://discord.gg/XrJsUfwqkf) and letting us know what you want to see here!

# Quick Start Guide
Adding OpenFlight to your world is relatively simple, and only requires a few steps.

!> It is HIGHLY recommended you read this guide in its entirety, as it contains important information about how to maintain your project after adding OpenFlight.

### Terminology
Before reading the rest of this guide, it is important to understand the terminology used throughout the guide.
- **OpenFlight**: The flight system itself, which is a collection of scripts and prefabs.
- **OpenFlight Repository**: The repository containing the OpenFlight system, along with official addon packages.
- **OpenFlight Addon**: An addon package for OpenFlight, which adds additional functionality to the system.
- **VCC**: The [VRChat Creator Companion](https://vcc.docs.vrchat.com/), which is used to launch Unity projects and manage project packages.
- **VRC**: VRChat, the game itself.

## Requirements
- Make sure you have the latest version of the [VRChat Creator Companion](https://vcc.docs.vrchat.com/) installed.
  - For Windows 10, this is the regular UI version.
  - For MacOS and Linux, the only available version that VRChat provides is the [CLI Version](https://vcc.docs.vrchat.com/vpm/cli)
- World project is already migrated into the VCC.
  - This guide assumes you have already migrated your world project into the VCC. If you have not done this yet, please follow the [VCC Migration Guide](https://vcc.docs.vrchat.com/vpm/migrating) before continuing.

## Adding the OpenFlight Repository to the VCC
The steps for loading the OpenFlight repository into the VCC are different depending on the type of VCC install you are using. This process only needs to be done once, and will allow you to use the OpenFlight system in any of your projects.
<!-- tabs:start -->
#### **GUI**
For the graphical based VCC, all you need to do is click [Add To VCC](vcc://vpm/addRepo?url=https%3A%2F%2FMattshark89.github.io%2FOpenFlight-VRC%2Findex.json). This will show a popup on your VCC asking you to confirm the installation of the repository. Click yes, and the repository will be available for use in any of your projects.

?> If you are having issues with the Add To VCC link not working, your VCC is likely out of date
#### **CLI**
For the CLI based VCC, you will need to run the following command in your terminal:
```bash
vpm add repo https://Mattshark89.github.io/OpenFlight-VRC/index.json
```
<!-- tabs:end -->

## Installing the OpenFlight Package into a project
Once you have the OpenFlight repository loaded into your VCC, you can now install the OpenFlight package into one of your projects.  

!> IMPORTANT! Adding OpenFlight to an existing project *may* update some of the other packages in that project to a newer version. This should not cause any issues, but if you feel inclined, now would be a good time to make a backup of your project.
<!-- tabs:start -->
#### **GUI**
Navigate to the Projects tab of the VCC, find your project, and click on the "Manage Projects" Button associated with it. This will open a window with all of the packages that your project currently has installed. Find or search for the package labelled "OpenFlight", and click the plus button to the right of it. This will install the OpenFlight system into your project, along with any other dependencies that are required. You can now open the project and continue with the next steps.

#### **CLI**
Navigate to the root directory of your project in your terminal, and run the following command:
```bash
vpm add package com.mattshark.openflight
```
This will install the OpenFlight system into your project, along with any other dependencies that are required. You can now open the project and continue with the next steps.
<!-- tabs:end -->

?> Make sure you check for and update the package whenever you return to work on your world! New features and bug fixes are being added all the time, and you don't want to miss out on them.

## Adding OpenFlight to your scene
OpenFlight currently comes with two distinct world prefabs. These can be found by navigating to the top of your unity editor window, and finding the dropdown labelled "VRC Packages". Click on this dropdown, and inside you should see a folder labelled "OpenFlight". Inside that is a folder labelled "Prefabs". Inside this folder are the two prefabs that are currently available for OpenFlight. Clicking on either of these prefabs will add them to your scene automatically.

- ### Lite
This version is purely a selector button, allowing for users to press it to cycle between ON, AUTO and OFF. Good for a barebones implementation where users are locked to your default physics.

- ### Full
This version contains a tablet allowing for users to configure their flight settings, along with a host of debug options and the information required to add new avatars to the system. Recommended.

## Wrapping Up
Once you have added one of the prefabs to your scene, make sure you position it in a place that is easily accessible to your users. Once you have done this, you can either proceed to uploading your world, or learn how to [customize the settings for your world](CUSTOMIZATION.md).

?> We recommend tagging your world with the [`openflight`](https://vrchat.com/home/search/openflight) tag, so that users can easily find worlds that use OpenFlight.
