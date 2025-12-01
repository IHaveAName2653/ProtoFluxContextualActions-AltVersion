# USING THE MOD

Hi!\
This is the possibly documentation on how to use the mod.

# Controls (Default)
This is by far the most important part of the mod, so here is how to use it.

> NOTE:
> I will refer to "Menu", "Secondary", "Primary" and "Grip" as controls, rather than the real controller input.
> Each bind has a "Primary" and "Opposite" side as well. "Primary" side is where the tool is, "Opposite" is the opposite controller
> Each bind can also be "Press" (pressed this update), "Release" (released this update), "Held" (is pressed) or "Long hold" (Holding for over a second).
> They can also be "Not", meaning you want to not do that action
> I will format it as [\Not\] \[Action\] \[Side\] \[Bind\], Meaning Opposite secondary, or Primary Primary
> On desktop, Primary side is the normal binds, and Opposite is customizable. by default, Opposite Secondary is "`" (Back quote) and Opposite Menu is Left Shift.
> Both of these are changable in the mod settings.
> Remember to remap in the case that your keyboard is nonstandard/not qwerty!
> Opposite Primary/Grip is Mouse 4/5, these are not changable because i am lazy.

## Desktop
#### Select
Press Opposite Secondary and Not Hold Opposite Menu
#### Swap
Press Opposite Secondary and Hold Opposite Menu
#### Reference
Have a reference grabbed and Press Opposite Secondary
> These binds mean that (`) and (Left Shift) are very commonly used binds.

## VR
#### Select
Hold Opposite secondary and Press Primary Menu
#### Swap
Long Hold Opposite Secondary and Press Primary Menu
OR
Hold Opposite Secondary and Press Opposite Primary
#### Reference
Have a reference grabbed, Hold Opposite Secondary and Press Primary Menu

## CUSTOM
Custom binds are currently managed through a JSON file, located at (Resonite Install)/mod_fluxConfig/FluxContextActions/Binds.json\
I use a custom folder for my config to make it easier to find, and more consistent between different mod loaders (as they seem to make different config paths)

> NOTE:
> When saying what a string provides, be sure that you use the EXACT string. "swap" will not be accepted, while "Swap" will be.\
> If you use an invalid string, the "Invalid" option will be used instead.\
> "Invalid" is treated as 'none' or whatever the default is, which can lead to unexpected behaviour. ALWAYS BE SURE THE VALUES ARE VALID!!!

### Template
A bind looks like this:
```json
{
  "Action": "Reference",
  "Inputs": [
    {
      "Bind": "Trigger",
      "IsPrimary": false,
      "FireCondition": {
        "State": "press",
        "Invert": false
      }
    },
    {
      "Bind": "Grip",
      "IsPrimary": false,
      "FireCondition": {
        "State": "pressed",
        "Invert": false
      }
    }
    ],
  "IsDesktopBind": false
}
```
This is what it means:\
`VR: Hold Opposite Trigger and Press Opposite Grip = Reference`\

### Format
Each Bind is as such:
- Action: Can be "None", "Select", "Swap" or "Reference". None will treat the bind as 'dont trigger anything else'\
- IsDesktopBind: Should this bind be available on desktop. if false, the bind will not work on desktop (unless the config says otherwise)
- Inputs: A List of:
  - Bind: Can be "Primary", "Grip", "Secondary" or "Menu"
  - IsPrimary: If true, this bind is on the controller with the tool. if false, opposite controller.
  - FireCondition:
    - State: Can be "Pressed" (Is Held), "Held" (Held for >0.5s), "Press" (Pressed this frame), "Release" (Released this frame), "DoubleTap" (Double press this frame \[doubletap counts if < 0.5s\])
    - Invert: If false, this condition will pass if the controller state is true. If true, this condition will pass if the controller state is false.

The json file is an array, so you can have:
```json
[
  {"structure defined above"},
  {"still defined above..."},
  ...
]
```

When using the mod for the first time, a template bind will be created. it is suggested to use that bind as a template, and this as a guide to the valid values.\
> NOTE:\
> THIS CONFIG UPDATES IN (almost) REAL TIME!\
> Every 5 seconds, the file will check for an update. if it has updated, the new binds will apply instantly.\

> WARNING:\
> Do not delete the whole contents of this file if you want to clear it.\
> Instead, set the content to "[]".\
> That will parse correctly, but have no binds defined.

### Dynamic Impulses
This system integrates with the dynhook system, so you can modify the config using dynmaic impulses and values.\
This isnt actually implemented just yet, this part will be updated once it is.

# Flux Recipes
I dont wanna write this rn i wanna code the dynhook system to have the improved logic brb gonna go do that\
plus i already wrote so much why would i wanna continue
