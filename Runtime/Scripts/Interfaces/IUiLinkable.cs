using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Toorah.MirrorUI
{
    public interface IUiLinkable<T>
    {
        /// <summary>
        /// Link the UI with the properties from <paramref name="instance"/>
        /// </summary>
        /// <param name="instance">Class/Object instance reference</param>
        public void Link(T instance);
    }
}