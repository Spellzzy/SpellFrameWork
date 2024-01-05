using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;
using cfg;
using SpellFramework;

public class DataSystem
{
    private static Tables _allTables;
    public static Tables AllTables
    {
        get { return _allTables; }
    }

    public static void LoadAllTables()
    {
        string DataDir = Path.Combine(Util.DataPath, "data");
        _allTables = new cfg.Tables(file => JSON.Parse(File.ReadAllText($"{DataDir}/{file}.json")));
        
        var loginList = _allTables.TbLogin.DataList;
        foreach (var login in loginList)
        {
            Debug.LogFormat("{0} -- {1}", login.User ,login.Password);
        }
        
        cfg.Login loginInfo = _allTables.TbLogin.Get("zzy");
    }
}
