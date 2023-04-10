Source code: https://github.com/Mattshark89/OpenFlight-VRC/

## Want to test OpenFlight first?

You can try out the latest stable version of OpenFlight here: https://vrchat.com/home/launch?worldId=wrld_e61cba97-a1a9-4c53-93d2-7bb77240a2e7
(Alternate testing/dev world by GitHub user Happyrobot33: https://vrchat.com/home/launch?worldId=wrld_ef75dcc8-02fa-43ef-8f42-7b8159783d3d)


## Installation

You will need all of these installed in your project first in order for OpenFlight to work. Install these in order:
1. [UdonSharp](https://github.com/MerlinVR/UdonSharp)
2. [udon-json](https://github.com/koyashiro/udon-json)
3. [udon-list](https://github.com/koyashiro/udon-list)
4. [udon-dictionary](https://github.com/koyashiro/udon-dictionary)


## How to use

After installing the four packages listed above, place the prefab "OpenFlight" anywhere in your world. This should be all you need for most worlds.
If you would rather limit the OpenFlight system to a simple toggle button, use "OpenFlight (Lite)" instead of "OpenFlight".

To fine-tune the flight physics and other settings, select the WingedFlight GameObject inside of the prefab and check the Inspector. You can also test these values ingame using the OpenFlight tablet.
(If you plan on hiding the tablet from the average user, keep in mind the tablet will resize itself to match the player's scale; it might peek up through the floor! Instead you should use `OpenFlight (Lite)` and (optionally) disable/remove the WingedToggleBox GameObject.)


## Credits

The VRChat team for their awesome social platform
`Mattshark89`: Flight/Gliding physics engine, repository manager
`Happyrobot33`: Avatar detection system, settings tablet
Github contributors: https://github.com/Mattshark89/OpenFlight-VRC/graphs/contributors

...and several others for giving suggestions and helping fix critical bugs. You all are great!
