using System;
using UnityEngine;

public class Explosion : WarEntity
{
    static int colorPropertyID = Shader.PropertyToID("_Color");

    static MaterialPropertyBlock propertyBlock;    
    
    [SerializeField]
    private AnimationCurve opacityCurve = default;
    [SerializeField]
    private AnimationCurve scaleCurve = default;
    [SerializeField, Range(0f, 1f)]
    float duration = 0.5f;
    [SerializeField]
    private Color showColor;

    float age;
    float scale;

    MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }


    public void Initialize (Vector3 position, float blastRadius, float damage = 0f) {
        age = 0f;
        if (damage > 0f)
        {
            TargetPoint.FillBuffer(position, blastRadius);
            for (int i = 0; i < TargetPoint.BufferedCount; i++) {
                TargetPoint.GetBuffered(i).Enemy.ApplyDamage(damage);
            }
        }
        transform.localPosition = position;
        scale = 2f * blastRadius;
    }
    

    public override bool GameUpdate () {
        age += Time.deltaTime;
        if (age >= duration) {
            OriginFactory.Reclaim(this);
            return false;
        }
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        float t = age / duration;
        // Color c = Color.clear;
        showColor.a = opacityCurve.Evaluate(t);
        propertyBlock.SetColor(colorPropertyID, showColor);
        meshRenderer.SetPropertyBlock(propertyBlock);
        transform.localScale = Vector3.one * (scale * scaleCurve.Evaluate(t));        
        return true;
    }
}