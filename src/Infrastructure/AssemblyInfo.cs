using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;

[assembly: AssemblyTitle(Cms21ImmersionPlus.BuildInfo.Name)]
[assembly: AssemblyDescription(Cms21ImmersionPlus.BuildInfo.Description)]
[assembly: AssemblyCompany(Cms21ImmersionPlus.BuildInfo.Company)]
[assembly: AssemblyProduct(Cms21ImmersionPlus.BuildInfo.Name)]
[assembly: AssemblyCopyright("CMS21 Immersion+ contributors; based in part on QoLmod by Meitzi")]
[assembly: AssemblyVersion(Cms21ImmersionPlus.BuildInfo.Version)]
[assembly: AssemblyFileVersion(Cms21ImmersionPlus.BuildInfo.Version)]
[assembly: AssemblyCulture("")]
[assembly: MelonInfo(typeof(Cms21ImmersionPlus.Main), Cms21ImmersionPlus.BuildInfo.ShortName,
    Cms21ImmersionPlus.BuildInfo.Version, Cms21ImmersionPlus.BuildInfo.Author, Cms21ImmersionPlus.BuildInfo.DownloadLink)]
#if NET6_0_OR_GREATER
[assembly: MelonColor(255, 4, 163, 204)]
#else
[assembly: MelonColor()]
#endif
[assembly: MelonGame(Cms21ImmersionPlus.BuildInfo.MelonGameCompany, Cms21ImmersionPlus.BuildInfo.MelonGameName)]
[assembly: HarmonyDontPatchAll]
[assembly: ComVisible(false)]
[assembly: Guid("90264309-A13B-47CF-92CA-464C40F95ECB")]
