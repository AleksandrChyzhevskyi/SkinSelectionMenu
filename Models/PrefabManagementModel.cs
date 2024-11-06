using _Development.Scripts.Data;
using _Development.Scripts.SaveLoadDatesPlayer.InterfaceSaveDatesPlayer;
using _Development.Scripts.SkinSelectionMenu.Infrastructure;
using _Development.Scripts.SkinSelectionMenu.View;
using _Development.Scripts.SkinsPlayer.PlayerSkinsData;
using UnityEngine;

namespace _Development.Scripts.BeginnersFest.Models
{
    public class PrefabManagementModel : IPrefabManagementModel
    {
        private readonly Renderer _renderer;
        private readonly GameObject _prefab;
        private readonly IconSkinView _iconSkin;
        private readonly StaticData.ShapeShiftingAbilityApply _shiftingAbilityApply;

        private AnimalListSkinsView _animalListSkinsView;
        private IAnimalListSkinsModel _listSkinsModel;
        
        public AnimalsToEffectID EffectID { get; }
        public int AutoAttackAbilityId { get; }

        public PrefabManagementModel(GameObject prefab, StaticData.ShapeShiftingAbilityApply shiftingAbilityApply, IconSkinView iconSkin, int autoAttackAbilityId)
        {
            _prefab = prefab;
            _shiftingAbilityApply = shiftingAbilityApply;
            _iconSkin = iconSkin;
            AutoAttackAbilityId = autoAttackAbilityId;
            _renderer = prefab.GetComponentInChildren<Renderer>();
            EffectID = (AnimalsToEffectID)shiftingAbilityApply.ShapeshiftId;
        }

        public void Create(AnimalListSkinsView AnimalListSkins, Transform parent,
            ILoadSavePlayerSkins loadSavePlayerSkins)
        {
            _animalListSkinsView = Object.Instantiate(AnimalListSkins, parent);
            _listSkinsModel = new AnimalListSkinsModel(_iconSkin, _animalListSkinsView.Content, loadSavePlayerSkins);
            _listSkinsModel.Create(_shiftingAbilityApply);
            _listSkinsModel.TouchedIcon += UpdateMaterial;
        }

        public void Unsubscribe()
        {
            _listSkinsModel.TouchedIcon -= UpdateMaterial;
            _listSkinsModel.Unsubscribe();
        }

        public void RotatePrefab(Vector2 direction) =>
            _prefab.transform.Rotate(new Vector3(0, direction.x, direction.y));

        public void SetBehaviourPrefab(bool isActive)
        {
            _prefab.SetActive(isActive);
            _animalListSkinsView.gameObject.SetActive(isActive);
        }

        public void ActiveSkins(PlayerSkinDataFile skinDataFile) => 
            _listSkinsModel.ActiveSkins(skinDataFile);

        public void ActiveOneSkin(SkinID skinID) => 
            _listSkinsModel.ActiveOneSkin(skinID);

        private void UpdateMaterial(SkinData skinData) =>
            _renderer.sharedMaterial = skinData.Material;
    }
}