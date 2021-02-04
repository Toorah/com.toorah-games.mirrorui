using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System.Reflection;

namespace Toorah.MirrorUI
{
    public static class UILinker
    {
        #region Conversion
        static CultureInfo Invariant => CultureInfo.InvariantCulture;
        static NumberStyles Float => NumberStyles.Float;
        static NumberStyles Double => NumberStyles.Float | NumberStyles.AllowThousands;
        static NumberStyles Integer => NumberStyles.Integer;
        static NumberStyles UInteger => NumberStyles.Integer &~NumberStyles.AllowLeadingSign;
        static NumberStyles Short => Integer;
        static NumberStyles UShort => UInteger;
        static NumberStyles Long => Integer;
        static NumberStyles ULong => UInteger;
        #endregion


        #region Utils
        /// <summary>
        /// Default conversion from <typeparamref name="T"/> to string
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <param name="arg">value</param>
        /// <returns><code>arg.ToString();</code></returns>
        public static string DefaultFunc<T>(T arg)
        {
            return arg.ToString();
        }
        #endregion

        #region Dropdown
        /// <summary>
        /// Link a Dropdown to an Enum Value <paramref name="e"/>, and raise <paramref name="callback"/> on value changed
        /// </summary>
        /// <typeparam name="TEnum">An Enum Type</typeparam>
        /// <param name="dropdown">Reference to a Dropdown</param>
        /// <param name="e">Current Enum Value</param>
        /// <param name="callback">Callback when the dropdown value changes</param>
        public static void LinkDropdown<TEnum>(TMP_Dropdown dropdown, TEnum e, Action<TEnum> callback) where TEnum : Enum
        {
            var type = e.GetType();
            var stringValue = e.ToString();

            var options = new List<string>();
            options.AddRange(Enum.GetNames(type));
            var index = options.IndexOf(stringValue);
            var values = (int[])Enum.GetValues(type);
            dropdown.value = index;
            dropdown.onValueChanged.AddListener(v => callback?.Invoke((TEnum)(object)values[v]));
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }

        /// <summary>
        /// Link a dropdown to a generic IEnumerable with a Function to convert the elements to string and a callback on value changed
        /// </summary>
        /// <typeparam name="T">IEnumerable type</typeparam>
        /// <param name="dropdown">Reference to a dropdown</param>
        /// <param name="val">current value. MUST BE INCLUDED IN <paramref name="values"/></param>
        /// <param name="values">IEnumerable of <typeparamref name="T"/></param>
        /// <param name="callback">Callback that is raised when the dropdown changes</param>
        /// <param name="toString">Optional Function to convert <typeparamref name="T"/> to string, if NULL it uses <see cref="DefaultFunc{T}(T)"/></param>
        public static void LinkDropdown<T>(TMP_Dropdown dropdown, T val, IEnumerable<T> values, Action<T> callback, Func<T, string> toString = null)
        {
            List<T> valuesList = values.ToList();

            var options = new List<string>();
            if (toString == null)
                toString = DefaultFunc;

            options.AddRange(valuesList.Select(x => toString(x)));
            var index = valuesList.IndexOf(val);

            dropdown.value = index;
            dropdown.onValueChanged.AddListener(v => callback?.Invoke(valuesList[v]));
            dropdown.ClearOptions();
            dropdown.AddOptions(options);
        }
        #endregion

        #region Sliders
        public static void LinkSlider(this Slider slider, float value, Action<float> callback)
        {
            slider.value = value;
            slider.onValueChanged.AddListener(v => callback?.Invoke(v));
        }
        public static void LinkSlider(this Slider slider, float value, float? min, float? max, Action<float> callback)
        {
            if(min.HasValue)
                slider.minValue = min.Value;
            if(max.HasValue)
                slider.maxValue = max.Value;
            slider.LinkSlider(value, callback);
        }
        #endregion

        #region Toggles
        /// <summary>
        /// Link
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="isOn"></param>
        /// <param name="callback"></param>
        public static void LinkToggle(this Toggle toggle, bool isOn, Action<bool> callback)
        {
            toggle.isOn = isOn;
            toggle.onValueChanged.AddListener(b => callback?.Invoke(b));
        }
        #endregion

        #region Input Field
        public static void LinkInput(this TMP_InputField input, string value, Action<string> callback)
        {
            input.text = value;
            input.onValueChanged.AddListener(s => callback?.Invoke(s));
        }
        public static void LinkInput(this TMP_InputField input, float value, Action<float> callback)
        {
            input.text = value.ToString(Invariant);
            input.onValueChanged.AddListener(s =>
            {
            if (float.TryParse(s, Float, Invariant, out var res))
                {
                    callback?.Invoke(res);
                }
            });
        }
        public static void LinkInput(this TMP_InputField input, double value, Action<double> callback)
        {
            input.text = value.ToString(Invariant);
            input.onValueChanged.AddListener(s =>
            {
                if (double.TryParse(s, Double, Invariant, out var res))
                {
                    callback?.Invoke(res);
                }
            });
        }
        public static void LinkInput(this TMP_InputField input, int value, Action<int> callback)
        {
            input.text = value.ToString(Invariant);
            input.onValueChanged.AddListener(s =>
            {
                if (int.TryParse(s, Integer, Invariant, out var res))
                {
                    callback?.Invoke(res);
                }
            });
        }
        public static void LinkInput(this TMP_InputField input, uint value, Action<uint> callback)
        {
            input.text = value.ToString(Invariant);
            input.onValueChanged.AddListener(s =>
            {
                if (uint.TryParse(s, UInteger, Invariant, out var res))
                {
                    callback?.Invoke(res);
                }
            });
        }

        public static void LinkInput(this TMP_InputField input, short value, Action<short> callback)
        {
            input.text = value.ToString(Invariant);
            input.onValueChanged.AddListener(s =>
            {
                if (short.TryParse(s, Short, Invariant, out var res))
                {
                    callback?.Invoke(res);
                }
            });
        }
        public static void LinkInput(this TMP_InputField input, ushort value, Action<ushort> callback)
        {
            input.text = value.ToString(Invariant);
            input.onValueChanged.AddListener(s =>
            {
                if (ushort.TryParse(s, UShort, Invariant, out var res))
                {
                    callback?.Invoke(res);
                }
            });
        }

        public static void LinkInput(this TMP_InputField input, long value, Action<long> callback)
        {
            input.text = value.ToString(Invariant);
            input.onValueChanged.AddListener(s =>
            {
                if (long.TryParse(s, Long, Invariant, out var res))
                {
                    callback?.Invoke(res);
                }
            });
        }
        public static void LinkInput(this TMP_InputField input, ulong value, Action<ulong> callback)
        {
            input.text = value.ToString(Invariant);
            input.onValueChanged.AddListener(s =>
            {
                if (ulong.TryParse(s, ULong, Invariant, out var res))
                {
                    callback?.Invoke(res);
                }
            });
        }
        #endregion
    }
}
