using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using ZResLoad;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _ins;

    public static GameAssets Instance
    {
        get
        {
            return _ins = ZResourceManager.LoadModel("GameAssets").GetComponent<GameAssets>();
        }
    }

    public Transform DamagePopup;

}
