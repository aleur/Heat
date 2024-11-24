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

public class Heat : Script
{
    public ScriptSettings Config;
    private List<WeaponHash> weaponList = new List<WeaponHash>();
    private List<int[]> bagList = new List<int[]>();
    private string currentAnim, currentDict;

    private int equipDelay = 1000;
    private int lastSprintedTime = 0;
    private int hatComponent = -1, hatTexture = -1, maskComponent = 0, maskTexture = -1, glassesComponent = -1, glassesTexture = -1;

    private Keys equipMaskKey = Keys.Oemcomma, equipHatKey = Keys.OemPeriod, equipGlassesKey = Keys.OemQuestion;

    public Heat()
    {
        Tick += OnTick;
        LoadIniFile("scripts//Heat.ini");
        KeyUp += OnKeyUp;
    }

    public void LoadIniFile(string iniFile)
    {
        if (!File.Exists(iniFile))
        {
            CreateDefaultConfig(iniFile);
        }
        try
        {
            Config = ScriptSettings.Load(iniFile);
            equipDelay = Config.GetValue("SETTINGS", "EquipDelay", equipDelay);
            equipMaskKey = Config.GetValue("SETTINGS", "EquipMaskKey", equipMaskKey);
            equipHatKey = Config.GetValue("SETTINGS", "EquipHatKey", equipHatKey);
            equipGlassesKey = Config.GetValue("SETTINGS", "EquipGlassesKey", equipGlassesKey);

            string[] hashes = Config.GetAllValues("HASHES", "HASH");

            foreach (var hash in hashes)
            {
                if (uint.TryParse(hash.Trim(), out uint weaponHash))
                {
                    weaponList.Add((WeaponHash)weaponHash);
                }
            }

            string[] bags = Config.GetAllValues("BAGS", "BAG");
            foreach (var bag in bags)
            {
                string[] parts = bag.Split(',');
                if (int.TryParse(parts[0].Trim(), out int componentID) && int.TryParse(parts[1].Trim(), out int textureID))
                {
                    int[] bagID = { componentID, textureID };
                    bagList.Add(bagID);
                }
            }

            UI.Notify("~r~Heat~s~: Loaded " + weaponList.Count + " weapons from " + iniFile);
        }
        catch (Exception ex)
        {
            UI.Notify("~r~Heat Error~s~: " + iniFile + " resetting ini file");
            CreateDefaultConfig(iniFile);
        }
    }

