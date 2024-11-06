using System;
using System.Collections.Generic;
using System.Linq;
using _Development.Scripts.Data;
using _Development.Scripts.SaveLoadDatesPlayer.InterfaceSaveDatesPlayer;
using _Development.Scripts.SkinSelectionMenu.Enum;
using _Development.Scripts.SkinSelectionMenu.View;
using _Development.Scripts.SkinsPlayer.PlayerSkinsData;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.LogicMono;
using BLINK.RPGBuilder.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Development.Scripts.SkinSelectionMenu.Infrastructure
{
    public class AnimalListSkinsModel : IAnimalListSkinsModel
    {
        public event Action<SkinData> TouchedIcon;

        private readonly IconSkinView _iconSkin;
        private readonly Transform _parent;
        private readonly ILoadSavePlayerSkins _loadSavePlayerSkins;

        private Dictionary<SkinData, IconSkinView> _skinViews;
        private IconSkinView _currentView;
        private int _effectID;

        public AnimalListSkinsModel(IconSkinView iconSkin, Transform parent, ILoadSavePlayerSkins loadSavePlayerSkins)
        {
            _iconSkin = iconSkin;
            _parent = parent;
            _loadSavePlayerSkins = loadSavePlayerSkins;
        }

        public void Create(StaticData.ShapeShiftingAbilityApply shiftingAbilityApply)
        {
            _skinViews = new Dictionary<SkinData, IconSkinView>();
            _effectID = shiftingAbilityApply.ShapeshiftId;

            foreach (SkinData skinData in shiftingAbilityApply.SkinDates)
            {
                IconSkinView skinView = Object.Instantiate(_iconSkin, _parent);
                skinView.SetTextNameSkin(skinData.Info);
                skinView.SetIconSkin(skinData.ChooseIcon);

                Subscribe(skinView);

                if (skinData.ID == SkinID.DefaultSkin)
                    skinView.SetBehaviourSkin(StateSkin.Default);
                else
                    skinView.SetBehaviourSkin(StateSkin.Buy, skinData.Currency, skinData.Price);

                _skinViews.Add(skinData, skinView);
            }

            SetCurrentView();
        }

        public void Unsubscribe()
        {
            foreach (IconSkinView iconSkinView in _skinViews.Values)
            {
                iconSkinView.ClickedBuyButton -= OnClickedBuyButton;
                iconSkinView.ClickedSelectButton -= OnClickedSelectButton;
                iconSkinView.TouchedIcon -= OnTouchedIcon;
                GameEvents.EnablePanelSkinSelection -= OnEnablePanelSkinSelection;
            }
        }

        public void ActiveSkins(PlayerSkinDataFile playerSkinDataFile)
        {
            foreach (SkinIDDataFile skinIDDataFile in playerSkinDataFile.Skinslist)
            {
                if (TryGetSkinData(skinIDDataFile.SkinIDData, out var skinData) == false)
                    return;

                IconSkinView skinView = _skinViews[skinData];

                if (skinIDDataFile.IsActive)
                {
                    skinView.SetBehaviourSkin(StateSkin.Active);
                    _currentView = skinView;
                }
                else
                    skinView.SetBehaviourSkin(StateSkin.Select);
            }
        }

        private void SetCurrentView()
        {
            SkinData defaultSkinData = _skinViews.Keys.FirstOrDefault(data => data.ID == SkinID.DefaultSkin);
            _currentView = _skinViews[defaultSkinData];
        }

        public void ActiveOneSkin(SkinID skinId)
        {
            if (_currentView != null && _currentView._stateSkin == StateSkin.Active)
                _currentView.SetBehaviourSkin(StateSkin.Select);

            if (TryGetSkinData(skinId, out var skinData) == false)
                return;

            _currentView = _skinViews[skinData];
            _currentView.SetBehaviourSkin(StateSkin.Active);
        }

        private void Subscribe(IconSkinView skinView)
        {
            skinView.ClickedBuyButton += OnClickedBuyButton;
            skinView.ClickedSelectButton += OnClickedSelectButton;
            skinView.TouchedIcon += OnTouchedIcon;
            GameEvents.EnablePanelSkinSelection += OnEnablePanelSkinSelection;
        }

        private void OnEnablePanelSkinSelection() =>
            OnTouchedIcon(_currentView);

        private void OnTouchedIcon(IconSkinView skinView) =>
            TouchedIcon?.Invoke(GetSkinData(skinView));

        private void OnClickedSelectButton(IconSkinView skinView)
        {
            SkinData skinData = GetSkinData(skinView);

            foreach (PlayerSkinDataFile playerSkinDataFile in Character.Instance.CharacterData.Skins.DataFiles)
            {
                if (playerSkinDataFile.EffectID != _effectID)
                    continue;

                foreach (SkinIDDataFile skinIDDataFile in playerSkinDataFile.Skinslist)
                {
                    if (skinIDDataFile.SkinIDData != skinData.ID)
                    {
                        skinIDDataFile.IsActive = false;
                        IconSkinView iconSkinView = GetIconSkinView(skinIDDataFile.SkinIDData);
                        iconSkinView.SetBehaviourSkin(StateSkin.Select);
                    }
                    else
                        skinIDDataFile.IsActive = true;
                }
            }

            TouchedIcon?.Invoke(skinData);

            skinView.SetBehaviourSkin(StateSkin.Active);
            _currentView = skinView;

            _loadSavePlayerSkins.SavePlayerSkins(Character.Instance.CharacterData.Skins);

            if (GameState.playerEntity.IsShapeshifted() == false) 
                return;
            
            RPGBuilderEssentials.Instance.ModelShape.ApplyEffect(_effectID, skinData);
            GameEvents.Instance.OnFinishedActionWithSkins();
        }

        private void OnClickedBuyButton(IconSkinView skinView)
        {
            SkinData skinData = GetSkinData(skinView);

            if (Character.Instance.getCurrencyAmount(skinData.Currency) - skinData.Price < 0)
                return;

            BuySkin(skinData);

            if (GameState.playerEntity.IsShapeshifted())
            {
                RPGBuilderEssentials.Instance.ModelShape.ApplyEffect(_effectID, skinData);
                GameEvents.Instance.OnFinishedActionWithSkins();
            }

            RPGEffect effect = GameDatabase.Instance.GetEffects()[_effectID];
            GeneralEvents.Instance.OnOpenedNewSkin(effect, skinData.ID);

            TouchedIcon?.Invoke(skinData);

            if (_currentView._stateSkin == StateSkin.Active)
                _currentView.SetBehaviourSkin(StateSkin.Select);

            skinView.SetBehaviourSkin(StateSkin.Active);
            _currentView = skinView;
        }

        private SkinData GetSkinData(IconSkinView skinView)
        {
            foreach (KeyValuePair<SkinData, IconSkinView> valuePair in _skinViews)
            {
                if (valuePair.Value == skinView)
                    return valuePair.Key;
            }

            return null;
        }

        private IconSkinView GetIconSkinView(SkinID skinData) =>
            _skinViews[_skinViews.Keys.FirstOrDefault(data => data.ID == skinData)];

        private bool TryGetSkinData(SkinID skinIDData, out SkinData skinData)
        {
            skinData = _skinViews.Keys.FirstOrDefault(data => data.ID == skinIDData);
            return skinData != null;
        }

        private void BuySkin(SkinData skinData)
        {
            float currencyAmount = Character.Instance.getCurrencyAmount(skinData.Currency) - skinData.Price;
            EconomyUtilities.setCurrencyAmount(skinData.Currency, (int)currencyAmount);
            GeneralEvents.Instance.OnPlayerCurrencyChanged(skinData.Currency);
        }
    }
}