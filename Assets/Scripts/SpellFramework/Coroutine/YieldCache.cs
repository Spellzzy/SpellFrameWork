using UnityEngine;

namespace SpellFramework.Coroutine
{
    public class YieldCache
    {
        private static YieldInstruction _WaitForEndOfFrame;
        private static YieldInstruction _WaitForFixedUpdate;

        private static YieldInstruction WaitForEndOfFrame
        {
            get
            {
                if (_WaitForEndOfFrame == null)
                {
                    _WaitForEndOfFrame = new YieldInstruction();
                }

                return _WaitForEndOfFrame;
            }
        }

        private static YieldInstruction WaitForFixedUpdate
        {
            get
            {
                if (_WaitForFixedUpdate == null)
                {
                    _WaitForFixedUpdate = new YieldInstruction();
                }

                return _WaitForFixedUpdate;
            }
        }

        public static YieldInstruction GetWaitForSeconds(float seconds)
        {
            return new WaitForSeconds(seconds);
        }

        public static YieldInstruction GetWaitForEndOfFrame()
        {
            return WaitForEndOfFrame;
        }

        public static YieldInstruction GetWaitForFixedUpdate()
        {
            return WaitForFixedUpdate;
        }
    }
}