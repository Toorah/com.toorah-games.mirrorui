

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
using UnityEngine.UI;
using static ToorahEditor.MirrorUI.UIBuilderWindow;
using TMPro;
using Toorah.MirrorUI;

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
        Vector2 m_searchScroll;
        [SerializeField] Type m_selectedType;
        GUIStyle m_btn;

        PropertyInfo[] m_properties;
        bool[] m_useProperties;
        string m_generatedClass;

        string m_className = "TestUI";
        bool m_isMono = true;
        Vector2 m_textScroll;

        private void OnEnable()
        {
            m_searchField = new SearchField() { autoSetFocusOnFindCommand = true };

            m_allTypes = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(assembly => assembly.GetTypes()).ToArray();
        }

        private void OnGUI()
        {
            SetEditorStyles();

            DrawSearchBar();
            DrawBody();
            DrawResults();
        }

        void SetEditorStyles()
        {
            if (m_btn == null)
            {
                m_btn = new GUIStyle("button");
                m_btn.alignment = TextAnchor.UpperLeft;
            }
        }
        void DrawSearchBar()
        {
            var rect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                m_searchText = m_searchField.OnGUI(rect, m_searchText);
                if (change.changed)
                {
                    OnSearched(m_searchText);
                }
            }
        }
        void DrawBody()
        {
            if (m_selectedType != null)
            {
                DrawType();
            }
            else
            {
                EditorGUILayout.HelpBox("Select a Type by searching for it", MessageType.Info);
            }
        }
        void DrawResults()
        {
            if (m_searchText != "" && m_searchText == m_lastSearch)
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
                    File.WriteAllText(path, m_generatedClass.SpacesToTabs());
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            

            StringBuilder sb = new StringBuilder();
            sb.GenerateNotice();
            sb.AddUsingStatements("System",
                                    "System.Collections.Generic",
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
                    //sb.Tab().CreateTypeTooltip(prop.PropertyType);
                    if (prop.PropertyType.IsEnum)
                    {
                        sb.Tab().DefineDropdown(prop.Name);
                    }
                    else if(prop.PropertyType.IsNumber())
                    {
                        sb.Tab().DefineSlider(prop.Name);
                    }
                    else if (prop.PropertyType.IsText())
                    {
                        sb.Tab().DefineInput(prop.Name);
                    }
                    else if (prop.PropertyType.IsBool())
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

                    if (prop.PropertyType.IsEnum)
                    {
                        sb.LinkDropdown(prop, 2);
                    }
                    else if (prop.PropertyType.IsNumber())
                    {
                        sb.LinkSlider(prop, 2, new SliderOptions(prop.PropertyType));
                    }
                    else if (prop.PropertyType.IsText())
                    {
                        sb.LinkInputField(prop, 2);
                    }
                    else if (prop.PropertyType.IsBool())
                    {
                        sb.LinkToggle(prop, 2);
                    }

                    cnt++;
                }
            }

            sb.Tab().AppendLine("}");



            sb.AppendLine("}");

            m_generatedClass = sb.ToString();

            using(var scope = new GUILayout.ScrollViewScope(m_textScroll))
            {
                m_textScroll = scope.scrollPosition;
                GUILayout.Label(m_generatedClass, EditorStyles.helpBox);
            }
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
                using(var scope = new GUILayout.ScrollViewScope(m_searchScroll, GUILayout.ExpandWidth(true)))
                {
                    m_searchScroll = scope.scrollPosition;

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
        public struct SliderOptions
        {
            public float? min;
            public float? max;

            public Type sliderType;


            public SliderOptions(Type type)
            {
                sliderType = type;
                min = null;
                max = null;
            }

            public SliderOptions(Type type, float? min, float? max)
            {
                sliderType = type;
                this.min = min;
                this.max = max;
            }
        }
    }


    public static class StringEx
    {
        static readonly string TabsAsSpaces = "    ";

        public static StringBuilder GenerateNotice(this StringBuilder sb, params string[] lines)
        {
            sb.AppendLine("/*#############################################")
                .Append("#").Tab().AppendLine($"This Script was automatically generated from {nameof(UIBuilderWindow)}")
                .Append("#").Tab().AppendLine("DO NOT MODIFY, CHANGES MAY BE LOST!");

            foreach(var line in lines)
            {
                sb.Append("#").Tab().AppendLine(line);
            }

            sb.AppendLine("#############################################*/");
            sb.EmptyLine();
            return sb;
        }

        public static StringBuilder Tab(this StringBuilder sb)
        {
            return sb.Append(TabsAsSpaces);
        }

        public static StringBuilder Tab(this StringBuilder sb, int tabs)
        {
            for (int i = 0; i < tabs; i++)
                sb.Tab();

            return sb;
        }

        public static string SpacesToTabs(this string text)
        {
            return text.Replace(TabsAsSpaces, "\t");
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
                sb.AppendLine($"public class {className} : {nameof(MonoBehaviour)}, IUiLinkable<{typeName}> {{");
            }
            else
            {
                sb.AppendLine($"public class {className} : IUiLinkable<{typeName}> {{");
            }

            return sb;
        }

        
        public static StringBuilder CreateTypeTooltip(this StringBuilder sb, Type type)
        {
            return sb.AppendLine($"[Tooltip(\"{type.GetScriptType().FormatType()}\")]");
        }
        public static StringBuilder DefineDropdown(this StringBuilder sb, string name)
        {
            return sb.AppendLine($"public {nameof(TMP_Dropdown)} dropdown_{name};");
        }
        public static StringBuilder DefineSlider(this StringBuilder sb, string name)
        {
            return sb.AppendLine($"public {nameof(Slider)} slider_{name};");
        }
        public static StringBuilder DefineToggle(this StringBuilder sb, string name)
        {
            return sb.AppendLine($"public {nameof(Toggle)} toggle_{name};");
        }
        public static StringBuilder DefineInput(this StringBuilder sb, string name)
        {
            return sb.AppendLine($"public {nameof(TMP_InputField)} input_{name};");
        }
        public static StringBuilder DefineMethod(this StringBuilder sb, string methodName, Type type)
        {
            sb.AppendLine($"public void {methodName}({type.GetScriptType()} instance) {{");
            return sb;
        }

        public static StringBuilder LinkDropdown(this StringBuilder sb, PropertyInfo prop, int tab = 0)
        {
            var dropdownName = $"dropdown_{prop.Name}";
            var instanceName = $"instance.{prop.Name}";
            sb.Tab(tab).AppendLine($"{nameof(UILinker)}.{nameof(LinkDropdown)}({dropdownName}, {instanceName}, r => {instanceName} = r);");

            return sb;
        }

        public static StringBuilder LinkSlider(this StringBuilder sb, PropertyInfo prop, int tab = 0, SliderOptions? options = null)
        {
            var sliderName = $"slider_{prop.Name}";
            var instanceName = $"instance.{prop.Name}";
            var cast = "";

            sb.Tab(tab).AppendLine($"{sliderName}.{nameof(Slider.value)} = (float){instanceName};");
            if(options.HasValue)
            {
                if(options.Value.min.HasValue)
                    sb.Tab(tab).AppendLine($"{sliderName}.{nameof(Slider.minValue)} = {options.Value.min.Value};");
                if(options.Value.max.HasValue)
                    sb.Tab(tab).AppendLine($"{sliderName}.{nameof(Slider.maxValue)} = {options.Value.max.Value};");


                cast = $"({options.Value.sliderType.GetScriptType().FormatType()})";
                if (options.Value.sliderType.IsInteger())
                {
                    sb.Tab(tab).AppendLine($"{sliderName}.{nameof(Slider.wholeNumbers)} = true;");
                }
            }
            sb.Tab(tab).AppendLine($"{sliderName}.{nameof(Slider.onValueChanged)}.{nameof(Slider.SliderEvent.AddListener)}(v => {instanceName} = {cast}v);");

            return sb;
        }


        public static StringBuilder LinkInputField(this StringBuilder sb, PropertyInfo prop, int tab = 0)
        {
            var inputName = $"input_{prop.Name}";
            var instanceName = $"instance.{prop.Name}";
            sb.Tab(tab).AppendLine($"{inputName}.{nameof(UILinker.LinkInput)}({instanceName}, s => {instanceName} = s);");

            return sb;
        }
        public static StringBuilder LinkToggle(this StringBuilder sb, PropertyInfo prop, int tab = 0)
        {
            var toggleName = $"toggle_{prop.Name}";
            var instanceName = $"instance.{prop.Name}";
            sb.Tab(tab).AppendLine($"{toggleName}.{nameof(UILinker.LinkToggle)}({instanceName}, b => {instanceName} = b);");

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
