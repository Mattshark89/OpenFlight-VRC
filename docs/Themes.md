# Themes
<!-- panels:start -->
<!-- div:left-panel -->
Using work derived from the [USharpVideo Project](https://github.com/MerlinVR/USharpVideo) from MerlinVR, OpenFlight has a built-in theme system that allows you to quickly change the look of the tablet. Doing so allows you to better match the tablet to your worlds design and theme, instead of being out of place. By default, OpenFlight comes with these default themes:
<!-- tabs:start -->
#### **Avali**
This theme represents the original color scheme of the tablet, and is the default theme upon install.
![Avali](Assets/UI%20Styler/Demo/Avali.png)
#### **Dark**
This theme is a general dark theme, with a transparent background.
![Dark](Assets/UI%20Styler/Demo/Dark.png)
#### **Midnight**
This theme is a darker dark theme, with a non-transparent background.
![Midnight](Assets/UI%20Styler/Demo/Midnight.png)
#### **Light**
This theme is a general light theme, if you like that sort of thing.
![Light](Assets/UI%20Styler/Demo/Light.png)
#### **Discord**
This theme is based on the Discord desktop app, using color codes provided from [eggradients.com](https://www.eggradients.com/tool/discord-color-codes).
![Discord](Assets/UI%20Styler/Demo/Discord.png)
#### **VRC**
This theme is based on the client sim tool for VRC.
![VRC](Assets/UI%20Styler/Demo/VRC.png)
<!-- tabs:end -->

<!-- div:right-panel -->
[Loop](Assets/UI%20Styler/Demo/Loop.mp4 ':include :type=video width=100% height=auto autoplay muted loop controls')

<!-- panels:end -->

## Selecting a theme
<!-- panels:start -->
<!-- div:left-panel -->
Selecting a theme for the tablet is relatively simple. First, find the tablet root in the scene, labelled `OpenFlight Tablet` (You can find this under the `OpenFlight` prefab). In the inspector for this object, you will find the `Styler` component, with a dropdown to select a style from your project. Any custom styles you make will also be shown here.

<!-- div:right-panel -->
[Changing Styles](Assets/UI%20Styler/Demo/ChangingStyles.mp4 ':include :type=video width=100% height=auto autoplay muted loop controls')
<!-- panels:end -->

## Creating custom themes
<!-- panels:start -->
<!-- div:left-panel -->
Creating a custom theme is very straightforward aswell. We recommend opening the development scene found in the `Runtime/Styles` Folder of the package. Once in that scene, navigate to your assets folder and right-click, and navigate to the `Create > VRC Packages > OpenFlight > UIStyle` selection. This will make a new style asset populated with default values. Instead of editing the values in the inspector of the asset, it is recommended to select the tablet and modify the values there, as it will show you the changes in realtime.

!> **Important Note** Make sure you create this asset in your assets folder! Anything made in the packages folder will be deleted upon updating the package!
<!-- div:right-panel -->
[New Styles](Assets/UI%20Styler/Demo/NewStyle.mp4 ':include :type=video width=100% height=auto autoplay muted loop controls')
<!-- panels:end -->
