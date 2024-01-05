using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using SpellFramework;

namespace ZResLoad
{
    public partial class BuildABEditor : Editor
    {
        private static string _ExportPath = "";

        private static List<AssetBundleBuild> _ABBS = new List<AssetBundleBuild>(10000);
        private static Dictionary<string, string> _ABBGUIDS = new Dictionary<string, string>();

        private const int BASE_PRIORITY = 100;
        private static string _AssetPath = Path.Combine("StreamingAssets", AppConst.StreamingAssetsAbOutPath);

        public static string BuildAssetPath
        {
            get
            {
                string path = Path.GetFullPath(Application.dataPath + "/../" + GetPlatformAssetCachePath()
                    + "/" + _AssetPath);
                return path;
            }
        }

        private static string CopyToStreamingAssetPath
        {
            get
            {
                return Path.GetFullPath(Application.dataPath + "/" + _AssetPath);
            }
        }

        public static string GetPlatformAssetCachePath()
        {
            return "AssetBundle/Cache";
        }

        [MenuItem("Tools/打开启动场景", false, BASE_PRIORITY - 1)]
        public static void OpenMainScene()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        }

        [MenuItem("Tools/Open Folder/PersistentDataPath", false)]
        public static void OpenPersisitentDataPath()
        {
            Application.OpenURL(Application.persistentDataPath);
        }

        [MenuItem("Tools/Open Folder/DataPath", false)]
        public static void OpenDataPath()
        {
            Application.OpenURL(Application.dataPath);
        }

        [MenuItem("Tools/Open Folder/StreamingAssetsPath", false)]
        public static void OpenStreamingAssetsPath()
        {
            Application.OpenURL(Application.streamingAssetsPath);
        }

        [MenuItem("Tools/AB/Build All", false, BASE_PRIORITY + 1)]
        public static void BuildAllAB()
        {
            Debug.Log(BuildAssetPath);
            if (!Directory.Exists(BuildAssetPath))
            {
                Directory.CreateDirectory(BuildAssetPath);
            }
            BuildAB(BuildAssetPath, null);

            CopyCacheAssetToStreamingAssetPath();

            Util.CopyFlag = false;
        }

        [MenuItem("Tools/AB/清除AB复制标记", false, BASE_PRIORITY + 2)]
        public static void ClearCopyFlag()
        {
            Util.CopyFlag = false;
        }


        /// <summary>
        /// 一键AB
        /// </summary>
        /// <param name="buildPath">打包地址</param>
        /// <param name="packageNames">ab包名</param>
        /// <param name="exceptNames">过滤的包名</param>
        /// <param name="mode">打包模式</param>
        public static void BuildAB(string buildPath, string packageNames, string exceptNames = null, PackMode mode = PackMode.Dependence)
        {
            if (Application.isPlaying)
            {
                Debug.LogError("请停止游戏后尝试");
                return;
            }
            StartPackage(buildPath);
            {
                PackABFilter(packageNames, exceptNames, mode);
            }
            EndPackage();
        }


        private static void StartPackage(string exportPath)
        {
            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();
            // 导出路径
            _ExportPath = exportPath;
            _ABBS.Clear();
            _ABBGUIDS.Clear();
        }

        private static void EndPackage(PackMode mode = PackMode.Dependence)
        {
            if (!Directory.Exists(_ExportPath))
            {
                Directory.CreateDirectory(_ExportPath);
            }
            if (mode == PackMode.Dependence)
            {
                AnalyseABB();
            }
            Debug.LogFormat("<color=red>Total AB:</color>{0}", _ABBS.Count);


#if UNITY_IPHONE
            var buildOption = BuildAssetBundleOptions.ChunkBasedCompression;           
#else
            var buildOption = BuildAssetBundleOptions.ChunkBasedCompression;
#endif

            var oldStreamingAsset = Path.GetFullPath(_ExportPath + "/" + _AssetPath);
            var newStreamingAsset = Path.GetFullPath(_ExportPath + "/" + _AssetPath + "/Tmp");

            if (mode == PackMode.Single)
            {
                if (File.Exists(oldStreamingAsset))
                {
                    File.Delete(newStreamingAsset);
                    File.Move(oldStreamingAsset, newStreamingAsset);
                }
            }

            var abm = BuildPipeline.BuildAssetBundles(_ExportPath, _ABBS.ToArray(), buildOption, EditorUserBuildSettings.activeBuildTarget);

            if (mode == PackMode.Single)
            {
                if (File.Exists(newStreamingAsset))
                {
                    File.Delete(oldStreamingAsset);
                    File.Move(newStreamingAsset, oldStreamingAsset);
                }
            }
            // 生成版本文件
            GenAssetBundleVersionFiles(abm);

            AssetDatabase.Refresh();
        }

