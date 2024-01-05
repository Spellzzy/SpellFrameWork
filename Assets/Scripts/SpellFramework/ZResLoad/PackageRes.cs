using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ZResLoad
{
    internal class PackageRes : IResPackage
    {
        protected string PackageURL { get; private set; }
        protected string PackageName { get; private set; }

        protected const string _PACKAGEEXT = "ab";

        private AssetBundle _ResFile;

        private List<IResPackage> _DepPackages = new List<IResPackage>();

        private List<PackageItemRes> _ItemCache = new List<PackageItemRes>();

        public int ResLoadedCount
        {
            get {
                return _ItemCache.Count;
            }
        }

        public int RefCount { get; private set; }

        protected PackageRes(string packageName)
        {
            this.PackageName = packageName;
        }

        #region 引用计数

        public int IncRef()
        {
            RefCount++;
            return RefCount;
        }

        public int DecRef()
        {
            RefCount--;
            return RefCount;
        }

        public void AddDeps(IResPackage package)
        {
            _DepPackages.Add(package);
        }

        public IList<IResPackage> GetDeps()
        {
            return _DepPackages;
        }

        #endregion

        /// <summary>
        /// 创建package资源
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static IResPackage Create(string packageName)
        {
            var package = new PackageRes(packageName);
            return package;
        }

        /// <summary>
        /// 检查是否存在该package文件
        /// </summary>
        /// <param name="packageUrl"></param>
        /// <param name="packageName"></param>
        /// <returns></returns>
        public static bool HasPackageFile(string packageUrl, string packageName)
        {
            var path = string.Format("{0}/{1}.ab", packageUrl, packageName);
            Debug.Log("path ---> " + path);
            if (System.IO.File.Exists(path))
                return true;

            if (FileIOManager.CheckStreamingFileExist(packageName))
                return true;

            return false;
        }

        private PackageItemRes AddRes(string name, Object asset)
        {
            var res = new PackageItemRes(name, asset);
            _ItemCache.Add(res);
            return res;
        }

        private bool RemoveRes(PackageItemRes removeRes)
        {
            PackageItemRes res = null;
            for (int i = 0; i < _ItemCache.Count; i++)
            {
                res = _ItemCache[i];
                if (res == removeRes)
                {
                    _ItemCache.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        private PackageItemRes FindRes(string name)
        {
            PackageItemRes res = null;
            for (int i = 0; i < _ItemCache.Count; i++)
            {
                res = _ItemCache[i];
                if (res != null && res.Name == name)
                {
                    return res;
                }
            }
            return null;
        }

        private PackageItemRes FindResByObj(Object obj)
        {
            PackageItemRes res = null;
            for (int i = 0; i < _ItemCache.Count; i++)
            {
                res = _ItemCache[i];
                if (res != null && res.GetItem() == obj)
                {
                    return res;
                }
            }
            return null;
        }

        private bool HasRes(string name)
        {
            return FindRes(name) != null;
        }

        protected virtual string GetPackagePath()
        {
            return string.Format("{0}/{1}", PackageURL, PackageName);
        }

        protected virtual string GetPackageName()
        {
            return PackageName;
        }

        private void UnloadCachedRes()
        {
            PackageItemRes res = null;
            for (int i = 0; i < _ItemCache.Count; i++)
            {
                res = _ItemCache[i];
                if (res != null)
                {
                    res.UnloadItem();
                }
            }
            _ItemCache.Clear();
            _ItemCache.TrimExcess();
            _ItemCache = null;
        }        

        private UObject LoadPackageItem(string name)
        {
            if (_ResFile == null)
            {
                Debug.Log("_ResFile is null!!!!!!!!!!");
                return null;
            }
            return _ResFile.LoadAsset(name);
        }

        private AssetBundleRequest AsyncLoadPackageItem(string name)
        {
            if (_ResFile == null)
            {
                return null;
            }
            return _ResFile.LoadAssetAsync(name);
        }

        private UObject[] LoadPackageAllItem()
        {
            if (_ResFile == null)
            {
                return null;
            }
            return _ResFile.LoadAllAssets();
        }


        #region 接口实现

        public IEnumerator CoLoadPackage()
        {
            if (_ResFile == null)
            {
                var op = FileIOManager.AsyncLoadFromFile(PackageURL, GetPackageName());
                if (op == null)
                {
                    yield break;
                }

                while (!op.isDone)
                {
                    yield return null;
                }
                _ResFile = op.assetBundle;
            }

            yield return _ResFile;
        }

        public IEnumerator CoLoadRes(string name)
        {
            var res = FindRes(name);
            if (res == null)
            {
                var op = AsyncLoadPackageItem(name);
                if (op == null)
                {
                    yield break;
                }
                while (!op.isDone)
                {
                    yield return null;
                }
                res = FindRes(name);
                if (res == null)
                {
                    res = AddRes(name, op.asset);
                }
            }
            res.IncRef();

            yield return res.GetItem();
        }
       

        public UObject[] LoadAllRes()
        {
            var assets = LoadPackageAllItem();
            if (assets == null)
            {
                return null;
            }
            var num = assets.Length;
            for (int i = 0; i < num; i++)
            {
                var asset = assets[i];
                AddRes(asset.name, asset);
            }

            return assets;
        }

        public bool LoadPackage()
        {
            if (_ResFile == null)
            {
                _ResFile = FileIOManager.LoadFromFile(PackageURL, GetPackageName());
            }

            return _ResFile != null;
        }

        public UObject LoadRes(string name)
        {
            var res = FindRes(name);
            if (res == null)
            {
                var asset = LoadPackageItem(name);
                if (asset == null)
                {
                    Debug.LogFormat("<color=yellow>Load</color> {0}:{1} \t <color=red>[Empty]</color>", PackageName, name);
                    return null;
                }
                res = AddRes(name, asset);
            }
            res.IncRef();

            Debug.LogFormat("<color=yellow>Load</color> {0}:{1} \tRef:{2}", PackageName, res.Name, res.RefCount);
            return res.GetItem();
        }

        public string Name()
        {
            return PackageName;
        }

        public AssetBundle GetAB()
        {
            return _ResFile;
        }

        public void SetResURL(string url)
        {
            PackageURL = url;
        }        

        public void UnloadPackage()
        {
            if (_ResFile != null)
            {
                _ResFile.Unload(true);
                _ResFile = null;
            }
        }

        public bool UnloadRes(UObject unityObj)
        {
            var res = FindResByObj(unityObj);
            if (res == null)
            {
                Debug.LogFormat("没有找到释放资源:{0}", unityObj.name);
                return false;
            }

            int refCount = res.DecRef();
            Debug.LogError("refCount == > " + refCount);
            if (refCount <= 0)
            {
                res.UnloadItem();
                RemoveRes(res);
                return true;
            }
            return false;
        }

        public void UnloadAll()
        {
            UnloadCachedRes();

            UnloadPackage();
        }

        #endregion
    }
}
