using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toorah.MirrorUI
{
    public class InputHintAttribute : Attribute
    {
        public string hintText;

        public InputHintAttribute(string text)
        {
            hintText = text;
        }
    }
}
