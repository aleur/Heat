using GTA;
using GTA.Math;
using GTA.Native;
using GTA.UI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TrailCarry
{
    internal class Utils
    {
        public static int Shootdodge;
        public static Type DodgeType;
        public static FieldInfo DodgeField;

        public static bool DualWielding;
        public static Type DualWieldType;
        public static FieldInfo DualWieldField;

        public static Type VW_MainType;
        public static Type VW_ExtType;
        public static FieldInfo VisibleWeaponsField1;
        public static FieldInfo VisibleWeaponsField2;
        public static FieldInfo VisibleWeaponsField3;
        public static FieldInfo VisibleWeaponsField4;
        public static FieldInfo VisibleWeaponsField_1;
        public static FieldInfo VisibleWeaponsField_2;
        public static FieldInfo VisibleWeaponsField_3;
        public static FieldInfo VisibleWeaponsField_4;
        public static MethodInfo VisibleWeaponsCreator;
        public static Dictionary<WeaponHash, Prop> VisibleWeaponsDict = new Dictionary<WeaponHash, Prop>();

        private static bool conflictCheckDone = false;
        private static DateTime? checkTimeout = null;

        public static bool DoesExists(Entity entity)
        {
            return Function.Call<bool>(Hash.DOES_ENTITY_EXIST, entity);
        }

        public static void GetAttachments(Ped ped, WeaponHash sourceWpn, Entity targetWpnObj)
        {
            foreach (var component in WeaponComponent.GetAllHashes()) //SHVDN NativeMemory saves the day again
            {
                if (ped != Game.Player.Character && component == WeaponComponentHash.FlashlightLight)
                    continue;

                if (Function.Call<bool>(Hash.HAS_PED_GOT_WEAPON_COMPONENT, ped, sourceWpn, component))
                {
                    //int tintCompID = Function.Call<int>(Hash.GET_WEAPON_OBJECT_COMPONENT_TINT_INDEX, sourceWpnObj, component);
                    int tintCompID = Function.Call<int>(Hash.GET_PED_WEAPON_COMPONENT_TINT_INDEX, ped, sourceWpn, component);
                    Function.Call(Hash.GIVE_WEAPON_COMPONENT_TO_WEAPON_OBJECT, targetWpnObj, component);
                    Function.Call(Hash.SET_WEAPON_OBJECT_COMPONENT_TINT_INDEX, targetWpnObj, component, tintCompID);
                }
            }
            //int tintID = Function.Call<int>(Hash.GET_WEAPON_OBJECT_TINT_INDEX, sourceWpnObj);
            int tintID = Function.Call<int>(Hash.GET_PED_WEAPON_TINT_INDEX, ped, sourceWpn);
            Function.Call(Hash.SET_WEAPON_OBJECT_TINT_INDEX, targetWpnObj, tintID);
            int camoID = Function.Call<int>(Hash.GET_PED_WEAPON_CAMO_INDEX, ped, sourceWpn);
            Function.Call(Hash.SET_WEAPON_OBJECT_CAMO_INDEX, targetWpnObj, camoID);
        }

        public static void SetIkTarget(Ped ped)
        {
            Vector3 target = GameplayCamera.Position + (GameplayCamera.ForwardVector * 9999f);
            if (Game.Player.Character.IsShooting)
                target += new Vector3(0f, 0f, 5f);
            Function.Call(Hash.SET_IK_TARGET, ped, 4, null, -1, target.X, target.Y, target.Z, 0, -8, 8);
            Function.Call(Hash.SET_IK_TARGET, ped, 1, null, -1, target.X, target.Y, target.Z, 0, -8, 8);
        }

        public static void SetIK(bool on_off, Ped ped)
        {
            Function.Call(Hash.SET_PED_CAN_ARM_IK, ped, on_off);
            Function.Call(Hash.SET_PED_CAN_HEAD_IK, ped, on_off);
        }

        public static bool PlayerChangingGun()
        {
            return Function.Call<bool>(Hash.IS_PED_SWITCHING_WEAPON, HandheldWeapons.MC);
        }

        public static void CheckConflict()
        {
            if (DodgeField != null)
                Shootdodge = (int)DodgeField.GetValue(null);
            else
                Shootdodge = 0;

            if (DualWieldField != null)
                DualWielding = (bool)DualWieldField.GetValue(null);
            else
                DualWielding = false;

            // Clear dictionary before updating
            VisibleWeaponsDict.Clear();

            // Add weapon-Prop pairs directly into the dictionary
            if (VisibleWeaponsField1 != null && VisibleWeaponsField_1 != null)
                VisibleWeaponsDict[(WeaponHash)VisibleWeaponsField1.GetValue(null)] = (Prop)VisibleWeaponsField_1.GetValue(null);

            if (VisibleWeaponsField2 != null && VisibleWeaponsField_2 != null)
                VisibleWeaponsDict[(WeaponHash)VisibleWeaponsField2.GetValue(null)] = (Prop)VisibleWeaponsField_2.GetValue(null);

            if (VisibleWeaponsField3 != null && VisibleWeaponsField_3 != null)
                VisibleWeaponsDict[(WeaponHash)VisibleWeaponsField3.GetValue(null)] = (Prop)VisibleWeaponsField_3.GetValue(null);

            if (VisibleWeaponsField4 != null && VisibleWeaponsField_4 != null)
                VisibleWeaponsDict[(WeaponHash)VisibleWeaponsField4.GetValue(null)] = (Prop)VisibleWeaponsField_4.GetValue(null);
        }

        public static void ConflictGetter()
        {
            if (conflictCheckDone)
                return;

            if (checkTimeout == null)
                checkTimeout = DateTime.Now.AddMilliseconds(10000); // Set timeout for 7 seconds

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            bool allTypesFound = true;

            foreach (var assembly in assemblies)
            {
                if (DodgeType == null)
                {
                    DodgeType = assembly.GetType("Shootdodge.Main");
                    if (DodgeType != null)
                        DodgeField = DodgeType.GetField("ScriptStatus", BindingFlags.Static | BindingFlags.Public);
                }
                if (DualWieldType == null)
                {
                    DualWieldType = assembly.GetType("DualWield.Main");
                    if (DualWieldType != null)
                        DualWieldField = DualWieldType.GetField("DualWielding", BindingFlags.Static | BindingFlags.Public);
                }
                if (VW_MainType == null)
                {
                    VW_MainType = assembly.GetType("VisibleWpn.Main");
                    if (VW_MainType != null)
                    {
                        VisibleWeaponsField1 = VW_MainType.GetField("backWpn", BindingFlags.Static | BindingFlags.Public);
                        VisibleWeaponsField2 = VW_MainType.GetField("rightHip", BindingFlags.Static | BindingFlags.Public);
                        VisibleWeaponsField3 = VW_MainType.GetField("frontWpn", BindingFlags.Static | BindingFlags.Public);
                        VisibleWeaponsField4 = VW_MainType.GetField("leftHip", BindingFlags.Static | BindingFlags.Public);
                        VisibleWeaponsField_1 = VW_MainType.GetField("WpnOnBack", BindingFlags.Static | BindingFlags.Public);
                        VisibleWeaponsField_2 = VW_MainType.GetField("GunHipR", BindingFlags.Static | BindingFlags.Public);
                        VisibleWeaponsField_3 = VW_MainType.GetField("WpnOnFront", BindingFlags.Static | BindingFlags.Public);
                        VisibleWeaponsField_4 = VW_MainType.GetField("ItemHipL", BindingFlags.Static | BindingFlags.Public);
                    }
                }
                if (VW_ExtType == null)
                {
                    VW_ExtType = assembly.GetType("VisibleWpn.External");
                    if (VW_ExtType != null)
                        VisibleWeaponsCreator = VW_ExtType.GetMethod("Creator", BindingFlags.Static | BindingFlags.Public);
                }
            }

            // Check if all types have been successfully found
            if (DodgeType == null || DualWieldType == null || VW_MainType == null || VW_ExtType == null)
                allTypesFound = false;

            if (allTypesFound)
            {
                conflictCheckDone = true; // Stop checking in future ticks
                checkTimeout = null; // Reset timeout
                return;
            }

            // If time has exceeded 7 seconds, stop checking
            if (DateTime.Now >= checkTimeout)
            {
                if (VW_MainType != null && VW_ExtType == null)
                {
                    Notification.PostTicker("~r~TrailCarry WARNING: ~n~~w~You are using an older incompatible version of Visible Weapons. Please update for proper synchronization with Trail Carry.", true);
                }
                conflictCheckDone = true;
                checkTimeout = null;
                return;
            }
        }


        public static void VisibleWeaponsConflictSolver() // for OnTick prop cleaning
        {
            if (VisibleWeaponsDict.Count < 1)
                return;

            foreach (var entry in VisibleWeaponsDict)
            {
                WeaponHash weapon = entry.Key;
                Prop weaponProp = entry.Value;

                if (DoesExists(weaponProp) && DoesExists(HandheldWeapons.gunCarried) && HandheldWeapons.lastWeapon != null && weapon == HandheldWeapons.lastWeapon.Hash)
                {
                    weaponProp.Delete();
                }
            }
        }

        public static void VisibleWeaponsConflictSolver(WeaponHash toDelete) // for specific weapon (pistol/triggerwpns)
        {
            if (VisibleWeaponsDict.Count < 1)
                return;

            foreach (var entry in VisibleWeaponsDict)
            {
                WeaponHash weapon = entry.Key;
                Prop weaponProp = entry.Value;

                if (DoesExists(weaponProp) && DoesExists(HandheldWeapons.gunCarried) && weapon == toDelete)
                {
                    weaponProp.Delete();
                }
            }
        }

        public static void VisibleWeaponsConflictSolver(Weapon toMake) // for specific weapon (pistol/triggerwpns)
        {
            if (VisibleWeaponsCreator != null)
            {
                if (toMake != null)
                {
                    var methodDelegate = (Action<Weapon>)Delegate.CreateDelegate(typeof(Action<Weapon>), VisibleWeaponsCreator);
                    methodDelegate(toMake);
                }
                //else Notification.PostTicker("Input Weapon = null , Visible Weapons", true);
            }
        }
        public static bool IsBagEquipped(Ped playerPed)
        {
            if (Game.Player.Character.Model == new Model("player_zero") || Game.Player.Character.Model == new Model("player_one") || Game.Player.Character.Model == new Model("player_two")) return Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, playerPed, 9) != 0;
            return Function.Call<int>(Hash.GET_PED_DRAWABLE_VARIATION, playerPed, 5) != 0;
        }
    }
}
