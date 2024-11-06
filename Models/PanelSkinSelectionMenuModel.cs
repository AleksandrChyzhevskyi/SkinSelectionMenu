using System;
using System.Collections.Generic;
using System.Linq;
using _Development.Scripts.BeginnersFest.Models;
using _Development.Scripts.Boot;
using _Development.Scripts.Data;
using _Development.Scripts.Data.Enum;
using _Development.Scripts.SkinSelectionMenu.Enum;
using _Development.Scripts.SkinsPlayer.PlayerSkinsData;
using _Development.Scripts.Upgrade.Data;
using BLINK.RPGBuilder.Characters;
using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

namespace _Development.Scripts.SkinSelectionMenu.Models
{
    public class PanelSkinSelectionMenuModel : IPanelSkinSelectionMenuModel
    {
        private readonly AllPlayerSkins _playerSkins;
        private readonly Dictionary<int, IPrefabManagementModel> _prefabManagements;
        private readonly PanelSkinSelectionMenuView _panelSkinSelectionMenuView;

        private IPrefabManagementModel _currentPrefabManagementModel;

        public PanelSkinSelectionMenuModel(AllPlayerSkins playerSkins,
            Dictionary<int, IPrefabManagementModel> prefabManagements,
            PanelSkinSelectionMenuView panelSkinSelectionMenuView)
        {
            _playerSkins = playerSkins;
            _prefabManagements = prefabManagements;
            _panelSkinSelectionMenuView = panelSkinSelectionMenuView;
        }

        public void StartModel()
        {
            Subscribe();
            ActiveCurrentModel();
            ActivePanel(StateButton.Default);
        }

        private void Subscribe()
        {
            _panelSkinSelectionMenuView.ClickedNextButton += OnClickedNextButton;
            _panelSkinSelectionMenuView.ClickedReturnButton += OnClickedReturnButton;
            _panelSkinSelectionMenuView.ClickedRotateLeftButton += OnRotateLeftButton;
            _panelSkinSelectionMenuView.ClickedRotateRightButton += OnRotateRightButton;
            GeneralEvents.OpenedNewSkin += OnOpenedNewSkin;
            GameEvents.EnablePanelSkinSelection += ActiveCurrentModel;
        }

        public void Unsubscribe()
        {
            _panelSkinSelectionMenuView.ClickedNextButton -= OnClickedNextButton;
            _panelSkinSelectionMenuView.ClickedReturnButton -= OnClickedReturnButton;
            _panelSkinSelectionMenuView.ClickedRotateLeftButton -= OnRotateLeftButton;
            _panelSkinSelectionMenuView.ClickedRotateRightButton -= OnRotateRightButton;
            GeneralEvents.OpenedNewSkin -= OnOpenedNewSkin;
            GameEvents.EnablePanelSkinSelection += ActiveCurrentModel;

            foreach (IPrefabManagementModel prefabManagementModel in _prefabManagements.Values)
                prefabManagementModel.Unsubscribe();
        }

        private void OnRotateRightButton(int SpeedRotate) =>
            _currentPrefabManagementModel.RotatePrefab(Vector2.left * SpeedRotate);

        private void OnRotateLeftButton(int SpeedRotate) =>
            _currentPrefabManagementModel.RotatePrefab(Vector2.right * SpeedRotate);

        private void OnClickedReturnButton() =>
            ActivePanel(StateButton.Return);

        private void OnClickedNextButton() =>
            ActivePanel(StateButton.Next);

        private void ActivePanel(StateButton direction)
        {
            SetTextButton(direction);
            SetParametersForModel(_currentPrefabManagementModel);

            if (direction == StateButton.Default)
            {
                _panelSkinSelectionMenuView.SetTextInPanel(TextPanelModel.Current,
                    _currentPrefabManagementModel.EffectID.ToString());
                SetParametersForModel(_currentPrefabManagementModel);
                return;
            }

            _currentPrefabManagementModel.SetBehaviourPrefab(false);
            _currentPrefabManagementModel = _prefabManagements[OffsetNumber((int)direction)];
            _currentPrefabManagementModel.SetBehaviourPrefab(true);
            _panelSkinSelectionMenuView.SetTextInPanel(TextPanelModel.Current,
                _currentPrefabManagementModel.EffectID.ToString());
            SetParametersForModel(_currentPrefabManagementModel);
        }

