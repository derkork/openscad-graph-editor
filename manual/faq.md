# Frequently asked questions

## I cannot open the app on the Mac, it says it is damaged

Apple doesn't like apps which are not going through their App store. To fix this first unzip the application in the `Downloads` folder, then run the following commands in a Terminal:

```bash
cd ~/Downloads
sudo xattr -rd com.apple.quarantine OpenSCAD\ Graph\ Editor.app
```

This will remove the quarantine flag from the application and it should now run. You can then drag it to your `Applications` folder.

## Can it run on Apple silicon?
Not natively. You will need to use [Rosetta 2](https://support.apple.com/en-us/HT211861) to let it run on Apple silicon.

## What is this app built with?
The app is built with the [Godot game engine](https://godotengine.org).

## Can I use this for commercial purposes?
**The output of this program is not covered by the license so you can use the generated OpenSCAD code for any purposes you like.**
For the program itself, please check the [license](../LICENSE) for what is allowed and what not. 