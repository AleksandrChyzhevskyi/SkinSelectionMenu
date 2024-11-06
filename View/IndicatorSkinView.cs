using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IndicatorSkinView : MonoBehaviour
{
    [SerializeField] private Image Icon;
    [SerializeField] private TMP_Text Description;
    [SerializeField] private Color ColorText;
    [SerializeField] private Color DefaultColorText;

    public void UpdateParameters(string text, Sprite sprite, bool isDefaultColor = true)
    {
        Description.color = isDefaultColor ? DefaultColorText : ColorText;
        Description.text = text ?? $"{0}%";

        if (sprite != null)
            Icon.sprite = sprite;
    }
}