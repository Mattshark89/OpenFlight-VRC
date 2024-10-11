# Flight Physics
While the default flight physics should work for most worlds, some creators will want to adapt them to fit their world better; for instance, it may be more convenient to fly faster to travel through a particuarly large world. To experiment with these settings yourself, find the `WingedFlight` GameObject inside of your OpenFlight object to reveal its settings in your inspector:

![WingedFlight](Assets/Unity%20Interface/WingedFlightLocation.png)

To learn more about each setting, hover over each item within the inspector, or view the documentation [here.](ScriptReference/Flight/WingFlightPlusGlide.md)

## Presets
Here are some community-made settings presets you can use as a starting point for experimentation:

<!-- tabs:start -->
#### **More Realistic**
Gravity feels heavier and flapping is weaker in general.
* Flap Strength Base: 170
* Flight Gravity Base: 0.55
* Air Friction: 0.045
#### **More Realistic (Better Control)**
Gravity feels heavier, but falling never felt so fun.
* Flap Strength Base: 285
* Flight Gravity Base: 0.75
* Air Friction: 0.025
#### **Acrobatic**
Fly at high speeds with rather tight turns.
* Flap Strength Base: 270
* Flight Gravity Base: 0.75
* Glide Control: 3
* Air Friction: 0.03
#### **Open World**
Faster flying with minimal resistance.
* Flap Strength Base: 330
* Flight Gravity Base: 0.3
* Air Friction: 0.004
<!-- tabs:end -->
