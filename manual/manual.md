# OpenSCAD Graph Editor Manual

_This manual is currently being written._

## Introduction
OpenSCAD is a 3D CAD program that can be used to create 3D models through scripting. The OpenSCAD Graph Editor is a graphical user interface for OpenSCAD that allows you to create 3D models by connecting nodes in a graph. The editor will automatically generate OpenSCAD code for you.

## Installation & Setup

You can download the latest release of the OpenSCAD Graph Editor from [GitHub](https://github.com/derkork/openscad-graph-editor/releases) extract it to a convenient location and run the program. No installation procedure is required. 

### Prerequisites for live preview
If you want to have a live preview while you are editing, you will also need to download and install [OpenSCAD](https://openscad.org/downloads.html). Then arrange the windows of OpenSCAD Graph Editor and OpenSCAD so that they are next to each other (or place them on different monitors if you happen to have a multi-monitor setup). It is recommended that you hide the editor in the OpenSCAD window (_Window_ -> _Hide Editor_).

After this, you can set up the location of OpenSCAD in the settings menu:

![](images/enabling_live_preview.png)

1. Press the _Settings_ button.
2. In the _Settings_ dialog, press the _Select_ button. A file dialog will appear. Select the OpenSCAD executable. Where this is located depends on how you installed OpenSCAD and which operating system you are using.
3. Press the _Open_ button to select the OpenSCAD executable.
4. Press the _OK_ button to close the settings dialog and save the settings.

You are now ready to use the OpenSCAD Graph Editor.

## Getting Started
### Nodes
The editor is a graph editor. It allows you to create 3D models by connecting nodes in a graph. Nodes are connected through ports (these are the colored circles left and right of the node). A node can have input ports - the ones on the left - and output ports - the ones on the right. This, for example is a _Cube_ node:

![](images/node_overview.png)

It has two input ports which allow you to configure the size and position of the cube and it has one output port where the node will output the resulting geometry. You may notice that the ports have different colors. These colors indicate what kind of data the port can accept (for input ports) and what kind of data the port will output (for output ports).

![](images/port_colors.png)

The following colors are used:

- _Purple_ - any value except geometry
- _Green_ - a number
- _Red_ - a boolean value (true or false)
- _Very Light Blue_ - a vector2 (a combination of 2 numbers), used for 2D coordinates
- _Light Blue_ - a vector3 (a combination of 3 numbers), used for 3D coordinates
- _Blue_ - a vector (a list of arbitrary length with arbitrary contents)
- _Yellow_ - a string (a text value)
- _White_ - geometry (a 2D or 3D object)

## Adding nodes
There are multiple ways to add nodes to the graph. You can right-click on a free space in the graph to open the _Add Node_ dialog. This contains a list of all available nodes. You can then type the name of the node you want to add and press `Enter` or double-click the node in the list to add it to the graph.

![](images/add_node.mp4)

## Keyboard shortcuts

- `F1` - Open help for the currently selected node.
- `F2` - Change comment for the currently selected node.
- `Q` - straighten connections between the selected nodes.
- `Ctrl+C` / `Cmd+C` - Copy the selected nodes.
- `Ctrl+V` / `Cmd+V` - Paste the copied nodes.
- `Ctrl+X` / `Cmd+X` - Cut the selected nodes.
- `Ctrl+Z` / `Cmd+Z` - Undo the last action.
- `Ctrl+Shift+Z` / `Cmd+Shift+Z` - Redo the last action.
