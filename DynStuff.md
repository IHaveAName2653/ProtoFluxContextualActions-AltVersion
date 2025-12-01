This mod really likes dynamic variables and impulses, so this is how it works.

# IMPULSES
Dynamic impulses are hooked into with this mod, and if the right tag is used, the dynamic impulse will be cancelled.

## General Info
There are 2 different ways to call a function:
- Invoke_[Method]
- Invoke (DynVar\<string\> "DynHooks/Method")

From there, you must provide some dynamic variables.\
Anything that comes from impulse hooks will *always* use DynHooks/[variable].\
DynVar\<string\> "Function" controls what function you call within the selected method.\
Every method has a different set of used variables, so i will list them all.\
Every dynvar used within the functions can be found by name or index. this includes inputs and returns.\
will format variables as `DynVar<T> [index] "[VariableName]"`\
full variable names is DynHooks/[Variable] or DynHooks/Arg[index], full return names is DynHooks/Return_[name] or DynHooks/Return_[index]

## "RecipeConfig"

### "Get"
Returns a DynVar\<string\> 0 "RecipeData", containing the data for the flux recipe provided by the name.\
DynVar\<string\> 0 "RecipeName": The name of the target recipe
### "GetAll"
Returns a DynVar\<string\> 0 "AllRecipeData", containing an array of every recipe that the caller has access to.\
Takes no inputs
### "GetAllNames"
Returns a DynVar\<string\> 0 "AllNames", containing a comma seperated list, of every recipe available to the caller.
Takes no inputs
### "AddRecipeString"
Has no return.\
DynVar\<string\> 0 "RecipeString": The recipe to add to the calling user recipe list
### "AddMultiple"
Has no return.\
DynVar\<string\> 0 "RecipeStringArray": The recipes to add to the calling user recipe list.\
Note: this is in JSON format. to add multiple, either use AddRecipeString, or add all recipes together, seperated by commas, and surround with `[ ]`
### "RemoveRecipe"
Has no return.\
DynVar\<string\> 0 "TargetRecipe": the name of the recipe to remove
### "SetAll"
Spooky!\
Has no return.\
DynVar\<string\> 0 "AllRecipes": the JSON string of all recipes. use `[]` to remove all recipes

## "RecipeData"

### "Reload"
No return, No input.\
Reloads recipes from the config file

## "RecipeMaker"

### "AddRecipe"
Has no return.\
Will make a recipe from the provided slots, which are:
- DynVar\<Slot\> 0 "RecipeSlot": The slot that contains the flux nodes in the recipe
- DynVar\<string\> 1 "RecipeName": The name of the recipe, once its created
- DynVar\<Slot\> 2 "RecipeRootNode": The origin node for the recipe, which is where it creates.
- DynVar\<Type\> 3 "RecipeType": The type of input/output that this recipe can spawn from
- DynVar\<bool\> 4 "RecipeIsOutput": If the recipe comes from an output proxy. false is input proxy
- DynVar\<colorX\> 5 "RecipeColor" (Optional): The color to have the recipe be in the context menu. null/undefined defaults to light grey

If everything is properly provided, you will have a recipe to use whenever, wherever
### "DetermineRoot"
Has no return.\
Will set the variables from "AddRecipe", based on the selected node.\
DynVar\<Slot\> 0 "TargetNode": The node which is the root. Has to be a relay node\
This isnt required to make a recipe, but makes it a lot faster.\
Only supports relay nodes due to generic types and multiple inputs/outputs being weird to deal with.
### "BuildToSlot"
Has no return.\
Will build the given flux recipe into the slot you provide.\
DynVar\<string\> 0 "RecipeName": The recipe to construct\
DynVar\<Slot\> 1 "TargetParent": the slot to parent the nodes under\
DynVar\<ProtoFluxTool\> 2 "FluxTool": the tool required for construction

## "Binds"
i still havent done this part yet

# VARIABLES
When it comes to the Flux tool, you can customize the binds in-world, using only 5 components.\
All components must be on the Flux tool itself.\
<br>
`PFCA` Dynamic Variable space: required for all other variables\
`PFCA/Override` DynVar\<bool\>: If the binds should be overridden with the other variables\
`PFCA/Select` DynVar\<bool\>: When written to true, instantly writes back to false and triggers a 'select' action\
`PFCA/Swap` DynVar\<bool\>: When written to true, instantly writes back to false and triggers a 'swap' action\
`PFCA/Reference` DynVar\<bool\>: When written to true, instantly writes back to false and triggers a 'reference' action\
<br>
its pretty simple i think
