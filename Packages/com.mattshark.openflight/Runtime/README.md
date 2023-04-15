Source code: https://github.com/Mattshark89/OpenFlight-VRC/

## Want to test OpenFlight first?

You can try out the latest stable version of OpenFlight here: https://vrchat.com/home/launch?worldId=wrld_e61cba97-a1a9-4c53-93d2-7bb77240a2e7
(Alternate testing/dev world by GitHub user Happyrobot33: https://vrchat.com/home/launch?worldId=wrld_ef75dcc8-02fa-43ef-8f42-7b8159783d3d)


## Installation

First step: make sure your VRChat world SDK is up to date and you are using the VRChat Creator Companion. The avatar detection system uses remote string loading, a feature that is very new to Udon and not available in older SDKs (which means you MUST use the VCC to install UdonSharp)

You first need to add Koyashiro's repo to your VCC for dependencys
[Koyashiro's VPM Repos](https://vpm.koyashiro.net/install)

Then, click this link and add the OpenFlight repo to your VCC using the Add to VCC button
[OpenFlight](https://mattshark89.github.io/OpenFlight-VRC/?install=true)

If clicking on either of those links seems to have done nothing on your VRChat Creator Companion window, you need to update it.

Now you can use VCC's Manage Project button to manually add OpenFlight to your world. It should automatically enable all dependencies as well.


## How to use

After installing OpenFlight, you should see a VRC Packages button at the top of your Unity window. From there you can select which OpenFlight prefab you want to use.

Lite only includes a toggle button for flight. In most cases it is recommended you use Full instead which includes a tablet allowing users to tweak their flight settings and enable extra features.

To fine-tune the flight physics and other settings, select the WingedFlight GameObject inside of the prefab and check the Inspector. You can also test these values ingame using the OpenFlight tablet.
(If you plan on hiding the tablet from the average user, keep in mind the tablet will resize itself to match the player's scale; it might peek up through the floor! Instead you should use `OpenFlight (Lite)` and (optionally) disable/remove the WingedToggleBox GameObject.)


## Credits

The VRChat team for their awesome social platform
`Mattshark89`: Flight/Gliding physics engine, repository manager
`Happyrobot33`: Avatar detection system, settings tablet
Github contributors: https://github.com/Mattshark89/OpenFlight-VRC/graphs/contributors

...and several others for giving suggestions and helping fix critical bugs. You all are great!
