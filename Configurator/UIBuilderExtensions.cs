using System;
using TMPro;
using Unfoundry;

public static class UIBuilderExtensions
{
    public static UIBuilder Element_CustomEditorFloat(this UIBuilder builder, string label, Func<float> get, Action<float> set)
    {
        return builder.Element_Label($"Label{label}", $"{label}:", 20.0f)
                .WithComponent<TextMeshProUGUI>(tmp =>
                {
                    tmp.alignment = TextAlignmentOptions.MidlineRight;
                })
            .Done
            .Element_InputField($"InputField {label}", Convert.ToString(get(), System.Globalization.CultureInfo.InvariantCulture), TMP_InputField.ContentType.DecimalNumber)
                .Layout()
                    .PreferredHeight(30.0f)
                    .MinHeight(30.0f)
                .Done
                .WithComponent<TMP_InputField>(inputField =>
                {
                    inputField.onValueChanged.AddListener((value) =>
                    {
                        try
                        {
                            set(Convert.ToSingle(value, System.Globalization.CultureInfo.InvariantCulture));
                        }
                        catch (Exception) { }
                    });
                });
    }

    public static UIBuilder Element_CustomEditorInt(this UIBuilder builder, string label, Func<int> get, Action<int> set)
    {
        return builder.Element_Label($"Label{label}", $"{label}:", 20.0f)
                .WithComponent<TextMeshProUGUI>(tmp =>
                {
                    tmp.alignment = TextAlignmentOptions.MidlineRight;
                })
            .Done
            .Element_InputField($"InputField {label}", Convert.ToString(get()), TMP_InputField.ContentType.DecimalNumber)
                .Layout()
                    .PreferredHeight(30.0f)
                    .MinHeight(30.0f)
                .Done
                .WithComponent<TMP_InputField>(inputField =>
                {
                    inputField.onValueChanged.AddListener((value) =>
                    {
                        try
                        {
                            set(Convert.ToInt32(value));
                        }
                        catch (Exception) { }
                    });
                });
    }
}
