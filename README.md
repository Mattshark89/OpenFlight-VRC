# OpenFlight-VRC

This is a VRChat flight system that allow all kinds of avatars to fly. As of right now, it supports a variety of avatar bases that have wings; just jump and flap!
Note that this is a World/Udon project. This system cannot be installed to an avatar directly, only a world.

![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/todo_count)
![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/unique_avatar_count)
![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/total_avatar_count)
![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/unique_hash_count)

## How does it work?

At its base level, winged avatars are permitted to fly in the world. Flight is performed by jumping and immediately flapping one's arms like a bird. Optionally, one can also hold their arms straight out to glide. Additional features and settings can be enabled within the Unity inspector or on the provided in-world settings tablet.
The list of avatars that are permitted to fly is fetched from a GitHub-hosted json file as soon as the player joins the world. That means as soon as an avatar is added to the list, it is automatically supported by every world that has OpenFlight installed.


## Installation

First step: make sure your VRChat world SDK is up to date. The avatar detection system uses remote string loading, a feature that is very new to Udon!

Next, you will need all of these installed in your project first in order for Open-Flight to work. Install these in order:
1. [UdonSharp](https://github.com/MerlinVR/UdonSharp)
2. [udon-json](https://github.com/koyashiro/udon-json)
3. [udon-list](https://github.com/koyashiro/udon-list)
4. [udon-dictionary](https://github.com/koyashiro/udon-dictionary)

You can try out the latest stable version of OpenFlight here: https://vrchat.com/home/launch?worldId=wrld_e61cba97-a1a9-4c53-93d2-7bb77240a2e7
Alternate development world by Happyrobot33: https://vrchat.com/home/launch?worldId=wrld_ef75dcc8-02fa-43ef-8f42-7b8159783d3d


## How to use

After installing the four packages listed above, import the Unity package from [Releases](https://github.com/Mattshark89/OpenFlight-VRC/releases) and place the prefab "OpenFlight" anywhere in your world. This should be all you need for most worlds.
To fine-tune the flight settings, select the WingedFlight GameObject inside of the prefab and check the Inspector. Check the README.md file within the Unity Package for more details.


## I want to add an avatar to the list!

Neato! Check out the json file (https://github.com/Mattshark89/OpenFlight-VRC/blob/main/Packages/com.mattshark.openflight/Runtime/data.json) and make a pull request with your avatar added to the list. Follow the formatting of those who came before you.
Name is the name of the avatar, Creator the creator, Introducer is you. If the avatar you are requesting was made from a public base, `Name` and `Creator` should reference the public base. Don't put your own names here; put your name in `Introducer` instead.
The Hash can be obtained from the OpenFlight settings tablet (Debug tab) while your avatar is being worn. WingtipOffset can also be found by turning on `Show Gizmos` in the tablet and adjusting the `WingtipOffset` slider until the center of the sphere is roughly on the tip of your wing/feathers/whathaveyou.
Once your pull request is made, leave a comment along with it that either has a link to the avatar base or, if there is no public page for it, a photo/screenshot of the avatar.
Avatars will only be added if they meet one criterion: it has wings that move along with your arms. Wings on your back do not count if they aren't parented to your arms.
Please let me have three days or so to merge the request before bugging me on Discord: `@Mattshark#1439`


## Plans for the future

In no particular order:
- Desktop support
- In-game "How to fly" reference image
- Rework glide detection to be more forgiving while banking
- Landing during a glide causes the player to slide to a stop
- Additional flight systems
  - Winged (current system): Flap arms to fly, hold arms out to glide
  - Engine: Arms out to sides to propel forward with greater control, arms down in an A-Pose to stay stationary/fly slowly (iron-man style)
  - Creative (Desktop users): Minecraft-inspired Creative mode flight
- Wing trails (while gliding)
- Setup/Settings guide, basic API for hooking in other plugins
- Equippable props (such as mechanical wings) for players that manually enable flight


## Credits

The VRChat team for their awesome social platform
`Mattshark89`: Flight/Gliding physics engine, repository manager
`Happyrobot33`: Avatar detection system, settings tablet
Github contributors: https://github.com/Mattshark89/OpenFlight-VRC/graphs/contributors

...and several others for giving suggestions and helping fix critical bugs. You all are great!
