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

public class EquipRunningPrimaryWeapon : Script
{
    public ScriptSettings Config;
    private List<WeaponHash> weaponList = new List<WeaponHash>();

    private int equipDelay = 1000;
    private int lastSprintedTime = 0;

    public EquipRunningPrimaryWeapon()
    {
        Tick += OnTick;
        LoadIniFile("scripts//Heat.ini");
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
            string[] hashes = Config.GetAllValues("HASHES", "HASH");

            foreach (var hash in hashes)
            {
                if (uint.TryParse(hash.Trim(), out uint weaponHash))
                {
                    weaponList.Add((WeaponHash)weaponHash);
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
            writer.WriteLine("[SETTINGS]");
            writer.WriteLine("EquipDelay=1000");
        }
        UI.Notify("~g~Default configuration created: " + iniFile);
    }
    private void OnTick(object sender, EventArgs e)
    {
        if (Game.Player.Character.IsInVehicle() || !Game.Player.Character.IsAlive)
            return;

        bool isSprinting = IsPlayerSprinting();

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
        }
    }

    private bool IsPlayerSprinting()
    {
        // Check if the player is sprinting (not just running)
        return Game.Player.Character.IsSprinting;
    }

    private bool IsPrimaryWeapon(WeaponHash weaponHash)
    {
        return weaponList.Contains(weaponHash);
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
}