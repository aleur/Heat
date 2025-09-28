using GTA;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace TrailCarry
{
    public class Config : Script
    {
        public static string GetPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
        public static readonly string folder = GetPath();
        public static readonly ScriptSettings iniFile = ScriptSettings.Load(Path.Combine(folder, "TrailCarry.ini"));
        public static readonly bool EnableOnWeaponSwap = iniFile.GetValue("Config", "EnableOnWeaponSwap", false, CultureInfo.InvariantCulture);
        public static readonly bool KeepWeaponAfterReload = iniFile.GetValue("Config", "KeepCarryAfterReload", false, CultureInfo.InvariantCulture);
        public static readonly bool DisableReloadActivation = iniFile.GetValue("Config", "DisableOnReloadActivation", false, CultureInfo.InvariantCulture);
        public static readonly bool ControlYourCarry = iniFile.GetValue("Config", "ControlYourCarry", false, CultureInfo.InvariantCulture);
        public static readonly bool FirstPersonDisable = iniFile.GetValue("Config", "DisableOnFPV", false, CultureInfo.InvariantCulture);
        public static readonly bool HideCarriedOnReload = iniFile.GetValue("Config", "HideCarriedOnReload", false, CultureInfo.InvariantCulture);
    }
}
