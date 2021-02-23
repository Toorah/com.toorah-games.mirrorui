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
using System.Globalization;
using ToorahEditor.ScriptCreator.Utils;
using ToorahEditor.ScriptCreator;

namespace ToorahEditor.MirrorUI
{
    public class UIBuilderWindow : EditorWindow
    {
        [MenuItem("Mirror UI/Builder", priority = 0)]
        static void Open()
        {
            var window = GetWindow<UIBuilderWindow>();
            window.titleContent = new GUIContent("UI Builder", Resources.Load<Texture>("mirror"), "Mirror UI Builder Window");
            window.Show();
        }
        

        Type[] m_allTypes;
        HashSet<Type> m_searchTypes = new HashSet<Type>();
        string m_searchText = "";
        string m_lastSearch = "";
        SearchField m_searchField;
        Vector2 m_searchScroll;
        [SerializeField] Type m_selectedType;
        [SerializeField] string m_selectedTypeString = string.Empty;
        GUIStyle m_btn;
        GUIStyle m_label;
        GUIStyle m_labelRight;

        MirrorData[] m_properties;
        bool[] m_useProperty;
        MirrorData[] m_fields;
        bool[] m_useField;
        string m_generatedClass;

        string m_className = "TestUI";
        bool m_isMono = true;
        Vector2 m_textScroll;

        private void OnEnable()
        {
            m_searchField = new SearchField() { autoSetFocusOnFindCommand = true };

            m_allTypes = AppDomain.CurrentDomain.GetAssemblies()
                       .SelectMany(assembly => assembly.GetTypes()).ToArray();

            if (!string.IsNullOrEmpty(m_selectedTypeString) && m_allTypes.Length > 0)
            {
                m_selectedType = m_allTypes.First(x => x.FullName == m_selectedTypeString);
                if(m_selectedType != null)
                {
                    GetTypeData();
                }
            }
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
            if (m_label == null)
            {
                m_label = new GUIStyle(EditorStyles.helpBox);
                m_label.richText = true;
            }
            if (m_labelRight == null)
            {
                m_labelRight = new GUIStyle(m_label);
                m_labelRight.alignment = TextAnchor.UpperRight;
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
            if (m_searchText != "")
            {
                using (new GUILayout.AreaScope(new Rect(0, EditorGUIUtility.singleLineHeight, position.width, 200), GUIContent.none, "window"))
                {
                    using (var scope = new GUILayout.ScrollViewScope(m_searchScroll, GUILayout.ExpandWidth(true)))
                    {
                        m_searchScroll = scope.scrollPosition;
                        if(m_searchText == m_lastSearch)
                        {
                            Results();
                        }
                    }
                }
            }
        }


        void GetTypeData()
        {
            var data = new List<MirrorData>();

            var properties = m_selectedType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach(var p in properties)
            {
                data.Add(new MirrorData(p));
            }
            m_properties = data.ToArray();
            m_useProperty = new bool[data.Count];
            
            data.Clear();

            var fields = m_selectedType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            foreach (var f in fields)
            {
                data.Add(new MirrorData(f));
            }
            m_fields = data.ToArray();
            m_useField = new bool[data.Count];
        }

        

        void DrawType()
        {
            m_className = EditorGUILayout.TextField("Class Name:", m_className);
            m_isMono = EditorGUILayout.Toggle("Is MonoBehaviour:", m_isMono);
            GUILayout.Space(10);


            GUILayout.Label(m_selectedType.Name);
            GUILayout.Label($"Namespace: {m_selectedType.Namespace}", EditorStyles.miniLabel);
            GUILayout.Space(10);

            using(new GUILayout.HorizontalScope(GUILayout.ExpandHeight(false)))
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.ExpandHeight(false)))
                {
                    GUILayout.Label($"Fields: ", EditorStyles.centeredGreyMiniLabel);
                    for (int i = 0; i < m_fields.Length; i++)
                    {
                        var prop = m_fields[i];
                        var use = m_useField[i];

                        if (prop.CanRead && prop.CanWrite)
                        {
                            use = EditorGUILayout.ToggleLeft($"{prop.Name} <{prop.Type.FormatScriptType()}>", use);
                        }
                        m_useField[i] = use;
                    }
                }

                

