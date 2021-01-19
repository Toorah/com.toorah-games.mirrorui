using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class BaseUIBuilder
{
    protected PropertyInfo[] m_propertyInfos;
    public abstract Type BuilderType();
    public abstract void BuildUI<T>(T target, Transform container);

    private BindingFlags m_flags = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

    virtual public BindingFlags Flags
    {
        get => m_flags;
        protected set => m_flags = value;
    }

    public BaseUIBuilder()
    {
        m_propertyInfos = BuilderType().GetProperties(Flags);
    }
}

public abstract class BaseUIBuilder<T> : BaseUIBuilder
{
    public override Type BuilderType()
    {
        return typeof(T);
    }
}
