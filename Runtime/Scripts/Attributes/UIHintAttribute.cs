using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toorah.MirrorUI
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class UIHintAttribute : Attribute
    {
        public string hintText;

        public UIHintAttribute(string text)
        {
            hintText = text;
        }
    }
}
