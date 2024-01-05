using UnityEngine;
using FairyGUI;

namespace SpellFramework.UI
{
    public class UIBase
    {
        public string Name;
        public GComponent Panel;

        public UILayerSort LayerSort;

        public virtual void OnInit(params object[] args)
        {
            Debug.Log("UIBase OnInit");
        }

        public virtual void OnEnter()
        {
            
        }

        public virtual void OnClose(bool clear = false)
        {
            if (!clear)
            {
                UIManager.CloseUI(this);
            }
            GRoot.inst.RemoveChild(Panel, true);
            Panel.Dispose();
        }
        
    }
}