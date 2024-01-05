using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZResLoad
{
    public enum PackMode
    {
        None = 0,
        /// <summary>
        /// 独立包
        /// </summary>
        Single = 1,
        /// <summary>
        /// 依赖包
        /// </summary>
        Dependence = 2
    }

    public class PackAttribute : Attribute
    {
        public string Name { get; private set; }
        public PackMode mode;

        public PackAttribute(string name)
        {
            this.Name = name;
            this.mode = PackMode.Dependence;
        }

        public PackAttribute(string name, PackMode mode)
        {
            this.Name = name;
            this.mode = mode;
        }
    }
}

