using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public enum Clip { Move, Intro, Outro, Dying }

[System.Serializable]
public struct EnemyAnimator
{
    private PlayableGraph _graph;

    private AnimationMixerPlayable _mixer;
    
    public Clip CurrentClip
    {
        get;
        private set;
    }

    private Clip previousClip;

    private float transitionProgress;

    private const float transitionSpeed = 5f;

    public bool IsDone => GetPlayable(CurrentClip).IsDone();
    
    public void Config(Animator animator, EnemyAnimationConfig config)
    {
        _graph = PlayableGraph.Create();
        _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        _mixer = AnimationMixerPlayable.Create(_graph, 4);
        
        var clip = AnimationClipPlayable.Create(_graph, config.Move);
        clip.Pause();
        _mixer.ConnectInput((int)Clip.Move, clip, 0);

        clip = AnimationClipPlayable.Create(_graph, config.Intro);
        clip.SetDuration(config.Intro.length);
        _mixer.ConnectInput((int)Clip.Intro, clip, 0);
        
        clip = AnimationClipPlayable.Create(_graph, config.Outro);
        clip.SetDuration(config.Outro.length);
        clip.Pause();
        _mixer.ConnectInput((int)Clip.Outro, clip, 0);
        
        clip = AnimationClipPlayable.Create(_graph, config.Dying);
        clip.SetDuration(config.Dying.length);
        clip.Pause();
        _mixer.ConnectInput((int)Clip.Dying, clip, 0);
        
        
        var output = AnimationPlayableOutput.Create(_graph, "Enemy", animator);
        output.SetSourcePlayable(_mixer);
        
    }

    public void GameUpdate()
    {
        if (transitionProgress >= 0f)
        {
            transitionProgress += Time.deltaTime * transitionSpeed;
            if (transitionProgress >= 1f)
            {
                transitionProgress = -1f;
                SetWeight(CurrentClip, 1f);
                SetWeight(previousClip, 0f);
                GetPlayable(previousClip).Pause();
            }
            else
            {
                SetWeight(CurrentClip, transitionProgress);
                SetWeight(previousClip, 1 - transitionProgress);
            }
        }
        
    }

    // public void Play(float speed)
    // {
    //     _graph.GetOutput(0).GetSourcePlayable().SetSpeed(speed);
    //     _graph.Play();
    // }

    public void PlayIntro()
    {
        SetWeight(Clip.Intro, 1f);
        CurrentClip = Clip.Intro;
        _graph.Play();
        transitionProgress = -1f;
    }

    public void PlayMove(float speed)
    {
        // SetWeight(CurrentClip, 0f);
        // SetWeight(Clip.Move, 1f);
        // var clip = GetPlayable(Clip.Move);
        // clip.SetSpeed(speed);
        // clip.Play();
        // CurrentClip = Clip.Move;

        GetPlayable(Clip.Move).SetSpeed(speed);
        BeginTransition(Clip.Move);
    }

    public void PlayOutro()
    {
        // SetWeight(CurrentClip, 0f);
        // SetWeight(Clip.Outro, 1f);
        // GetPlayable(Clip.Outro).Play();
        // CurrentClip = Clip.Outro;
        BeginTransition(Clip.Outro);
    }
    
    public void PlayDying () {
        BeginTransition(Clip.Dying);
    }

    Playable GetPlayable(Clip clip)
    {
        return _mixer.GetInput((int)clip);
    }

    void SetWeight(Clip clip, float weight)
    {
        _mixer.SetInputWeight((int)clip, weight);
    }

    // 动画融合
    void BeginTransition(Clip nextClip)
    {
        previousClip = CurrentClip;
        CurrentClip = nextClip;
        transitionProgress = 0f;
        GetPlayable(nextClip).Play();
    }

    public void Stop()
    {
        _graph.Stop();
    }

    public void Destroy()
    {
        _graph.Destroy();
    }
}
