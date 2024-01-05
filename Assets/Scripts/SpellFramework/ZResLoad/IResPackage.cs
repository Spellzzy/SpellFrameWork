using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace ZResLoad
{
    public interface IResPackage
    {
        /// <summary>
        /// 已加载资源数
        /// </summary>
        int ResLoadedCount { get; }

        /// <summary>
        /// 引用计数
        /// </summary>
        int RefCount { get; }

        /// <summary>
        /// 添加引用
        /// </summary>
        /// <returns></returns>
        int IncRef();

        /// <summary>
        /// 解除引用
        /// </summary>
        /// <returns></returns>
        int DecRef();

        /// <summary>
        /// 添加依赖包
        /// </summary>
        /// <param name="package"></param>
        void AddDeps(IResPackage package);

        /// <summary>
        /// 获取依赖包
        /// </summary>
        /// <returns></returns>
        IList<IResPackage> GetDeps();

        /// <summary>
        /// 资源包名
        /// </summary>
        /// <returns></returns>
        string Name();

        /// <summary>
        /// 资源包的相对路径
        /// </summary>
        /// <param name="url"></param>
        void SetResURL(string url);

        /// <summary>
        /// 加载资源包到内存
        /// </summary>
        /// <returns></returns>
        bool LoadPackage();

        /// <summary>
        /// 协程加载
        /// </summary>
        /// <returns></returns>
        IEnumerator CoLoadPackage();

        /// <summary>
        /// 卸载资源包
        /// </summary>
        void UnloadPackage();

        /// <summary>
        /// 获取一项资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        UObject LoadRes(string name);

        /// <summary>
        /// 协程获取资源
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEnumerator CoLoadRes(string name);

        /// <summary>
        /// 获取所有资源
        /// </summary>
        /// <returns></returns>
        UObject[] LoadAllRes();

        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="unityObj"></param>
        /// <returns></returns>
        bool UnloadRes(UObject unityObj);
        
        /// <summary>
        /// 释放所有资源
        /// </summary>
        void UnloadAll();
        
        /// <summary>
        /// 获取AB
        /// </summary>
        /// <returns></returns>
        AssetBundle GetAB();
    }

}
