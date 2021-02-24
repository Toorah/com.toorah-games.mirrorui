using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;

namespace ToorahEditor.MirrorUI
{
    public class AboutWindow : EditorWindow
    {
        static Texture s_mirrorTex;
        static Texture s_githubTex;

        [SerializeField] VisualTreeAsset vta;
        [SerializeField] StyleSheet uss;


        /// <summary>
        /// Open this Window
        /// </summary>
        [MenuItem("Mirror UI/About", priority = 1000)]
        static void Open()
        {
            var win = CreateInstance<AboutWindow>();
            win.titleContent = new GUIContent("Mirror UI: About");
            win.ShowUtility();
        }

        /// <summary>
        /// Called when the Window is opened, or it's open but Unity reloads, eg. recompile
        /// </summary>
        private void OnEnable()
        {
            minSize = new Vector2(350, 220);
            maxSize = new Vector2(350, 220);
            if (s_mirrorTex == null)
                s_mirrorTex = Resources.Load<Texture>("logo");

            if (s_githubTex == null)
                s_githubTex = Resources.Load<Texture>("git_white");


            if (vta == null)
            {
                vta = Resources.Load<VisualTreeAsset>("about_window");
            }
            if (uss == null)
            {
                uss = Resources.Load<StyleSheet>("editorStyleSheet");
            }

            rootVisualElement.Add(vta.CloneTree());
            rootVisualElement.styleSheets.Add(uss);

            rootVisualElement.Q<Button>("btn-git").clicked += OpenGitHub;
            rootVisualElement.Q<Button>("btn-twitter").clicked += OpenTwitter;
            rootVisualElement.Q<Button>("btn-kofi").clicked += OpenKofi;
        }
        
        
        /// <summary>
        /// Open the URL to ko-fi
        /// </summary>
        private void OpenKofi()
        {
            Application.OpenURL("https://ko-fi.com/toorah");
        }
        /// <summary>
        /// Open the URL to Twitter
        /// </summary>
        private void OpenTwitter()
        {
            Application.OpenURL("https://twitter.com/AtahanTKiltan");
        }
        /// <summary>
        /// Open the URL to Github
        /// </summary>
        private void OpenGitHub()
        {
            Application.OpenURL("https://github.com/Toorah/com.toorah-games.mirrorui");
        }
    }
}
