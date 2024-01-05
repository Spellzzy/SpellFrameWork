using System;
using System.Collections.Generic;
using System.IO;
using FairyGUI;
using UnityEngine;
using ZResLoad;

namespace SpellFramework.UI
{
    // ui层级
    public enum UILayerSort
    {
        Common = 0,     // 普通层级
        Tip = 100,        // 弹窗层级
        Top = 10000        // 顶层    
    }
    
    public static class UIManager
    {
        // 根据层级存储当前展示UI项
        private static Dictionary<UILayerSort, Stack<UIBase>> _uiLayerDic;
        public static void Init()
        {
            _uiLayerDic = new Dictionary<UILayerSort, Stack<UIBase>>();
        }


        // 打开ui 传入ui名称 枚举 加载该UI预设 传入参数
        public static void OpenUI(UILayerSort layer,string pkgName, string uiName, params object[] args)
        {
            if (!_uiLayerDic.ContainsKey(layer))
            {
                _uiLayerDic[layer] = new Stack<UIBase>();
            }
            else
            {
                foreach (var uiIns in _uiLayerDic[layer])
                {
                    if (uiIns.Name == uiName)
                    {
                        Debug.LogError("当前UI 已存在 不可重复打开");
                        return;
                    }
                }
            }
            var uiType = Type.GetType(uiName);
            if (uiType == null) return;
            {
                UIBase uiIns = (UIBase)uiType.Assembly.CreateInstance(uiName);
                if (uiIns == null) return;
                if (AppConst.LoadFromAB)
                {
                    var abDesc = ZResPackage.LoadPackage(string.Format("ui/{0}_fui", pkgName));
                    var abAtlas = ZResPackage.LoadPackage(string.Format("ui/{0}_atlas", pkgName));
                    UIPackage.AddPackage(abDesc, abAtlas);
                }
                else
                {
                    UIPackage.AddPackage(string.Format("UI/{0}", pkgName));
                }
                uiIns.OnInit(args);
                uiIns.Name = uiName;
                uiIns.LayerSort = layer;
                    
                // 动态设置Panel的sortingorder 来达到分层效果
                uiIns.Panel.sortingOrder = (int)layer + _uiLayerDic[layer].Count;
                // 新打开UI入栈
                _uiLayerDic[layer].Push(uiIns);
            }
        }

        public static void CloseUI(UIBase ui)
        {
            _uiLayerDic[ui.LayerSort].Pop();
        }
        
        public static void ClearAll()
        {
            foreach (var layerRoot in _uiLayerDic)
            {
                int childCount = layerRoot.Value.Count;
                for (int i = 0; i < childCount; i++)
                {
                    var uiIns = layerRoot.Value.Pop();
                    uiIns.OnClose(true);
                }
                layerRoot.Value.Clear();
            }   
        }
    }
}