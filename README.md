# OpenFlight-VRC

This is a VRChat flight system that allow all kinds of avatars to fly. As of right now, it supports a variety of avatar bases that have wings; just jump and flap!
Note that this is a World/Udon project. This system cannot be installed to an avatar directly, only a world.

![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/unique_avatar_count)
![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/total_avatar_count)
![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/unique_hash_count)  
![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/todo_count)
![](https://byob.yarr.is/Mattshark89/OpenFlight-VRC/current_release_downloads)

### WIP New docs!
We are transitioning to these new docs:
https://mattshark89.github.io/OpenFlight-VRC/docs/#/
This readme will retain its current format for now incase any information is missing from the new docs.

## How does it work?

At its base level, winged avatars are permitted to fly in the world. Flight is performed by jumping and immediately flapping one's arms like a bird. Optionally, one can also hold their arms straight out to glide. Additional features and settings can be enabled within the Unity inspector or on the provided in-world settings tablet.

The list of avatars that are permitted to fly is fetched from a GitHub-hosted json file as soon as the player joins the world. That means as soon as an avatar is added to the list, it is automatically supported by every world that has OpenFlight installed.


## Installation

First step: make sure your VRChat world SDK is up to date and you are using the VRChat Creator Companion. The avatar detection system uses remote string loading, a feature that is very new to Udon and not available in older SDKs (which means you MUST use the VCC to install UdonSharp)

Click this link to open the VCC and automatically add the repository
[OpenFlight](https://mattshark89.github.io/OpenFlight-VRC/?install=true)

If clicking on the link seems to have done nothing on your VRChat Creator Companion window, you need to update it.

You can try out the latest stable version of OpenFlight here: [Stable World](https://vrchat.com/home/launch?worldId=wrld_e61cba97-a1a9-4c53-93d2-7bb77240a2e7)<br>
Alternate development world by Happyrobot33: [Bleeding Edge Hash World](https://vrchat.com/home/launch?worldId=wrld_ef75dcc8-02fa-43ef-8f42-7b8159783d3d)<br>

<details>
<summary>Public Community worlds with OpenFlight:</summary>

- [Featherdale - Chipori Village](https://vrchat.com/home/world/wrld_fe48b7d3-1eb7-4e4c-9d8a-97da1d14a4e4)
  - No Tablet in world, just toggle button
- [Clone Ship](https://vrchat.com/home/world/wrld_2a9832e0-1b68-4a4b-9905-c7d54dc02a69)
  - Tablet in world, but indoors
- [Literally Furry Avatars](https://vrchat.com/home/world/wrld_9cfb9bf2-d667-4740-80aa-5b5e70ba48a39)
  - Tablet in world, largescale mountainous terrain
</details>

## How to use

After installing the VCC package listed above, add them to your project by clicking "Manage Project" in the VCC and selecting OpenFlight. Once you are in your project, look for the dropdown labeled VRC Packages along the bar that has File and Edit, and inside it go to OpenFlight > Prefabs. Clicking on one of the listed prefabs will automatically add it to your currently open scene (The main/full prefab includes the recommended tablet that includes additional settings. If all you want is a toggle button, use the Lite prefab instead. You only need to select one prefab.) As long as you dont unlink the prefab that it adds, there is no need to do this again if you update the package, as that prefab will update with it.

To fine-tune the flight settings, select the WingedFlight GameObject inside of the prefab and check the Inspector. Check the README.md file within the Package for more details.

It is recommended that if you add OpenFlight to your world, that you tag your world with OpenFlight so users can find it easier!

## I want to add an avatar to the list!

Neato! Check out the json file (https://github.com/Mattshark89/OpenFlight-VRC/blob/main/Packages/com.mattshark.openflight/Runtime/data.json) and make a pull request with your avatar added to the list. Follow the formatting of those who came before you.

Name is the name of the avatar, Creator the creator, Introducer is you. If the avatar you are requesting was made from a public base, `Name` and `Creator` should reference the public base. Don't put your own name here; put your name in `Introducer` instead.

The Hash can be obtained from the OpenFlight settings tablet (Debug tab) while your avatar is being worn. Ensure you are submitting a v2 hash, which can be identified by having `v2` at the end. WingtipOffset can also be found by turning on `Show Gizmos` in the tablet and adjusting the `WingtipOffset` slider until the center of the sphere is roughly on the tip of your wing/feathers/whathaveyou.
Once your pull request is made, leave a comment along with it that either has a link to the avatar base or, if there is no public page for it, a photo/screenshot of the avatar.

Avatars will only be added if they meet one criterion: it has wings that move along with your arms. Wings on your back do not count if they aren't parented to your arms since flapping... well, your arms, shouldn't cause you to fly now should it?

Please let me have three days or so to merge the request before DM'ing me on Discord: `@Mattshark#1439`


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

- The VRChat team for their awesome social platform

- `Mattshark89`: Flight/Gliding physics engine, repository manager

- `Happyrobot33`: Avatar detection system, settings tablet, VCC package manager shenanigans

- Github contributors: https://github.com/Mattshark89/OpenFlight-VRC/graphs/contributors

...and several others for giving suggestions and helping fix critical bugs. You all are great!

<a href="https://github.com/Mattshark89/OpenFlight-VRC/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=Mattshark89/OpenFlight-VRC" />
</a>
