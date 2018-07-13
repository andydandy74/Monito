A view extension for the Dynamo visual programming environment. 

Still very much a work in progress.
Currently includes the following tools:
* Find and Fix Ungrouped
* Manage Dynamo Player Inputs
* My Graphs (quick access to graph directories)
* New Workspace from Template
* Package Directories
* Search in Workspace

## Instructions for Dynamo 1.3.x

* Build in Visual Studio from ```Dynamo_1.3``` or ```master``` branch
* Copy ```src/Monito_ViewExtensionDefinition.xml``` to ```C:\Program Files\Dynamo\Dynamo Core\1.3\viewExtensions```
* Copy ```src/bin/Debug/Monito.dll``` to ```C:\Program Files\Dynamo\Dynamo Core\1.3```
* Copy ```src/bin/Debug/Monito.dll.config``` to ```C:\Program Files\Dynamo\Dynamo Core\1.3```
* After launching Dynamo you should see a new menu item ```DynaMonito```

## Instructions for Dynamo 2.x
I currently work mainly in Dynamo 1.3.x so some of the functionality may not have been tested thoroughly for Dynamo 2.x ...

* Build in Visual Studio from ```Dynamo_2.0.1``` branch
* Copy ```src/Monito_ViewExtensionDefinition.xml``` to ```C:\Program Files\Dynamo\Dynamo Core\2\viewExtensions```
* Copy ```src/bin/Debug/Monito.dll``` to ```C:\Program Files\Dynamo\Dynamo Core\2```
* Copy ```src/bin/Debug/Monito.dll.config``` to ```C:\Program Files\Dynamo\Dynamo Core\2```
* After launching Dynamo you should see a new menu item ```DynaMonito```

## Customization

Look at ```Monito.dll.config``` for some options to customize the view extension (currently mostly turning things off/on).

## Work in progress ##

All WIP branches should be built against Dynamo 1.3

## Known issues

* Template files need to be saved more or less in the same version as the current Dynamo version used.
* Ungrouped objects list doesn't update when objects are added to or removed from groups manually
* Zooming into groups from Search in Workspace or Manage Dynamo Player Inputs does not center the screen exactly on the selected group
