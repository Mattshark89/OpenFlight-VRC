!> This documentation is incomplete! Help us improve it by contributing to the [OpenFlight Repository](https://github.com/Mattshark89/OpenFlight-VRC/tree/main/docs) or by joining the [OpenFlight Discord](https://discord.gg/XrJsUfwqkf) and letting us know what you want to see here!
# OpenFlight
Welcome to the documentation for OpenFlight! Read the [Quick Start Guide](QUICKSTART.md) to jump right into using OpenFlight, or read the rest of this page if you want to learn more about OpenFlight itself. The public discord for OpenFlight is here: [OpenFlight Discord](https://discord.gg/XrJsUfwqkf)

[![Donate](https://liberapay.com/assets/widgets/donate.svg)](https://liberapay.com/OpenFlight/donate)

![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/unique_avatar_count)
![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/total_avatar_count)
![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/unique_hash_count)  
![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/todo_count)
![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/current_release_downloads)
![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/lifetime_release_downloads)

## What is OpenFlight?
OpenFlight is a flight system designed for VRChat worlds, allowing users to fly around just by flapping their hands! Alongside that, the system is setup to automatically detect the avatar you are wearing, allowing for the automatic enabling of flight when you put on an avatar wing arm attached wings. Of course, if your avatar isnt supported, or even just doesnt have wings at all, you can still force the flight system on to enjoy it.

### Features
- Automatic avatar detection
  - Dynamic avatar list
- Automatic flight enabling
- Gliding
- Customizable flight settings


## Can I get a demo?
There are a few official worlds by the maintainers of OpenFlight that you can visit to try out the system in VRChat.
- Happyrobot33
  - [Bleeding Edge Hash World](https://vrchat.com/home/launch?worldId=wrld_ef75dcc8-02fa-43ef-8f42-7b8159783d3d)
  - [Clone Ship](https://vrchat.com/home/world/wrld_2a9832e0-1b68-4a4b-9905-c7d54dc02a69)
- Mattshark89
  - [Stable World](https://vrchat.com/home/launch?worldId=wrld_e61cba97-a1a9-4c53-93d2-7bb77240a2e7)
  - [UnStable World](https://vrchat.com/home/world/wrld_21a31257-db78-472a-8fc7-b01edaf96e97)

Alongside these, we maintain a list of unofficial worlds that are known to use OpenFlight, however we cannot guarantee that these worlds will always be up to date with the latest version of OpenFlight itself.
- [Furry Hideout](https://vrchat.com/home/world/wrld_4b341546-65ff-4607-9d38-5b7f8f405132)
- [Island Rest Spot](https://vrchat.com/home/world/wrld_c8ed9f99-451d-40d2-aa7e-db3751eb1ac7)
- [Featherdale - Chipori Village](https://vrchat.com/home/world/wrld_fe48b7d3-1eb7-4e4c-9d8a-97da1d14a4e4)
- [Literally Furry Avatars](https://vrchat.com/home/world/wrld_9cfb9bf2-d667-4740-80aa-5b5e70ba48a39)

Lastly, we implore world creators to tag their world with the [`openflight`](https://vrchat.com/home/search/openflight) tag, so that users can easily find worlds that use OpenFlight. If you are a world creator, please consider doing this. It'll help other users find your world as well.

## How does it work?
There are quite a few systems that make up OpenFlight, so giving a concise awnser to this question is difficult. However, we will try to give a brief overview of the systems that make up OpenFlight.
### Flight System
The flight system is the core of OpenFlight, and is responsible for the actual flight itself. This is mainly handled by [WingFlightPlusGlide.cs](/ScriptReference/Flight/WingFlightPlusGlide.md).
### Avatar Detection
The detection is handled by a few scripts. [AvatarDetection.cs](/ScriptReference/Detection/AvatarDetection.md) is the main script that handles the detection, using [AvatarListLoader.cs](/ScriptReference/Detection/AvatarListLoader.md) to load the avatar list from GitHub. The detection uses the currently worn avatars Spine, Chest, Head, Neck, Shoulder, Upper/Lower arm and hand bones to create a hash to compare against the list of known avatars. If a match is found, the flight system is enabled, and the avatar specific settings are loaded.

## Credits
- The VRChat team for their awesome social platform
- `Mattshark89`: Flight/Gliding physics engine, repository manager
- `Happyrobot33`: Avatar detection system, settings tablet, VCC package manager shenanigans, and many other small tasks
- `MerlinVR`: UI Styler system source code
- Github contributors: <br>
<a href="https://github.com/Mattshark89/OpenFlight-VRC/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=Mattshark89/OpenFlight-VRC" />
</a>

...and several others for giving suggestions and helping fix critical bugs. You all are great!