        private static void GenAssetBundleVersionFiles(AssetBundleManifest abm)
        {
            if (abm == null)
                return;
            var abs = abm.GetAllAssetBundles();
            for (int i = 0; i < abs.Length; i++)
            {
               Debug.LogFormat("<color=red>AB:</color>{0}", abs[i]);
            }

            string buildVersion = System.DateTime.Now.ToString("yyyyMMddHHmmss");

            string resPath = _ExportPath;
            if (!Directory.Exists(resPath))
            {
                Directory.CreateDirectory(resPath);
            }
            var newFilesPath = string.Format("{0}{1}", resPath, "/files.txt");

            var paths = new List<string>();
            var files = new List<string>();

            var allFiles = Directory.GetFiles(resPath, "*.*", SearchOption.AllDirectories);
            files.AddRange(allFiles);

            if (File.Exists(newFilesPath))
            {
                File.Delete(newFilesPath);
            }

            var fs = new FileStream(newFilesPath, FileMode.CreateNew);
            var sw = new StreamWriter(fs);
            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                if (file.EndsWith(".DS_Store"))
                    continue;
                string ext = Path.GetExtension(file);
                // 过滤
                if (ext.Equals(".meta"))
                    continue;
                if (ext.EndsWith(".bat"))
                    continue;
                if (ext.EndsWith(".txt"))
                    continue;
                if (ext.EndsWith(".manifest"))
                    continue;
                if (ext.EndsWith(".zip"))
                    continue;
                if (ext.EndsWith(".mp4"))
                    continue;

                var versionFile = new VersionFile {
                    Path = file.Replace(resPath, string.Empty).Replace("\\", "/"),
                    Hash = Util.HashFile(file), //MD5
                    Version = buildVersion
                };
                sw.WriteLine(versionFile);
            }
            sw.Close();
            fs.Close();
        }

        // 统计依赖引用
        private static Dictionary<string, int> _DepsRef = new Dictionary<string, int>();
        private static void AnalyseABB()
        {
            _DepsRef.Clear();

            var abbArray = _ABBS.ToArray();
            foreach (var abb in abbArray)
            {
                var mainPaths = abb.assetNames;
                if (mainPaths.Length == 0 || string.IsNullOrEmpty(mainPaths[0]))
                    continue;

                foreach (var path in mainPaths)
                {
                    var guid = AssetDatabase.AssetPathToGUID(path);
                    UpdateGUIDRefs(guid);
                }
                // 是否递归 首先检查是否为场景 todo
                bool useRecursive = abb.assetBundleName.StartsWith("Scenes");
                bool hasLightmap = false;
                if (useRecursive)
                {
                    //todo 场景文件处理
                    var openScene = EditorSceneManager.OpenScene(mainPaths[0]);
                    var lightmaps = LightmapSettings.lightmaps;
                    hasLightmap = lightmaps != null && lightmaps.Length > 0;

                    {
                        //Trim EditorOnly GameObject
                        var editorOnlyGos = FindSceneGameObjectByTags(openScene, "EditorOnly");
                        if (editorOnlyGos.Length > 0)
                        {
                            foreach (var go in editorOnlyGos)
                            {
                                GameObject.DestroyImmediate(go, true);
                            }
                        }

                        //Trim android platform "ios tag" GameObject
                        var editorIOSGos = FindSceneGameObjectByTags(openScene, "ios");
                        if (editorIOSGos.Length > 0)
                        {
                            foreach (var go in editorIOSGos)
                            {
#if UNITY_ANDROID
                                GameObject.DestroyImmediate(go, true);
#endif
                            }
                        }

                        if (editorIOSGos.Length > 0 || editorIOSGos.Length > 0)
                        {
                            EditorSceneManager.MarkSceneDirty(openScene);
                            EditorSceneManager.SaveOpenScenes();
                        }
                    }


                }
                else
                {
                    // 检查是否为prefab
                    var isPrefab = mainPaths[0].EndsWith("prefab");
                    useRecursive = isPrefab;
                }
                // 获取依赖
                var deps = AssetDatabase.GetDependencies(mainPaths, useRecursive);
                foreach (var dep in deps)
                {
                    var depGUID = AssetDatabase.AssetPathToGUID(dep);
                    if (hasLightmap)
                    {
                        if (dep.EndsWith("prefab", System.StringComparison.CurrentCultureIgnoreCase))
                            continue;
                        if (dep.EndsWith("fbx", System.StringComparison.CurrentCultureIgnoreCase))
                            continue;

                        UpdateGUIDRefs(depGUID);
                    }
                    else
                    {
                        UpdateGUIDRefs(depGUID);
                    }
                }
            }

            foreach (var dep in _DepsRef)
            {
                var guid = dep.Key;
                var refCount = dep.Value;
                // 把引用数大于1的文件加到 _ABBGUIDS中 并收集abb 加入abb列表
                if (refCount > 1)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    if (string.IsNullOrEmpty(assetPath) || _ABBGUIDS.ContainsKey(guid)
                        || assetPath.EndsWith(".cs") || assetPath.EndsWith(".js"))
                    {
                        continue;
                    }

                    _ABBGUIDS.Add(guid, "");
                    Debug.LogFormat("AB:<color=green>{0}</color> - {1}", refCount, AssetDatabase.GUIDToAssetPath(guid));

                    var type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                    if (type == typeof(Shader))
                    {
                        var abb = new AssetBundleBuild();
                        abb.assetBundleName = string.Format("Deps/{0}.ab","Shaders");
                        abb.assetNames = new string[] { assetPath };
                        AddABBS(abb);
                    }
                    else
                    {
                        var hash = guid;
                        var abb = new AssetBundleBuild();
                        abb.assetBundleName = string.Format("Deps/{0}.ab", hash);
                        abb.assetNames = new string[] { assetPath };
                        AddABBS(abb);
                    }
                }
            }
        }

        private static void UpdateGUIDRefs(string guid)
        {
            if (_DepsRef.ContainsKey(guid))
            {
                _DepsRef[guid] += 1;
            }
            else
            {
                _DepsRef[guid] = 1;
            }
        }

        /// <summary>
        /// 打包指定package 不指定则全部打包
        /// </summary>
        /// <param name="packageNames"></param>
        /// <param name="exceptPackages"></param>
        /// <param name="mode"></param>
        private static void PackABFilter(string packageNames, string exceptPackages, PackMode mode)
        {
            var methods = typeof(BuildABEditor).GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(PackAttribute), false);
                if (attributes.Length > 0)
                {
                    var pa = attributes[0] as PackAttribute;
                    // 检查是否被剔除
                    if (exceptPackages != null && exceptPackages.Contains(pa.Name))
                        continue;
                    if (packageNames == null || packageNames.Contains(pa.Name))
                    {
                        Debug.Log("<color=yellow>Pack</color> ===>" + pa.Name);
                        method.Invoke(null, null);
                    }
                }
            }
        }

        private static void AddABBS(AssetBundleBuild abb)
        {
            if (abb.assetBundleName.Length <= 3)
            {
                Debug.Log("Found No Name ABB: " + abb.assetNames);
                return;
            }
            _ABBS.Add(abb);
        }

        /// <summary>
        /// Filter文件 打整个AB包
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="filterName"></param>
        /// <param name="packagePaths"></param>
        private static void PackageAB(string packageName, string filterName, params string[] packagePaths)
        {
            var guids = AssetDatabase.FindAssets(filterName, packagePaths);

            var abb = new AssetBundleBuild();
            abb.assetBundleName = packageName;
            abb.assetNames = new string[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                var filePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                var fileInfo = new FileInfo(filePath);

                var dirPath = filePath.Substring(0, filePath.LastIndexOf('/'));

                if (!filterName.Contains("*") && !packagePaths.Contains(dirPath))
                    continue;

                if (filterName.Contains("**") && !fileInfo.Name.StartsWith(filterName.Split(' ')[0]))
                    continue;

                if (_ABBGUIDS.ContainsKey(guids[i]))
                {
                    Debug.LogWarning("Add AB has the same guids :" + filePath);
                    continue;
                }
                _ABBGUIDS.Add(guids[i], "");
                abb.assetNames[i] = filePath;
            }
            Debug.Log(abb.assetBundleName);
            AddABBS(abb);
        }

        /// <summary>
        /// Filter文件 分别打AB包
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="filterName"></param>
        /// <param name="packagePaths"></param>
        private static void PackageSplitAB(string packageName, string filterName, params string[] packagePaths)
        {
            var guids = AssetDatabase.FindAssets(filterName, packagePaths);
            Debug.Log("guids.Length ==> " + guids.Length);
            for (int i = 0; i < guids.Length; i++)
            {
                var filePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!File.Exists(filePath))
                {
                    Debug.LogErrorFormat("Asset is Null : {0}", filePath);
                    continue;
                }
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var dirPath = filePath.Substring(0, filePath.LastIndexOf('/'));

                if (!fileName.Contains("*") && !packagePaths.Contains(dirPath))
                    continue;

                if (_ABBGUIDS.ContainsKey(guids[i]))
                {
                    Debug.LogWarning("Add AB has the same guids :" + filePath);
                    continue;
                }
                _ABBGUIDS.Add(guids[i], "");

                var abb = new AssetBundleBuild();
                abb.assetBundleName = string.Format(packageName, fileName);
                abb.assetNames = new string[] { filePath };                
                AddABBS(abb);
            }
        }


        private static List<string> GetCacheAssetFileList()
        {
            var result = new List<string>(20000);
            var files = Directory.GetFiles(BuildAssetPath, "*.ab", SearchOption.AllDirectories);
            result.AddRange(files);
            files = Directory.GetFiles(BuildAssetPath, "*.bin", SearchOption.AllDirectories);
            result.AddRange(files);
            files = Directory.GetFiles(BuildAssetPath, "*.txt", SearchOption.AllDirectories);
            result.AddRange(files);
            files = Directory.GetFiles(BuildAssetPath, "*.json", SearchOption.AllDirectories);
            result.AddRange(files);
            //files = Directory.GetFiles(BuildAssetPath, "*.manifest", SearchOption.AllDirectories);
            //result.AddRange(files);
            //result.Add(BuildAssetPath + "/StreamingAssets");
            result.Add(Path.Combine(BuildAssetPath, AppConst.StreamingAssetsAbOutPath));

            return result;
        }

        public static void CopyCacheAssetToStreamingAssetPath()
        {
            var files = GetCacheAssetFileList();
            var fromPath = BuildAssetPath;
            var toPath = CopyToStreamingAssetPath;

            foreach (var file in files)
            {
                var relativePath = file.Replace(fromPath, "");
                var dstPath = Path.GetFullPath(toPath + relativePath);
                FileIOManager.DeleteFile(dstPath);
                FileIOManager.EnsureFilePath(dstPath);
                File.Copy(file, dstPath);
            }
        }

        private static GameObject[] FindSceneGameObjectByTags(UnityEngine.SceneManagement.Scene scene, string tag)
        {
            var results = new List<GameObject>(4);

            var roots = scene.GetRootGameObjects();

            foreach (var go in roots)
            {
                var tempTrans = go.transform.GetComponentInChildren<Transform>();
                foreach (Transform tran in tempTrans)
                {
                    if (tempTrans.gameObject.CompareTag(tag))
                    {
                        results.Add(tran.gameObject);
                    }
                }
            }
            return results.ToArray();
        }

        public static void CreateGameSetting(string updateURL, string CVer)
        {
            var assetPath = "Assets/Resources/Settings/GameSettingSO.asset";
            if (File.Exists(assetPath))
            {
                File.Delete(assetPath);
            }
            var so = CreateInstance<GameSettingSO>();
            so.UpdateUrl = updateURL;
            so.CVer = CVer;
            so.IntVer = ConvertVersion(CVer);

            Selection.activeObject = so;
            AssetDatabase.CreateAsset(so, assetPath);
        }

        private static int ConvertVersion(string cver)
        {
            var tempVer = cver.Split('.');
            var version = string.Format("{0}{1:D3}", Convert.ToInt32(tempVer[0]), Convert.ToInt32(tempVer[1]));
            return Convert.ToInt32(version);
        }
    }
}


public class GameSettingSO : ScriptableObject
{
    private static GameSettingSO _Instance;
    public static GameSettingSO Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = Resources.Load<GameSettingSO>("Settings/GameSettingSO");
            }
            return _Instance;
        }
    }

    public string CVer;

    public int IntVer;

    public static string RVer { get; set; }

    public string UpdateUrl;

    private int _Version;
    public int Version
    {
        get
        {
            if (_Version == 0)
            {
                if (!string.IsNullOrEmpty(CVer))
                {
                    var tempVer = CVer.Split('.');
                    var version = string.Format("{0}{1:D3}{2:D3}",
                        Convert.ToInt32(tempVer[0]),
                        Convert.ToInt32(tempVer[1]),
                        Convert.ToInt32(tempVer[2]));
                    if (!int.TryParse(version, out _Version))
                        _Version = 1;
                }
            }
            return _Version;
        }
    }

}