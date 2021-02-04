using System;
using System.Collections.Generic;
using System.Reflection;
using Toorah.MirrorUI;
using UnityEngine;

namespace ToorahUI.Test
{


    public class TestUIBuilder : MonoBehaviour
    {
        Dictionary<Type, BaseUIBuilder> m_uiBuilders = new Dictionary<Type, BaseUIBuilder>();
        public Transform container;
        public TestClass test;
        // Start is called before the first frame update
        void Start()
        {
            test = new TestClass();
            //m_uiBuilders.Add(typeof(TestClass), new TestClassUIBuilder());

            var uiBuilder = GetBuilder<TestClass>();
            uiBuilder.BuildUI(test, container);
        }

        BaseUIBuilder<T> GetBuilder<T>(BindingFlags? flags = null)
        {
            if (m_uiBuilders.TryGetValue(typeof(T), out var builder))
            {
                return (BaseUIBuilder<T>)builder;
            }
            else
            {
                Debug.Log($"Created new UI Builder<{typeof(T)}>");
                if (flags != null)
                {
                    var b = new GenericUIBuilder<T>((BindingFlags)flags);
                    m_uiBuilders.Add(typeof(T), b);
                    return b;
                }
                else
                {
                    var b = new GenericUIBuilder<T>();
                    m_uiBuilders.Add(typeof(T), b);
                    return b;
                }
            }
        }

        [System.Serializable]
        public class TestClass
        {
            [UIHint("Enter Firstname")]
            public string firstname { get; set; }
            public string surname { get; set; }
            [UIHint("Email")]
            public string address { get; set; }
            public bool isDeveloper { get; set; }
            public bool isPatron { get; set; }
            public bool subscribeToSpam { get; set; }
            public string[] ArrayTest { get; set; } = new string[] { "A", "B", "C" };

            private string sessionId { get; set; }
        }

    }

}