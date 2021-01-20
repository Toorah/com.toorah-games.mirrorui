using System;
using System.Collections.Generic;
using System.Reflection;
using Toorah.MirrorUI.Resources;
using UnityEngine;

public abstract class BaseUIBuilder
{
    protected PropertyInfo[] m_propertyInfos;
    protected List<FieldInfo> m_fieldInfos = new List<FieldInfo>();

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
        m_fieldInfos.AddRange(BuilderType().GetFields(Flags));
        foreach (var info in m_propertyInfos)
        {
            m_fieldInfos.Remove(m_fieldInfos.Find(f => f.Name == GetBackingFieldName(info)));
        }
    }




    protected string GetBackingFieldName(PropertyInfo pi)
    {
        return $"<{pi.Name}>k__BackingField";
    }
}

public abstract class BaseUIBuilder<T> : BaseUIBuilder
{
    public override Type BuilderType()
    {
        return typeof(T);
    }
}
