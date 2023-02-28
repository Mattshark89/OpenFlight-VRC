# OpenFlight-VRC

Creating an open source system to allow all kinds of avatars to fly. Uses Udon Sharp (Make sure it's installed! [Link](https://github.com/MerlinVR/UdonSharp))

You can try out the latest stable version of OpenFlight here: https://vrchat.com/home/launch?worldId=wrld_e61cba97-a1a9-4c53-93d2-7bb77240a2e7

## How to use

Import the Unity package from [Releases](https://github.com/Mattshark89/OpenFlight-VRC/releases) and place the prefab "OpenFlight" anywhere in your world. This should be all you need for most worlds.
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
