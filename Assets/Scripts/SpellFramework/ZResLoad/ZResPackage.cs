using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UObject = UnityEngine.Object;
using SpellFramework;

namespace ZResLoad
{
    // 资源调用接口
    public class ZResPackage
    {
        public static AssetBundleManifest _ABM;
        private static readonly Dictionary<string, IResPackage> _PackageName2ResCache = new Dictionary<string, IResPackage>();

        private static readonly Dictionary<int, IResPackage> _AssetObj2ResCache = new Dictionary<int, IResPackage>();

        public static void Init(string resURL, string appContentURL)
        {
            PackageFactory.Init(resURL, appContentURL);
//#if !UNITY_EDITOR || DEBUG_RES
            if (_ABM == null)
            {
                var StreamingAssets = AssetBundle.LoadFromFile(Path.Combine(Util.DataPath, AppConst.AbmName));
                _ABM = StreamingAssets.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
//#endif
        }

        public static T LoadAsset<T>(string packageName, string itemName) where T : UObject
        {
            if (string.IsNullOrEmpty(packageName))
                return null;
            var package = GetOrCreatePackage(packageName);
            var res = package.LoadRes(itemName) as T;
            if (res == null) return res;
            package.IncRef();

            CacheObject(package, res);
            return res;
        }

        public static AssetBundle LoadPackage(string packageName)
        {
            if (string.IsNullOrEmpty(packageName))
                return null;
            var package = GetOrCreatePackage(packageName, false);
            return package.GetAB();
        }

        public static IResPackage GetPackage(string packageName)
        {
            IResPackage package = null;
            if (!string.IsNullOrEmpty(packageName))
            {
                _PackageName2ResCache.TryGetValue(packageName, out package);
            }
            return package;
        }

        public static IResPackage GetPackage(UObject obj)
        {
            IResPackage package = null;
            if (obj != null)
            {
                _AssetObj2ResCache.TryGetValue(obj.GetInstanceID(), out package);
            }

            return package;
        }

        public static T LoadRawAsset<T>(string packageName, string itemName) where T : UObject
        {
            if (string.IsNullOrEmpty(packageName))
                return null;

            var package = GetPackage(packageName) ?? CreateRawPackage(packageName);

            if (package == null)
            {
                Debug.LogFormat(string.Format("Cant Find RawPackage:{0}", packageName));
                return null;
            }

            var res = package.LoadRes(itemName) as T;
            if (res == null) return res;
            Debug.LogFormat("Cache Object:{0}", res.name);
            CacheObject(package, res);

            return res;
        }

        private static IResPackage CreateRawPackage(string packageName)
        {
            var package = PackageFactory.CreateRawPackge(packageName);
            if (package == null)
                return null;
            package.IncRef();

            AddPackage(packageName, package);
            return package;
        }

        public static IResPackage GetOrCreatePackage(string packageName, bool needDep = true)
        {
            var package = GetPackage(packageName) ?? CreatePackage(packageName);
            return package;
        }

        private static void CacheObject(IResPackage package, UObject res)
        {
            if (res != null)
            {
                _AssetObj2ResCache[res.GetInstanceID()] = package;
            }
        }

        private static void RemoveCacheObject(UObject res)
        {
            if (res != null)
            {
                _AssetObj2ResCache.Remove(res.GetInstanceID());
            }
        }

        private static void AddPackage(string packageName, IResPackage package)
        {
            if (_PackageName2ResCache.ContainsKey(packageName))
            {
                return;
            }
            _PackageName2ResCache[packageName] = package;
        }

        private static void RemovePackage(string packageName)
        {
            _PackageName2ResCache.Remove(packageName);
        }


        private static IResPackage CreatePackage(string packageName, bool needDep = true)
        {
            var package = PackageFactory.CreatePackage(packageName);
            if (package == null)
                return null;
            if (needDep)
            {
                // 创建依赖包
                CreateDepPackage(package);
            }

            AddPackage(packageName, package);

            return package;
        }

        private static void CreateDepPackage(IResPackage package)
        {
            var packageAssetName = package.Name();

            var depPkgs = _ABM.GetDirectDependencies(packageAssetName + ".ab");
            for (int i = 0; i < depPkgs.Length; i++)
            {
                var pkgName = depPkgs[i];
                if (pkgName.Contains(".ab"))
                {
                    pkgName = pkgName.Replace(".ab", "");
                }
                var depPkg = GetOrCreatePackage(pkgName);
                if (depPkg != null)
                {
                    package.AddDeps(depPkg);
                    depPkg.IncRef();
                }
            }
        }

        /// <summary>
        /// 卸载资源 返回true表示底层已将其从内存中卸载干净
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool UnloadAsset(UObject obj)
        {
            bool removeFromMemory = false;
            var package = GetPackage(obj);
            if (package != null)
            {
                removeFromMemory = package.UnloadRes(obj);
                if (!removeFromMemory) return removeFromMemory;
                RemoveCacheObject(obj);
                if (package.ResLoadedCount <= 0)
                {
                    UnloadPackage(package.Name());
                }
            }
            else
            {
                if (!(obj is GameObject || obj is AssetBundle || obj is Component))
                {
                    Resources.UnloadAsset(obj);
                }
            }

            return removeFromMemory;
        }

        private static void UnloadPackage(string pkgName)
        {
            var pkg = GetPackage(pkgName);
            UnloadPackage(pkg);
        }

        private static void UnloadPackage(IResPackage pkg)
        {
            if (pkg == null) return;

            pkg.DecRef();
            if (pkg.RefCount > 1 || pkg.ResLoadedCount > 0) return;
            UnloadDepPackage(pkg);
            pkg.UnloadAll();
            RemovePackage(pkg.Name());
        }

        private static void UnloadDepPackage(IResPackage package)
        {
            var list = package.GetDeps();
            var count = list.Count;
            for (int i = 0; i < count; ++i)
            {
                IResPackage depPkg = list[i];
                var refCount = depPkg.DecRef();
                if (refCount == 1)
                {
                    UnloadPackage(depPkg);
                }
            }
        }

        /// <summary>
        /// 移除所有已加载资源
        /// </summary>
        public static void UnloadAllPackage()
        {
            using (var i = _PackageName2ResCache.GetEnumerator())
            {
                IResPackage package = null;
                while (i.MoveNext())
                {
                    package = i.Current.Value;
                    if (package != null)
                    {
                        package.UnloadAll();
                    }
                }
            }

            _ABM = null;
            _PackageName2ResCache.Clear();
            _AssetObj2ResCache.Clear();
        }

    }

}
