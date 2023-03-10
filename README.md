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

- Desktop support
- Avatar detection system to automatically grant flight to certain avatar bases (for example, winged flight to avali)
- Equippable props (such as mechanical wings) that permit flying for everyone

## Plans for the future

In no particular order:
- In-game "How to fly" reference image
- Debugging tool/sliders to modify flight settings in-world (for creators)
- Additional flight systems
  - Winged (current system): Flap arms to fly, hold arms out to glide
  - Engine: Arms out to sides to propel forward with greater control, arms down in an A-Pose to stay stationary/fly slowly (iron-man style)
  - Creative (Desktop users): Minecraft-inspired Creative mode flight
- Wing trails (while gliding)
