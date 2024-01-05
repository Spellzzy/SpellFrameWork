using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SpellFramework;
using SpellFramework.Event;
using SpellFramework.Coroutine;
using UnityEngine.Networking;
///persistentDataPath，不管什么平台都可以用io类，所以用GetPersistentPathForIO()方法
///如果要用WWW加载，用GetPersistentPathForWWW()方法
///
///StreamingAssets，除安卓以外的平台，都可以用io类，所以用GetStreamingAssetsPathForIO()方法
///安卓平台StreamingAssets不可直接io类操作，要用WWW复制文件，所以用GetStreamingAssetsPathForWWW()方法

/// <summary>
/// 将StreamingAssets里面的文件拷贝到PersistentData,因为PersistentData下的文件可读写更改，方便热更新替换资源
/// </summary>
public class StreamingAssetsCopyToPersistentData : MonoBehaviour
{
    private int originFilesCount = 0;
    private int copiedFilesCount = 0;
    private GameObject uiCopyLoadingObj;
    public void DoCopy()
    {
        //只要复制过，就不再复制了
        if (Util.CopyFlag)
        {
            EventSystem.Dispatch(EventHandleType.OnResourceCopyOver);
            return;
        }

        string fullPDPath = Path.Combine(Application.persistentDataPath, AppConst.PersistentAbPath);
        if (Directory.Exists(fullPDPath))
        {
            Directory.Delete(fullPDPath, true);
        }
        Directory.CreateDirectory(fullPDPath);

        // 需要加入全部文件复制完成的逻辑 加进度条 以及解压资源 
        // 算上files.txt文件 所以从1起始
        originFilesCount = 1;
        using (StreamReader sr = new StreamReader(Path.Combine(Application.dataPath, AppConst.AssetDirName, AppConst.StreamingAssetsAbOutPath, AppConst.FileListName)))
        {
            while (sr.ReadLine() != null)
            {
                originFilesCount++;
            }
        }
        copiedFilesCount = 0;
        
        // 打开Loading 界面
        var canvas = GameObject.Find("Canvas");
        uiCopyLoadingObj = (GameObject)Instantiate(Resources.Load("UICopyLoading"));
        uiCopyLoadingObj.transform.SetParent(canvas.transform, false);
        var loadingCom = uiCopyLoadingObj.GetComponent<UICopyLoading>();
        loadingCom.SetMax(originFilesCount);

        // 延后一帧开启复制
        CoroutineManager.StartDelayFrameAction(() =>
        {
            CopySADirToPD(AppConst.StreamingAssetsAbOutPath, AppConst.PersistentAbPath, OnCopyFinishedAction);
        });

        // CopySADirToPD(AppConst.StreamingAssetsAbOutPath, AppConst.PersistentAbPath, OnCopyFinishedAction);
    }

    private void OnCopyFinishedAction()
    {
        // 检查解压资源是否完成
        if (++copiedFilesCount == originFilesCount)
        {
            Util.CopyFlag = true;
            EventSystem.Dispatch(EventHandleType.OnResourceCopyOver);
            uiCopyLoadingObj = null;
        }else
        {
            // 做进度显示
            EventSystem.Dispatch(EventHandleType.OnResourceCopyProgress, copiedFilesCount);
        }
    }

    /// <summary>
    /// 将StreamingAssets里面的某个文件夹及里面包含的所有文件拷贝到PersistentData
    /// </summary>
    /// <param name="SADirPath"></param>
    /// <param name="PDDirPath"></param>
    /// <param name="OnCopyFinishedAction"></param>
    public void CopySADirToPD(string SADirPath, string PDDirPath, Action OnCopyFinishedAction = null)
    {

#if !UNITY_EDITOR && UNITY_ANDROID
        //安卓平台需要根据打包时创建的FileList路径集合，通过WWW类复制 
        string fileListPath = Path.Combine(Path.Combine(GetStreamingAssetsPathForWWW(), AppConst.STREAMINGASSETS_ABOUTPATH), AppConst.FILELISTNAME);
        string fileListPathInPD = Path.Combine(Path.Combine(Application.persistentDataPath, AppConst.PERSISTENT_ABPATH), AppConst.FILELISTNAME);
        Debug.Log("fileListPath ===> " + fileListPath);
        Debug.Log("fileListPathInPD ===> " + fileListPathInPD);
        StartCoroutine(Load(fileListPath, fileListPathInPD,()=> {
            string[] fileNames = File.ReadAllLines(fileListPathInPD);
            foreach (string fileNameInfo in fileNames)
            {
                string fileName = fileNameInfo.Substring(0, fileNameInfo.IndexOf('|'));
                //string pathInSA = Path.Combine(GetStreamingAssetsPathForWWW(), fileName);
                string pathInSA = Path.Combine(GetStreamingAssetsPathForWWW(), AppConst.STREAMINGASSETS_ABOUTPATH) + fileName;

                //string pathInPD = Path.Combine(Path.Combine(Application.persistentDataPath, AppConst.PERSISTENT_ABPATH), fileName);
                string pathInPD = Path.Combine(Application.persistentDataPath, AppConst.PERSISTENT_ABPATH) + fileName;

                Debug.Log("fileName ===> " + fileName);
                Debug.Log("pathInSA ===> " + pathInSA);
                Debug.Log("pathInPD ===> " + pathInPD);
                StartCoroutine(Load(pathInSA, pathInPD));
            }
        }));
#else
        //其他平台通过io类复制
        string fullPath = Path.Combine(GetStreamingAssetsPathForIO(), SADirPath);
        int dirPathLength = fullPath.Length + 1;
        int StreamingAssetsPathLength = fullPath.Length - SADirPath.Length;

        //复制目录下的文件
        string[] files = Directory.GetFiles(fullPath);
        foreach (string filePath in files)
        {
            if (File.Exists(filePath) && Path.GetExtension(filePath) != ".meta")
            {
                int index = Util.FormatPath(filePath).LastIndexOf("/") + 1;
                string fileName = filePath.Substring(index);
                string desfileName = Path.Combine(PDDirPath, fileName);
                CopySAFileToPD(filePath.Substring(StreamingAssetsPathLength), desfileName, OnCopyFinishedAction);
            }
        }
        //递归目录下的文件夹
        string[] dirs = Directory.GetDirectories(fullPath);
        foreach (string dirPath in dirs)
        {
            string dirName = dirPath.Substring(dirPathLength);
            string desDirName = Path.Combine(PDDirPath, dirName);
            CopySADirToPD(dirPath, desDirName, OnCopyFinishedAction);
        }
#endif
    }

