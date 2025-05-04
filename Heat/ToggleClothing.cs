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

public class ToggleClothing : Script
{
    private string clothingAnim, clothingDict;
    private int hatComponent = -1, hatTexture = -1, maskComponent = 0, maskTexture = -1, glassesComponent = -1, glassesTexture = -1;

    private Keys EquipMaskKey = HeatSettings.equipMaskKey;
    private Keys EquipHatKey = HeatSettings.equipHatKey;
    private Keys EquipGlassesKey = HeatSettings.equipGlassesKey;

    public ToggleClothing()
    {
        KeyUp += OnKeyUp;
    }
    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (isAnimPlaying(clothingAnim, clothingDict)) return;

        if (e.KeyCode == EquipMaskKey)
        {
            ToggleMask();
        }
        else if (e.KeyCode == EquipHatKey)
        {
            ToggleHat();
        }
        else if (e.KeyCode == EquipGlassesKey)
        {
            ToggleGlasses();
        }
    }

    private void ToggleMask()
    {
        Ped playerPed = Game.Player.Character;

        if (playerPed == null)
            return;

        if (GetComponentVariation(playerPed, 1) == 0 && maskComponent != 0)
        {
            EquipMask();
        }
        else if (GetComponentVariation(playerPed, 1) != 0)
        {
            UnequipMask();
        }
    }

    private void ToggleHat()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed == null)
            return;
        if (GetPropVariation(playerPed, 0) == -1 && hatComponent != -1)
        {
            EquipHat();
        }
        else if (GetPropVariation(playerPed, 0) != -1)
        {
            UnequipHat();
        }
    }

    private void ToggleGlasses()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed == null)
            return;

        if (GetPropVariation(playerPed, 1) == -1 && glassesComponent != -1)
        {
            EquipGlasses();
        }
        else if (GetPropVariation(playerPed, 1) != -1)
        {
            UnequipGlasses();
        }
    }
    private void UnequipMask()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed == null)
            return;
        string animDict = "missfbi4";
        string animName = "takeoff_mask";

        if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
        {
            Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict);
        }
        Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, animDict, animName, 8f, 8f, 1200, 49, 0, 0, 0, 0);
        clothingAnim = animName;
        clothingDict = animDict;
        Wait(800);

        maskComponent = GetComponentVariation(playerPed, 1);
        maskTexture = GetComponentTextureVariation(playerPed, 1);

        SetComponentVariation(playerPed, 1, 0, 0, 0);
    }
    private void EquipMask()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed == null)
            return;

        string animDict = playerPed.IsInVehicle() ? "mp_masks@standard_car@ds@" : "mp_masks@on_foot";
        string animName = "put_on_mask";

        if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
            Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict);

        Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, animDict, animName, 8f, 8f, 750, 49, 0, 0, 0, 0);
        clothingAnim = animName;
        clothingDict = animDict;
        Wait(350);

        SetComponentVariation(playerPed, 1, maskComponent, maskTexture, 0);
    }
    private void UnequipHat()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed == null)
            return;
        string animDict = "veh@common@fp_helmet@";
        string animName = "take_off_helmet_stand";

        if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
            Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict);

        Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, animDict, animName, 8f, 8f, 1200, 49, 0, 0, 0, 0);
        clothingAnim = animName;
        clothingDict = animDict;
        Wait(1200);

        hatComponent = GetPropVariation(playerPed, 0);
        hatTexture = GetPropTextureVariation(playerPed, 0);

        Function.Call(GTA.Native.Hash.CLEAR_PED_PROP, playerPed, 0);
    }
    private void EquipHat()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed == null)
            return;
        string animDict = "missheistdockssetup1hardhat@";
        string animName = "put_on_hat";

        if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
            Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict);

        Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, animDict, animName, 8f, 8f, 1700, 49, 0, 0, 0, 0);
        clothingAnim = animName;
        clothingDict = animDict;
        Wait(1700);

        SetPropVariation(playerPed, 0, hatComponent, hatTexture, true);
    }
    private void UnequipGlasses()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed == null)
            return;
        string animDict = "clothingspecs";
        string animName = "take_off";

        if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
            Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict);

        Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, animDict, animName, 8f, 8f, 1500, 49, 0, 0, 0, 0);
        clothingAnim = animName;
        clothingDict = animDict;
        Wait(1300);

        glassesComponent = GetPropVariation(playerPed, 1);
        glassesTexture = GetPropTextureVariation(playerPed, 1);

        Function.Call(GTA.Native.Hash.CLEAR_PED_PROP, playerPed, 1);
    }
    private void EquipGlasses()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed == null)
            return;

        string animDict = "clothingspecs";
        string animName = "try_glasses_negative_b";

        if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
            Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict);

        Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, animDict, animName, 8f, 8f, 1700, 49, 0, 0, 0, 0);
        clothingAnim = animName;
        clothingDict = animDict;
        Wait(1500);

        SetPropVariation(playerPed, 1, glassesComponent, glassesTexture, true);
    }
    private int GetPropVariation(Ped playerPed, int componentId)
    {
        return Function.Call<int>(GTA.Native.Hash.GET_PED_PROP_INDEX, playerPed, componentId);
    }
    private int GetPropTextureVariation(Ped playerPed, int componentId)
    {
        return Function.Call<int>(GTA.Native.Hash.GET_PED_PROP_TEXTURE_INDEX, playerPed, componentId);
    }
    private void SetPropVariation(Ped playerPed, int componentId, int drawableId, int textureId, bool attach)
    {
        Function.Call(GTA.Native.Hash.SET_PED_PROP_INDEX, playerPed, componentId, drawableId, textureId, attach);
    }
    private int GetComponentVariation(Ped playerPed, int componentId)
    {
        return Function.Call<int>(GTA.Native.Hash.GET_PED_DRAWABLE_VARIATION, playerPed, componentId);
    }
    private int GetComponentTextureVariation(Ped playerPed, int componentId)
    {
        return Function.Call<int>(GTA.Native.Hash.GET_PED_TEXTURE_VARIATION, playerPed, componentId);
    }
    private void SetComponentVariation(Ped playerPed, int componentId, int drawableId, int textureId, int paletteId)
    {
        Function.Call(GTA.Native.Hash.SET_PED_COMPONENT_VARIATION, playerPed, componentId, drawableId, textureId, paletteId);
    }
    private bool isAnimPlaying(string anim, string dict)
    {
        if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, dict)) return false;
        return Function.Call<bool>(GTA.Native.Hash.IS_ENTITY_PLAYING_ANIM, Game.Player.Character, dict, anim, 3);
    }
}