        private void SetParametersForModel(IPrefabManagementModel currentPrefabManagementModel)
        {
            _panelSkinSelectionMenuView.ResetText();
            
            RPGEffect.RPGEffectRankData effect =
                GameDatabase.Instance.GetEffects()[(int)currentPrefabManagementModel.EffectID].ranks.FirstOrDefault();

            foreach (RPGEffect.STAT_EFFECTS_DATA statEffectsData in effect.statEffectsData)
            {
                UpgradeData data = Game.instance.GetStaticData().UpgradesDate
                    .FirstOrDefault(upgradeData => upgradeData.ID == statEffectsData.statID);

                string text = statEffectsData.statEffectModification.ToString();

                _panelSkinSelectionMenuView.SetParametersForIndicatorSkin(statEffectsData.TypeStatEffect,
                    data.IconUpgrade, text);
            }

            RPGAbility.RPGAbilityRankData ability =
                GameDatabase.Instance.GetAbilities()[currentPrefabManagementModel.AutoAttackAbilityId].ranks
                    .FirstOrDefault();

            int damage = 0;

            foreach (RPGAbility.AbilityEffectsApplied effectsApplied in ability.effectsApplied)
            {
                RPGEffect.RPGEffectRankData effect2 =
                    GameDatabase.Instance.GetEffects()[effectsApplied.effectID].ranks.FirstOrDefault();

                damage += (int)effect2.damageStatModifier;
            }

            _panelSkinSelectionMenuView.SetParametersForIndicatorSkin(IndicatorSkill.Attack, Game.instance
                    .GetStaticData().UpgradesDate
                    .FirstOrDefault(upgradeData => upgradeData.ID == (int)IndicatorSkill.Attack).IconUpgrade,
                damage.ToString());

            RPGEffect rpgEffect = GameDatabase.Instance.GetEffects()[ability.effectsApplied.FirstOrDefault().effectID];

            _panelSkinSelectionMenuView.SetAbilityParameters(rpgEffect.entryIcon, rpgEffect.entryDescription);
            _panelSkinSelectionMenuView.SetIsInitializedTextPanel();
        }

        private void ActiveCurrentModel()
        {
            if (_currentPrefabManagementModel == null)
            {
                foreach (PlayerSkinDataFile skinDataFile in _playerSkins.DataFiles)
                {
                    if (_currentPrefabManagementModel != null)
                        continue;

                    if (skinDataFile.Skinslist.FirstOrDefault(skinIDDataFile => skinIDDataFile.IsActive) != null)
                        _currentPrefabManagementModel =
                            _prefabManagements.Values.FirstOrDefault(managementModel =>
                                managementModel.EffectID == (AnimalsToEffectID)skinDataFile.EffectID);
                }
            }
            else
            {
                _currentPrefabManagementModel.SetBehaviourPrefab(false);

                if (GameState.playerEntity.IsShapeshifted())
                {
                    _currentPrefabManagementModel =
                        _prefabManagements.Values.FirstOrDefault(prefabManagementModel =>
                            prefabManagementModel.EffectID ==
                            (AnimalsToEffectID)GameState.playerEntity.ShapeshiftedEffect.ID);
                }
            }

            _currentPrefabManagementModel.SetBehaviourPrefab(true);
        }

        private void OnOpenedNewSkin(RPGEffect effect, SkinID skinID) =>
            _prefabManagements.Values.FirstOrDefault(managementModel =>
                managementModel.EffectID == (AnimalsToEffectID)effect.ID)!.ActiveOneSkin(skinID);

        private void SetTextButton(StateButton direction)
        {
            switch (direction)
            {
                case StateButton.Default:
                    _panelSkinSelectionMenuView.SetTextInPanel(TextPanelModel.Next, _prefabManagements[OffsetNumber(1)]
                        .EffectID.ToString());
                    _panelSkinSelectionMenuView.SetTextInPanel(TextPanelModel.Return,
                        _prefabManagements[OffsetNumber(-1)].EffectID.ToString());
                    break;
                case StateButton.Return:
                    _panelSkinSelectionMenuView.SetTextInPanel(TextPanelModel.Next,
                        _currentPrefabManagementModel.EffectID.ToString());
                    _panelSkinSelectionMenuView.SetTextInPanel(TextPanelModel.Return,
                        _prefabManagements[OffsetNumber(-2)].EffectID
                            .ToString());
                    break;
                case StateButton.Next:
                    _panelSkinSelectionMenuView.SetTextInPanel(TextPanelModel.Next, _prefabManagements[OffsetNumber(2)]
                        .EffectID.ToString());
                    _panelSkinSelectionMenuView.SetTextInPanel(TextPanelModel.Return,
                        _currentPrefabManagementModel.EffectID.ToString());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        private int GetNumberCurrentPanel()
        {
            foreach (KeyValuePair<int, IPrefabManagementModel> valuePair in _prefabManagements)
            {
                if (_currentPrefabManagementModel != valuePair.Value)
                    continue;

                return valuePair.Key;
            }

            return 0;
        }

        private int OffsetNumber(int direction)
        {
            int number = GetNumberCurrentPanel() + direction;

            if (number > _prefabManagements.Keys.Count)
                number -= _prefabManagements.Keys.Count;

            if (number <= 0)
                number = _prefabManagements.Keys.Count - Math.Abs(number);

            return number;
        }
    }
}