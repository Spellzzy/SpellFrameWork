using FairyGUI;
using SpellFramework.UI;
using UnityEngine;

public class UITip : UIBase
{
    private GTextField _tipText;
    private GButton _confirmBtn;
    public void DoShow()
    {
        Panel = UIPackage.CreateObject("common", "TipPanel").asCom;
        _tipText = Panel.GetChild("tipText").asTextField;
        
        _confirmBtn = Panel.GetChild("confirmBtn").asButton;
        _confirmBtn.onClick.Set(() => { OnClose(); });
        GRoot.inst.AddChild(Panel);
        Panel.MakeFullScreen();
    }

    public override void OnInit(params object[] args)
    {
        base.OnInit(args);
        DoShow();
        _tipText.text = args[0].ToString();
    }

    public override void OnEnter()
    {
        
    }

    public override void OnClose(bool clear = false)  
    {
        base.OnClose(clear);
        Debug.Log("tip onclose");
    }
}