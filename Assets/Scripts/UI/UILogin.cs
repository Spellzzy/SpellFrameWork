using System.IO;
using FairyGUI;
using SpellFramework;
using SpellFramework.UI;
using SpellFramework.Coroutine;
using SpellFramework.Sound;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZResLoad;

public class UILogin : UIBase
{
    private GButton _loginBtn;
    private GTextInput _userInputText;
    private GTextInput _passwordInputText;
    public override void OnInit(params object[] args)
    {
        base.OnInit(args);
        DoShow();
        
        var audio = ZResourceManager.LoadAudio("bg_loop");
        SoundManager.Instance.PlayBackground(audio);
    }

    public override void OnEnter()
    {
        
    }

    public override void OnClose(bool clear = false)
    {
        base.OnClose(clear);
        Debug.Log("login onclose");
    }

    public void DoShow()
    {
        Panel = UIPackage.CreateObject("common", "LoginPanel").asCom;
        GRoot.inst.AddChild(Panel);
        Panel.MakeFullScreen();

        _loginBtn = Panel.GetChild("loginBtn").asButton;
        _loginBtn.onClick.Set(OnClickLogin);

        _userInputText = Panel.GetChild("usernameInput").asTextInput;
        _passwordInputText = Panel.GetChild("passwordInput").asTextInput;
    }

    private void OnClickLogin(EventContext context)
    {
        string curUser = _userInputText.text;
        if (curUser == string.Empty)
        {
            return;
        }
        var data = DataSystem.AllTables.TbLogin.GetOrDefault(curUser);
        string statusStr = "";
        if (data != null)
        {
            string curPass = _passwordInputText.text;
            if (curPass == data.Password)
            {
                statusStr = "登录成功！！";
                
                CoroutineManager.StartDelaySecondAction(() =>
                {
                    UIManager.ClearAll();
                    UIManager.OpenUI(UILayerSort.Common, "common", UIPanelName.UIMain.ToString());
                    SceneManager.LoadScene("Main");
                }, 1, false);
            }
            else
            {
                statusStr = "密码错误！！";
            }
        }
        else
        {
            statusStr = "用户名错误！！";
        }
        UIManager.OpenUI(UILayerSort.Tip,"common", UIPanelName.UITip.ToString(), statusStr);

        var audio = ZResourceManager.LoadAudio("openbox");
        SoundManager.Instance.PlaySound(audio);
    }
}