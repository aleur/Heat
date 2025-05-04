using System;
using GTA;
using GTA.Native;
using GTA.Math;
using System.Threading;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using System.Windows.Forms;
using System.ComponentModel;
using static System.Collections.Specialized.BitVector32;
using System.Media;

public class BagSystem : Script
{
    private List<WeaponHash> weaponList = HeatSettings.weaponList;
    private List<int[]> bagList = HeatSettings.bagList;
    private string clothingAnim, clothingDict;

    public BagSystem()
    {
        Tick += OnTick;
    }
    private void OnTick(object sender, EventArgs e)
    {
        Ped playerPed = Game.Player.Character;
        int bagComponent = GetComponentVariation(playerPed, 5);

        int[] equippedBag = bagList.FirstOrDefault(bag => bag[0] == bagComponent || bag[1] == bagComponent);
        if (playerPed.IsInVehicle() || !playerPed.IsAlive || playerPed.IsSwimming)
            return;

        WeaponHash currentWeapon = Game.Player.Character.Weapons.Current.Hash;
        bool isSprinting = IsPlayerSprinting();
        bool isBagEquipped = IsBagEquipped(Game.Player.Character);

        if (!HasPrimaryWeapons(playerPed) || isAnimPlaying(clothingAnim, clothingDict))
            return;

        // Force player to equip weapon if bag not equipped
        if (!isBagEquipped)
        {
            if (currentWeapon != weaponList.FirstOrDefault(weaponHash => weaponHash == currentWeapon))
            {
                // If player has primary weapon, but no bag, equip primary weapon.
                EquipAppropriateWeapon();
            }
            return;
        }

        // Bag not recognized, exit method
        if (!HasConfigBagEquipped(playerPed))
            return;

        // Bag states - open/closed
        if (currentWeapon != weaponList.FirstOrDefault(weaponHash => weaponHash == currentWeapon))
        {
            if (bagComponent == equippedBag[1])
            {
                Wait(150);
                SetComponentVariation(playerPed, 5, equippedBag[0], 0, 0);
            }
        }
        else
        {
            if (bagComponent == equippedBag[0])
            {
                Wait(150);
                SetComponentVariation(playerPed, 5, equippedBag[1], 0, 0);
            }
        }
    }

    private bool IsPlayerSprinting()
    {
        return Game.Player.Character.IsSprinting;
    }

    private bool IsPrimaryWeapon(WeaponHash weaponHash)
    {
        return weaponList.Contains(weaponHash);
    }
    private bool HasPrimaryWeapons(Ped playerPed)
    {
        return weaponList.Any(weaponHash => playerPed.Weapons.HasWeapon(weaponHash));
    }

    private void EquipAppropriateWeapon()
    {
        // Equip the first available primary or additional weapon from the player's inventory
        foreach (WeaponHash weaponHash in weaponList)
        {
            WeaponHash currentWeaponHash = weaponHash;
            if (Game.Player.Character.Weapons.HasWeapon(currentWeaponHash) && IsPrimaryWeapon(currentWeaponHash))
            {
                Game.Player.Character.Weapons.Give(currentWeaponHash, 0, true, true);
                break;
            }
        }
    }
    private int GetComponentVariation(Ped playerPed, int componentId)
    {
        return Function.Call<int>(GTA.Native.Hash.GET_PED_DRAWABLE_VARIATION, playerPed, componentId);
    }
    private void SetComponentVariation(Ped playerPed, int componentId, int drawableId, int textureId, int paletteId)
    {
        Function.Call(GTA.Native.Hash.SET_PED_COMPONENT_VARIATION, playerPed, componentId, drawableId, textureId, paletteId);
    }

    private bool IsBagEquipped(Ped playerPed)
    {
        return Function.Call<int>(GTA.Native.Hash.GET_PED_DRAWABLE_VARIATION, playerPed, 5) != 0;
    }
    private bool HasConfigBagEquipped(Ped playerPed)
    {
        return bagList.Any(bag => bag[0] == Function.Call<int>(GTA.Native.Hash.GET_PED_DRAWABLE_VARIATION, playerPed, 5)) || bagList.Any(bag => bag[1] == Function.Call<int>(GTA.Native.Hash.GET_PED_DRAWABLE_VARIATION, playerPed, 5));
    }
    
    private bool isAnimPlaying(string anim, string dict)
    {
        if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, dict)) return false;
        return Function.Call<bool>(GTA.Native.Hash.IS_ENTITY_PLAYING_ANIM, Game.Player.Character, dict, anim, 3);
    }
}