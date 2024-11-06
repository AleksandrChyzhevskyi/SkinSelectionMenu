using System;
using _Development.Scripts.Data;
using _Development.Scripts.SkinsPlayer.PlayerSkinsData;

namespace _Development.Scripts.SkinSelectionMenu.Infrastructure
{
    public interface IAnimalListSkinsModel
    {
        void Create(StaticData.ShapeShiftingAbilityApply shiftingAbilityApply);
        void ActiveSkins(PlayerSkinDataFile playerSkinDataFile);
        void ActiveOneSkin(SkinID skinId);
        event Action<SkinData> TouchedIcon;
        void Unsubscribe();
    }
}