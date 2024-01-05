using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace ZResLoad
{
    public partial class BuildABEditor
    {
        [Pack("Material")]
        private static void BuildMaterial()
        {
            Debug.Log("Start Build Material");
            PackageSplitAB("mat/{0}.ab", "t:Material *", "Assets/Materials");
        }

        [Pack("Texture")]
        private static void BuildTexture()
        {
            Debug.Log("Start Build Texture");
            PackageSplitAB("tex/{0}.ab", "t:png *", "Assets/Art/tex");
        }

        [Pack("Model")]
        private static void BuildModel()
        {
            Debug.Log("Start Build Model");
            PackageSplitAB("model/{0}.ab", "t:prefab *", "Assets/Res/Prefabs", "Assets/Res/Model");
        }

        [Pack("Audio", PackMode.Single)]
        private static void BuildAudio()
        {
            Debug.Log("Start Build Audio");
            PackageSplitAB("audio/{0}.ab", "t:AudioClip *", "Assets/Res/Audio/Bg", "Assets/Res/Audio/Effect");
        }

        [Pack("UI", PackMode.Single)]
        private static void BuildUI()
        {
            string uiPath = "Assets/Res/UI";
            if (Directory.Exists(uiPath))
            {
                DirectoryInfo direction = new DirectoryInfo(uiPath);
                FileInfo[] files = direction.GetFiles("*.bytes", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string fileName = file.Name.Replace("_fui.bytes", "");
                    string packageAtlasName = string.Format("UI/{0}_atlas.ab", fileName);
                    string packageFUIName = string.Format("UI/{0}_fui.ab", fileName);
                    string filterNameAtlas = string.Format("{0}_atlas **", fileName);
                    string filterNameCfg = string.Format("{0}_fui **", fileName);
                    PackageAB(packageAtlasName, filterNameAtlas, uiPath);
                    PackageAB(packageFUIName, filterNameCfg, uiPath);
                }

                FileInfo[] prefabFiles = direction.GetFiles("*.prefab", SearchOption.AllDirectories);
                foreach (var prefab in prefabFiles)
                {
                    string fileName = prefab.Name.Replace(".prefab", "");
                    string packageName = string.Format("UI/{0}.ab", fileName);
                    PackageAB(packageName, fileName, uiPath);
                }
            }
        }
    }
}
