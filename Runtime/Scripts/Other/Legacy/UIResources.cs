using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Toorah.MirrorUI.Resources
{
    [CreateAssetMenu]
    public class UIResources : ScriptableObject
    {
        public Image image;
        public Scrollbar scrollBar;
        public Slider slider;
        public Button button;
        public TMP_Dropdown dropdown;
        public TMP_InputField inputField;
        public TextMeshProUGUI label;
        public Toggle toggle;
    }
}