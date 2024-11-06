using System;
using _Development.Scripts.SkinSelectionMenu.Enum;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Development.Scripts.SkinSelectionMenu.View
{
    public class IconSkinView : MonoBehaviour
    {
        public event Action<IconSkinView> ClickedBuyButton;
        public event Action<IconSkinView> ClickedSelectButton;
        public event Action<IconSkinView> TouchedIcon;

        [SerializeField] private Image _skinIcon;
        [SerializeField] private Image _borderImage;
        [SerializeField] private Image _activeImage;
        [SerializeField] private TMP_Text _nameSkin;
        [SerializeField] private Button _lockedButton;
        [SerializeField] private Button _buyButton;
        [SerializeField] private TMP_Text _textBuyButton;
        [SerializeField] private Button _selectButton;

        public StateSkin _stateSkin { get; private set; }

        public void TouchIcon() =>
            TouchedIcon?.Invoke(this);

        public void SetIconSkin(Sprite sprite) =>
            _skinIcon.sprite = sprite;

        public void SetTextNameSkin(string text) =>
            _nameSkin.text = text;

        public void SetBehaviourSkin(StateSkin state, RPGCurrency Currency = null, float cost = default)
        {
            _stateSkin = state;

            if (state == StateSkin.Buy)
                ShowElementsInSkin(Currency, cost);
            else
                ShowElementsInSkin();
        }

        private void ShowElementsInSkin(RPGCurrency Currency = null, float cost = default)
        {
            switch (_stateSkin)
            {
                case StateSkin.Default:
                case StateSkin.Active:
                    DisableButtons();
                    break;
                case StateSkin.Buy:
                    EnableButton($"<sprite name={Currency.entryName}> {cost}");
                    break;
                case StateSkin.Select:
                    EnableButton();
                    break;
            }
        }

        private void OnClickedSelectButton() =>
            ClickedSelectButton?.Invoke(this);

        private void OnClickedBuyButton() =>
            ClickedBuyButton?.Invoke(this);

        private void DisableOneButtonState(Button button, UnityAction action)
        {
            button.gameObject.SetActive(false);
            button.onClick.RemoveListener(action);
        }

        private void DisableButtons()
        {
            if (_stateSkin == StateSkin.Default)
            {
                SetStateObject(_borderImage, _activeImage);
                _lockedButton.gameObject.SetActive(true);
            }
            else
            {
                SetStateObject(_activeImage, _borderImage);
                _lockedButton.gameObject.SetActive(false);
            }

            DisableOneButtonState(_buyButton, OnClickedBuyButton);
            DisableOneButtonState(_selectButton, OnClickedSelectButton);
        }

        private void EnableButton(string text = null)
        {
            if (_stateSkin == StateSkin.Buy)
            {
                SetStateObject(_borderImage, _activeImage);
                SetBehaviourButton(_buyButton, _selectButton, OnClickedBuyButton, OnClickedSelectButton, text);
            }
            else
            {
                SetStateObject(_borderImage, _activeImage);
                SetBehaviourButton(_selectButton, _buyButton, OnClickedSelectButton, OnClickedBuyButton);
            }

            _lockedButton.gameObject.SetActive(false);
        }

        private void SetBehaviourButton(Button button1, Button button2, UnityAction actionButton1,
            UnityAction actionButton2, string text = null)
        {
            if (button1 == _buyButton)
                SetStateObject(button1, button2, text);
            else
                SetStateObject(button1, button2);
            
            button1.onClick.AddListener(actionButton1);
            button2.onClick.RemoveListener(actionButton2);
        }

        private void SetStateObject<T>(T object1, T object2, string text = null) where T : MonoBehaviour
        {
            object1.gameObject.SetActive(true);
            object2.gameObject.SetActive(false);

            if (object1 == _buyButton)
                _textBuyButton.text = text;
        }
    }
}