using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NUnit.Framework;
using UnityEditor;
class CrankcaseBuild
{

    static string[] scenes = { "Assets/MainScene.unity" };

    static void PerformBuildWindowsAppStore ()
     {
        BuildWindows(new String[] {"APPSTORE"},"Appstore");
     }

    static void PerformBuildWindowsDemo()
    {
        BuildWindows(null, "Demo");
    }

static void PerformBuildMacDemo()
    {
        BuildMac(null, "Demo");
    }


    static void BuildMac(String [] directives, String subFolder)
     {
         if (directives != null)
         {
             foreach (String directive in directives)
             {
                 PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, directive);
                 PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, directive);
            }
         }

        String baseFolder = "bin/" + subFolder;
        
        BuildPipeline.BuildPlayer(scenes, baseFolder + "/MacOSX/REVDemo", BuildTarget.StandaloneOSX, BuildOptions.None);
        BuildPipeline.BuildPlayer(scenes, baseFolder + "/iOS/REVDemo.", BuildTarget.iOS, BuildOptions.None);
     }

    static void BuildWindows(String [] directives, String subFolder)
     {
         if (directives != null)
         {
             foreach (String directive in directives)
             {
                 PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, directive);
                 PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, directive);
            }
         }

        String baseFolder = "bin/" + subFolder;
        
        BuildPipeline.BuildPlayer(scenes, baseFolder + "/Windows64/REVDemo.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
        BuildPipeline.BuildPlayer(scenes, baseFolder + "/Android/REVDemo.apk", BuildTarget.Android, BuildOptions.None);
     }



}