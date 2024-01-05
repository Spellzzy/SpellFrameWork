using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using SpellFramework;

public class Util
{

    public static bool CopyFlag
    {
        get {
            return PlayerPrefs.GetInt("HasCopyed", 0) == 1;
        }

        set {
            if (value == true)
            {
                PlayerPrefs.SetInt("HasCopyed", 1);
            }
            else
            {
                PlayerPrefs.SetInt("HasCopyed", 0);
            }

        }
    }

    /// <summary>
    /// 取得数据存放目录
    /// </summary>
    public static string DataPath
    {
        get
        {
#if UNITY_EDITOR && !DEBUG_RES
            if (AppConst.DebugMode)
            {
                return Application.dataPath + "/" + AppConst.AssetDirName + "/" + AppConst.AbmName;
            }
            return Path.Combine(Application.persistentDataPath, AppConst.PersistentAbPath);
#else
                return Application.persistentDataPath + "/" + AppConst.PersistentAbPath;
#endif

        }
    }

    public static string HashFile(string path)
    {
        return MD5File(path);
    }

    public static string MD5File(string filePath)
    {
        try
        {
            var fs = new FileStream(filePath, FileMode.Open);
            var md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            var sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("md5file() fail, error:" + ex.Message);
        }
    }

    public static string FormatPath(string path)
    {
        return path.Replace("\\", "/");
    }
}
