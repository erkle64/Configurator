using C3.ModKit;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unfoundry;
using UnityEngine;
using UnityEngine.UI;

namespace Configurator
{
    [UnfoundryMod(GUID)]
    public class Plugin : UnfoundryPlugin
    {
        public const string
            MODNAME = "Configurator",
            AUTHOR = "erkle64",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "0.1.3";

        public static LogSource log;

        private static GameObject _configuratorFrame;
        private static GameObject _configFrame;

        private static Dictionary<string, UnityEngine.Object> bundleMainAssets;

        public Plugin()
        {
            log = new LogSource(MODNAME);
        }

        public override void Load(Mod mod)
        {
            log.Log($"Loading {MODNAME}");

            bundleMainAssets = typeof(Mod).GetField("assets", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(mod) as Dictionary<string, UnityEngine.Object>;
        }
        public static T GetAsset<T>(string name) where T : UnityEngine.Object
        {
            if (!bundleMainAssets.TryGetValue(name, out var asset))
            {
                Debug.Log($"Missing asset '{name}'");
                return null;
            }

            return (T)asset;
        }

        public static void OpenConfigurator(GameObject canvas)
        {
            if (_configuratorFrame != null) CloseConfigurator();

            UIBuilder.BeginWith(canvas)
                .Element_Panel("ConfiguratorFrame", "corner_cut_outline", new Color(0.133f, 0.133f, 0.133f, 1.0f), new Vector4(13, 10, 8, 13))
                    .Keep(out _configuratorFrame)
                    .SetRectTransform(20.0f, 20.0f, -20.0f, -20.0f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                    .SetVerticalLayout(new RectOffset(0, 0, 0, 0), 0.0f, TextAnchor.UpperLeft, false, true, true, true, false, false, false)
                    .Element_Header("HeaderBar", "corner_cut_outline", new Color(0.0f, 0.6f, 1.0f, 1.0f), new Vector4(13, 3, 8, 13))
                        .SetRectTransform(0.0f, -60.0f, 599.0f, 0.0f, 0.5f, 1.0f, 0.0f, 1.0f, 0.0f, 1.0f)
                        .Layout()
                            .MinWidth(200)
                            .MinHeight(60)
                        .Done
                        .Element("Heading")
                            .SetRectTransform(0.0f, 0.0f, -60.0f, 0.0f, 0.0f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                            .Component_Text("Configurator", "OpenSansSemibold SDF", 34.0f, Color.white)
                        .Done
                        .Element_Button("Button Close", "corner_cut_fully_inset", Color.white, new Vector4(13.0f, 1.0f, 4.0f, 13.0f))
                            .SetOnClick(CloseConfigurator)
                            .SetRectTransform(-60.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.5f, 1.0f, 0.0f, 1.0f, 1.0f)
                            .SetTransitionColors(new Color(1.0f, 1.0f, 1.0f, 1.0f), new Color(1.0f, 0.25f, 0.0f, 1.0f), new Color(1.0f, 0.0f, 0.0f, 1.0f), new Color(1.0f, 0.25f, 0.0f, 1.0f), new Color(0.5f, 0.5f, 0.5f, 1.0f), 1.0f, 0.1f)
                            .Element("Image")
                                .SetRectTransform(5.0f, 5.0f, -5.0f, -5.0f, 0.5f, 0.5f, 0.0f, 0.0f, 1.0f, 1.0f)
                                .Component_Image("cross", Color.white, Image.Type.Sliced, Vector4.zero)
                            .Done
                        .Done
                    .Done
                    .Element("Content")
                        .SetRectTransform(0.0f, -855.0f, 599.0f, -60.0f, 0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f)
                        .SetVerticalLayout(new RectOffset(10, 10, 10, 10), 0.0f, TextAnchor.UpperLeft, false, true, true, true, false, false, false)
                        .Element("Padding")
                            .SetRectTransform(10.0f, -785.0f, 589.0f, -10.0f, 0.5f, 0.5f, 0.0f, 1.0f, 0.0f, 1.0f)
                            .SetHorizontalLayout(new RectOffset(0, 0, 0, 0), 5.0f, TextAnchor.UpperLeft, false, true, true, false, true, false, false)
                            .Element("Column Mods")
                                .Layout()
                                    .PreferredWidth(300.0f)
                                    .MinWidth(300.0f)
                                    .FlexibleWidth(0.0f)
                                .Done
                                .SetVerticalLayout(new RectOffset(0, 0, 0, 0), 0.0f, TextAnchor.UpperLeft, false, true, true, true, false, false, false)
                                .Do((UIBuilder builder) => {
                                    foreach (var kv in Config.allConfigs)
                                    {
                                        var configGUID = kv.Key;
                                        builder
                                            .Element_TextButton($"Mod Button {configGUID}", configGUID)
                                                .Layout()
                                                    .MinWidth(200.0f)
                                                .Done
                                                .SetOnClick(() => { OpenModConfig(configGUID); })
                                            .Done
                                        .End(false);
                                    }
                                })
                            .Done
                            .Element("Column Config")
                                .Keep(out _configFrame)
                                .SetVerticalLayout(new RectOffset(0, 0, 0, 0), 8.0f, TextAnchor.UpperLeft, false, true, true, true, false, false, false)
                                .Layout()
                                    .MinWidth(400.0f)
                                .Done
                            .Done
                        .Done
                    .Done
                .Done
            .End();
        }

        public static void CloseConfigurator()
        {
            if (_configuratorFrame == null) return;

            UnityEngine.Object.Destroy(_configuratorFrame);
            _configuratorFrame = null;
        }

        private static void OpenModConfig(string configGUID)
        {
            DestroyAllTransformChildren(_configFrame.transform);

            var modVersion = "unknown";
            if (configGUID == "erkle64.Unfoundry")
            {
                var versionConst = typeof(Unfoundry.Plugin).GetField("VERSION", BindingFlags.Public | BindingFlags.Static);
                if (versionConst != null)
                {
                    var version = versionConst.GetValue(null) as string;
                    if (version != null)
                    {
                        modVersion = version;
                    }
                }
            }
            else
            {
                var plugins = typeof(Unfoundry.Plugin).GetField("_unfoundryPlugins", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as Dictionary<string, UnfoundryPlugin>;
                if (plugins != null)
                {
                    if (plugins.TryGetValue(configGUID, out var plugin))
                    {
                        var versionConst = plugin.GetType().GetField("VERSION", BindingFlags.Public | BindingFlags.Static);
                        if (versionConst != null)
                        {
                            var version = versionConst.GetValue(null) as string;
                            if (version != null)
                            {
                                modVersion = version;
                            }
                        }
                    }
                }
            }

            var modConfig = Config.allConfigs[configGUID];
            var builder = UIBuilder.BeginWith(_configFrame)
                .Element($"Version Row")
                    .SetHorizontalLayout(new RectOffset(0, 0, 0, 0), 5.0f, TextAnchor.UpperLeft, false, true, true, false, true, false, false)
                    .Layout()
                        .PreferredHeight(30)
                        .MinHeight(30)
                        .FlexibleHeight(0)
                    .Done
                    .Element("Label")
                        .Layout()
                            .MinWidth(350.0f)
                            .PreferredWidth(350.0f)
                            .FlexibleWidth(0.0f)
                        .Done
                        .Component_Text($"Version: {modVersion}", "OpenSansSemibold SDF", 16.0f, Color.white)
                    .Done
                .Done
                .Element($"Restart Notification Row")
                    .Keep(out var restartRow)
                    .SetHorizontalLayout(new RectOffset(0, 0, 0, 0), 5.0f, TextAnchor.UpperLeft, false, true, true, false, true, false, false)
                    .Layout()
                        .PreferredHeight(30)
                        .MinHeight(30)
                        .FlexibleHeight(0)
                    .Done
                    .Element("Label")
                        .Layout()
                            .MinWidth(350.0f)
                            .PreferredWidth(350.0f)
                            .FlexibleWidth(0.0f)
                        .Done
                        .Component_Text("Options with <color=red>*</color> require restart.", "OpenSansSemibold SDF", 16.0f, Color.white)
                    .Done
                .Done;
            var requiresRestart = false;
            foreach (var group in modConfig)
            {
                builder = builder
                    .Element_Header($"Header {group.name}", "corner_cut_outline", new Color(0.0f, 0.6f, 1.0f, 1.0f), new Vector4(13, 3, 8, 13))
                        .Layout()
                            .MinWidth(400)
                            .MinHeight(45)
                        .Done
                        .Element("Heading")
                            .Component_Text(group.name, "OpenSansSemibold SDF", 22.0f, Color.white)
                        .Done
                    .Done;
                foreach (var entry in group)
                {
                    if (entry.requiresRestart) requiresRestart = true;
                    builder = builder
                        .Element($"Entry Row {entry.fullName}")
                            .SetHorizontalLayout(new RectOffset(0, 0, 0, 0), 5.0f, TextAnchor.MiddleLeft, false, true, true, false, true, false, false)
                            .Layout()
                                .PreferredHeight(30)
                                .MinHeight(30)
                                .FlexibleHeight(0)
                            .Done
                            .Element("Label")
                                .Layout()
                                    .MinWidth(350.0f)
                                    .PreferredWidth(350.0f)
                                    .FlexibleWidth(0.0f)
                                .Done
                                .Component_Text(entry.requiresRestart ? $"{entry.name}<color=red>*</color>" : entry.name, "OpenSansSemibold SDF", 16.0f, Color.white)
                            .Done
                            .Element("Value")
                                .Layout()
                                    .FlexibleWidth(1.0f)
                                .Done
                                .SetHorizontalLayout(new RectOffset(0, 0, 0, 0), 5.0f, TextAnchor.MiddleLeft, false, true, true, false, true, false, false)
                                .Do((UIBuilder valueBuilder) =>
                                {
                                    //[typeof(bool)] Done
                                    //[typeof(char)]
                                    //[typeof(byte)]
                                    //[typeof(sbyte)]
                                    //[typeof(short)]
                                    //[typeof(ushort)]
                                    //[typeof(int)] Done
                                    //[typeof(uint)] Done
                                    //[typeof(long)]
                                    //[typeof(ulong)]
                                    //[typeof(float)] Done
                                    //[typeof(double)] Done
                                    //[typeof(decimal)]
                                    //[typeof(string)] Done
                                    //[typeof(Enum)] Done
                                    //[typeof(Vector2)] Done
                                    //[typeof(Vector3)] Done
                                    //[typeof(Vector4)] Done
                                    //[typeof(Vector2Int)] Done
                                    //[typeof(Vector3Int)] Done
                                    //[typeof(Vector4Int)] Done
                                    if (entry is TypedConfigEntry<bool> boolEntry)
                                    {
                                        var toggleObject = UnityEngine.Object.Instantiate(GetAsset<GameObject>("ConfigEntryToggle"), valueBuilder.GameObject.transform);
                                        var toggle = toggleObject.GetComponentInChildren<Toggle>();
                                        toggle.isOn = boolEntry.Get();
                                        toggle.onValueChanged.AddListener((value) => boolEntry.Set(value));
                                    }
                                    else if (entry is TypedConfigEntry<string> stringEntry)
                                    {
                                        valueBuilder = valueBuilder
                                            .Element_InputField($"InputField {entry.fullName}", entry.ToString(), TMP_InputField.ContentType.Standard)
                                                .Layout()
                                                    .PreferredHeight(30.0f)
                                                    .MinHeight(30.0f)
                                                .Done
                                                .WithComponent<TMP_InputField>(inputField =>
                                                {
                                                    inputField.onValueChanged.AddListener((value) => stringEntry.Set(value));
                                                })
                                            .Done;
                                    }
                                    else if (entry is TypedConfigEntry<int> intEntry)
                                    {
                                        valueBuilder = valueBuilder
                                            .Element_InputField($"InputField {entry.fullName}", entry.ToString(), TMP_InputField.ContentType.IntegerNumber)
                                                .Layout()
                                                    .PreferredHeight(30.0f)
                                                    .MinHeight(30.0f)
                                                .Done
                                                .WithComponent<TMP_InputField>(inputField =>
                                                {
                                                    inputField.onValueChanged.AddListener((value) => {
                                                        try
                                                        {
                                                            var intValue = Convert.ToInt32(value);
                                                            intEntry.Set(intValue);
                                                        }
                                                        catch (Exception) { }
                                                    });
                                                })
                                            .Done;
                                    }
                                    else if (entry is TypedConfigEntry<uint> uintEntry)
                                    {
                                        valueBuilder = valueBuilder
                                            .Element_InputField($"InputField {entry.fullName}", entry.ToString(), TMP_InputField.ContentType.IntegerNumber)
                                                .Layout()
                                                    .PreferredHeight(30.0f)
                                                    .MinHeight(30.0f)
                                                .Done
                                                .WithComponent<TMP_InputField>(inputField =>
                                                {
                                                    inputField.onValueChanged.AddListener((value) => {
                                                        try
                                                        {
                                                            var uintValue = Convert.ToUInt32(value);
                                                            uintEntry.Set(uintValue);
                                                        }
                                                        catch (Exception) { }
                                                    });
                                                })
                                            .Done;
                                    }
                                    else if (entry is TypedConfigEntry<float> floatEntry)
                                    {
                                        valueBuilder = valueBuilder
                                            .Element_InputField($"InputField {entry.fullName}", entry.ToString(), TMP_InputField.ContentType.DecimalNumber)
                                                .Layout()
                                                    .PreferredHeight(30.0f)
                                                    .MinHeight(30.0f)
                                                .Done
                                                .WithComponent<TMP_InputField>(inputField =>
                                                {
                                                    inputField.onValueChanged.AddListener((value) => {
                                                        try
                                                        {
                                                            var floatValue = Convert.ToSingle(value);
                                                            floatEntry.Set(floatValue);
                                                        }
                                                        catch (Exception) { }
                                                    });
                                                })
                                            .Done;
                                    }
                                    else if (entry is TypedConfigEntry<double> doubleEntry)
                                    {
                                        valueBuilder = valueBuilder
                                            .Element_InputField($"InputField {entry.fullName}", entry.ToString(), TMP_InputField.ContentType.DecimalNumber)
                                                .Layout()
                                                    .PreferredHeight(30.0f)
                                                    .MinHeight(30.0f)
                                                .Done
                                                .WithComponent<TMP_InputField>(inputField =>
                                                {
                                                    inputField.onValueChanged.AddListener((value) => {
                                                        try
                                                        {
                                                            var doubleValue = Convert.ToDouble(value);
                                                            doubleEntry.Set(doubleValue);
                                                        }
                                                        catch (Exception) { }
                                                    });
                                                })
                                            .Done;
                                    }
                                    else if (typeof(Enum).IsAssignableFrom(entry.valueType))
                                    {
                                        var enumType = entry.valueType;
                                        var enumValues = Enum.GetValues(enumType);
                                        var enumNames = Enum.GetNames(enumType);

                                        var dropdownObject = UnityEngine.Object.Instantiate(GetAsset<GameObject>("ConfigEntryDropdown"), valueBuilder.GameObject.transform);
                                        var dropdown = dropdownObject.GetComponentInChildren<TMP_Dropdown>();
                                        dropdown.ClearOptions();
                                        foreach (var enumName in enumNames)
                                        {
                                            dropdown.options.Add(new TMP_Dropdown.OptionData(enumName));
                                        }
                                        dropdown.value = Array.IndexOf(enumNames, entry.ToString());
                                        dropdown.onValueChanged.AddListener((value) => {
                                            if (value >= 0 && value < enumNames.Length)
                                            {
                                                entry.selfType.GetMethod("Set").Invoke(entry, new object[] { enumValues.GetValue(value) });
                                            }
                                        });
                                    }
                                    else if (entry is TypedConfigEntry<Vector2> vector2Entry)
                                    {
                                        valueBuilder = valueBuilder
                                            .Element_CustomEditorFloat("X",
                                                () => vector2Entry.Get().x,
                                                (value) => { vector2Entry.Set(new Vector2(value, vector2Entry.Get().y)); })
                                            .Done
                                            .Element_CustomEditorFloat("Y",
                                                () => vector2Entry.Get().y,
                                                (value) => { vector2Entry.Set(new Vector2(vector2Entry.Get().x, value)); })
                                            .Done;
                                    }
                                    else if (entry is TypedConfigEntry<Vector3> vector3Entry)
                                    {
                                        valueBuilder = valueBuilder
                                            .Element_CustomEditorFloat("X",
                                                () => vector3Entry.Get().x,
                                                (value) => { vector3Entry.Set(new Vector3(value, vector3Entry.Get().y, vector3Entry.Get().z)); })
                                            .Done
                                            .Element_CustomEditorFloat("Y",
                                                () => vector3Entry.Get().y,
                                                (value) => { vector3Entry.Set(new Vector3(vector3Entry.Get().x, value, vector3Entry.Get().z)); })
                                            .Done
                                            .Element_CustomEditorFloat("Z",
                                                () => vector3Entry.Get().z,
                                                (value) => { vector3Entry.Set(new Vector3(vector3Entry.Get().x, vector3Entry.Get().y, value)); })
                                            .Done;
                                    }
                                    else if (entry is TypedConfigEntry<Vector4> vector4Entry)
                                    {
                                        valueBuilder = valueBuilder
                                            .Element_CustomEditorFloat("X",
                                                () => vector4Entry.Get().x,
                                                (value) => { vector4Entry.Set(new Vector4(value, vector4Entry.Get().y, vector4Entry.Get().z, vector4Entry.Get().w)); })
                                            .Done
                                            .Element_CustomEditorFloat("Y",
                                                () => vector4Entry.Get().y,
                                                (value) => { vector4Entry.Set(new Vector4(vector4Entry.Get().x, value, vector4Entry.Get().z, vector4Entry.Get().w)); })
                                            .Done
                                            .Element_CustomEditorFloat("Z",
                                                () => vector4Entry.Get().z,
                                                (value) => { vector4Entry.Set(new Vector4(vector4Entry.Get().x, vector4Entry.Get().y, value, vector4Entry.Get().w)); })
                                            .Done
                                            .Element_CustomEditorFloat("W",
                                                () => vector4Entry.Get().w,
                                                (value) => { vector4Entry.Set(new Vector4(vector4Entry.Get().x, vector4Entry.Get().y, vector4Entry.Get().z, value)); })
                                            .Done;
                                    }
                                    else if (entry is TypedConfigEntry<Vector2Int> vector2IntEntry)
                                    {
                                        valueBuilder = valueBuilder
                                            .Element_CustomEditorInt("X",
                                                () => vector2IntEntry.Get().x,
                                                (value) => { vector2IntEntry.Set(new Vector2Int(value, vector2IntEntry.Get().y)); })
                                            .Done
                                            .Element_CustomEditorInt("Y",
                                                () => vector2IntEntry.Get().y,
                                                (value) => { vector2IntEntry.Set(new Vector2Int(vector2IntEntry.Get().x, value)); })
                                            .Done;
                                    }
                                    else if (entry is TypedConfigEntry<Vector3Int> vector3IntEntry)
                                    {
                                        valueBuilder = valueBuilder
                                            .Element_CustomEditorInt("X",
                                                () => vector3IntEntry.Get().x,
                                                (value) => { vector3IntEntry.Set(new Vector3Int(value, vector3IntEntry.Get().y, vector3IntEntry.Get().z)); })
                                            .Done
                                            .Element_CustomEditorInt("Y",
                                                () => vector3IntEntry.Get().y,
                                                (value) => { vector3IntEntry.Set(new Vector3Int(vector3IntEntry.Get().x, value, vector3IntEntry.Get().z)); })
                                            .Done
                                            .Element_CustomEditorInt("Z",
                                                () => vector3IntEntry.Get().z,
                                                (value) => { vector3IntEntry.Set(new Vector3Int(vector3IntEntry.Get().x, vector3IntEntry.Get().y, value)); })
                                            .Done;
                                    }
                                })
                            .Done
                            .Do(extraBuilder =>
                            {
                                if (entry.description != null && entry.description.Length > 0)
                                {
                                    extraBuilder = extraBuilder
                                        .Element("InfoWrapper")
                                            .Layout()
                                                .MinWidth(30.0f)
                                                .PreferredWidth(30.0f)
                                                .MinHeight(30.0f)
                                                .PreferredHeight(30.0f)
                                                .FlexibleWidth(0.0f)
                                            .Done
                                            .Element_Panel("Info", "icons8-info-512", Color.white, Vector4.zero, Image.Type.Simple)
                                                .SetRectTransform(0, 0, 0, 0, 0.5f, 0.5f, 0, 0, 1, 1)
                                                .Do(infoBuilder =>
                                                {
                                                    infoBuilder.GameObject.AddComponent<TooltipTrigger>().tooltipText = string.Join("\n", entry.description);
                                                })
                                            .Done
                                        .Done;
                                }
                            })
                        .Done;
                }
            }
            if (!requiresRestart)
            {
                restartRow.SetActive(false);
            }
        }

        private static void DestroyAllTransformChildren(Transform transform)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                child.SetParent(null, false);
                UnityEngine.Object.Destroy(child.gameObject);
            }
        }

        [HarmonyPatch]
        public static class Patch
        {
            [HarmonyPatch(typeof(MainMenuManager), "Start")]
            [HarmonyPostfix]
            public static void MainMenuManagerStart(MainMenuManager __instance)
            {
                if (__instance.uiCanvas == null) return;

                UIBuilder.BeginWith(__instance.uiCanvas)
                    .Element_Panel("Configurator", "corner_cut_outline", new Color(0.133f, 0.133f, 0.133f, 1.0f), new Vector4(13, 10, 8, 13))
                        .SetRectTransform(-200.0f, -140.0f, -20.0f, -90.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f)
                        .SetHorizontalLayout(new RectOffset(0, 0, 0, 0), 5.0f, TextAnchor.UpperLeft, false, true, true, true, true, false, false)
                        .Element_TextButton("Button Configurator", "Configurator")
                            .SetOnClick(() => { OpenConfigurator(__instance.uiCanvas); })
                        .Done
                    .Done
                .End();
            }

            [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.onUIFrameOpen))]
            [HarmonyPostfix]
            public static void MainMenuManagerOnUIFrameOpen(MainMenuManager __instance, UIFrame frame)
            {
                CloseConfigurator();
            }
        }
    }
}
