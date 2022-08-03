# Frequently asked questions


## Installation and setup problems
### I cannot open the app on the Mac, it says it is damaged

Apple doesn't like apps which are not going through their App store. To fix this first unzip the application in the `Downloads` folder, then run the following commands in a Terminal:

```bash
cd ~/Downloads
sudo xattr -rd com.apple.quarantine OpenSCAD\ Graph\ Editor.app
```

This will remove the quarantine flag from the application and it should now run. You can then drag it to your `Applications` folder.

### Can it run on Apple silicon?
Not natively. You will need to use [Rosetta 2](https://support.apple.com/en-us/HT211861) to let it run on Apple silicon.

### The font is really tiny

You can change the UI scaling in the settings dialog. Click the the button _Settings_ and then change the UI scaling to 150% or 200%. You will need to restart the editor afterwards.

## Questions about features
### Can this do full roundtrip between OpenSCAD code and the visual graph?

Currently only a limited form of roundtrip is supported. It is possible to reference to functions, modules and variables included from other text-based OpenSCAD files. However, OpenSCAD graph editor currently cannot load a text based OpenSCAD file and convert it into a node graph. 

While OpenSCAD graph editor can parse OpenSCAD files, these files only contain instructions on how to build the CAD model. They lack information about the positioning of the nodes (as this is not needed for the CAD model). So while it would be possible to build a node graph from a text based OpenSCAD file, the node graph would need to be layouted. This is not unsolvable but automatically producing readable node layouts with consistent results is very hard to get right and right now this is not a priority. Also all user made changes to the layout would get lost on a roundtrip unless they are saved in some proprietary format.

## Miscellaneous questions
## Can I use this for commercial purposes?
**The output of this program is not covered by the license so you can use the generated OpenSCAD code for any purposes you like.**
For the program itself, please check the [license](../LICENSE) for what is allowed and what not. 

## What is this app built with?
The app is built with the [Godot game engine](https://godotengine.org).

