using System;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu]
public class GameScenario : ScriptableObject {

    [SerializeField]
    EnemyWave[] waves = {};
    
    [SerializeField, Range(0, 10)]
    int cycles = 1;
    
    [SerializeField, Range(0f, 1f)]
    float cycleSpeedUp = 0.5f;

    public State Begin() => new State(this);
    [Serializable]
    public struct State
    {
        private GameScenario scenario;
        private int index;
        private int cycle;
        private float timeScale;
        private EnemyWave.State wave;
        public State(GameScenario scenario)
        {
            this.scenario = scenario;
            index = 0;
            cycle = 0;
            timeScale = 1f;
            wave = scenario.waves[0].Begin();
        }

        public bool Progress()
        {
            float deltaTime = wave.Progress(timeScale * Time.deltaTime);
            while (deltaTime >= 0f)
            {
                if (++index >= scenario.waves.Length)
                {
                    if (++cycle >= scenario.cycles && scenario.cycles > 0)
                    {
                        return false;
                    }
                    index = 0;
                    timeScale += scenario.cycleSpeedUp;
                }
                wave = scenario.waves[index].Begin();
                deltaTime = wave.Progress(deltaTime);
            }

            return true;
        }
    }
    
    
    
}