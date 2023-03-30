# OpenFlight-VRC

Creating an open source system to allow all kinds of avatars to fly.
You will need all of these installed in your project first in order for Open-Flight to work. Install these in order:
1. [UdonSharp](https://github.com/MerlinVR/UdonSharp)
2. [udon-json](https://github.com/koyashiro/udon-json)
3. [udon-list](https://github.com/koyashiro/udon-list)
4. [udon-dictionary](https://github.com/koyashiro/udon-dictionary)

You can try out the latest stable version of OpenFlight here: https://vrchat.com/home/launch?worldId=wrld_e61cba97-a1a9-4c53-93d2-7bb77240a2e7

You can try out the avatar detection system here: https://vrchat.com/home/launch?worldId=wrld_ef75dcc8-02fa-43ef-8f42-7b8159783d3d

## How to use

After installing the four packages listed above, import the Unity package from [Releases](https://github.com/Mattshark89/OpenFlight-VRC/releases) and place the prefab "OpenFlight" anywhere in your world. This should be all you need for most worlds.
To fine-tune the flight settings, select the WingedFlight GameObject inside of the prefab and check the Inspector.

## Roadmap to V1.0.0 release

- ~~Avatar detection system to automatically grant flight to certain avatar bases (for example, winged flight to avali)~~ Implemented as of [ae1c1f0](https://github.com/Mattshark89/OpenFlight-VRC/commit/ae1c1f0075f3a3108b3798301d78939fd8cfe216)

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
