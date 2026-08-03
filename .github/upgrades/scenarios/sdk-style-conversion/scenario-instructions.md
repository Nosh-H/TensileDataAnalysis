# SDK-style Conversion

## Preferences
- **Flow Mode**: Automatic

## Source Control
- **Source Branch**: master
- **Working Branch**: sdk-style-conversion-1
- **Commit Strategy**: After Each Task
- **Branch Sync**: Manual (user will merge master into working branch as needed)

## Key Decisions Log
- 2026-07-31: **Decision**: Leave TensileDataAnalyzer.vdproj in the repo as-is (do not attempt automatic conversion). Rationale: .vdproj is a legacy Visual Studio installer project not supported by SDK-style conversion; recommend recreating installer with modern tooling later.
