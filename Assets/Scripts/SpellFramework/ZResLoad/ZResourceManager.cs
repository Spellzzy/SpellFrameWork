using UnityEngine;

namespace ZResLoad
{
    public class ZResourceManager
    {
        public static void Init()
        {
            ZResource.Init(Util.DataPath, BetterStreamingAssets.Root);
        }

        public static GameObject LoadModel(string modelName)
        {
            var go = ZResource.Query<GameObject>(string.Format("model/{0}", modelName), modelName);
            return ZResource.CreateInstance(go);
        }

        public static AudioClip LoadAudio(string audioName)
        {
           return ZResource.Query<AudioClip>(string.Format("audio/{0}", audioName), audioName);
        }
    }
}