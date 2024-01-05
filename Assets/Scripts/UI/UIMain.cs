using UnityEngine;
using FairyGUI;
using SpellFramework.Coroutine;
using SpellFramework.UI;
using ZResLoad;
using SpellFramework.Tools;
using SpellFramework.Event;

public class UIMain : UIBase
{
    private GameObject _showGo;

    private GButton _stopBtn;

    private GTextField _timerText;

    private int _dialogID = 0;
    public void DoShow()
    {
        Panel = UIPackage.CreateObject("common", "MainPanel").asCom;
        GRoot.inst.AddChild(Panel);
        Panel.MakeFullScreen();
        
        _showGo = ZResourceManager.LoadModel("cube");
        _showGo.transform.SetParent(this.Panel.displayObject.gameObject.transform);
        CoroutineManager.StartDelaySecondAction(() =>
        {
            GameObject.Destroy(_showGo);
            _showGo = null;
        }, 5f);


        _timerText = Panel.GetChild("timrtText").asTextField;
        TimerTool.DoGameTime(60f);
        CoroutineManager.StartDelaySecondRepeatedAction(()=>{
            _timerText.text = TimerTool.GetTime().ToString("0.00");
        }, 0.0f, 0.15f, -1, true);

        _stopBtn = Panel.GetChild("stopBtn").asButton;
        _stopBtn.visible = false;
        _stopBtn.onClick.Set(()=>{
            UIManager.OpenUI(UILayerSort.Common, "common", UI.UIPanelName.UIDialog.ToString(), _dialogID);
        });

        EventSystem.AddListener("OnDialogEnter", (evt) =>
        {
            _dialogID = (int)evt.args[0];
            _stopBtn.visible = true;
        });

        EventSystem.AddListener("OnDialogExit", (evt) =>
        {
            _stopBtn.visible = false;
        });
    }

    public override void OnInit(params object[] args)
    {
        base.OnInit(args);
        DoShow();
    }

    public override void OnEnter()
    {
        
    }
    
}
