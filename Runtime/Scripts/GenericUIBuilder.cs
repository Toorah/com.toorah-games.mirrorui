using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class GenericUIBuilder<T> : BaseUIBuilder<T>
{
    public GenericUIBuilder() : base() { }

    public GenericUIBuilder(BindingFlags flags) : base() 
    {
        Flags = flags;
    }

    public override void BuildUI<T>(T target, Transform container)
    {
        Debug.Log(target.GetType().Name);

        foreach (var info in m_propertyInfos)
        {
            Debug.Log($"{info.Name}[{info.PropertyType.Name}] : {(info.CanRead ? info.GetValue(target) : "<cannot read>")} --> {(info.CanWrite ? "writable" : "readonly")}");
        }
    }


}