    private void CreateDefaultConfig(string iniFile)
    {
        using (StreamWriter writer = new StreamWriter(iniFile))
        {
            writer.WriteLine("[HASHES]");
            writer.WriteLine("HASH=2508868239");
            writer.WriteLine("HASH=1141786504");
            writer.WriteLine("HASH=2484171525");
            writer.WriteLine("HASH=736523883");
            writer.WriteLine("HASH=487013001");
            writer.WriteLine("HASH=4024951519");
            writer.WriteLine("HASH=1432025498");
            writer.WriteLine("HASH=3800352039");
            writer.WriteLine("HASH=2640438543");
            writer.WriteLine("HASH=2828843422");
            writer.WriteLine("HASH=984333226");
            writer.WriteLine("HASH=94989220");
            writer.WriteLine("HASH=3220176749");
            writer.WriteLine("HASH=961495388");
            writer.WriteLine("HASH=2210333304");
            writer.WriteLine("HASH=4208062921");
            writer.WriteLine("HASH=2937143193");
            writer.WriteLine("HASH=3231910285");
            writer.WriteLine("HASH=2526821735");
            writer.WriteLine("HASH=2508868239");
            writer.WriteLine("HASH=2132975508");
            writer.WriteLine("HASH=2228681469");
            writer.WriteLine("HASH=3520460075");
            writer.WriteLine("HASH=1924557585");
            writer.WriteLine("HASH=2634544996");
            writer.WriteLine("HASH=2144741730");
            writer.WriteLine("HASH=3686625920");
            writer.WriteLine("HASH=1627465347");
            writer.WriteLine("HASH=1853742572");
            writer.WriteLine("HASH=177293209");
            writer.WriteLine("HASH=205991906");
            writer.WriteLine("HASH=100416529");
            writer.WriteLine("HASH=1785463520");
            writer.WriteLine("HASH=3342088282");
            writer.WriteLine("HASH=2138347493");
            writer.WriteLine("HASH=2982836145");
            writer.WriteLine("HASH=1672152130");
            writer.WriteLine("HASH=2726580491");
            writer.WriteLine("HASH=1305664598");
            writer.WriteLine("HASH=1119849093");
            writer.WriteLine("HASH=1834241177");
            writer.WriteLine("HASH=3347935668"); 
            writer.WriteLine("[SETTINGS]");
            writer.WriteLine($"EquipDelay={equipDelay}");
            writer.WriteLine($"EquipMaskKey={equipMaskKey}");
            writer.WriteLine($"EquipHatKey={equipHatKey}");
            writer.WriteLine($"EquipGlassesKey={equipGlassesKey}");
            writer.WriteLine("[BAGS]");
            writer.WriteLine("BAG=41,40");
            writer.WriteLine("BAG=45,44");
            writer.WriteLine("BAG=82,81");
            writer.WriteLine("BAG=86,85");
        }
        UI.Notify("~g~Default configuration created: " + iniFile);
    }
    private void OnTick(object sender, EventArgs e)
    {
        Ped playerPed = Game.Player.Character;
        int bagComponent = GetComponentVariation(playerPed, 5);

        int[] equippedBag = bagList.FirstOrDefault(bag => bag[0] == bagComponent || bag[1] == bagComponent);
        if (playerPed.IsInVehicle() || !playerPed.IsAlive) { return; }

        WeaponHash currentWeapon = Game.Player.Character.Weapons.Current.Hash;
        bool isSprinting = IsPlayerSprinting();
        bool isBagEquipped = IsBagEquipped(Game.Player.Character);

        /*
        if (isSprinting)
        {
            if (Game.GameTime - lastSprintedTime >= equipDelay)
            {
                WeaponHash currentWeapon = Game.Player.Character.Weapons.Current.Hash;
                if (!IsPrimaryWeapon(currentWeapon))
                {
                    EquipAppropriateWeapon();
                    lastSprintedTime = Game.GameTime;
                }
            }
        }; 
        */
        if (HasPrimaryWeapons(playerPed))
        {
            if (isBagEquipped)
            {
                if (HasBagEquipped(playerPed))
                {
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
            }
            else
            {
                if (currentWeapon != weaponList.FirstOrDefault(weaponHash => weaponHash == currentWeapon))
                {
                    // If player has primary weapon, but no bag, equip primary weapon.
                    EquipAppropriateWeapon();
                }
            }
        }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
        if (!isAnimPlaying())
        {
            if (e.KeyCode == equipMaskKey)
            {
                ToggleMask();
            }
            else if (e.KeyCode == equipHatKey)
            {
                ToggleHat();
            }
            else if (e.KeyCode == equipGlassesKey)
            {
                ToggleGlasses();
            }
        }
    }

    private void ToggleMask()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed != null)
        {
            if (GetComponentVariation(playerPed, 1) == 0 && maskComponent != 0)
            {
                EquipMask();
            }
            else if(GetComponentVariation(playerPed, 1) != 0)
            {
                UnequipMask();
            }
        }
    }

    private void ToggleHat()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed != null)
        {
            if (GetPropVariation(playerPed, 0) == -1 && hatComponent != -1)
            {
                EquipHat();
            }
            else if (GetPropVariation(playerPed, 0) != -1)
            {
                UnequipHat();
            }
        }
    }

    private void ToggleGlasses()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed != null)
        {
            if (GetPropVariation(playerPed, 1) == -1 && glassesComponent != -1)
            {
                EquipGlasses();
            }
            else if (GetPropVariation(playerPed, 1) != -1)
            {
                UnequipGlasses();
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
    private void UnequipMask()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed != null)
        {
            string animDict = "missfbi4";
            string animName = "takeoff_mask";

            if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
            {
                Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict);
            }
            Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, animDict, animName, 8f, 8f, 1200, 49, 0, 0, 0, 0);
            currentAnim = animName;
            currentDict = animDict;
            Wait(800);

            maskComponent = GetComponentVariation(playerPed, 1);
            maskTexture = GetComponentTextureVariation(playerPed, 1);

            SetComponentVariation(playerPed, 1, 0, 0, 0);
        }
    }
    private void EquipMask()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed != null)
        {
            string animDict = "mp_masks@on_foot";
            string animName = "put_on_mask";

            if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
            {
                Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict);
            }
            Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, animDict, animName, 8f, 8f, 750, 49, 0, 0, 0, 0);
            currentAnim = animName;
            currentDict = animDict;
            Wait(350);

            SetComponentVariation(playerPed, 1, maskComponent, maskTexture, 0);
        }
    }
    private void UnequipHat()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed != null)
        {
            string animDict = "veh@common@fp_helmet@";
            string animName = "take_off_helmet_stand";

            if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
            {
                Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict); 
            }
            Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, animDict, animName, 8f, 8f, 1200, 49, 0, 0, 0, 0);
            currentAnim = animName;
            currentDict = animDict;
            Wait(1200);

            hatComponent = GetPropVariation(playerPed, 0);
            hatTexture = GetPropTextureVariation(playerPed, 0);

            Function.Call(GTA.Native.Hash.CLEAR_PED_PROP, playerPed, 0);
        }
    }
    private void EquipHat()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed != null)
        {
            string animDict = "missheistdockssetup1hardhat@";
            string animName = "put_on_hat";

            if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
            {
                Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict);
            }
            Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, animDict, animName, 8f, 8f, 1700, 49, 0, 0, 0, 0);
            currentAnim = animName;
            currentDict = animDict;
            Wait(1700);

            SetPropVariation(playerPed, 0, hatComponent, hatTexture, true);
        }
    }
    private void UnequipGlasses()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed != null)
        {
            string animDict = "clothingspecs";
            string animName = "take_off";

            if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
            {
                Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict);
            }
            Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, animDict, animName, 8f, 8f, 1500, 49, 0, 0, 0, 0);
            currentAnim = animName;
            currentDict = animDict;
            Wait(1300);

            glassesComponent = GetPropVariation(playerPed, 1);
            glassesTexture = GetPropTextureVariation(playerPed, 1);

            Function.Call(GTA.Native.Hash.CLEAR_PED_PROP, playerPed, 1);
        }
    }
    private void EquipGlasses()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed != null)
        {
            string animDict = "clothingspecs";
            string animName = "try_glasses_negative_b";

            if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
            {
                Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict);
            }
            Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, animDict, animName, 8f, 8f, 1700, 49, 0, 0, 0, 0);
            currentAnim = animName;
            currentDict = animDict;
            Wait(1500);

            SetPropVariation(playerPed, 1, glassesComponent, glassesTexture, true);
        }
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

    private bool IsBagEquipped(Ped playerPed)
    {
        return Function.Call<int>(GTA.Native.Hash.GET_PED_DRAWABLE_VARIATION, playerPed, 5) != 0;
    }
    private bool HasBagEquipped(Ped playerPed)
    {
        return bagList.Any(bag => bag[0] == Function.Call<int>(GTA.Native.Hash.GET_PED_DRAWABLE_VARIATION, playerPed, 5)) || bagList.Any(bag => bag[1] == Function.Call<int>(GTA.Native.Hash.GET_PED_DRAWABLE_VARIATION, playerPed, 5));
    }
    
    private bool isAnimPlaying()
    {
        return Function.Call<bool>(GTA.Native.Hash.IS_ENTITY_PLAYING_ANIM, Game.Player.Character, currentDict, currentAnim, 3);
    }
}