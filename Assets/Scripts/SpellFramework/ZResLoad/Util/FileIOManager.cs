using LIBS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileIOManager
{
    private static bool _UseInnerFile;

    private static bool _UseStream;

    public static bool IsUseInnerFile()
    {
        return _UseInnerFile;
    }

    public static void UseInnerFile()
    {
        _UseInnerFile = true;
    }

    public static bool IsUseStreamFile()
    {
        return _UseStream;
    }

    public static void UseStreamFile(bool value)
    {
        _UseStream = value;
    }

    public static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public static void EnsureFilePath(string filePath)
    {
        if (File.Exists(filePath))
            return;

        var dir = Path.GetDirectoryName(filePath);

        CreateDeepDirectory(dir);
    }

    /// <summary>
    /// 获取目录的父路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetParentPath(string path)
    {
        if (path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            return Path.GetFullPath(path + "..");
        else
            return Path.GetFullPath(path + Path.DirectorySeparatorChar + "..");
    }

    public static bool SafeCreateDirectory(string path)
    {
        var result = false;
        try
        {
            Directory.CreateDirectory(path);
            result = true;
        }
        catch (Exception e)
        {
            if (e.InnerException != null)
                Console.WriteLine(e.InnerException.Message);
            else
                Console.WriteLine(e.Message);
        }
        return result;
    }

    public static bool CreateDeepDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        if (!Directory.Exists(path))
        {
            var parent = GetParentPath(path);
            if (CreateDeepDirectory(parent))
            {
                return SafeCreateDirectory(path);
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    public static bool CheckStreamingFileExist(string path)
    {
        return BetterStreamingAssets.FileExists(path);
    }

    public static bool CheckFileIsExist(string filePath)
    {
        return File.Exists(filePath);
    }

    public static Stream ReadFileStream(string root, string resName)
    {
        var abPath = Path.Combine(root ,resName) + ".ab";
        //if (IsUseInnerFile())
        //{
        //    if (BetterStreamingAssets.FileExists(resName))
        //    {
        //        return BetterStreamingAssets.OpenRead(resName);
        //    }
        //}
        if (CheckFileIsExist(abPath))
        {
            return File.OpenRead(abPath);
        }

        if (BetterStreamingAssets.FileExists(resName))
        {
            return BetterStreamingAssets.OpenRead(resName);
        }
        return null;
    }

    public static byte[] ReadAllBytes(string root, string resName)
    {
        var abPath = Path.Combine(root, resName) + ".ab";
        Debug.Log("abPath ==> " + abPath);
        if (IsUseInnerFile())
        {
            if (BetterStreamingAssets.FileExists(resName))
            {
                return BetterStreamingAssets.ReadAllBytes(resName);
            }
        }

        var check = CheckFileIsExist(abPath);
        Debug.Log("check -- " + check);
        if (CheckFileIsExist(abPath))
        {
            Debug.Log("2222");
            return File.ReadAllBytes(abPath);
        }

        if (BetterStreamingAssets.FileExists(resName))
        {
            return BetterStreamingAssets.ReadAllBytes(resName);
        }
        return null;
    }

    private static byte[] Decode(byte[] data)
    {
        // check 
        if (data[0] == 0x55 &&
            data[1] == 0x6E &&
            data[2] == 0x69 &&
            data[3] == 0x74 &&
            data[4] == 0x79 &&
            data[5] == 0x46 &&
            data[6] == 0x53)
        {
            return data;
        }

        // decode
        if (data[0] == 0x59 &&
            data[1] == 0x44 &&
            data[2] == 0x58 &&
            data[3] == 0x59 &&
            data[4] == 0x2D &&
            data[5] == 0x46 &&
            data[6] == 0x53)
        {
            XorCrypto.Decrypt(data, 8, 128);
            data[0] = (byte)'U';
            data[1] = (byte)'n';
            data[2] = (byte)'i';
            data[3] = (byte)'t';
            data[4] = (byte)'y';
            data[5] = (byte)'F';
            data[6] = (byte)'S';
            data[7] = (byte)'\0';
        }

        return data;
    }

    public static AssetBundle LoadFromFile(string root, string resName)
    {
        if (IsUseStreamFile())
        {
            Debug.LogFormat("root : {0} --- resname {1}", root, resName);
            var stream = ReadFileStream(root, resName);
            if (stream == null)
                return null;
            return AssetBundle.LoadFromStream(new ABStream(resName, stream));
        }
        else
        {
            Debug.Log("root ==> " + root);
            Debug.Log("resName ==> " + resName);
            var data = ReadAllBytes(root, resName);
            Debug.Log("data ---> " + data);
            if (data == null)
                return null;

            data = Decode(data);

            //调试调用资源输出
            ResourceDebug(resName);

            return AssetBundle.LoadFromStream(new MemoryStream(data));
        }
    }

    public static AssetBundleCreateRequest AsyncLoadFromFile(string root, string resName)
    {
        var stream = ReadFileStream(root, resName);
        if (stream == null)
        {
            return null;
        }

        return AssetBundle.LoadFromStreamAsync(new ABStream(resName, stream));
    }

    public static AssetBundleCreateRequest AsyncLoadFromFileStream(string root, string resName)
    {
        var stream = ReadFileStream(root, resName);
        if (stream == null)
        {
            return null;
        }
        return AssetBundle.LoadFromStreamAsync(new ABStream(resName, stream));
    }

    /// <summary>
    /// 输出信息逻辑方法
    /// </summary>
    /// <param name="resName">加载的资源名称</param>
    [System.Diagnostics.ConditionalAttribute("DEBUG_RES")]
    public static void ResourceDebug(string resName)
    {
        //输出Level + 资源名内容
        string log = "<color=red>[RES_ANALYSIS]</color> - [" + Time.realtimeSinceStartup + "]"  + "，Name：" + "/" + resName;
        Debug.Log(log);
    }


    internal class ABStream : Stream
    {
        private string _Name;
        private Stream _Stream;
        private long _Length;

        public ABStream(string name, Stream stream)
        {
            this._Name = name;
            this._Stream = stream;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get {
                if (_Length == 0)
                {
                    _Length = _Stream.Length;
                }
                return _Length;
            }
        }

        public override long Position
        {
            get {
                return _Stream.Position;
            }
            set {
                _Stream.Position = value;
            }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int from = (int)_Stream.Position;
            int read_num;
            if (from == 0)
            {
                read_num = _Stream.Read(buffer, offset, count);
                Decode(buffer);
            }
            else if (from > 0 && from <= 128)
            {
                read_num = _Stream.Read(buffer, offset, count);
                XorCrypto.Decrypt(buffer, 0, 128 - from, from);
            }
            else
            {
                read_num = _Stream.Read(buffer, offset, count);
            }

            return read_num;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _Stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            if (_Stream != null)
            {
                _Stream.Close();
                _Stream = null;
                _Name = "";
            }
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (_Stream != null)
            {
                _Stream.Dispose();
                _Stream = null;
            }
            base.Dispose(disposing);
        }
    }
}
