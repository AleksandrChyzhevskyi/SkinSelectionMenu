using System.Collections.Generic;
using System.Linq;
using _Development.Scripts.BeginnersFest.Models;
using _Development.Scripts.Data;
using _Development.Scripts.SaveLoadDatesPlayer.InterfaceSaveDatesPlayer;
using _Development.Scripts.SkinSelectionMenu.Models;
using _Development.Scripts.SkinSelectionMenu.View;
using _Development.Scripts.SkinsPlayer.PlayerSkinsData;
using UnityEngine;

namespace _Development.Scripts.SkinSelectionMenu.Infrastructure
{
    public class CreatorSkinSelectionMenu : MonoBehaviour
    {
        public PanelSkinSelectionMenuView PanelSkinSelectionMenu;
        public IconSkinView IconSkin;
        public AnimalListSkinsView AnimalListSkins;

        private Dictionary<int, IPrefabManagementModel> _prefabManagements;
        private List<StaticData.ShapeShiftingAbilityApply> _shapeShiftingAbilities;
        private AllPlayerSkins _playerSkins;
        private ILoadSavePlayerSkins _loadSavePlayerSkins;
        private IPanelSkinSelectionMenuModel _selectionMenuModel;

        public void Initialize(IEnumerable<StaticData.ShapeShiftingAbilityApply> shapeShiftingAbilities,
            AllPlayerSkins playerSkins, ILoadSavePlayerSkins loadSavePlayerSkins)
        {
            _shapeShiftingAbilities = new List<StaticData.ShapeShiftingAbilityApply>(shapeShiftingAbilities);
            _playerSkins = playerSkins;
            _loadSavePlayerSkins = loadSavePlayerSkins;
        }

        public void Create()
        {
            _prefabManagements = new Dictionary<int, IPrefabManagementModel>();
            int numberList = 1;

            foreach (StaticData.ShapeShiftingAbilityApply shiftingAbilityApply in _shapeShiftingAbilities)
            {
                var prefab = Instantiate(shiftingAbilityApply.PrefabShapeShifting,
                    PanelSkinSelectionMenu.ContentModel);

                prefab.transform.Rotate(0,200,0);
                
                IPrefabManagementModel prefabManagementModel =
                    new PrefabManagementModel(prefab, shiftingAbilityApply, IconSkin, shiftingAbilityApply.SetAutoattackAbilityId);

                prefabManagementModel.Create(AnimalListSkins, PanelSkinSelectionMenu.ContentListSkins, _loadSavePlayerSkins);
                prefabManagementModel.SetBehaviourPrefab(false);

                _prefabManagements.Add(numberList++, prefabManagementModel);
            }

            foreach (PlayerSkinDataFile skinDataFile in _playerSkins.DataFiles)
            {
                IPrefabManagementModel prefabManagementModel = _prefabManagements.Values.FirstOrDefault(
                    managementModel =>
                        managementModel.EffectID == (AnimalsToEffectID)skinDataFile.EffectID);

                prefabManagementModel.ActiveSkins(skinDataFile);
            }

            _selectionMenuModel =
                new PanelSkinSelectionMenuModel(_playerSkins, _prefabManagements, PanelSkinSelectionMenu);

           _selectionMenuModel.StartModel();
        }

        public void Unsubscribe() => 
            _selectionMenuModel.Unsubscribe();
    }
}