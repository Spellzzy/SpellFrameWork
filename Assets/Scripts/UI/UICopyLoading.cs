using SpellFramework.Coroutine;
using SpellFramework.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICopyLoading : MonoBehaviour
{
    Image progressImg = null;
    TextMeshProUGUI progressText = null;
    private int curValue = 0;
    private int maxValue = 100;
    void Start()
    {
        progressImg = transform.Find("progressImg").GetComponent<Image>();
        progressText = transform.Find("progressText").GetComponent<TextMeshProUGUI>();
        progressText.SetText("--");
        progressImg.fillAmount = 0;
        EventSystem.AddListener(EventHandleType.OnResourceCopyProgress, OnResourceCopyProgress);
        EventSystem.AddListener(EventHandleType.OnResourceCopyOver, OnResourceCopyOver);
    }

    public void SetMax (int max)
    {
        maxValue = max;
    }

    private void OnResourceCopyOver(SpellFramework.Event.EventArgs args)
    {
        progressText.text = string.Format("{0}/{1}", maxValue, maxValue);
        progressImg.fillAmount = 1;
        CoroutineManager.StartDelayFrameAction(() =>
        {
            Destroy(this.gameObject);
        });
    }

    private void OnResourceCopyProgress(SpellFramework.Event.EventArgs args)
    {
        curValue = (int) args.args[0];
        progressText.text = string.Format("{0}/{1}", curValue, maxValue);
        progressImg.fillAmount = curValue / (float)maxValue;
    }
}
