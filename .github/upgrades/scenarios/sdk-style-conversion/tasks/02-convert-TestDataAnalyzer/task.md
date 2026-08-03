# 02-convert-TestDataAnalyzer: Convert TestDataAnalyzer

Convert TestDataAnalyzer\TestDataAnalyzer.csproj to SDK-style
   - Update test adapter references to PackageReference as needed
   - Ensure ProjectReference to DataAnalyzer works
   - Remove package imports that reference external packages folder; migrate to PackageReference
   - Done when: project file is SDK-style, tests build

## Scope Inventory
- Projects affected: TestDataAnalyzer (TestDataAnalyzer\TestDataAnalyzer.csproj)
- Distinct concerns:
  - Project file format conversion
  - packages.config migration
  - Test adapter and framework references currently referenced via relative packages folder (..\StressStrainData\packages)
  - Ensure ProjectReference to DataAnalyzer remains valid after conversion
- Files noted for review:
  - TestFileReader.cs, TestLOESS.cs, TestMatrixMath.cs, TestPolynomial.cs, Properties\AssemblyInfo.cs, packages.config
- Skills loaded: converting-to-sdk-style, managing-package-references, building-projects

## Initial Research Findings
- TestDataAnalyzer.csproj is non-SDK-style and includes packages.config
- It imports MSTest.TestAdapter and MSTest.TestFramework via relative HintPaths to ..\StressStrainData\packages
- Project has a ProjectReference to ..\DataAnalyzer\DataAnalyzer.csproj which was converted in the previous task

## Next Actions
1. Convert TestDataAnalyzer.csproj using the SDK-style conversion tool
2. Update package references: migrate MSTest packages to PackageReference and restore
3. Build the project using msbuild.exe and run tests if available
4. Document changes in progress-details.md
