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

public static class HeatSettings
{
    public static ScriptSettings Config { get; private set; }
    public static List<WeaponHash> weaponList { get; private set; } = new List<WeaponHash>();
    public static List<WeaponHash> exemptionList { get; private set; } = new List<WeaponHash>();
    public static List<int[]> bagList { get; private set; } = new List<int[]>();
    public static string isDryfireEnabled { get; private set; } = "True"; 
    public static string isBagSystemEnabled { get; private set; } = "True"; 
    public static string isDrivingStyleEnabled { get; private set; } = "True";
    public static Keys equipMaskKey { get; private set; } = Keys.Oemcomma; 
    public static Keys equipHatKey { get; private set; } = Keys.OemPeriod; 
    public static Keys equipGlassesKey { get; private set; } = Keys.OemQuestion; 
    public static Keys toggleDrivingStyleKey { get; private set; } = Keys.G;
    static HeatSettings()
    {
    }

    public static void LoadIniFile(string iniFile)
    {
        if (!File.Exists(iniFile))
        {
            CreateDefaultConfig(iniFile);
            LoadIniFile(iniFile);
        }
        try
        {
            Config = ScriptSettings.Load(iniFile);
            isDrivingStyleEnabled = Config.GetValue("SETTINGS", "RelaxedDrivingStyle", isDrivingStyleEnabled);
            isBagSystemEnabled = Config.GetValue("SETTINGS", "EnableBagSystem", isBagSystemEnabled);
            isDryfireEnabled = Config.GetValue("SETTINGS", "EnableDryfire", isDryfireEnabled);
            equipMaskKey = Config.GetValue("SETTINGS", "EquipMaskKey", equipMaskKey);
            equipHatKey = Config.GetValue("SETTINGS", "EquipHatKey", equipHatKey); 
            equipGlassesKey = Config.GetValue("SETTINGS", "EquipGlassesKey", equipGlassesKey);
            toggleDrivingStyleKey = Config.GetValue("SETTINGS", "ToggleDrivingStyleKey", toggleDrivingStyleKey);

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

            string[] exemptions = Config.GetAllValues("EXEMPTIONS", "HASH");
            foreach (var exemption in exemptions)
            {
                if (uint.TryParse(exemption.Trim(), out uint weaponHash))
                {
                    exemptionList.Add((WeaponHash)weaponHash);
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

    private static void CreateDefaultConfig(string iniFile)
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
            writer.WriteLine($"RelaxedDrivingStyle={isDrivingStyleEnabled}");
            writer.WriteLine($"EnableBagSystem={isBagSystemEnabled}");
            writer.WriteLine($"EnableDryfire={isDryfireEnabled}");
            writer.WriteLine($"EquipMaskKey={equipMaskKey}");
            writer.WriteLine($"EquipHatKey={equipHatKey}");
            writer.WriteLine($"EquipGlassesKey={equipGlassesKey}");
            writer.WriteLine($"ToggleDrivingStyleKey={toggleDrivingStyleKey}");
            writer.WriteLine("[BAGS]");
            writer.WriteLine("BAG=41,40");
            writer.WriteLine("BAG=45,44");
            writer.WriteLine("BAG=82,81");
            writer.WriteLine("BAG=86,85");
            writer.WriteLine("[EXEMPTIONS]");
            writer.WriteLine("HASH=2828843422");
            writer.WriteLine("HASH=911657153");
            writer.WriteLine("HASH=125959754");
            writer.WriteLine("HASH=1198879012");
            writer.WriteLine("HASH=1834241177");
            writer.WriteLine("HASH=2982836145"); 
            writer.WriteLine("HASH=2939590305");
            writer.WriteLine("HASH=3696079510");
        }
        UI.Notify("~r~Heat~s~ : ~g~Default Config created");
    }
}