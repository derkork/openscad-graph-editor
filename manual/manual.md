# OpenSCAD Graph Editor Manual

## Table of contents

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

## The basics
### Starting a new project

When you start OpenSCAD Graph Editor, it will show a blank graph. You can already add nodes to the graph but live preview is not enabled until you actually save the project somewhere. This is because live preview requires that OpenSCAD code is generated and written to a file so that the OpenSCAD executable can read it and produce a live preview. So you should immediately save the project somewhere. You can save it by pressing the _Save as..._ button.  After the project is saved you can start live preview by pressing the OpenSCAD icon in the top right corner of the editor, provided that you have set it up correctly. If you haven't set this up yet, check the previous section.

![](images/starting_live_preview.png)

From now on you will no longer need to manually save the project. The editor will automatically save the project when you change something in the graph. This will also trigger a refresh in the live preview.

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

### Adding nodes
There are multiple ways to add nodes to the graph. You can right-click on a free space in the graph to open the _Add Node_ dialog. This contains a list of all available nodes. You can then type the name of the node you want to add and press `Enter` or double-click the node in the list to add it to the graph. All built-in nodes come with a unique short name which is indicated in square brackets behind the node's full name. For example the _Cube_ node has the short name _Cbe_. This feature allows you to quickly add nodes by typing their short name and pressing `Enter`.

![](images/add_node.gif)

If you already have a node in your graph, you can also drag from one of its ports (input or output) to a free space in the graph. This will also open the _Add Node_ dialog. In addition it will filter the selection of nodes to only show the ones that can be connected to the port you dragged from. If for some reason you don't want the list filtered, you can uncheck the _Filter by current context_ checkbox in the _Add Node_ dialog.

![](images/add_node_by_dragging.gif)

After you have selected a node from the list the node will be added to the graph and will automatically be connected to the port you dragged from. 


### Getting help for a node

Sometimes it may not be immediately obvious what a node does and what needs to be connected to its inputs and outputs. You can get help for any node by selecting it and then pressing the `F1` key on your keyboard. This will show a help screen for that node which describes what the node does.

![](images/help_screen.gif)


### Connecting nodes

You can connect nodes by dragging one of output ports of a node to an input port of another node or vice versa. Every connection must be made between an input port and an output port. The connections indicate how data flows between nodes. **Data always flows from left to right.** This is true for all port types (including geometry). Let's explore this in a few examples:

![](images/flow_example_1.png)

Here you have two nodes representing the numbers 7 and 4. Their output goes into the two input ports of an addition node (`+`). This node will output the sum of the two numbers from its input ports. Then its output is connected to one input port of a multiplication node (`*`). This node will output the product of the two numbers from its input ports. You can see that nothing is connected to the second input port of the multiplication node. When you connect nothing to a node's input port, depending on the type of the port you can enter values directly. In this case `2`  was entered, so the multiplication node will now multiple the result from the addition node by 2. You can hover over nodes to see what the node will produce. In this example the whole chain of nodes will produce the expression `((7 + 4) * 2)`.

Lets check another example which involves geometry:

![](images/flow_example_2.png)

Here we have two nodes which create cube(oid)s. The first creates a cube with a size of 2x2x2 units. The second node creates a cuboid with a size of 1x3x1 units. We then add a _Difference_  node which subtracts the second cube from the first. As a result we get a 2x2x2 cube with a hole in the middle. In the example screenshot the second cube has been given a debug modifier so you can see it in translucent red in the 3D view. Without this modifier it would not be visible. Again you can see that **data always flows from left to right**. The two cubes on the left are the input for the _Difference_ node on the right and the _Difference_ node's output will contain new data produced from its inputs.

#### Port types

In general you can only connect ports which have the same port type (e.g. the same color), e.g. it just doesn't make sense to connect a boolean value to a port expecting geometry. There are some exceptions to this rule though:

- A port type of `any` (purple) can be connected to any other port type except `geometry` (white) and vice versa.
- A port type of `vector3` (light blue) can be connected to a port type of `vector` (blue) but not the other way around.
- A port type of `vector2` (very light blue) can be connected to a port type of `vector` (blue) but not the other way around.

Every input port (except geometry, see the next section for details) can only accept a single connection. It is also not allowed to connect nodes in a way that would form a loop. 

