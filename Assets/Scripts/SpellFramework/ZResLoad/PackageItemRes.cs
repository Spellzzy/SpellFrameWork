using UnityEngine;

namespace ZResLoad
{
    internal class PackageItemRes
    {
        public string Name { get; private set; }
        public int RefCount { get; protected set; }

        protected Object Res;

        public PackageItemRes(string name, Object res)
        {
            this.RefCount = 0;
            this.Name = name;
            this.Res = res;
        }

        public int IncRef()
        {
            RefCount++;
            return RefCount;
        }

        public int DecRef()
        {
            RefCount--;
            return RefCount;
        }

        public Object GetItem()
        {
            return Res;
        }

        public void UnloadItem()
        {
            if (!(Res is GameObject || Res is AssetBundle || Res is Component))
            {
                Resources.UnloadAsset(Res);
            }
            Res = null;
            RefCount = 0;
        }
    }
}