    /// <summary>
    /// 将StreamingAssets里面的某个文件拷贝到PersistentData
    /// </summary>
    /// <param name="inSAName">资源在StreamingAssets里的名字</param>
    /// <param name="inPDName">要复制到PersistentData里的名字</param>
    /// <param name="OnCopyFinishedAction"></param>
    public void CopySAFileToPD(string inSAName, string inPDName, Action OnCopyFinishedAction = null)
    {
        string streamingAssetsPath = Path.Combine(GetStreamingAssetsPathForIO(), inSAName);
        string persistentDataPath = Path.Combine(Application.persistentDataPath, inPDName);
        persistentDataPath = Util.FormatPath(persistentDataPath);
        //如果文件所在persistentData里面的文件夹不存在，则创建
        string dirPath = persistentDataPath.Substring(0, persistentDataPath.LastIndexOf("/"));
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        //安卓平台特殊处理,安卓平台的streamingAssets文件夹jar://开头，无法用文件操作类，只能用WWW类读取
#if !UNITY_EDITOR && UNITY_ANDROID
        streamingAssetsPath = Path.Combine(GetStreamingAssetsPathForWWW(), inSAName);
        StartCoroutine(Load(streamingAssetsPath, persistentDataPath, OnCopyFinishedAction));

#else
        //其他平台streamingAssets文件夹可以直接用文件操作类
        File.Copy(streamingAssetsPath, persistentDataPath, true);
        OnCopyFinishedAction();
#endif
    }

    /// <summary>
    /// WWW类复制文件到persistentDataPath
    /// </summary>
    /// <param name="streamingAssetsPath"></param>
    /// <param name="persistentDataPath"></param>
    /// <param name="loadComplete"></param>
    /// <returns></returns>
    IEnumerator Load(string streamingAssetsPath, string persistentDataPath, Action loadComplete = null)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(streamingAssetsPath))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || 
            www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                FileInfo info = new FileInfo(persistentDataPath);
                if (!info.Directory.Exists)
                {
                    Directory.CreateDirectory(info.Directory.FullName);
                }

                File.WriteAllBytes(persistentDataPath, www.downloadHandler.data);
                if (loadComplete != null)
                {
                    loadComplete.Invoke();
                }
            }
        }
        // WWW www = new WWW(streamingAssetsPath);
        // yield return www;
        // if (www.isDone && string.IsNullOrEmpty(www.error))
        // {
        //     FileInfo info = new FileInfo(persistentDataPath);
        //     if (!info.Directory.Exists)
        //     {
        //         Directory.CreateDirectory(info.Directory.FullName);
        //     }

        //     File.WriteAllBytes(persistentDataPath, www.bytes);
        //     if (loadComplete != null)
        //     {
        //         loadComplete.Invoke();
        //     }
        // }
        // else
        // {
        //     Debug.Log(www.error);
        // }
    }

    /// <summary>
    /// 用以WWW加载的路径：StreamingAssetsPath
    /// </summary>
    /// <returns></returns>
    public static string GetStreamingAssetsPathForWWW()
    {

#if UNITY_EDITOR
        return Application.streamingAssetsPath;
#endif
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                return Application.streamingAssetsPath;
            case RuntimePlatform.IPhonePlayer:
                return "file://" + Application.streamingAssetsPath;
            default:
                return "file://" + Application.streamingAssetsPath;
        }
    }

    /// <summary>
    /// 用以WWW加载的路径：PersistentPath
    /// </summary>
    /// <returns></returns>
    public static string GetPersistentPathForWWW()
    {
#if UNITY_EDITOR
        return Application.streamingAssetsPath;
#endif

        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                return "file://" + Application.persistentDataPath;
            case RuntimePlatform.IPhonePlayer:
                return "file://" + Application.persistentDataPath;
            default:
                return "file:///" + Application.persistentDataPath;
        }
    }

    /// <summary>
    /// 用以IO类操作的路径：StreamingAssetsPath
    /// </summary>
    /// <returns></returns>
    public static string GetStreamingAssetsPathForIO()
    {
#if UNITY_EDITOR
        return Application.streamingAssetsPath;
#endif

        switch (Application.platform)
        {
            //安卓的StreamingAssetsPath无法用于io操作，要用WWW操作
            case RuntimePlatform.Android:
                return "jar:file://" + Application.dataPath + "!/assets/";
            case RuntimePlatform.IPhonePlayer:
                return Application.dataPath + "/Raw/";
            default:
                return Application.streamingAssetsPath;
        }
    }

    /// <summary>
    /// 用以IO类操作的路径：PersistentPath
    /// </summary>
    /// <returns></returns>
    public static string GetPersistentPathForIO()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                return Application.persistentDataPath;
            case RuntimePlatform.IPhonePlayer:
                return Application.persistentDataPath;
            default:
                return Application.persistentDataPath;
        }
    }
}
