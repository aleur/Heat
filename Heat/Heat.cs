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

public class Heat : Script
{
    public ScriptSettings Config;
    private List<WeaponHash> weaponList = new List<WeaponHash>();
    private List<int[]> bagList = new List<int[]>();
    private string currentAnim, currentDict;

    private bool IsReloadDisabled = false;
    private int equipDelay = 1000;
    private int lastSprintedTime = 0;
    private int hatComponent = -1, hatTexture = -1, maskComponent = 0, maskTexture = -1, glassesComponent = -1, glassesTexture = -1;

    private Keys equipMaskKey = Keys.Oemcomma, equipHatKey = Keys.OemPeriod, equipGlassesKey = Keys.OemQuestion;

    public Heat()
    {
        Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.Attack, true); //doesn't do shit
        Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.Attack2, true);
        Tick += BagSystem;
        Tick += DisableAutoReload;

        LoadIniFile("scripts//Heat//Heat.ini");

        KeyUp += ToggleClothing;
        KeyUp += ManualReload;
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
    private void BagSystem(object sender, EventArgs e)
    {
        Ped playerPed = Game.Player.Character;
        int bagComponent = GetComponentVariation(playerPed, 5);

        int[] equippedBag = bagList.FirstOrDefault(bag => bag[0] == bagComponent || bag[1] == bagComponent);
        if (playerPed.IsInVehicle() || !playerPed.IsAlive)
            return;

        WeaponHash currentWeapon = Game.Player.Character.Weapons.Current.Hash;
        bool isSprinting = IsPlayerSprinting();
        bool isBagEquipped = IsBagEquipped(Game.Player.Character);

        if (!HasPrimaryWeapons(playerPed) || isAnimPlaying())
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
    private void DisableAutoReload(object sender, EventArgs e)
    {
        // Doesn't work for vehicles

        // Get the player and their current weapon
        Ped player = Game.Player.Character;
        Weapon currentWeapon = player.Weapons.Current;

        // Check if the player is holding a weapon
        if (currentWeapon == null || currentWeapon.Hash == WeaponHash.Unarmed)
            return;

        if (currentWeapon.AmmoInClip == 1)
        {
            Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.Attack, false); // keys are inconsistent
            Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.Attack2, false);
            IsReloadDisabled = true;

            if (Game.IsControlPressed(0, GTA.Control.Aim) && (Game.IsControlJustReleased(0, GTA.Control.Attack) || Game.IsControlJustReleased(0, GTA.Control.Attack2)))
            {
                if (IsPrimaryWeapon(player.Weapons.Current.Hash)) PlaySound("weap_dryfire_rifle.wav");
                else PlaySound("weap_dryfire_smg.wav");
            }
        } else
        {
            IsReloadDisabled = false;
        }
    }
    private void ManualReload(object sender, KeyEventArgs e)
    {
        Ped player = Game.Player.Character;
        Weapon currentWeapon = player.Weapons.Current;

        if (currentWeapon.AmmoInClip != 1) return;
        if (e.KeyCode == (Keys)GTA.Control.Reload)
        {
            Function.Call(GTA.Native.Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.Attack, true);// THIS SHIT DOESN TDO AW IAW HRTIAW
            Function.Call(GTA.Native.Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.Attack2, true);
            IsReloadDisabled = false;
        }
    }

    private void ToggleClothing(object sender, KeyEventArgs e)
    {
        if (isAnimPlaying()) return;

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
    private void PlaySound(string fileName)
    {
        string filePath = $"scripts//Heat//{fileName}";
        if (!File.Exists(filePath)) return;

        SoundPlayer sfxPlayer = new SoundPlayer(filePath);
        sfxPlayer.Play(); // Use PlaySync() to wait until it finishes
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
        if (playerPed == null)
            return;
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
        currentAnim = animName;
        currentDict = animDict;
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
        currentAnim = animName;
        currentDict = animDict;
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
        currentAnim = animName;
        currentDict = animDict;
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
        currentAnim = animName;
        currentDict = animDict;
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
        currentAnim = animName;
        currentDict = animDict;
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

    private bool IsBagEquipped(Ped playerPed)
    {
        return Function.Call<int>(GTA.Native.Hash.GET_PED_DRAWABLE_VARIATION, playerPed, 5) != 0;
    }
    private bool HasConfigBagEquipped(Ped playerPed)
    {
        return bagList.Any(bag => bag[0] == Function.Call<int>(GTA.Native.Hash.GET_PED_DRAWABLE_VARIATION, playerPed, 5)) || bagList.Any(bag => bag[1] == Function.Call<int>(GTA.Native.Hash.GET_PED_DRAWABLE_VARIATION, playerPed, 5));
    }
    
    private bool isAnimPlaying()
    {
        return Function.Call<bool>(GTA.Native.Hash.IS_ENTITY_PLAYING_ANIM, Game.Player.Character, currentDict, currentAnim, 3);
    }
}