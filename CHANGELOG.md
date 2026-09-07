# Changelog

All notable changes to this repository are documented here.

## Unreleased

### Added

- Added a dedicated MSTest regression suite for `ObjectListView2022`, targeting .NET Framework 4.8.1.
- Added behavioral coverage for filtering, sorting, grouping, fast/virtual lists, tree lists, check-state handling, Munger access, hot-item behavior, context-menu handling, state persistence, and performance-sensitive paths.
- Added `GetAllObjectsWithMappedCheckState()` for enumerating objects with a specific persisted check state.
- Added Windows CI workflows for building and running the unit test suite in Debug and Release configurations.

### Changed

- Replaced BinaryFormatter-based ObjectListView state persistence with XML serialization while preserving the existing `SaveState()` and `RestoreState()` byte-array API.
- Improved checked-object handling for ordinary lists by avoiding unnecessary timing overhead and reducing sparse-selection allocation cost.
- Restored the optimized non-virtual checked-object retrieval path while preserving virtual-list behavior.
- Preserved and expanded the newer cached Munger implementation and added coverage for repeated reads and writes.
- Aligned keyboard context-menu activation with mouse right-click behavior for the focused item, including Context Menu key and Shift+F10 handling.
- Restored design-time serialization and namespace compatibility metadata needed by existing WinForms designer integrations.
- Updated dependencies and CI metadata.
- Expanded and corrected XML documentation across the public surface.

### Fixed

- Fixed stale TreeListView object indexes after refreshing an expanded branch whose children changed.
- Restored the intended Release-mode default for `Munger.IgnoreMissingAspects`.
- Corrected persistent check-state regression coverage so list rebuild behavior is exercised with an actual column.
- Preserved mapped check states for filtered and virtual objects.
- Corrected XML documentation warnings, including stale or missing parameter documentation and undocumented public members.

### Removed

- Removed obsolete compatibility properties that were no longer needed.
- Removed stale source-control metadata, dead code, unused helpers, commented-out implementation remnants, and formatting debris.
- Removed BinaryFormatter object-graph deserialization from state persistence.

### Compatibility notes

- State saved by the previous BinaryFormatter implementation is not migrated and is rejected by the XML-based `RestoreState()` implementation.
- Applications that still reference the removed obsolete compatibility properties must update to the supported replacements.
- The library continues to target .NET Framework 4.8.1 and remains strongly named.
