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

public class Dryfire : Script
{
    private List<WeaponHash> weaponList = HeatSettings.weaponList;
    private List<WeaponHash> exemptionList = HeatSettings.exemptionList;

    public Dryfire()
    {
        Tick += OnTick;
    }
    private void OnTick(object sender, EventArgs e)
    {
        // Doesn't work for vehicles

        // Get the player and their current weapon
        Ped player = Game.Player.Character;
        Weapon currentWeapon = player.Weapons.Current; 
        bool weaponCheck = currentWeapon == null || Function.Call<bool>(GTA.Native.Hash.IS_PED_ARMED, player, 4) || currentWeapon.Hash == WeaponHash.Unarmed || IsWeaponExempted(currentWeapon.Hash) || IsThrowable(currentWeapon.Hash);

        // Check if the player is holding a weapon
        if (weaponCheck && (currentWeapon.AmmoInClip != 1 || IsWeaponExempted(currentWeapon.Hash)) || IsThrowable(currentWeapon.Hash))
        {
            Function.Call(GTA.Native.Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.Attack, true);
            Function.Call(GTA.Native.Hash.ENABLE_CONTROL_ACTION, 0, (int)GTA.Control.Attack2, true);
            return;
        }

        Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.Attack, false); // keys are inconsistent
        Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.Attack2, false);

        if (!player.IsReloading && (Game.Player.IsAiming || player.IsAimingFromCover) && (Game.IsControlJustReleased(0, GTA.Control.Attack) || Game.IsControlJustReleased(0, GTA.Control.Attack2)))
        {
            if (IsPrimaryWeapon(player.Weapons.Current.Hash)) PlaySound("weap_dryfire_rifle.wav");
            else PlaySound("weap_dryfire_smg.wav");
        }
    }

    private bool IsPrimaryWeapon(WeaponHash weaponHash)
    {
        return weaponList.Contains(weaponHash);
    }
    private void PlaySound(string fileName)
    {
        string filePath = $"scripts//Heat//{fileName}";
        if (!File.Exists(filePath)) return;

        SoundPlayer sfxPlayer = new SoundPlayer(filePath);
        sfxPlayer.Play(); // Use PlaySync() to wait until it finishes
    }
    private bool IsThrowable(WeaponHash weapon)
    {
        return weapon == WeaponHash.Grenade ||
               weapon == WeaponHash.Ball ||
               weapon == WeaponHash.BZGas ||
               weapon == WeaponHash.Flare ||
               weapon == WeaponHash.PipeBomb ||
               weapon == WeaponHash.ProximityMine ||
               weapon == WeaponHash.SmokeGrenade ||
               weapon == WeaponHash.Snowball ||
               weapon == WeaponHash.Molotov ||
               weapon == WeaponHash.StickyBomb ||
               weapon == WeaponHash.FireExtinguisher;
    }
    private bool IsWeaponExempted(WeaponHash weapon)
    {
        return exemptionList.Contains(weapon);
    }
}