                using (new GUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.ExpandHeight(false)))
                {
                    GUILayout.Label($"Properties: ", EditorStyles.centeredGreyMiniLabel);
                    for(int i = 0; i < m_properties.Length; i++)
                    {
                        var prop = m_properties[i];
                        var use = m_useProperty[i];

                        if(prop.CanRead && prop.CanWrite)
                        {
                            use = EditorGUILayout.ToggleLeft($"{prop.Name} <{prop.Type.FormatScriptType()}>", use);
                        }
                        m_useProperty[i] = use;
                    }
                }
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

            var script = FluentScript.Create(m_className);
            var type =  script.AddScriptHeader(
                    $"This Script was automatically generated with {nameof(UIBuilderWindow)}",
                    "ONLY MODIFY IF YOU KNOW WHAT YOU ARE DOING, CHANGES MAY BE LOST!");
            if (m_isMono)
                type.SetParentClass<MonoBehaviour>();

            var interfaceType = typeof(IUiLinkable<>);
            interfaceType = interfaceType.MakeGenericType(m_selectedType);

            type.AddInterface(interfaceType);
            var body = (IScriptBody)type;
            if (m_useField.Count(x => x == true) > 0)
            {
                body.AddRegion("Fields");
                DefineVariables(body, m_fields, m_useField);
            }
            if (m_useProperty.Count(x => x == true) > 0)
            {
                body.AddRegion("Properties");
                DefineVariables(body, m_properties, m_useProperty);
            }
            //var generated = body.Write();
            var preview = body.Preview();
            RenderPreview();

            void RenderPreview()
            {
                string[] lines = preview.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    var ind = i + 1;
                    lines[i] = $"{ind}.";
                }
                string lineText = string.Join("\n", lines).Color(Colors.HEX_Region);

