# 03-review-vdproj: Review TensileDataAnalyzer.vdproj

Review TensileDataAnalyzer.vdproj
   - Note: .vdproj cannot be converted automatically. Recommend leaving as-is or recreate installer using modern tooling after project migration
   - Done when: decision recorded in scenario-instructions.md

## Scope Inventory
- File: TensileDataAnalyzer\TensileDataAnalyzer.vdproj
- Type: Visual Studio Setup Project (.vdproj) — legacy installer project format
- Concern: Not supported by SDK-style conversion tools; conversion requires manual recreation using alternative installer tooling (e.g., WiX, MSIX, or Visual Studio Installer Projects extension)

## Recommendation
- Leave the .vdproj file as-is for now to preserve installer configuration and deployment artifacts.
- After codebase migration, consider one of:
  - Recreate installer using WiX Toolset or MSIX with modern tooling
  - Use the Visual Studio Installer Projects extension (if supported) to maintain similar project format
  - Document installer steps in the repository for CI/packaging

## Next Actions
1. Record decision in scenario-instructions.md (leave as-is) — done by writing progress-details and updating scenario-instructions if user confirms
2. Mark task complete
