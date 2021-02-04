using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toorah.MirrorUI
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UIRangeAttribute : Attribute
    {
        public float min;
        public float max;

        public UIRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
