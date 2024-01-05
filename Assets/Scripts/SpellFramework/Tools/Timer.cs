using UnityEngine;
using UnityEngine.PlayerLoop;

namespace SpellFramework.Tools
{
    public class TimerBehaviour : MonoBehaviour
    {
        void Update()
        {
            TimerTool.CheckTime();
        }
    }


    public class TimerTool
    {
        // 
        private static MonoBehaviour timerBehaviour;
        // 局内标识符
        private static bool isGaming;
        // 局内倒计时长
        private static float gameTimeValue;
        private static float checkTimeFlag;

        public static float DoGameTime(float timeValue)
        {
            gameTimeValue = timeValue;
            checkTimeFlag = Time.realtimeSinceStartup;
            isGaming = true;
            return gameTimeValue;
        }

        public static float GetTime()
        {
            if (isGaming)
            {
                return gameTimeValue;
            }
            return 0.0f;
        }

        public static void CheckTime()
        {
            if(isGaming)
            {
                var curTimeFlag = Time.realtimeSinceStartup;                 
                gameTimeValue -= curTimeFlag - checkTimeFlag;    
                checkTimeFlag = curTimeFlag;   
                if (gameTimeValue <= 0.0f)
                {
                    isGaming = false;
                    gameTimeValue = 0.0f;
                }         
            }
        }

        public static void Init(GameObject go)
        {
            timerBehaviour = go.AddComponent<TimerBehaviour>();
        }

    }
}