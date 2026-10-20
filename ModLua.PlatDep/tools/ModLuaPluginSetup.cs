// ModLuaPluginSetup.cs - shipped inside the UPM package (Editor/ folder).
// Copied verbatim by tools/pack.ps1; do not reference this file from the
// ModLua.PlatDep project itself.
//
// Purpose: guarantee the platform checkboxes of the shipped plugins are
// correct even if Unity regenerated the .meta files (fresh import, Library
// wipe, or a .meta format the current Unity version chose to reset).
// The two managed dlls share the assembly name "ModLua.PlatDep" - their
// platform sets MUST be mutually exclusive or Unity reports duplicate
// assemblies. This script is idempotent: it only calls SaveAndReimport when
// something actually changes.
#if UNITY_EDITOR
using System;
using UnityEditor;

namespace Mod.Lua.Editor
{
    [InitializeOnLoad]
    internal static class ModLuaPluginSetup
    {
        static ModLuaPluginSetup()
        {
            ConfigureManaged();
            ConfigureNative();
        }

        private static void ConfigureManaged()
        {
            // Finds every "ModLua.PlatDep.dll" under Assets or Packages.
            // The iOS flavor lives in a path containing "iOS".
            foreach (string guid in AssetDatabase.FindAssets("ModLua.PlatDep"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith("ModLua.PlatDep.dll", StringComparison.OrdinalIgnoreCase))
                    continue;
                var imp = AssetImporter.GetAtPath(path) as PluginImporter;
                if (imp == null) continue;
                bool isIos = path.Replace('\\', '/').Contains("iOS/");
                Configure(imp,
                    anyPlatform: false,
                    editor: !isIos,
                    android: !isIos,
                    standalone: !isIos,
                    ios: isIos);
            }
        }

        private static void ConfigureNative()
        {
            foreach (string guid in AssetDatabase.FindAssets("lua55"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string p = path.Replace('\\', '/');
                var imp = AssetImporter.GetAtPath(path) as PluginImporter;
                if (imp == null) continue;

                if (p.EndsWith("liblua55.so"))
                {
                    bool arm64 = p.Contains("arm64-v8a");
                    Configure(imp, anyPlatform: false, editor: false, android: true,
                        standalone: false, ios: false);
                    SetCpu(imp, BuildTarget.Android, arm64 ? "ARM64" : "ARMv7");
                }
                else if (p.EndsWith("liblua55.a"))
                {
                    Configure(imp, anyPlatform: false, editor: false, android: false,
                        standalone: false, ios: true);
                }
                else if (p.EndsWith("lua55.dll"))
                {
                    bool x64 = p.Contains("x86_64");
                    Configure(imp, anyPlatform: false, editor: x64, android: false,
                        standalone: true, ios: false);
                    SetCpu(imp, BuildTarget.StandaloneWindows64, "x86_64");
                    SetCpu(imp, BuildTarget.StandaloneWindows, "x86");
                    if (!x64) Configure(imp, anyPlatform: false, editor: false, android: false,
                        standalone: true, ios: false); // x86 flavor: no Editor
                }
                else if (p.EndsWith("liblua55.dylib"))
                {
                    Configure(imp, anyPlatform: false, editor: true, android: false,
                        standalone: true, ios: false);
                }
            }
        }

        private static void Configure(PluginImporter imp, bool anyPlatform, bool editor,
            bool android, bool standalone, bool ios)
        {
            bool changed = false;
            if (imp.GetCompatibleWithAnyPlatform() != anyPlatform)
            {
                imp.SetCompatibleWithAnyPlatform(anyPlatform);
                changed = true;
            }
            if (imp.GetCompatibleWithEditor() != editor)
            {
                imp.SetCompatibleWithEditor(editor);
                changed = true;
            }
            if (imp.GetCompatibleWithPlatform(BuildTarget.Android) != android)
            {
                imp.SetCompatibleWithPlatform(BuildTarget.Android, android);
                changed = true;
            }
            bool st = imp.GetCompatibleWithPlatform(BuildTarget.StandaloneWindows64);
            if (st != standalone)
            {
                imp.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, standalone);
                imp.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, standalone);
                imp.SetCompatibleWithPlatform(BuildTarget.StandaloneOSX, standalone);
                imp.SetCompatibleWithPlatform(BuildTarget.StandaloneLinux64, standalone);
                changed = true;
            }
            if (imp.GetCompatibleWithPlatform(BuildTarget.iOS) != ios)
            {
                imp.SetCompatibleWithPlatform(BuildTarget.iOS, ios);
                changed = true;
            }
            if (changed) imp.SaveAndReimport();
        }

        private static void SetCpu(PluginImporter imp, BuildTarget target, string cpu)
        {
            imp.SetPlatformSettings(target, "CPU", cpu);
        }
    }
}
#endif
