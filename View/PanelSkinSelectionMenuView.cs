using System;
using System.Collections;
using _Development.Scripts.Data.Enum;
using _Development.Scripts.SkinSelectionMenu.Enum;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelSkinSelectionMenuView : MonoBehaviour
{
    public event Action ClickedNextButton;
    public event Action ClickedReturnButton;
    public event Action<int> ClickedRotateLeftButton;
    public event Action<int> ClickedRotateRightButton;

    public Transform ContentListSkins;
    public Transform ContentModel;
    public Button ReturnButton;
    public TMP_Text TextReturnButton;
    public Button NextButton;
    public TMP_Text TextNextButton;
    public Scrollbar HB;
    public TMP_Text NameCurrentAnimal;
    public IndicatorSkinView HealthView;
    public IndicatorSkinView SpeedView;
    public IndicatorSkinView AttackView;
    public IndicatorSkinView AbilityView;

    public float UpdateCoroutine = 0.1f;
    public int SpeedRotate = 10;

    private Coroutine _coroutine;
    private bool _isWorkRotate;
    private float _currentSize;

    private bool _isInitializedTextPanel;

    private void OnEnable()
    {
        NextButton.onClick.AddListener(OnClickedNextButton);
        ReturnButton.onClick.AddListener(OnClickedReturnButton);
        GameEvents.Instance.OnEnablePanelSkinSelection();
        GameEvents.FinishedActionWithSkins += OnFinishedActionWithSkins;

        StartCoroutine(UpdateStatRotate());
    }

    private void OnDisable()
    {
        NextButton.onClick.RemoveListener(OnClickedNextButton);
        ReturnButton.onClick.RemoveListener(OnClickedReturnButton);
        GameEvents.FinishedActionWithSkins -= OnFinishedActionWithSkins;
    }

    public void SetIsInitializedTextPanel() =>
        _isInitializedTextPanel = true;

    public void SetTextInPanel(TextPanelModel panel, string text)
    {
        switch (panel)
        {
            case TextPanelModel.Current:
                NameCurrentAnimal.text = text;
                break;
            case TextPanelModel.Next:
                TextNextButton.text = text;
                break;
            case TextPanelModel.Return:
                TextReturnButton.text = text;
                break;
            case TextPanelModel.Default:
                new Exception($"{panel} This element is Default");
                break;
        }
    }

    public void SetParametersForIndicatorSkin(IndicatorSkill indicator, Sprite sprite, string text)
    {
        switch (indicator)
        {
            case IndicatorSkill.Attack:
                SetInitializeElement(AttackView, sprite, $"+{text}%");
                break;
            case IndicatorSkill.Health:
                SetInitializeElement(HealthView, sprite, $"+{text}%");
                break;
            case IndicatorSkill.Movement:
                SetInitializeElement(SpeedView, sprite, $"+{text}%");
                break;
        }
    }

    public void SetAbilityParameters(Sprite sprite, string text) =>
        SetInitializeElement(AbilityView, sprite, text);

    public void ResetText()
    {
        AttackView.UpdateParameters(null, null);
        HealthView.UpdateParameters(null, null);
        SpeedView.UpdateParameters(null, null);
        AbilityView.UpdateParameters(null, null);
    }

    private void SetInitializeElement<T>(T prefab, Sprite sprite, string text) where T : IndicatorSkinView => 
        prefab.UpdateParameters(text, sprite, false);

    private void ClickDownButton(Action<int> action)
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        _isWorkRotate = true;
        _coroutine = StartCoroutine(StartRotate(action));
    }

    private IEnumerator UpdateStatRotate()
    {
        while (_currentSize != HB.size)
        {
            ClickDownButton(HB.value > 0 ? ClickedRotateLeftButton : ClickedRotateRightButton);
            yield return new WaitForSeconds(0.1f);
        }

        _currentSize = HB.size;
    }

    private IEnumerator StartRotate(Action<int> action)
    {
        while (HB.size < 0.99f)
        {
            action?.Invoke(SpeedRotate);
            yield return new WaitForSeconds(UpdateCoroutine);
        }
    }

    private void OnFinishedActionWithSkins() =>
        gameObject.SetActive(false);

    private void OnClickedReturnButton() =>
        ClickedReturnButton?.Invoke();

    private void OnClickedNextButton() =>
        ClickedNextButton?.Invoke();
}