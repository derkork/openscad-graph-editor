# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Binary operators now have a context menu option that allows to quickly flip their inputs (fixes [#24](https://github.com/derkork/openscad-graph-editor/issues/24))


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
