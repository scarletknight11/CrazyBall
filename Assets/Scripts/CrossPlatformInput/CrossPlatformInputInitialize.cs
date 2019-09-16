using System;
using System.Collections.Generic;
//using UnityEditor;

namespace UnityStandardAssets.CrossPlatformInput.Inspector
{
    //[InitializeOnLoad]
    public class CrossPlatformInitialize
    {
        // Custom compiler defines:
        //
        // CROSS_PLATFORM_INPUT : denotes that cross platform input package exists, so that other packages can use their CrossPlatformInput functions.
        // EDITOR_MOBILE_INPUT : denotes that mobile input should be used in editor, if a mobile build target is selected. (i.e. using Unity Remote app).
        // MOBILE_INPUT : denotes that mobile input should be used right now!

        static CrossPlatformInitialize()
        {
            return;
        }


        private static void Enable()
        {
            return;
        }


        private static bool EnableValidate()
        {
            return true;
        }


        private static void Disable()
        {
            return;
        }


        private static bool DisableValidate()
        {
            return true;
        }


        //private static BuildTargetGroup[] buildTargetGroups = new BuildTargetGroup[]
        //    {
        //        BuildTargetGroup.Standalone,
        //        BuildTargetGroup.WebPlayer,
        //        BuildTargetGroup.Android,
        //        BuildTargetGroup.iOS,
        //        BuildTargetGroup.WP8,
        //        BuildTargetGroup.BlackBerry
        //    };

        //private static BuildTargetGroup[] mobileBuildTargetGroups = new BuildTargetGroup[]
        //    {
        //        BuildTargetGroup.Android,
        //        BuildTargetGroup.iOS,
        //        BuildTargetGroup.WP8,
        //        BuildTargetGroup.BlackBerry,
        //        BuildTargetGroup.PSM, 
        //        BuildTargetGroup.Tizen, 
        //        BuildTargetGroup.WSA 
        //    };


        private static void SetEnabled(string defineName, bool enable, bool mobile)
        {
            return;
        }


        //private static List<string> GetDefinesList(BuildTargetGroup group)
        //{
        //    return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
        //}
    }
}
