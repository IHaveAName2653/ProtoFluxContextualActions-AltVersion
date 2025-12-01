# ProtoFlux Contextual Actions

Public! After all this time!\
(This code isnt very good, but im making it public so others can get the mod way easier)

> #### NOTE
> This is an alternate version to the proper version of the mod.
This isnt a fork because i was stupid dumb idiot, and a merge would be *difficult*, if not impossible.
> check out the [Real version](https://github.com/brecert/ProtoFluxContextualActions) of the mod, its better anyways

### New changes (alt version)
- Increased selection/swap types
- support for flux 'recipes'
- - Flux 'recipes' can contain multiple nodes, rather than just one node
- Dynamic impulse hooks
- - allows for editing mod config without needing to close the game
- Custom flux recipe creation (uses dyn impulse hooks)
- Customizable binds
- - Default binds are different from the base mod
- - You can add custom binds in the config file
- - You can use dynvars to make custom binds in-world (if active, disables all normal binds)
- Only slightly behind on real repo commits
- - I try to mirror the changes, but it clearly doesnt work very well :)
- ### NODES ARE IN GROUPS
- - note: swiping seems to break sometimes when using groups. will fix later
- - warning: they are organized like actual shit and sometimes move. a lot. its approaching stability however
- - Increases the amount of selection nodes by simply grouping into folders
- - Improves readability and brain organization, its all seperated into 'nearly good' groups.
- I do take suggestions for node types, anything is (probably) possible
- - Only takes a rebuild of the project
- Supports Hot Reload (if its a debug build), so you can update the mod without relaunching
- Active developments on UI for managing everything, rather than config json and reading unreadable blocks of text
- Insane levels of bullshit just for a few simple features

Be sure to check the USING.md file to figure out how the hell this mod actually works

(This mod is just an excuse to look *really deep* into how frooxengine works)\
If you use this, i hope you enjoy my (really weird) version of this mod!

### Ok now for the normal gitignore

ProtoFlux Contextual Actions is a [Resonite Mod Loader](https://github.com/resonite-modding-group/ResoniteModLoader) mod that adds additional context menu actions for different contexts that revolve around protoflux.

Bug reports welcome, feel free to create an issue for any actions that you want added.

## Patches
There are currently a few patches.

### Contextual Actions
Adds 'Contextual Actions' to the ProtoFlux Tool. Pressing secondary while holding a protoflux tool will open a context menu of actions based on what wire you're dragging instead of always spawning an input/display node. Pressing secondary again will spawn out an input/display node like normal.

### Contextual Swap Actions
Adds 'Contextual Swapping Actions' to the ProtoFlux Tool. Double pressing secondary pointing at a node with protoflux tool will be open a context menu of actions to swap the node for another node.

This is intended to be paired with Contextual Actions.
For example a `ValueLessThan` may be wanted when dragging a `float` output wire, however that node will not appear in the context menu by default. Instead a `ValueEquals` should be selected first, then swapped for `ValueLessThan` using contextual swap actions.

https://github.com/user-attachments/assets/15ad6739-dbd2-44a1-a7f2-7315a6a429f5

Some actions are grouped together like that in order to keep a soft limit of 10 maximum items in the context menu at once.
This may be made configurable at some point.

### Dynamic Variable Input Creation
Adds a context menu item to create DynamicVariableInput nodes when holding a dynamic variable component with the ProtoFlux tool.

### Sample Spatial Variable Creation
Adds a context menu item to create SampleSpatialVariable nodes when holding a spatial variable source component with the ProtoFlux tool.





## Acknowledgements
The project structure is based on https://github.com/esnya/ResoniteEsnyaTweaks.
