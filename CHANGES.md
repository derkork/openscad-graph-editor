# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2022-08-15
### Added
- The main window now shows the version number in the title bar (fixes [#6](https://github.com/derkork/openscad-graph-editor/issues/6)).

## [0.1.14] - 2022-08-04
### Fixed
- All number literals are now parsed in a culture-invariant way. So all floating point numbers are formatted with a dot as the decimal separator (e.g. `0.12` instead of `0,12`) no matter which OS locale is set (fixes [#4](https://github.com/derkork/openscad-graph-editor/issues/4)).
- A few typos and inconsistencies in the manual have been fixed ([#5](https://github.com/derkork/openscad-graph-editor/issues/5)).

## [0.1.13] - 2022-07-19
- Initial public release
