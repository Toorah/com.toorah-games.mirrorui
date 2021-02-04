using System;
using System.Collections.Generic;
using System.Reflection;
using Toorah.MirrorUI.Resources;
using UnityEngine;

public abstract class BaseUIBuilder
{
    protected PropertyInfo[] m_propertyInfos;

    public abstract Type BuilderType();
    public abstract void BuildUI<T>(T target, Transform container);

    private UIResources m_uiResources;
    protected UIResources UIResouces => GetUIResources();

    protected virtual UIResources GetUIResources()
    {
        if (m_uiResources == null)
        {
            m_uiResources = Resources.Load<UIResources>("UI Resources");
        }
        return m_uiResources;
    }

    private BindingFlags m_flags = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public;

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