#### Connecting geometry ports

Geometry (white) ports are somewhat different from the other ports. They follow a concept called _implicit union_. This means that when you connect two geometry output ports into a single geometry input port, the resulting geometry will be the union of the two input geometries.

![](images/implicit_union_example.png)

We first have a 2x2x2 cube and a sphere with 1.2 units radius. Both of them are connected to the _Add_ port of the _Difference_ node. Then we have two cuboids forming a cross-like shape. Both of these are connected to the _Subtract_ port of the _Difference_ node. What will happen now is that the _union_ of everything connected to the _Subtract_ port will be subtracted from the _union_ of everything connected to the _Add_ port. So we get a cube with some parts of the sphere on the outside with two holes made by the two cuboids. Again for visualization purposes a debug modifier was added to the cuboid nodes, so you can see them in translucent red in the 3D view.

The _implicit_ union behaviour simplifies the node graphs as you don't need to add extra nodes when you want to build the union of some geometry.  

#### Unconnected geometry output ports

You may have noticed that in the previous examples the geometry output of the _Difference_ node is not connected to anything. Yet it still renders in the output. This is another side effect of the _implicit union_ behaviour. All geometry ports which are not connected to anything else will be implicitly added to the output geometry. So let's check another example:

![](images/implicit_union_example_2.png)

Here we have two spheres with a radius of `1`. The first sphere is translated on the x-axis by `-2`, the second sphere is translated on the x-axis by `2`. Both _Translate_ nodes are not connected to anything else. So the _implicit union_ behaviour will add the two translated spheres to the output geometry and this is what you can see in the result.

#### Literals for unconnected input ports

When you connect something to an input port the port will use the value that was connected to it. If you don't connect something to an input port, most ports (all except _geometry_, _vector_ and _any_) will allow you to specify the value directly in the node without having to create an extra node and connecting it to the port. This can help keep your node graphs smaller and simpler. Lets see this in action:

![](images/literals_example.png)

Here we have two cube nodes and a translate node. All nodes have nothing connected to their inputs, so their input ports now allow you to specify the value directly. You may notice that the cube node in the top left has two little buttons next to each of its input ports. These buttons are displayed when input port has a default value. Cube's have a default size of `[1,1,1]` and are not centered by default. When the button is not pressed, the built-in default value for that port will be used. If you press the button, you can override the default value with a custom value. This can be seen in the top right cube node where both the _Size_ and the _Center_ value are overridden. 

Some input ports may not have a default value and are therefore required. This is the case for the _Translate_ node. It does not have a default value for the translation _Offset_ so the little button is not displayed. You can still specify the value directly in the node or connect another node to the input.


## Overview of built-in nodes

