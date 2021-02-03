using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace ToorahEditor.MirrorUI
{
    public class UIBuilderWindow : EditorWindow
    {
        [MenuItem("UI/Builder")]
        static void Open()
        {
            var window = GetWindow<UIBuilderWindow>();
            window.titleContent.text = "UI Builder";
            window.Show();
        }


        Type[] m_allTypes;
        HashSet<Type> m_searchTypes = new HashSet<Type>();
        string m_searchText = "";
        string m_lastSearch = "";
        SearchField m_searchField;
        Vector2 m_scroll;
        [SerializeField] Type m_selectedType;
        GUIStyle m_btn;

        PropertyInfo[] m_properties;
        bool[] m_useProperties;
        string m_generatedClass;

        string m_className = "TestUI";
        bool m_isMono = true;

        private void OnEnable()
        {
            m_searchField = new SearchField() { autoSetFocusOnFindCommand = true };

            m_allTypes = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(assembly => assembly.GetTypes()).ToArray();
        }

        private void OnGUI()
        {
            if(m_btn == null)
            {
                m_btn = new GUIStyle("button");
                m_btn.alignment = TextAnchor.UpperLeft;
            }


            var rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            using(var change = new EditorGUI.ChangeCheckScope())
            {
                m_searchText = m_searchField.OnGUI(rect, m_searchText);
                if (change.changed)
                {
                    OnSearched(m_searchText);
                }
            }

            if(m_selectedType != null)
            {
                DrawType();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a Type by searching for it", MessageType.Info);
            }

            if(m_searchText != "" && m_searchText == m_lastSearch)
            {
                Results();
            }
        }

        void GetTypeData()
        {
            m_properties = m_selectedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            m_useProperties = new bool[m_properties.Length];
        }

        void DrawType()
        {
            m_className = EditorGUILayout.TextField("Class Name:", m_className);
            m_isMono = EditorGUILayout.Toggle("Is MonoBehaviour:", m_isMono);
            GUILayout.Space(10);


            GUILayout.Label(m_selectedType.Name);
            GUILayout.Label($"Namespace: {m_selectedType.Namespace}", EditorStyles.miniLabel);
            GUILayout.Space(10);
            GUILayout.Label($"Properties: ", EditorStyles.centeredGreyMiniLabel);
            

            for(int i = 0; i < m_properties.Length; i++)
            {
                var prop = m_properties[i];
                var use = m_useProperties[i];

                if(prop.CanRead && prop.CanWrite)
                {
                    use = EditorGUILayout.ToggleLeft($"{prop.Name} <{prop.PropertyType.FormatType()}>", use);
                }

                m_useProperties[i] = use;
            }

            if (GUILayout.Button("Generate"))
            {
                var path = EditorUtility.SaveFilePanel("Save Script", "", m_className, "cs");
                if(path != "")
                {
                    File.WriteAllText(path, m_generatedClass);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            

            StringBuilder sb = new StringBuilder();
            sb.AddUsingStatements("System",
                                    "UnityEngine",
                                    "UnityEngine.UI",
                                    "TMPro",
                                    "Toorah.MirrorUI");
            if (!string.IsNullOrEmpty(m_selectedType.Namespace))
                sb.AddUsingStatements(m_selectedType.Namespace);

            sb.EmptyLine();
            sb.DefineClass(m_className, m_isMono, m_selectedType.GetScriptType().FormatType());

            for(int i = 0; i < m_properties.Length; i++)
            {
                var prop = m_properties[i];
                var use = m_useProperties[i];

                if (use)
                {
                    if(prop.PropertyType == typeof(float))
                    {
                        sb.Tab().DefineSlider(prop.Name);
                    }
                    else if (prop.PropertyType == typeof(double))
                    {
                        sb.Tab().DefineSlider(prop.Name);
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        sb.Tab().DefineSlider(prop.Name);
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        sb.Tab().DefineInput(prop.Name);
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        sb.Tab().DefineToggle(prop.Name);
                    }
                }
            }

            sb.EmptyLine();
            sb.Tab().DefineMethod($"Link", m_selectedType);
            int cnt = 0;
            for (int i = 0; i < m_properties.Length; i++)
            {
                var prop = m_properties[i];
                var use = m_useProperties[i];

                if (use)
                {
                    if(cnt != 0)
                        sb.AppendLine("");
                    if (prop.PropertyType == typeof(float))
                    {
                        sb.Tab().Tab().AppendLine($"slider_{prop.Name}.value = instance.{prop.Name};");
                        sb.Tab().Tab().AppendLine($"slider_{prop.Name}.onValueChanged.AddListener(v => instance.{prop.Name} = v);");
                    }
                    else if (prop.PropertyType == typeof(int))
                    {
                        sb.Tab().Tab().AppendLine($"slider_{prop.Name}.value = instance.{prop.Name};");
                        sb.Tab().Tab().AppendLine($"slider_{prop.Name}.wholeNumbers = true;");
                        sb.Tab().Tab().AppendLine($"slider_{prop.Name}.onValueChanged.AddListener(v => instance.{prop.Name} = (int)v);");
                    }
                    else if (prop.PropertyType == typeof(double))
                    {
                        sb.Tab().Tab().AppendLine($"slider_{prop.Name}.value = (float)instance.{prop.Name};");
                        sb.Tab().Tab().AppendLine($"slider_{prop.Name}.onValueChanged.AddListener(v => instance.{prop.Name} = (double)v);");
                    }
                    else if (prop.PropertyType == typeof(string))
                    {
                        sb.Tab().Tab().AppendLine($"input_{prop.Name}.text = instance.{prop.Name};");
                        sb.Tab().Tab().AppendLine($"input_{prop.Name}.onValueChanged.AddListener(v => instance.{prop.Name} = v);");
                    }
                    else if(prop.PropertyType == typeof(bool))
                    {
                        sb.Tab().Tab().AppendLine($"toggle_{prop.Name}.isOn = instance.{prop.Name};");
                        sb.Tab().Tab().AppendLine($"toggle_{prop.Name}.onValueChanged.AddListener(v => instance.{prop.Name} = v);");
                    }
                    cnt++;
                }
            }

            sb.Tab().AppendLine("}");



            sb.AppendLine("}");

            m_generatedClass = sb.ToString();

            GUILayout.Label(m_generatedClass, EditorStyles.helpBox);
        }

        async void OnSearched(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                m_lastSearch = searchText;
                return;
            }

            await Task.Run(() => 
            {
                var types = m_allTypes.Where(x => x.Name.ToLowerInvariant().Contains(m_searchText.ToLowerInvariant())).Take(25).ToArray();
                m_searchTypes.Clear();
                foreach (var t in types)
                    m_searchTypes.Add(t);
            });

            m_lastSearch = searchText;
            Repaint();
        }


        void Results()
        {
            using (new GUILayout.AreaScope(new Rect(0, EditorGUIUtility.singleLineHeight, position.width, 200), GUIContent.none, "window"))
            {
                using(var scope = new GUILayout.ScrollViewScope(m_scroll, GUILayout.ExpandWidth(true)))
                {
                    m_scroll = scope.scrollPosition;

                    foreach(var t in m_searchTypes)
                    {
                        if (GUILayout.Button(t.GetScriptType(), m_btn))
                        {
                            EditorGUI.FocusTextInControl(null);
                            m_selectedType = t;
                            m_searchText = "";

                            GetTypeData();
                        }
                    }
                }
            }
        }
    }


    public static class StringEx
    {
        public static StringBuilder Tab(this StringBuilder sb)
        {
            return sb.Append("   ");
        }
        public static StringBuilder TabLine(this StringBuilder sb)
        {
            return sb.AppendLine("   ");
        }

        public static StringBuilder EmptyLine(this StringBuilder sb)
        {
            return sb.AppendLine("");
        }

        public static StringBuilder AddUsingStatements(this StringBuilder sb, params string[] statements)
        {
            foreach (var s in statements)
            {
                sb.AppendLine($"using {s};");
            }
            return sb;
        }
        public static StringBuilder DefineClass(this StringBuilder sb, string className, bool isMono, string typeName)
        {
            if (isMono)
            {
                sb.AppendLine($"public class {className} : MonoBehaviour, IUiLinkable<{typeName}> {{");
            }
            else
            {
                sb.AppendLine($"public class {className} : IUiLinkable<{typeName}> {{");
            }

            return sb;
        }

        public static StringBuilder DefineSlider(this StringBuilder sb, string name)
        {
            return sb.AppendLine($"public Slider slider_{name};");
        }
        public static StringBuilder DefineToggle(this StringBuilder sb, string name)
        {
            return sb.AppendLine($"public Toggle toggle_{name};");
        }
        public static StringBuilder DefineInput(this StringBuilder sb, string name)
        {
            return sb.AppendLine($"public TMP_InputField input_{name};");
        }

        public static StringBuilder DefineMethod(this StringBuilder sb, string methodName, Type type)
        {
            sb.AppendLine($"public void {methodName}({type.GetScriptType()} instance) {{");
            return sb;
        }



        public static string FormatType(this Type type)
        {
            return type.Name.FormatType();
        }
        public static string FormatType(this string typeString)
        {
            var p = typeString;

            switch (typeString)
            {
                case "Single":
                    p = "float";
                    break;
                case "Boolean":
                    p = "bool";
                    break;
                case "String":
                    p = "string";
                    break;
                case "Double":
                    p = "double";
                    break;
                case "Int32":
                    p = "int";
                    break;
                case "UInt32":
                    p = "uint";
                    break;
                case "Int64":
                    p = "long";
                    break;
                case "UInt64":
                    p = "ulong";
                    break;
                case "Int16":
                    p = "short";
                    break;
                case "UInt16":
                    p = "ushort";
                    break;
            }

            return p;
        }

        public static string GetScriptType(this Type type)
        {
            return type.FullName.Replace($"{type.Namespace}.", "").Replace("+", ".").Replace(" ", "");
        }
    }
}
