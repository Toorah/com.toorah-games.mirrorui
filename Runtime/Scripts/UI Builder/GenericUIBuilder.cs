using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Toorah.MirrorUI;
using UnityEngine;

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
            Transform ui = null;

            if(info.PropertyType == typeof(string))
            {
                if (isReadonly)
                {
                    var label = GameObject.Instantiate(UIResouces.label);
                    ui = label.transform;

                    label.text = (string)info.GetValue(target);
                }
                else
                {
                    var label = GameObject.Instantiate(UIResouces.label);

                    label.text = info.Name;
                    label.transform.SetParent(container, false);

                    var inputField = GameObject.Instantiate(UIResouces.inputField);
                    ui = inputField.transform;

                    inputField.text = (string)info.GetValue(target);
                    inputField.onValueChanged.AddListener(s => info.SetValue(target, s));

                    var attr = info.GetCustomAttribute<InputHintAttribute>();
                    if(attr != null)
                    {
                        inputField.placeholder.GetComponent<TextMeshProUGUI>().text = attr.hintText;
                    }
                }
            }else if(info.PropertyType == typeof(bool))
            {
                var toggle = GameObject.Instantiate(UIResouces.toggle);
                ui = toggle.transform;

                var label = toggle.GetComponentInChildren<TextMeshProUGUI>();
                label.text = info.Name;

                toggle.isOn = (bool)info.GetValue(target);
                toggle.onValueChanged.AddListener(b => info.SetValue(target, b));

                toggle.enabled = !isReadonly;
            }


            if (ui)
            {
                ui.SetParent(container, false);
            }

        }


        foreach (var info in m_fieldInfos)
        {
            Transform ui = null;

            if (info.FieldType == typeof(string))
            {
                var label = GameObject.Instantiate(UIResouces.label);

                label.text = info.Name;
                label.transform.SetParent(container, false);

                var inputField = GameObject.Instantiate(UIResouces.inputField);
                ui = inputField.transform;

                inputField.text = (string)info.GetValue(target);
                inputField.onValueChanged.AddListener(s => info.SetValue(target, s));

                var attr = info.GetCustomAttribute<InputHintAttribute>();
                if (attr != null)
                {
                    inputField.placeholder.GetComponent<TextMeshProUGUI>().text = attr.hintText;
                }
            }
            else if (info.FieldType == typeof(bool))
            {
                var toggle = GameObject.Instantiate(UIResouces.toggle);
                ui = toggle.transform;

                var label = toggle.GetComponentInChildren<TextMeshProUGUI>();
                label.text = info.Name;

                toggle.isOn = (bool)info.GetValue(target);
                toggle.onValueChanged.AddListener(b => info.SetValue(target, b));
            }


            if (ui)
            {
                ui.SetParent(container, false);
            }

        }
    }


}
