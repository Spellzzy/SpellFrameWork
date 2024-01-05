using UnityEngine;

[CreateAssetMenu]
public class EnemyAnimationConfig : ScriptableObject
{
    [SerializeField]
    private AnimationClip move = default;

    [SerializeField]
    private AnimationClip intro = default;
    
    [SerializeField]
    private AnimationClip outro = default;
    
    [SerializeField]
    private AnimationClip dying = default;
    
    public AnimationClip Move => move;

    public AnimationClip Intro => intro;

    public AnimationClip Outro => outro;

    public AnimationClip Dying => dying;
}
