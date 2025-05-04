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

public class RelaxedDrivingStyle : Script
{
    private Keys ToggleDrivingStyleKey = HeatSettings.toggleDrivingStyleKey;
    private bool isDrivingStyleOn = false;

    private string vehAnim = "sit";
    private string vehDict = "";

    public RelaxedDrivingStyle()
    {
        if (Game.Player.Character.IsInVehicle()) LoadVehAnims();

        Tick += OnTick;
        KeyUp += ToggleDrivingStyle;
    }
    private void PlaySitAnimation()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed == null || !playerPed.IsAlive || playerPed.CurrentVehicle.Speed >= 27) { isDrivingStyleOn = false;  return; }

        Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM, playerPed, vehDict, vehAnim, 1.0f, 1.0f, -1, 42, 1.0f, false, false, false);
        playerPed.CurrentVehicle.RollDownWindow(0);
        isDrivingStyleOn = true;
    }
    private void StopSitAnimation()
    {
        Ped playerPed = Game.Player.Character;
        if (playerPed == null || !playerPed.IsAlive || string.IsNullOrEmpty(vehDict) || string.IsNullOrEmpty(vehAnim)) return;

        Function.Call(GTA.Native.Hash.CLEAR_PED_SECONDARY_TASK, playerPed);
        Function.Call(GTA.Native.Hash.STOP_ANIM_TASK, playerPed, vehDict, vehAnim, 1);
        isDrivingStyleOn = false;
    }
    private void OnTick(object sender, EventArgs e)
    {
        Ped playerPed = Game.Player.Character;
        Vehicle vehicle = playerPed.CurrentVehicle;

        // Reset toggle bool when player dies or switches char
        if (playerPed == null || !playerPed.IsAlive) { isDrivingStyleOn = false; return; }

        // If player playing animation, stop if player leaves vehicle.
        if (!playerPed.IsInVehicle() && isAnimPlaying(vehAnim, vehDict)) { StopSitAnimation(); return; }

        // Prevent tick from running if player is not in vehicle
        if (!playerPed.IsInVehicle()) { isDrivingStyleOn = false;  return; }

        LoadVehAnims();

        if (isAnimPlaying(vehAnim, vehDict))
        {
            // need to add steer_lean anims when player turning
            if (!isDrivingStyleOn || vehicle.Speed >= 27 || Game.IsControlJustPressed(0, GTA.Control.VehicleHorn) || playerPed.IsDoingDriveBy) { StopSitAnimation(); }
        }
        else if (isDrivingStyleOn) { PlaySitAnimation();  }
    }
    private void LoadVehAnims()
    {
        string vehicleType = GetVehicleType(Game.Player.Character.CurrentVehicle.Model.Hash);

        GetVehAnimDict(vehicleType);

        Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, vehDict);
        while (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, vehDict)) Wait(10);
    }
    private string GetVehicleType(int modelHash)
    {
        string modelName = Function.Call<string>(GTA.Native.Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, modelHash).ToLower();

        if (modelName.Contains("low")) return "low";
        else if (modelName.Contains("high")) return "high";
        else if (modelName.Contains("suv") || modelName.Contains("truck")) return "suv";
        else if (modelName.Contains("bike") || modelName.Contains("motorcycle")) return "bike";

        return "car"; // Default type
    }
    private void GetVehAnimDict(string vehicleType)
    {
        vehAnim = "sit";

        if (vehicleType.Contains("_low"))
        {
            vehAnim = "sit_low";
            vehicleType = vehicleType.Replace("_low", "");
        }
        else if (vehicleType.Contains("_high"))
        {
            vehAnim = "sit_high";
            vehicleType = vehicleType.Replace("_high", "");
        }

        if (vehicleType == "car")
        {
            vehDict = "anim@veh@lowrider@low@front_ds@arm@base";
        }
        else
        {
            vehDict = $"anim@veh@sit_variations@{vehicleType}@front@idle_a";
        }
    }
    private void ToggleDrivingStyle(object sender, KeyEventArgs e)
    {
        /*
        if (e.KeyCode == Keys.Y)
        {
            //UI.Notify($"{Function.Call<float>(GTA.Native.Hash.GET_ENTITY_ANIM_CURRENT_TIME, Game.Player.Character, vehDict, vehAnim)}");
            //UI.Notify($"{isAnimPlaying(vehAnim, vehDict)}");
            StopSitAnimation();
        }
        if (e.KeyCode == Keys.B)
        {
            Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS_IMMEDIATELY, Game.Player.Character);
            UI.Notify($"clear anim");
        }*/
        if (!Game.Player.Character.IsInVehicle()) return;

        if (e.KeyCode == ToggleDrivingStyleKey)
        {
            isDrivingStyleOn = !isDrivingStyleOn;
        }
    }
    private bool isAnimPlaying(string anim, string dict)
    {
        if (!Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, dict)) return false;
        return Function.Call<bool>(GTA.Native.Hash.IS_ENTITY_PLAYING_ANIM, Game.Player.Character, dict, anim, 3);
    }
}