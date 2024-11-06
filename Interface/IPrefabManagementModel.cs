using _Development.Scripts.Data;
using _Development.Scripts.SaveLoadDatesPlayer.InterfaceSaveDatesPlayer;
using _Development.Scripts.SkinSelectionMenu.Infrastructure;
using _Development.Scripts.SkinsPlayer.PlayerSkinsData;
using UnityEngine;

namespace _Development.Scripts.BeginnersFest.Models
{
    public interface IPrefabManagementModel
    {
        AnimalsToEffectID EffectID { get; }
        int AutoAttackAbilityId { get; }

        void Create(AnimalListSkinsView AnimalListSkins, Transform parent,
            ILoadSavePlayerSkins loadSavePlayerSkins);

        void RotatePrefab(Vector2 direction);
        void SetBehaviourPrefab(bool isActive);
        void ActiveSkins(PlayerSkinDataFile skinDataFile);
        void ActiveOneSkin(SkinID skinID);
        void Unsubscribe();
    }
}