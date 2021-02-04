using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Toorah.MirrorUI;
using UnityEngine;
using UnityEngine.UI;

public class GenericUIBuilder<T> : BaseUIBuilder<T>
{
    public GenericUIBuilder() : base() { }

    public GenericUIBuilder(BindingFlags flags) : base() 
    {
        Flags = flags;
    }

#pragma warning disable 693
    public override void BuildUI<T>(T target, Transform container)
    {
        Debug.Log(target.GetType().Name);
        
        foreach (var info in m_propertyInfos)
        {
            var isReadonly = info.CanRead && !info.CanWrite;

            if(info.PropertyType == typeof(string))
            {
                if (isReadonly)
                {
                    var label = GameObject.Instantiate(UIResouces.label);

                    label.text = (string)info.GetValue(target);
                    label.SetParent(container);
                }
                else
                {
                    var label = GameObject.Instantiate(UIResouces.label);

                    label.text = info.Name;
                    label.transform.SetParent(container, false);

                    var inputField = GameObject.Instantiate(UIResouces.inputField);

                    inputField.text = (string)info.GetValue(target);
                    inputField.onValueChanged.AddListener(s => info.SetValue(target, s));


                    info.AttributeAction<UIHintAttribute>((a) => 
                    {
                        inputField.placeholder.GetComponent<TextMeshProUGUI>().text = a.hintText;
                    });

                    inputField.SetParent(container);
                }
            }else if(info.PropertyType == typeof(bool))
            {
                var toggle = GameObject.Instantiate(UIResouces.toggle);

                var label = toggle.GetComponentInChildren<TextMeshProUGUI>();
                label.text = info.Name;

                toggle.isOn = (bool)info.GetValue(target);
                toggle.onValueChanged.AddListener(b => info.SetValue(target, b));

                toggle.enabled = !isReadonly;

                toggle.SetParent(container);
            }else if(info.PropertyType.IsArray)
            {
                var elementType = info.PropertyType.GetElementType();
                if(elementType == typeof(string))
                {
                    var dropdown = GameObject.Instantiate(UIResouces.dropdown);
                    dropdown.SetParent(container);
                    
                }
                else{
                    var label = GameObject.Instantiate(UIResouces.label);

                    label.text = "Array";
                    label.SetParent(container);
                }
            }
        }
    }
}

public static class UIExtensions
{
    public static void SetOptions<T>(this TMP_Dropdown dropdown, T[] options)
    {
        List<string> optionNames = new List<string>();
        for (int i = 0; i < optionNames.Count; i++)
        {
            optionNames.Add(options[i].ToString());
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(optionNames);
    }
    public static void SetOptions<T>(this TMP_Dropdown dropdown, List<T> options)
    {
        List<string> optionNames = new List<string>();
        for (int i = 0; i < optionNames.Count; i++)
        {
            optionNames.Add(options[i].ToString());
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(optionNames);
    }
    public static void SetOptions<T>(this TMP_Dropdown dropdown) where T : Enum
    {
        var names = Enum.GetNames(typeof(T));
        List<string> optionNames = new List<string>();
        optionNames.AddRange(names);

        dropdown.ClearOptions();
        dropdown.AddOptions(optionNames);
    }




    public static void AttributeAction<T>(this PropertyInfo pi, Action<T> action) where T : Attribute
    {
        var attr = pi.GetCustomAttribute<T>();
        if (attr != null)
            action(attr);
    }

    public static void SetParent(this Graphic ui, Transform parent, bool worldPositionStays = false)
    {
        ui.transform.SetParent(parent, worldPositionStays);
    }
    public static void SetParent(this Selectable ui, Transform parent, bool worldPositionStays = false)
    {
        ui.transform.SetParent(parent, worldPositionStays);
    }
    public static void SetParent(this MaskableGraphic ui, Transform parent, bool worldPositionStays = false)
    {
        ui.transform.SetParent(parent, worldPositionStays);
    }
}