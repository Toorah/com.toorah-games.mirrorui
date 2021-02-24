using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Toorah.MirrorUI
{
    /// <summary>
    /// Reflection Helper Extension Class
    /// </summary>
    public static class Mirror
    {
        #region Check Types
        /// <summary>
        /// Return if <paramref name="type"/> is a string
        /// </summary>
        /// <param name="type">Type reference</param>
        /// <returns>true if string</returns>
        public static bool IsText(this Type type)
        {
            return Type.GetTypeCode(type) == TypeCode.String;
        }
        /// <summary>
        /// Return if <paramref name="type"/> is a boolean
        /// </summary>
        /// <param name="type">Type reference</param>
        /// <returns>true if bool</returns>
        public static bool IsBool(this Type type)
        {
            return Type.GetTypeCode(type) == TypeCode.Boolean;
        }
        /// <summary>
        /// Return if <paramref name="type"/> is a number
        /// </summary>
        /// <param name="type">Type reference</param>
        /// <returns>true if number</returns>
        public static bool IsNumber(this Type type)
        {
            return type.IsDecimal() || type.IsInteger();
        }
        /// <summary>
        /// Return if <paramref name="type"/> is a decimal type (float, double, decimal)
        /// </summary>
        /// <param name="type">Type reference</param>
        /// <returns>true if decimal type</returns>
        public static bool IsDecimal(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Return if <paramref name="type"/> is an integer type (int, uint, short, ushort, long, ulong)
        /// </summary>
        /// <param name="type">Type reference</param>
        /// <returns>true if integer type</returns>
        public static bool IsInteger(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// Return if <paramref name="type"/> is an unsigned integer
        /// </summary>
        /// <param name="type">Type reference</param>
        /// <returns>true if an unsigned integer</returns>
        public static bool IsUnsigned(this Type type)
        {
            if (!type.IsNumber())
                return false;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }
        #endregion

        #region Enum
        /// <summary>
        /// Get the Values available in an Enum
        /// </summary>
        /// <example>GetValues<MyEnum>();</example>
        /// <typeparam name="TEnum">The Type of the Enum</typeparam>
        /// <typeparam name="T">Destination type: e.g. int</typeparam>
        /// <seealso cref="GetNames{TEnum}"/>
        /// <returns>Array of type <typeparamref name="T"/></returns>
        public static T[] GetValues<TEnum, T>() where TEnum : Enum
        {
            return (T[])Enum.GetValues(typeof(TEnum));
        }
        /// <summary>
        /// Get the Names available in an Enum
        /// </summary>
        /// <example>GetNames<MyEnum>();</example>
        /// <typeparam name="TEnum">The Enum Type</typeparam>
        /// <seealso cref="GetValues{TEnum, T}"/>
        /// <returns>Array of Strings, representing the Enum Options</returns>
        public static string[] GetNames<TEnum>() where TEnum : Enum
        {
            return Enum.GetNames(typeof(TEnum));
        }
        #endregion

        /// <summary>
        /// Try to get the <see cref="UIRangeAttribute"/> of a property and extract the <paramref name="min"/> and <paramref name="max"/> values
        /// </summary>
        /// <param name="prop"><see cref="PropertyInfo"/> Reference</param>
        /// <param name="min"><see cref="UIRangeAttribute.min"/></param>
        /// <param name="max"><see cref="UIRangeAttribute.max"/></param>
        /// <returns>true if <paramref name="prop"/> has a <see cref="UIRangeAttribute"/></returns>
        public static bool TryGetRangeAttribute(this PropertyInfo prop, out float min, out float max)
        {
            min = 0;
            max = 0;
            var range = prop.GetCustomAttribute<UIRangeAttribute>();
            if (range != null)
            {
                min = range.min;
                max = range.max;
                return true;
            }

            return false;
        }
        /// <summary>
        /// Try to get the <see cref="RangeAttribute"/> of a property and extract the <paramref name="min"/> and <paramref name="max"/> values
        /// </summary>
        /// <param name="field"><see cref="FieldInfo"/> Reference</param>
        /// <param name="min"><see cref="RangeAttribute.min"/></param>
        /// <param name="max"><see cref="RangeAttribute.max"/></param>
        /// <returns>true if <paramref name="field"/> has a <see cref="RangeAttribute"/></returns>
        public static bool TryGetRangeAttribute(this FieldInfo field, out float min, out float max)
        {
            min = 0;
            max = 0;
            var range = field.GetCustomAttribute<RangeAttribute>();
            if (range != null)
            {
                min = range.min;
                max = range.max;
                return true;
            }

            var uirange = field.GetCustomAttribute<UIRangeAttribute>();
            if (uirange != null)
            {
                min = uirange.min;
                max = uirange.max;
                return true;
            }

            return false;
        }
    }
}
