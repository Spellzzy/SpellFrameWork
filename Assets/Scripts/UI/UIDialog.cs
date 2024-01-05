using SpellFramework.UI;
using FairyGUI;
using UnityEngine;
public class UIDialog : UIBase
{
    private TypingEffect _typingEffect;
    private GTextField _contentText; 
    private GButton _nextBtn;
    private Controller _stateController;
    private int _dialogID;   
    private int[] _showItems;
    private int _curIndex = 0;
    public override void OnInit(params object[] args)
    {
        base.OnInit(args);
        _dialogID = (int)args[0];
        var data = DataSystem.AllTables.TbDialog.Get(_dialogID);  
        _showItems = data.Content;
        DoShow();
    }

    public override void OnEnter()
    {
        
    }

    public override void OnClose(bool clear = false)
    {
        base.OnClose(clear);
    }

    public void DoShow()
    {
        Panel = UIPackage.CreateObject("common", "DialogPanel").asCom;
        GRoot.inst.AddChild(Panel);
        Panel.MakeFullScreen();

        _contentText = Panel.GetChild("contentText").asTextField;

        _nextBtn = Panel.GetChild("nextBtn").asButton;
        _nextBtn.onClick.Set(OnClickNext);

        _stateController = Panel.GetController("state");

        _typingEffect = new TypingEffect(_contentText);
        DoShowDialog(0);
    }

    private void DoShowDialog(int index)
    {
        var data = DataSystem.AllTables.TbDialogItem.Get(_showItems[index]);
        _contentText.text = data.Text;
        _stateController.selectedIndex = data.Type - 1;
        _typingEffect.Start();
        Timers.inst.StartCoroutine(_typingEffect.Print(0.050f));
    }

    private void OnClickNext(EventContext context)
    {
        _curIndex++;
        if (_curIndex >= _showItems.Length)
        {
            _typingEffect.Cancel();
            OnClose();
            return;
        }
        DoShowDialog(_curIndex);
    }

}
