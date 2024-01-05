using UnityEngine;
using SpellFramework;
using SpellFramework.Coroutine;
using SpellFramework.Sound;
using SpellFramework.UI;
using SpellFramework.Event;
using UI;
using ZResLoad;
using UnityEngine.InputSystem;
using SpellFramework.Tools;

public class GameMain : MonoBehaviour
{
    void Start()
    {
        EventSystem.AddListener(EventHandleType.OnResourceCopyOver, OnResourceCopyOver);

        FileIOManager.UseStreamFile(true);

        // Screen.orientation = ScreenOrientation.LandscapeRight;
        Application.targetFrameRate = AppConst.GameFrameRate;  

        CoroutineManager.Init(this.gameObject);
        TimerTool.Init(this.gameObject);
        SoundManager.Instance.Init();      
        UIManager.Init();            
        
        // 首包资源检查
        StreamingAssetsCopyToPersistentData sa2pd = this.gameObject.AddComponent<StreamingAssetsCopyToPersistentData>();
        sa2pd.DoCopy();
    }

    // 等待解压资源完成后再进行下一步
    void OnResourceCopyOver(EventArgs args)
    {
        ZResourceManager.Init();
        BetterStreamingAssets.Initialize();

        // 初始化数据 依赖json文件
        DataSystem.LoadAllTables();

        UIManager.OpenUI(UILayerSort.Common, "common", UIPanelName.UILogin.ToString());
    }


    void Update()
    {
        // 资源管理
        var deltaTime = Time.deltaTime;
        ZResource.Tick(deltaTime);
        
        var mouse = Mouse.current;
        if (mouse == null)
        {
            return;
        }

        if (mouse.rightButton.isPressed)
        {
            // var pos = mouse.position;
            // Debug.LogFormat("{0},{1}", pos.x.value, pos.y.value);
            var pos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
            pos.z = 0;
            Debug.Log("pos ==》 " + pos);
            // DamagePopup.Create(pos);
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            Debug.Log("22222");
        }

        if (mouse.rightButton.wasReleasedThisFrame)
        {
           Debug.Log("33333333");
           Debug.Log(mouse.position);
        }

        var touch = Touchscreen.current; 
        if (touch == null)
        {
            return;
        }
    }
}