                using (var scope = new GUILayout.ScrollViewScope(m_textScroll))
                {
                    m_textScroll = scope.scrollPosition;
                    using (new GUILayout.HorizontalScope())
                    {

                        GUILayout.Label(lineText, m_labelRight, GUILayout.ExpandWidth(false));

                        GUILayout.Label(preview, m_label, GUILayout.ExpandWidth(true));
                    }
                }
            }

            //StringBuilder sb = new StringBuilder();

            //WriteScript();

            //RenderPreview();


            //void WriteScript()
            //{
            //    sb.AppendGenerationNotice(
            //        $"This Script was automatically generated with {nameof(UIBuilderWindow)}",
            //        "ONLY MODIFY IF YOU KNOW WHAT YOU ARE DOING, CHANGES MAY BE LOST!");
            //    WriteUsingStatements();

            //    sb.EmptyLine();
            //    sb.DefineClass(m_className, m_isMono, m_selectedType.GetFullScriptTypeName().FormatScriptType());

            //    WriteFieldVariables();
            //    WritePropertyVariables();

            //    sb.EmptyLine();

            //    WriteMethodHeader();

            //    WriteFieldLogic();
            //    WritePropertyLogic();

            //    sb.Tab().AppendLine("}");



            //    sb.AppendLine("}");
            //}

            //void WriteUsingStatements()
            //{
            //    sb.AddUsingStatements("System",
            //                        "System.Collections.Generic",
            //                        "UnityEngine",
            //                        "UnityEngine.UI",
            //                        "TMPro",
            //                        "Toorah.MirrorUI");
            //    if (!string.IsNullOrEmpty(m_selectedType.Namespace))
            //        sb.AddUsingStatements(m_selectedType.Namespace);
            //}

            //void WriteMethodHeader()
            //{
            //    var colSummary = $"<color=#{Colors.HEX_Summary}>";
            //    var colVar = $"<color=#{Colors.HEX_Variable}>";
            //    var end = "</color>";
            //    sb.Tab().AppendLine($"{colSummary}/// <summary>");
            //    sb.Tab().AppendLine($"/// Link the UI to <paramref {end}name{colSummary}={end}\"{colVar}instance{end}\"{colSummary}/>");
            //    sb.Tab().AppendLine("/// </summary>");
            //    sb.Tab().AppendLine($"/// <param {end}name{colSummary}={end}\"{colVar}instance{end}\"{colSummary}>Instance to link the UI to</param>{end}");
            //    sb.Tab().DefineMethod("Link", m_selectedType);
            //}

            //void WriteFieldVariables()
            //{
            //    if (m_useField.Count(x => x == true) > 0)
            //    {
            //        sb.Tab().AppendLine("#region Field UI");
            //        DefineVariables(sb, m_fields, m_useField, 1);
            //        sb.Tab().AppendLine("#endregion");
            //    }
            //}
            //void WritePropertyVariables()
            //{
            //    if (m_useProperty.Count(x => x == true) > 0)
            //    {
            //        if (m_useField.Count(x => x == true) > 0)
            //            sb.EmptyLine();

            //        sb.Tab().AppendLine("#region Property UI");
            //        DefineVariables(sb, m_properties, m_useProperty, 1);
            //        sb.Tab().AppendLine("#endregion");
            //    }
            //}

            //void WriteFieldLogic()
            //{
            //    if (m_useField.Count(x => x == true) > 0)
            //    {
            //        sb.Tab(2).AppendLine("#region Field Logic");
            //        DefineUsage(sb, m_fields, m_useField, 2);
            //        sb.Tab(2).AppendLine("#endregion");
            //    }
            //}
            //void WritePropertyLogic()
            //{
            //    if (m_useProperty.Count(x => x == true) > 0)
            //    {
            //        if (m_useField.Count(x => x == true) > 0)
            //            sb.EmptyLine();

            //        sb.Tab(2).AppendLine("#region Property Logic");
            //        DefineUsage(sb, m_properties, m_useProperty, 2);
            //        sb.Tab(2).AppendLine("#endregion");
            //    }
            //}

            //void RenderPreview()
            //{
            //    m_generatedClass = sb.ToString();
            //    var preview = ColorPreview(m_generatedClass);

            //    string[] lines = preview.Split('\n');
            //    for (int i = 0; i < lines.Length; i++)
            //    {
            //        var ind = i + 1;
            //        lines[i] = $"{ind}.";
            //    }
            //    string lineText = string.Join("\n", lines).Color(Colors.HEX_Region);

            //    using (var scope = new GUILayout.ScrollViewScope(m_textScroll))
            //    {
            //        m_textScroll = scope.scrollPosition;
            //        using (new GUILayout.HorizontalScope())
            //        {

            //                GUILayout.Label(lineText, m_labelRight, GUILayout.ExpandWidth(false));

            //            GUILayout.Label(preview, m_label, GUILayout.ExpandWidth(true));
            //        }
            //    }
            //}

            //string ColorPreview(string text)
            //{
            //    string[] blue = new string[] 
            //    {
            //        "using", "public", "private", "class", "void", "int", "string", "float", "bool", "uint", "short", "ushort", "long", "ulong", "decimal", "double", "null"
            //    };

            //    text = text.Replace("/*", $"<color=#{Colors.HEX_Comment}>/*").Replace("*/", "*/</color>");
            //    text = text.Colorize("IUiLinkable", Colors.HEX_Interface);
            //    text = text.Colorize("MonoBehaviour", Colors.HEX_Class);
            //    text = text.Colorize(m_selectedType.Name, Colors.HEX_Class);
            //    text = text.ColorizeRegion(Colors.HEX_Region);
            //    text = text.Colorize("=>", Colors.HEX_Lambda);
            //    foreach(var b in blue)
            //    {
            //        text = text.Colorize(b, Colors.HEX_Keyword);
            //    }

            //    string[] lines = text.Split('\n');
            //    for(int i = 0; i < lines.Length; i++)
            //    {
            //        lines[i] = lines[i].ColorizeComment(Colors.HEX_Comment);
            //    }
            //    text = string.Join("\n", lines);

            //    return text;
            //}
        }

        void DefineVariables(StringBuilder sb, MirrorData[] data, bool[] uses, int tab = 0)
        {
            for (int i = 0; i < data.Length; i++)
            {
                var prop = data[i];
                var use = uses[i];

                if (use)
                {
                    //sb.Tab().CreateTypeTooltip(prop.PropertyType);
                    if (prop.Type.IsEnum)
                    {
                        sb.Tab(tab).DefineDropdown(prop.Name);
                    }
                    else if (prop.Type.IsNumber())
                    {
                        sb.Tab(tab).DefineSlider(prop.Name);
                    }
                    else if (prop.Type.IsText())
                    {
                        sb.Tab(tab).DefineInput(prop.Name);
                    }
                    else if (prop.Type.IsBool())
                    {
                        sb.Tab(tab).DefineToggle(prop.Name);
                    }
                }
            }
        }

        void DefineVariables(IScriptBody script, MirrorData[] data, bool[] uses)
        {
            for (int i = 0; i < data.Length; i++)
            {
                var prop = data[i];
                var use = uses[i];

                if (use)
                {
                    //sb.Tab().CreateTypeTooltip(prop.PropertyType);
                    if (prop.Type.IsEnum)
                    {
                        script.AddField<TMP_Dropdown>(prop.Name, Accessor.Public);
                    }
                    else if (prop.Type.IsNumber())
                    {
                        script.AddField<Slider>(prop.Name, Accessor.Public);
                    }
                    else if (prop.Type.IsText())
                    {
                        script.AddField<TMP_InputField>(prop.Name, Accessor.Public);
                    }
                    else if (prop.Type.IsBool())
                    {
                        script.AddField<Toggle>(prop.Name, Accessor.Public);
                    }
                }
            }
        }
        void DefineUsage(StringBuilder sb, MirrorData[] data, bool[] uses, int tab = 0)
        {
            int cnt = 0;
            for (int i = 0; i < data.Length; i++)
            {
                var prop = data[i];
                var use = uses[i];

                if (use)
                {
                    if (cnt != 0)
                        sb.AppendLine("");

                    if (prop.Type.IsEnum)
                    {
                        sb.LinkDropdown(prop, tab);
                    }
                    else if (prop.Type.IsNumber())
                    {
                        float min = 0;
                        float max = 1;

                        if (prop.isField ? prop.field.TryGetRangeAttribute(out min, out max) : prop.property.TryGetRangeAttribute(out min, out max))
                        {
                            sb.LinkSlider(prop, min, max, tab);
                        }
                        else
                        {
                            sb.LinkSlider(prop, null, null, tab);
                        }
                    }
                    else if (prop.Type.IsText())
                    {
                        sb.LinkInputField(prop, tab);
                    }
                    else if (prop.Type.IsBool())
                    {
                        sb.LinkToggle(prop, tab);
                    }

                    cnt++;
                }
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
                var types = m_allTypes.Where(x => FormatText(x.Name).Contains(FormatText(m_searchText))).Take(25).ToArray();
                m_searchTypes.Clear();
                foreach (var t in types)
                    m_searchTypes.Add(t);
            });

            m_lastSearch = searchText;
            Repaint();


            string FormatText(string text)
            {
                return text.Replace(" ", "").ToLowerInvariant();
            }
        }
        void Results()
        {
            foreach (var t in m_searchTypes)
            {
                if (GUILayout.Button(t.GetFullScriptTypeName(), m_btn))
                {
                    EditorGUI.FocusTextInControl(null);
                    m_selectedType = t;
                    m_selectedTypeString = m_selectedType.FullName;
                    m_searchText = "";

                    GetTypeData();
                }
            }
        }


        public class MirrorData
        {
            public string Name { get; set; }
            public string Namespace { get; set; }
            public Type Type { get; set; }

            public PropertyInfo property;
            public FieldInfo field;
            public bool isField;

            public readonly bool CanRead;
            public readonly bool CanWrite;

            public MirrorData(PropertyInfo pi)
            {
                Name = pi.Name;
                Namespace = pi.PropertyType.Namespace;
                Type = pi.PropertyType;

                property = pi;
                isField = false;

                CanRead = pi.CanRead;
                CanWrite = pi.CanWrite;
            }
            public MirrorData(FieldInfo fi)
            {
                Name = fi.Name;
                Namespace = fi.FieldType.Namespace;
                Type = fi.FieldType;

                field = fi;
                isField = true;

                CanRead = fi.IsPublic;
                CanWrite = fi.IsPublic && !fi.IsInitOnly;
            }
        }
    }


    public static class StringEx
    {

        public static string Color(this string s, Color color)
        {
            return s.Color(ColorUtility.ToHtmlStringRGB(color));
        }
        public static string Color(this string s, string hex)
        {
            return $"<color=#{hex}>{s}</color>";
        }
        public static string Colorize(this string s, string target, Color color)
        {
            return s.Replace(target, target.Color(color));
        }
        public static string Colorize(this string s, string target, string hex)
        {
            return s.Replace(target, target.Color(hex));
        }
        public static string ColorizeRegion(this string s, Color color)
        {
            return s.ColorizeRegion(ColorUtility.ToHtmlStringRGB(color));
        }
        public static string ColorizeRegion(this string s, string hex)
        {
            return s.Colorize("#region", hex).Colorize("#endregion", hex);
        }
        public static string ColorizeComment(this string s, Color color)
        {
            return s.ColorizeComment(ColorUtility.ToHtmlStringRGB(color));
        }
        public static string ColorizeComment(this string s, string hex)
        {
            if (s.Trim().StartsWith("//") && !s.Trim().StartsWith("///"))
                return s.Color(hex);
            else
                return s;
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
                sb.AppendLine($"public class {className.Color(Colors.HEX_Class)} : {nameof(MonoBehaviour)}, IUiLinkable<{typeName}> {{");
            }
            else
            {
                sb.AppendLine($"public class {className.Color(Colors.HEX_Class)} : IUiLinkable<{typeName}> {{");
            }

            return sb;
        }

        
        public static StringBuilder CreateTypeTooltip(this StringBuilder sb, Type type)
        {
            return sb.AppendLine($"[{("Tooltip".Color(Colors.HEX_Class))}({"\"{type.GetScriptType().FormatType()}\"".Color(Colors.HEX_String)})]");
        }
        public static StringBuilder DefineDropdown(this StringBuilder sb, string name)
        {
            return sb.AppendLine($"public {nameof(TMP_Dropdown).Color(Colors.HEX_Class)} dropdown_{name};");
        }
        public static StringBuilder DefineSlider(this StringBuilder sb, string name)
        {
            return sb.AppendLine($"public {nameof(Slider).Color(Colors.HEX_Class)} slider_{name};");
        }
        public static StringBuilder DefineToggle(this StringBuilder sb, string name)
        {
            return sb.AppendLine($"public {nameof(Toggle).Color(Colors.HEX_Class)} toggle_{name};");
        }
        public static StringBuilder DefineInput(this StringBuilder sb, string name)
        {
            return sb.AppendLine($"public {nameof(TMP_InputField).Color(Colors.HEX_Class)} input_{name};");
        }
        
        public static StringBuilder DefineMethod(this StringBuilder sb, string methodName, Type type)
        {
            sb.AppendLine($"public void {methodName.Color(Colors.HEX_Method)}({type.GetFullScriptTypeName().Color(Colors.HEX_Class)} {("instance".Color(Colors.HEX_Variable))}) {{");
            return sb;
        }

        public static StringBuilder LinkDropdown(this StringBuilder sb, MirrorData prop, int tab = 0)
        {
            var dropdownName = $"dropdown_{prop.Name}";
            var instanceName = "instance".Color(Colors.HEX_Variable) + $".{prop.Name}";

            sb.Tab(tab).AppendLine($"{dropdownName}.{nameof(UILinker.LinkDropdown).Color(Colors.HEX_Method)}({instanceName}, {("v").Color(Colors.HEX_Variable)} => {instanceName} = {("v").Color(Colors.HEX_Variable)});");

            return sb;
        }
        public static StringBuilder LinkSlider(this StringBuilder sb, MirrorData prop, float? min = null, float? max = null, int tab = 0)
        {
            var sliderName = $"slider_{prop.Name}";
            var instanceName = "instance".Color(Colors.HEX_Variable) + $".{prop.Name}";

            string minText = min.HasValue ? min.Value.ToString(CultureInfo.InvariantCulture).Color(Colors.HEX_StructEtc) : "null";
            string maxText = max.HasValue ? max.Value.ToString(CultureInfo.InvariantCulture).Color(Colors.HEX_StructEtc) : "null";

            sb.Tab(tab).AppendLine($"{sliderName}.{nameof(UILinker.LinkSlider).Color(Colors.HEX_Method)}({instanceName}, {minText}, {maxText}, {("v").Color(Colors.HEX_Variable)} => {instanceName} = {("v").Color(Colors.HEX_Variable)});");
            return sb;
        }
        public static StringBuilder LinkInputField(this StringBuilder sb, MirrorData prop, int tab = 0)
        {
            var inputName = $"input_{prop.Name}";
            var instanceName = "instance".Color(Colors.HEX_Variable) + $".{prop.Name}";

            sb.Tab(tab).AppendLine($"{inputName}.{nameof(UILinker.LinkInput).Color(Colors.HEX_Method)}({instanceName}, {("s").Color(Colors.HEX_Variable)} => {instanceName} = {("s").Color(Colors.HEX_Variable)});");

            return sb;
        }
        public static StringBuilder LinkToggle(this StringBuilder sb, MirrorData prop, int tab = 0)
        {
            var toggleName = $"toggle_{prop.Name}";
            var instanceName = "instance".Color(Colors.HEX_Variable) + $".{prop.Name}";
            sb.Tab(tab).AppendLine($"{toggleName}.{nameof(UILinker.LinkToggle).Color(Colors.HEX_Method)}({instanceName}, {("b").Color(Colors.HEX_Variable)} => {instanceName} = {("b").Color(Colors.HEX_Variable)});");

            return sb;
        }
    }
}