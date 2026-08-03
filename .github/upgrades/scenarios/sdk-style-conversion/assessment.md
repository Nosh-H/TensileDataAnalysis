# Assessment: SDK-style Conversion

## Projects to Convert
| Project | Path | packages.config | Custom Imports | Special Type | Risk |
|---------|------|----------------|----------------|--------------|------|
| DataAnalyzer | DataAnalyzer\DataAnalyzer.csproj | Yes | Import of Microsoft.CSharp.targets; Import of Microsoft.Common.props | WinForms app (WinExe); explicit file includes | Medium (explicit file lists, packages.config, WinForms resource handling) |
| TestDataAnalyzer | TestDataAnalyzer\TestDataAnalyzer.csproj | Yes | Imports referencing external packages in ..\StressStrainData\packages; Import of Microsoft.CSharp.targets; TeamTest targets | Unit test project (MSTest) | Medium (package imports via relative paths, packages.config) |
| TensileDataAnalyzer.vdproj | TensileDataAnalyzer\TensileDataAnalyzer.vdproj | N/A | N/A | Visual Studio setup project (.vdproj) — unsupported by SDK-style | High (not convertible; leave as-is or replace with new installer project) |

## Already SDK-style (no action needed)
- None

## Baseline
- Solution builds: Not run yet — will build after conversion steps to establish baseline and validate changes.
- Warning count: N/A

## Key Findings
- Both C# projects target .NET Framework 4.8 and use legacy (non-SDK) project format (no Sdk attribute, explicit <Compile> includes, and imports of Microsoft.CSharp.targets).
- Both projects include packages.config entries and reference NuGet packages using a packages folder layout (some hints reference ..\StressStrainData\packages). These will need migration to PackageReference or preserved references.
- The setup project (.vdproj) is a Visual Studio installer project that is not supported by SDK-style conversion and cannot be converted automatically; recommend leaving it or recreating using modern installer tooling (MSIX/Setup Project extension) after code conversion.
- Test project depends on DataAnalyzer; conversion order should convert DataAnalyzer first, then TestDataAnalyzer.
- WinForms project (DataAnalyzer) requires the Microsoft.NET.Sdk.WindowsDesktop Sdk when converted; keep TargetFramework as net48 (SDK-style supports net48 with WindowsDesktop SDK via UseWindowsForms/UseWPF properties).

## Recommendation
- Convert projects in dependency order: DataAnalyzer → TestDataAnalyzer.
- Migrate packages.config to PackageReference for each project during conversion, or preserve existing package references if migration is risky. Record decisions in plan.
- Preserve AssemblyInfo attributes: SDK-style auto-includes some assembly attributes; ensure no duplicate definitions remain.
- Do not change TargetFrameworkVersion (keep .NET Framework 4.8). Use Microsoft.NET.Sdk.WindowsDesktop for WinForms when converting DataAnalyzer.


