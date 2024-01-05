using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DamagePopup : MonoBehaviour
{
    private TextMeshPro textMesh;
    private Vector3 moveVector;
    private Color textColor;
    public float disappearTime;
    public float disappearSpeed;
    private int sortingOrder = 0;
    void Awake()
    {
        textMesh = gameObject.GetComponent<TextMeshPro>();
        textColor = textMesh.color;
    }

    public static DamagePopup Create(Vector3 pos, int damageValue, bool isCritical = false)
    {
        var go = Instantiate(GameAssets.Instance.DamagePopup, pos, Quaternion.identity);
        var com = go.GetComponent<DamagePopup>();
        com.Setup(damageValue, isCritical);
        return com;
    }

    private void Setup(int damageValue, bool isCritical)
    {
        if (isCritical)
        {
            textColor = Color.red;
            textMesh.color = textColor;
            textMesh.fontSize = 7;
        }
        moveVector = new Vector3(0.7f, 1);
        sortingOrder++;
        textMesh.sortingOrder = sortingOrder;
        textMesh.text = damageValue.ToString();
    }

    private void Update()
    {
        disappearTime -= Time.deltaTime;
        if (disappearTime <= 0)
        {
            textColor.a -= Time.deltaTime * disappearSpeed;
            textMesh.color = textColor;
            if (textColor.a <= 0)
            {
                Destroy(this.gameObject);
            }
        }
        else
        {
            transform.position += moveVector * Time.deltaTime;
        }
    }
}
