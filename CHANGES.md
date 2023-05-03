# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Improved
- When moving external SCAD files around the editor now handles absent files more gracefully. It will show a warning when trying to load a file that no longer exists or has become invalid. It will operate on the last known good state. This makes it easier to move files around without breaking your graph. ([#36](https://github.com/derkork/openscad-graph-editor/issues/36)).
- Operator nodes now automatically switch their input and output types depending on what is connected to them. It is no longer necessary to switch the operator input type manually. For operator nodes it is now also possible to draw a connection to an input port if the type currently does not fit. The operator node will automatically adjust the type of the input port to match the new incoming connection. This greatly eases working with operator nodes.
- All context menus are now created through a central code instance making their layout and item order consistent no matter if you right-click something in the tree or a node in the graph.
- A lot of code has been reorganized under the hood to make it easier to add new features and fix bugs.

### Fixed
- Reroute nodes now properly restore their port type to "Reroute" when you remove the last connection from them. Before they sometimes kept the previously used port type which made it impossible to connect them to other nodes.

## [0.9.3] - 2023-04-05
### Improved
- The editor is now properly code signed and notarized by Apple. This should make it easier to install and use it on Mac OSX. ([#33](https://github.com/derkork/openscad-graph-editor/issues/33)). **A huge thanks goes out to [Will Adams](https://github.com/WillAdams) for donating the funds on [ko-fi](https://ko-fi.com/derkork) to make this possible!**


## [0.9.2] - 2023-04-02
### Added

- You can now select all nodes in a graph by pressing `Ctrl+A`/`Cmd+A`.

### Improved

- The stylus debug dialog shows now more information about received events. Information can be easily copied and pasted into bug reports.

### Fixed
- Pressing a combination of `W`, `A`, `S` and `D` with `Cmd`/`Ctrl` no longer will trigger a node layout.

## [0.9.1] - 2023-03-21
### Fixed

- Selecting the OpenSCAD executable in the settings dialog now again works correctly. This was broken in 0.9.0.

## [0.9.0] - 2023-03-02
### Added
- Added support for OpenScad's `surface` and `import` functions. You can now import 3D models and heightmaps into your graph ([#17](https://github.com/derkork/openscad-graph-editor/issues/17))

### Improved
- You can now also delete nodes by pressing the `Backspace` key, which is more convenient when working with a Macbook keyboard ([#43](https://github.com/derkork/openscad-graph-editor/issues/43)).

## [0.8.0] - 2023-02-26
### Added
- Added missing `intersection` node. This now allows you to generate intersections of multiple objects.
- You can now easily duplicate modules, functions and variables by right-clicking them and selecting _Duplicate_ ([#41](https://github.com/derkork/openscad-graph-editor/issues/41)).
- Added a range of utility nodes that simplify common use cases ([#42](https://github.com/derkork/openscad-graph-editor/issues/42)):
  -  `Double` allows you to calculate the double of an input number or vector.
  - `Half` allows you to calculate the half of an input number.
  - `MinusOne` allows you to subtract one from an input number.
  - `PlusOne` allows you to add one to an input number.

## [0.7.0] - 2023-01-04
### Added
- You can now set up a preamble which will be rendered on top of the generated OpenSCAD code. This is useful if you want to add a license header or other comments to the generated code ([#39](https://github.com/derkork/openscad-graph-editor/issues/39)).

### Changed
- The code preview now uses a monospace font. This makes it easier to read the generated code.
- Includes are now rendered before variable declarations. This allows overwriting variables in included files.
- When you try to load a file that no longer exists the editor will now show an error message instead of silently failing.

### Fixed
- `include` and `use` statements are now correctly generated without a terminating semicolon ([#39](https://github.com/derkork/openscad-graph-editor/issues/39)).
- The directory of the currently open file is now remembered correctly no matter whether you use the file dialog or the recent file list to open a file ([#40](https://github.com/derkork/openscad-graph-editor/issues/40)).


## [0.6.1] - 2022-12-12
### Fixed
- You can now create [special variables](https://en.wikibooks.org/wiki/OpenSCAD_User_Manual/Other_Language_Features#Special_variables) starting with `$`. This was not allowed before, but should have.


## [0.6.0] - 2022-12-07
### Added
- You can now delete connections by right-clicking them. This is especially useful when multiple connections go into the same node, where until now you had to one by one drag them out of the node until you had the correct one ([#28](https://github.com/derkork/openscad-graph-editor/issues/28)).
- You can now quickly insert a reroute node on a connection by moving the mouse over a connection and then `Shift`+`Right-Click`. If you additionally hold `Ctrl`/`Cmd` while doing this a wireless reroute node is inserted. This complements the already existing functionality of deleting a reroute node with `Shift`+`Delete`, which will delete the reroute node but keep the connection.

## [0.5.2] - 2022-12-02
### Added
- It is now possible to modify references to external files (until now you could only delete and add them again).

### Fixed
- Importing external references has received a complete overhaul. The import now properly handles transitive dependencies and handles files which are included multiple times through different paths. It can now also handle functions modules and variables being moved between files.


## [0.5.1] - 2022-11-29
### Fixed
- Spatially [arranging nodes](manual/manual.md#things-to-keep-in-mind-when-using-variables) to affect output order now also properly works for nested modules.


## [0.5.0] - 2022-11-07
### Added
- Support for OpenSCAD's customizer. You can now set up if and how variables should appear in the customizer ([#23](https://github.com/derkork/openscad-graph-editor/issues/23)). 
- Variables now have default values. A new panel has been added that allows you to quickly set initial values for all variables. This largely alleviates the need to create a _Set Variable_ node to initialize each variable.
- Binary operators now have a context menu option that allows to quickly flip their inputs (fixes [#24](https://github.com/derkork/openscad-graph-editor/issues/24))
- You can now duplicate nodes by pressing `Ctrl+D`/`Cmd+D`.
- When deleting a reroute node, you can now hold `Shift` to retain the reroute node's connections (e.g. only delete the node itself).
- When you open a file, the editor will create a backup of the file in the same directory with the extension `.1`. Older backups will be renamed to `.2`, `.3`, etc. You can change the number of backups that are kept in the settings (default is 5, set to 0 to disable backups).

### Changed
- The project tree is now expanded by default.

## [0.4.1] - 2022-09-05
### Fixed

- When changing the parameter/return type of modules and functions the connections to these parameters will now only be removed if the connection would be invalid with the new type. Before, all connections to these parameters would be deleted.  (fixes [#20](https://github.com/derkork/openscad-graph-editor/issues/20)).

## [0.4.0] - 2022-09-05
### Breaking change
- Nodes are now translated into OpenSCAD code from top to bottom and then left to right. Before, the order was undefined. This change allows you to influence the order in which assignments of variables happen and where exactly in the code `echo` statements are executed.

  Depending on how you use variables and `echo` nodes in your graph this change may modify the behaviour of your graph, hence I declare this a breaking change. You may need to rearrange these nodes in your graph to get back the previous execution order.

### Added
- The manual now contains a section on [how to use variables](manual/manual.md#variables) (fixes [#18](https://github.com/derkork/openscad-graph-editor/issues/18)).

## [0.3.1] - 2022-08-22
### Fixed

- When you undo a change the editor no longer switches to an unrelated tab (fixes [#16](https://github.com/derkork/openscad-graph-editor/issues/16)).
- The _Negate_ node now can also be used for vectors (fixes [#14](https://github.com/derkork/openscad-graph-editor/issues/14))
- When doing undo/redo the graph is now properly repainted.

## [0.3.0] - 2022-08-20
### Added
- It is now possible to rename variables.
- When you press the `Enter` key, in the variable edit dialog the dialog closes like if you would have clicked the _OK_ button (fixes [#11](https://github.com/derkork/openscad-graph-editor/issues/11)).
 
### Fixed
- When you align nodes to the right, their right edge is now aligned properly (fixes [#12](https://github.com/derkork/openscad-graph-editor/issues/12)).
- Color values are now properly rendered regardless of the current system locale (fixes [#10](https://github.com/derkork/openscad-graph-editor/issues/10)).
- Deleting a function/module that is currently open no longer will throw an exception.
- When renaming/creating a module/function the dialog will now check that the name is not already used.
- When creating a function with no parameters, the renderer will no longer throw an exception.

## [0.2.0] - 2022-08-15
### Added
- The main window now shows the version number in the title bar (fixes [#6](https://github.com/derkork/openscad-graph-editor/issues/6)).

## [0.1.14] - 2022-08-04
### Fixed
- All number literals are now parsed in a culture-invariant way. So all floating point numbers are formatted with a dot as the decimal separator (e.g. `0.12` instead of `0,12`) no matter which OS locale is set (fixes [#4](https://github.com/derkork/openscad-graph-editor/issues/4)).
- A few typos and inconsistencies in the manual have been fixed ([#5](https://github.com/derkork/openscad-graph-editor/issues/5)).

## [0.1.13] - 2022-07-19
- Initial public release
