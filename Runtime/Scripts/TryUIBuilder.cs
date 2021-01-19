using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

public class TryUIBuilder : MonoBehaviour
{
    Dictionary<Type, BaseUIBuilder> m_uiBuilders = new Dictionary<Type, BaseUIBuilder>();


    // Start is called before the first frame update
    void Start()
    {
        m_uiBuilders.Add(typeof(TestClass), new TestClassUIBuilder());

        var uiBuilder = GetBuilder<TestClass>();
        uiBuilder.BuildUI(new TestClass(), null);

        var uiBuilder2 = GetBuilder<TestClass2>();
        uiBuilder2.BuildUI(new TestClass2(), null);

        var uiBuilder3 = GetBuilder<Transform>();
        uiBuilder3.BuildUI(transform, null);

    }

    BaseUIBuilder<T> GetBuilder<T>(BindingFlags? flags = null)
    {
        if(m_uiBuilders.TryGetValue(typeof(T), out var builder))
        {
            return (BaseUIBuilder<T>)builder;
        }
        else
        {
            Debug.Log($"Created new UI Builder<{typeof(T)}>");
            if(flags != null)
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

    public class TestClassUIBuilder : BaseUIBuilder<TestClass>
    {

        public override void BuildUI<T>(T target, Transform container)
        {
            Debug.Log(target.GetType().Name);

            foreach (var info in m_propertyInfos)
            {
                Debug.Log($"{info.Name}[{info.PropertyType.Name}] : {(info.CanRead ? info.GetValue(target) : "<cannot read>")} --> {(info.CanWrite ? "writable" : "readonly")}");
            }
        }

        public override BindingFlags Flags
        {
            get => BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public;
        }
    }

    public class TestClass
    {
        public bool test;
        private float test2;

        public bool Flunk { get; set; }
        private float Flunk2 { get; set; }
        public int NoNo { get; }
        public static string SomeVal { get; } = "Testings";

        public TestClass()
        {
            test = Random.Range(0, 2) == 0;
            test2 = Random.Range(0, 10);

            Flunk = Random.Range(0, 10) < 5;
            Flunk2 = Random.Range(0.1f, 2.7f);
            NoNo = Random.Range(-5, 5);
        }
    }
    public class TestClass2
    {
        public bool test;
        private float test2;

        public bool Flunk5 { get; set; }
        private float Flunk7 { get; set; }
        public int NoNo { get; }
        public static string SomeVal { get; } = "Nothings";

        public TestClass2()
        {
            test = Random.Range(0, 2) == 0;
            test2 = Random.Range(0, 10);

            Flunk5 = Random.Range(0, 10) < 5;
            Flunk7 = Random.Range(0.1f, 2.7f);
            NoNo = Random.Range(-5, 5);
        }
    }
}