OpenSCAD graph editor contains nodes for all [functions and modules](https://openscad.org/cheatsheet/) built into OpenSCAD. So if you look for a certain function you can just type its name into the _Add node_ dialog. 

There are some nodes which are not directly representing a function in OpenSCAD but are used to achieve the same effects as a built-in language construct in the original OpenSCAD language. These will be described in the following sections.

### Flow control nodes

#### Branch (if/else)

The branch node allows you to render geometry depending on a condition. The condition is specified by a boolean expression.

![](images/branch.png)

The node will output the geometry connected to the _true_ port if the condition is `true`, and the geometry connected to the _false_ port if the condition is `false`. In the example it would output the geometry of the sphere as the condition is not connected and set to `false`.

#### For-Loop

The for-loop node allows you to repeatedly create geometry with different inputs. It actually consists of two nodes: _Start loop_ and _End loop_ but you will only need to add a _For loop_ node and the _Loop end_ node will be added automatically. Whenever you select any of the two nodes belonging to a loop the editor will automatically highlight the other so you can keep track of which _Start loop_ and _End loop_ nodes belong together.

Consider the following example of a for-loop:

![](images/loop_example.png)

This will create a circle of spheres offset by 30 degrees from each other. The _Start loop_ node has an input port for any vector-like construct (e.g. a `vector2`, `vector3` or `vector`). The loop will be repeated for each item in the given vector. In this example we use the _Construct Range_ node to build a vector that has the numbers `0` to `360` in it with steps of `30`. 

We can give the loop variable a name (though it is not needed, if you don't give a name then OpenSCAD graph editor will internally generate a unique variable name for you). Now inside the loop we build a sphere, offset it by 10 units on the x-axis and rotate it by the current angle. The result of this will connected to the _End loop_ node. This will generate 12 spheres and because of the _implicit union_ behaviour they will be added to the output geometry one by one. The result is a circle of spheres.

##### Nested loops

You can nest loops inside of each other by creating multiple _For loop_ nodes. However this quickly can clutter your node graph. Instead, you can right-click on a _Start loop_ node and select the _Add loop nest level_ option:

![](images/add_loop_nest_level.png)

This will add a second input to the _Loop start_ node and the loop will now repeat for each combination of values in the two input vectors. Consider this example:

![](images/nested_loop_example.png)


We again use a _Construct range_ node, to build a range of the numbers `0` to `9`. Then we increase the loop nest level to 3 so we have three loops running running over all combinations of three vectors holding the numbers `0` to `9`. This will in total yield `1000` iterations (`10 * 10 * 10` numbers). We call the loop variables `x`, `y` and `z`.  Now we use a _Construct `vector3`_ to build a new `vector3` from the current loop variables. We multiply this vector by 2 so we will get `0,0,0`, `0,0,2`, etc.. Now we use these coordinates to translate a cube in three-dimensional space. The result is `1000` small cubes forming a bigger cube.

#### Intersection loops

A _For loop_ by defaults returns the union of all iterations. Sometimes you want to return the intersection of all iterations. To do this, right-click the _Loop start_ node and select the _Toggle intersection mode_ option. This will will switch the _For loop_ to an _Intersection-for loop_.

![](images/intersect_for.gif)

In the example we have three spheres which are created by a _For loop_. You can see the three spheres in the output geometry. Now we switch the _For loop_ to an _Intersection-for loop_ and we get only the intersection of all spheres.

### Vector nodes

Vectors play a large role in OpenSCAD. They are used to represent positions and directions and are also a generic way of grouping data. The following nodes are used to create and manipulate vectors:

#### Construct `vector2` / `vector3`

These nodes allow you to build a `vector2` or `vector3` from numbers. 

![](images/construct_vector23.png)

You can either give the values directly or connect other nodes to the input ports.

#### Split `vector2` / `vector3`

These node do the opposite of _Construct `vector2`/`vector3`_ - they allow you to extract the values of a `vector2` or `vector3`:

![](images/split_vector23.png)


You connect a `vector2` or `vector3` to the input port and the output port will contain the values of the vector.

#### Construct vector `any`/`number`/`string`/`boolean`/`vector2`/`vector3`

These nodes allow you to construct a vector of arbitrary length that contains values of different types.

![](images/construct_vectorxy.png)

These nodes are useful if for example you want to create a list of points as input for the _Polygon_ or _Polyhedron_ nodes. You can add new items to the vector by right-clicking it and selecting _Add item_.

![](images/add_item.png)

Similarly you can remove items from the vector by right-clicking it and selecting _Remove item_.

![](images/remove_item.png)

#### Index `vector` / `string`

This node allows you to extract a value from a `vector` or `string` value. 

![](images/index_vector_string.png)

In this example we have a `vector` with three values (`7`, `6`, `2`). The node is configured to extract the value at index 1. So the output port will contain `6` (indices are zero-based, so an index of `0` will return the first value, `1` will return the second value and so on). For strings this will return the character at the given index.

#### Pairwise vector multiplication `vector2` / `vector3` / `vector`

This special node takes two `vector2`, `vector3` or `vector` values. It multiply the values of the given vectors pairwise and returns a new `vector2`, `vector3` or `vector` with the result. 

![](images/pairwise_multiply.png)

This functionality is not actually built into OpenSCAD (e.g. using the normal multiplication operator `*` will return the dot product of the vectors). Under the hood this will build a loop that walks over the vectors and multiplies the values pairwise. This was added as this operation is quite useful and would otherwise require you to build a lot more nodes.

### Miscellaneous nodes
#### Cast

Sometimes a node needs an input value that doesn't match the type of the output value. For example you may want to construct a range which has 3 items in it and technically is a `vector`. But in this case you are sure it is actually a `vector3`. The editor has no way of knowing this without actually running the program, so it will not let you connect the output of _Construct range_ to anything that takes a `vector3`. You can use the _Cast_ node to work around this. The _Cast_ node has an input and output port of type `any` so it can be connected to anything (except geometry).

![](images/cast.png)

The _Cast_ node will not magically convert a node to a different type. It is simply a way of telling the editor: _"I know what I am doing, trust me that this is the correct type."_ If you connect ports that actually do not match the OpenSCAD compiler will show you an error when you try to run the program.

## Keeping the graph neat and tidy

When you add and connect a lot of nodes it is easy to quickly get into a big ball of spaghetti which is not very readable. You want to keep your graph as tidy as possible, so you can look at it some time later and still know what you were doing. OpenSCAD graph editor has some features to help you keep your graph clean.

### Comments

You can add a comment to any node by right-clicking it and selecting _Add comment_ or by selecting a node and pressing the `F2` key. This allows you to add a short comment which will appear on top of the node.

![](images/node_comments.gif)


### Reroute nodes

When you have quite a few nodes connected to each other it can become difficult to avoid having connections crossing over other connections and thereby making the graph look messy. The _Reroute_ node allows you to reroute a connection, so you can prevent connections from crossing over each other. You can create a reroute node either by using the _Add node_ dialog or by holding `Shift` while you drag a connection to an empty space. 

#### Wireless reroute nodes

Sometimes even with reroute nodes a graph will become too messy because there are just so many connections and there is no really good way of routing them so they don't create spaghetti. For this case you can use a _Wireless reroute_ node. This works the same way as a normal reroute node but it only shows its connection to the input port when it is selected. This can be very useful in de-cluttering dense graphs. You should however use the wireless nodes sparingly as they keep you from understanding your node graph at a glance.

To create a _Wireless reroute_ node you can either right-click an existing reroute node and select _Make wireless_ in the popup menu or you can hold `Ctrl+Shift` while dragging a connection to an empty spot.

You can also turn a _Wireless reroute_ node back into a normal _Reroute node_ by right-clicking it and selecting _Make wired_ in the popup menu.

The following image shows how you can clean up a cluttered node graph with reroute nodes:

![](images/using_reroute_nodes.gif)

### Straighten connections

You can straighten the connections between nodes by selecting a the nodes between which you want to straighten the connections and then pressing the `Q` key. 

Connections will be straightened from right to left, e.g. the right-most node will never be moved. The algorithm will work from the right-most node and then move nodes left of the right-most node up and down so that the first connection between them is straight. 

## Debugging aids

Sometimes it is not immediately obvious why something is not working as expected. OpenSCAD has some debugging aids built-in which you can quickly toggle for all nodes which produce geometry.

### Changing the color of the output

A very simple way to see what is going on is simply adding a bit of color to a node so you can see which node produces which output. You can color the output of a node by right-clicking it and selecting _Set color_.

![](images/changing_color.gif)

In this example we give sphere a purple color and the cube a blue color.

### Debug modifiers

OpenSCAD provides several modifiers which change the way a sub-tree is rendered. You can quickly toggle these modifiers for any node outputting geometry through the node's context menu.

![](images/debugging_aids_context_menu.png)

#### Debug subtree

The _Debug subtree_ modifier will render the node's output in a translucent red color. This is for example useful in debugging a  _Difference_ node as it allows you to see the geometry that is being subtracted.

![](images/debug_modifier.png)

In this example where two cylinders are subtracted from a cube, one cylinder has a debug modifier applied. You can see this by the little "bug" icon above the node. Now the cylinder is rendered in a translucent red color.

#### Background subtree

The _Background subtree_ works similar to the _Debug subtree_ modifier but it will render the node's output in a translucent grey color. Also the output of the node will not be used for any subsequent operations in the graph.

![](images/background_modifier.png)

This is the same example as above but with the background modifier applied. You can see the cylinder is rendered in a translucent grey color and is no longer subtracted from the cube.


#### Disable subtree

The _Disable subtree_ modifier will disable the node's output. This is useful if you want to temporarily disable a node.

![](images/disable_modifier.png)

The second cylinder is now disabled. It's node is being displayed slightly translucent in the graph editor to indicate this. The cylinder does no longer appear in live preview.

#### Make root modifiers

The _Make root modifiers_ modifier is essentially the opposite of the _Disable subtree_ modifier. It will disable all other nodes and only render the node with which has the modifier applied.

![](images/make_root_modifier.png)

Now only the cylinder is rendered, everything else is disabled. Only one node can have this modifier at a time. If you add the modifier to another node, it will remove the modifier from the node which previously had it.


## Reusable code with functions and modules
### Modules
#### Creating a new module
OpenSCAD allows you to build custom modules, so you can reuse code. In OpenSCAD graph editor you can also create custom modules. To create a new module, click the _M_ icon above the project tree. 

![](images/create_module.gif)

A popup dialog will open. In this dialog you can specify the name of the module and any parameters and their types.  Once you are done press _OK_ to create the module. A new tab will open in which you can edit the graph of the module. It will already contain one node - the module's entry point - from which you can drag out the parameters given to your module.

Editing a module graph works exactly the same as editing the main graph (the main graph is also a module in a sense). Also the rules about _implicit union_ behaviour apply, so every node that has an unconnected _geometry_ port will be implicitly added to the output geometry of the module. 

#### Using a module

To use a module in you main graph (or another module) you can either add it through the _Add Node_ dialog by just typing the module's name or you can drag it from the project tree into the graph like this:

![](images/drag_module.gif)

#### Default values for module parameters

Module parameters can be given default values. To specify a default value for a module parameter click the pen icon next to the parameter's name in the module's entry point. This will then add a literal field to this parameter where you can enter the default value:

![](images/default_parameters.gif)


#### Building operator modules

OpenSCAD allows you to build so-called operator modules. These are modules which modify input geometry. You can make any module an operator module by adding the _Child_ or _Children_ nodes. These nodes allow you to access the input geometry of the module. For example if you want to make a module that moves everything by 20 units on the x-axis, you could build it like this:

![](images/operator_module_example.png)

If you now use the module in another graph, you will notice that it has gotten an additional geometry input. This is where you will connect geometry that should be modified by the operator module:

![](images/operator_module_usage_example.png)


#### Changing the module's name and parameters

It is possible to rename a module and add, reorder, rename and delete parameters. To do so, right-click either the module's entry point or any instance where you use the module. A popup menu will open. Select the _Refactor &lt;module&gt;_ entry.

![](images/refactoring_popup.png)

Now the refactoring dialog will open and you can change the module's name, the name and type of any of its parameters. You can also reorder parameters by pressing the up and down buttons next to each parameter. You can also delete parameters. Once you are done, press _OK_ to apply the changes.

![](images/refactoring_dialog.png)

This will automatically update the module's entry point and all places where the module is used. The following rules will apply:

- All name changes will be reflected in the node view and the generated code.
- If you reorder parameters, connections will be preserved and reordered accordingly.
- If you rename a parameter this has no effect on the connections to this parameter, e.g. all connections to the parameter will remain as they were.
- If you delete a parameter, all connections to that parameter will be deleted.
- If you add a parameter it will be available as new port for the module's node but no connections will be created for it.
- If you change parameter types, the connections to the parameter will be preserved as long as the types are compatible. If the types are no longer compatible, connections to that parameter's port will be deleted.

#### Finding usages of modules

After you have refactored a module, you may want to check places where this module is used e.g. to add new connections to an added parameter. To find all places where a module is used, right-click the module's entry point or any instance where you use the module. A popup menu will open. Select the _Find usages of &lt;module&gt;_ entry. A tool window will open with a list of all places where the module is used. You can double-click any entry in this list and the corresponding node will be shown on screen and briefly highlighted.

![](images/usage_search.gif)


#### Adding documentation to modules

You can add documentation to your modules, so you and other users can understand what the module does. To add documentation to a module, right-click either the module's entry point or any instance where you use the module. A popup menu will open. Select the _Edit documentation of &lt;module&gt;_ entry:

![](images/edit_documentation_popup.png)

Now a dialog will open where you can add descriptions to the module and the parameters. 

![](images/documentation_dialog.png)

When you are done, press _OK_ to save the documentation. The documentation will now be shown if you request help for the module (by pressing `F1`). 

![](images/port_colors.png)

It will also be rendered out in the generated OpenSCAD code. 

![](images/documentation_in_code.png)

### Functions

Functions work very much like modules, except that they have a return value and they cannot create or work with geometry. You can add a new function by pressing the _F_ icon above the project tree. A popup dialog will open. In this dialog you can specify the name of the function, its return type and any parameters and their types. Once you are done press _OK_ to create the function. A new tab will open in which you can edit the graph of the function. It will already contain two nodes:

- the function's entry point, from which you can drag out the parameters given to your function
- the function's return value node which has an input port for the function's return value

![](images/create_function.gif)

#### Making a function return a value

Functions have an additional node where you can connect the return value of the function. For example, let's see a function that simply adds two numbers:

![](images/add_two_numbers_example.png)

The two numbers are given as parameters to the function. Then we use an _Add_ node to add the two numbers. The output of the _Add_ node is connected to the _Return_ node's input port, so now the function will return the sum of the two numbers.

From here on functions work exactly the same as modules. So you can use them the same way by dragging them from the project tree into the graph, change, move or delete parameters and the return type, find their usages and can edit their documentation in the same way as you do for modules. 

## Reference
### Keyboard shortcuts

- `F1` - Open help for the currently selected node.
- `F2` - Change comment for the currently selected node.
- `Q` - straighten connections between the selected nodes.
- `Ctrl+C` / `Cmd+C` - Copy the selected nodes.
- `Ctrl+V` / `Cmd+V` - Paste the copied nodes.
- `Ctrl+X` / `Cmd+X` - Cut the selected nodes.
- `Ctrl+Z` / `Cmd+Z` - Undo the last action.
- `Ctrl+Shift+Z` / `Cmd+Shift+Z` - Redo the last action.
- Holding `Shift` while dragging a connection to an empty space - Create a reroute node.
- Holding `Ctrl+Shift` while dragging a connection to an empty space - Create a wireless node.

### Documentation comment format
OpenSCAD graph editor uses a standardized format for documentation comments. If you follow this format in text-based OpenSCAD libraries, OpenSCAD graph editor will be able to parse the format and show better documentation for the nodes when pressing `F1`. 

#### An example
```openscad

/**
 * Lorem ipsum dolor sit amet, consetetur sadipscing elitr,
 * sed diam nonumy eirmod tempor invidunt ut labore et dolore
 * magna aliquyam.
 * @param a Lorem ipsum dolor sit amet
 * @param b Lorem ipsum dolor sit amet
 * @param c [vector3] Lorem ipsum dolor sit amet 
 */
module foo(a = 10, b = [1,2,3], c) { /* ... */ }

/**
 * Lorem ipsum dolor sit amet, consetetur sadipscing elitr,
 * sed diam nonumy eirmod tempor invidunt ut labore et dolore
 * magna aliquyam.
 * @param a [number] Lorem ipsum dolor sit amet
 * @param b ipsum dolor sit amet
 * @return [number] diam nonumy eirmod tempor invidunt
 */
function bar(a, b = "foo") = (/* ... */) ;
```
Parameters for both modules and functions are documented as:

```
@param <name> [<type>] <description>
```

where the type is written in `[]` and is optional. Return values of functions are documented as: 

```
@return [<type>] <description>
``` 

where again the type is written in `[]` and is optional.

#### Supported data types
- `vector2` - a 2-dimensional vector of numbers
- `vector3` - a 3-dimensional vector of numbers
- `vector` - an vector of arbitrary length and/or arbitrary contents
- `boolean` - a boolean value
- `number` - a numeric value
- `string` - a string value
- `any` - no specific type or mixed


#### Parsing Rules

When OpenSCAD parses parameters and return values it uses the following rules to infer the data type:

##### For parameters
In this order, first matching rule wins:

- When a type is explicitly documented this type will be used. If the documented type is none of the supported types it is assumed to be `any`.
- If no type is documented and the parameter has a default value, the type is inferred from the default value. The following rules apply for type inference (in this order, first match wins):
    - if the default value is a number literal: `number`
    - if the default value is a string literal: `string`
    - if the default value is a boolean literal: `boolean`
    - if the default value is an vector literal with 3 values in it and all values are number literals: `vector3`
    - if the default value is an vector literal with 2 values in it and all values are number literals: `vector2`
    - if the default value is an vector literal: `vector`
- Otherwise: `any`

##### For return values
In this order, first matching rule wins:

- When a type is explicitly documented this type will be used. If the documented type is none of the supported types it is assumed to be `any`.
- Otherwise: `any`