using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using static TrailCarry.Utils;

namespace TrailCarry
{
    public class HandheldWeapons : Script
    {
        public static Ped MC = Game.Player.Character;
        public static Prop gunCarried = null;
        private static readonly CrClipAsset handAnim = new CrClipAsset("combat@aim_variations@1h@gang", "aim_variation_d");
        private static readonly CrClipAsset handAnim2 = new CrClipAsset("combat@aim_variations@1h@hillbilly", "aim_variation_f");
        private static readonly ClipSet moveSet = new ClipSet("weapons@pistol_1h@gang");
        public static Weapon lastWeapon;
        private static DateTime heldKeyTimer;
        private static readonly int holdKeyLength = 800;
        private static bool keyHeld = false;
        private static bool wasReloading = false;
        public static readonly WeaponGroup[] TargetWpns = new WeaponGroup[3]
        {
            WeaponGroup.SMG,
            WeaponGroup.AssaultRifle,
            WeaponGroup.Shotgun
        };
        public static readonly WeaponGroup[] TriggerWpns = new WeaponGroup[1]
        {
            WeaponGroup.Pistol
        };

        public HandheldWeapons()
        {
            Tick += new EventHandler(OnTick);
        }

        private void OnTick(object sender, EventArgs e)
        {
            MC = Game.Player.Character;

            if (IsBagEquipped(MC)) return;

            ConflictGetter();
            CheckConflict();
            VisibleWeaponsConflictSolver();
            OptionalPassiveMode();

            //Animation
            Animation();

            WeaponSwapMode();

            //Prop Creation & Attaching 
            if (PlayerChangingGun() && !Config.ControlYourCarry && !Game.Player.IsAiming && !Config.EnableOnWeaponSwap)
            {
                Wait(100);
                if (TargetWpns.Contains(MC.Weapons.Current.Group))
                {
                    lastWeapon = MC.Weapons.Current;
                    //Notification.PostTicker("Weapon Recorded" + lastWeapon.DisplayName, true);
                }
                else if (TriggerWpns.Contains(MC.Weapons.Current.Group) && lastWeapon != null)
                {
                    if (DoesExists(gunCarried))
                        gunCarried.Delete();
                    float WpnLength = lastWeapon.Model.Dimensions.frontTopRight.X - lastWeapon.Model.Dimensions.rearBottomLeft.X;
                    bool longWpn = WpnLength > 0.60f;
                    if (lastWeapon.Group == WeaponGroup.SMG && !longWpn)
                        return;

                    VisibleWeaponsConflictSolver(lastWeapon);
                    //Notification.PostTicker("Weapon Creation Reached", true);
                    if (DoesExists(gunCarried))
                        lastWeapon = null;
                }

                if (DoesExists(gunCarried) && (lastWeapon == MC.Weapons.Current))
                {
                    gunCarried.Delete();
                    //Notification.PostTicker("Weapon Deleted" + lastWeapon.DisplayName, true);
                    lastWeapon = null;
                }

                if (DoesExists(gunCarried) && !TargetWpns.Concat(TriggerWpns).Contains(MC.Weapons.Current.Group))
                {
                    gunCarried.Delete();
                    VisibleWeaponsConflictSolver(MC.Weapons.Current);
                    VisibleWeaponsConflictSolver(lastWeapon);
                }
            }


            //First Person Disable
            if (DoesExists(gunCarried) && gunCarried.IsVisible && GameplayCamera.FollowPedCamViewMode == CamViewMode.FirstPerson && Config.FirstPersonDisable)
            {
                gunCarried.IsVisible = false;
                EndAnimation();
            }
            else if (DoesExists(gunCarried) && !gunCarried.IsVisible && GameplayCamera.FollowPedCamViewMode != CamViewMode.FirstPerson && !Config.ControlYourCarry)
                gunCarried.IsVisible = true;

            if (DoesExists(gunCarried) && TriggerWpns.Contains(MC.Weapons.Current.Group) && Game.IsControlJustPressed(Control.Reload) && !Config.KeepWeaponAfterReload && !Config.ControlYourCarry)
            {
                gunCarried.Delete();
                VisibleWeaponsConflictSolver(lastWeapon);
                lastWeapon = null;
            }

            //Empty Ammo Activation
            if (!DoesExists(gunCarried) && lastWeapon != null && TargetWpns.Contains(MC.Weapons.Current.Group) && MC.Weapons.Current.AmmoInClip == 0 && !PlayerChangingGun() && !MC.IsAiming && !Config.DisableReloadActivation && !Config.ControlYourCarry)
            {
                if (TriggerWpns.Contains(MC.Weapons.Current.Group))
                    return;
                EmptyAmmoInitializer(MC);
            }
            if (DoesExists(gunCarried) && TriggerWpns.Contains(MC.Weapons.Current.Group) && MC.Weapons.Current.AmmoInClip == 0 && !PlayerChangingGun()
                && !Config.KeepWeaponAfterReload)
            {
                if (Config.ControlYourCarry)
                    return;

                if (!DualWielding)
                    SetIK(false, MC);

                EndAnimation();
                gunCarried.Delete();
                VisibleWeaponsConflictSolver(lastWeapon);
                lastWeapon = null;
            }

            //Hidden on Reload
            if (DoesExists(gunCarried) && gunCarried.IsVisible && MC.IsReloading && Config.KeepWeaponAfterReload && !Config.ControlYourCarry && Config.HideCarriedOnReload)
            {
                gunCarried.IsVisible = false;
            }

            //Shutdown on Condition
            if (DoesExists(gunCarried) && (MC.IsDead || MC.IsInWater || MC.IsInVehicle() || Game.IsCutsceneActive || !MC.IsVisible || Shootdodge != 0 || DualWielding))
            {
                gunCarried.Delete();
                lastWeapon = null;
            }
        }


        private Prop CreateGunProp(WeaponHash source, Ped sourcePed)
        {
            Vector3 pos = sourcePed.Position;
            return Function.Call<Prop>(Hash.CREATE_WEAPON_OBJECT, source, 0, pos.X, pos.Y, pos.Z, false, 1f, sourcePed.Weapons[source].Model.Hash, false, true);
        }

        private void EmptyAmmoInitializer(Ped player)
        {
            List<Weapon> pistols = player.Weapons.Where(wpn => wpn.Group == WeaponGroup.Pistol && wpn.Ammo > 0 && wpn.MaxAmmoInClip > 1).ToList();
            List<Weapon> visibleWeapons = VisibleWeaponsDict.Where(entry => entry.Key != 0 && MC.Weapons.HasWeapon(entry.Key)).Select(entry => MC.Weapons[entry.Key]).ToList();

            if (pistols.Any())
            {
                // Select random pistol from the list
                Game.DisableControlThisFrame(Control.Aim);
                Game.DisableControlThisFrame(Control.Reload);

                Weapon chosenVisibleWeapon = visibleWeapons.FirstOrDefault(wpn => wpn.Group == WeaponGroup.Pistol && wpn.Ammo > 0 && wpn.MaxAmmoInClip > 1);
                Weapon randomPistol = pistols[new Random().Next(pistols.Count)];
                Weapon chosenPistol;
                if (chosenVisibleWeapon != null)
                    chosenPistol = chosenVisibleWeapon;
                else chosenPistol = randomPistol;

                float WpnLength = lastWeapon.Model.Dimensions.frontTopRight.X - lastWeapon.Model.Dimensions.rearBottomLeft.X;
                bool longWpn = WpnLength > 0.60f;
                if (lastWeapon.Group == WeaponGroup.SMG && !longWpn)
                    return;

                gunCarried = CreateGunProp(lastWeapon, MC);
                GetAttachments(MC, lastWeapon, gunCarried);
                player.Weapons.Select(chosenPistol, true);

                PedBone hand = MC.Bones[Bone.SkelLeftHand];
                Vector3 gunBarrelPos = gunCarried.Bones["gun_barrels"].GetPositionOffset(gunCarried.Position);
                gunCarried.AttachTo(hand, gunBarrelPos + new Vector3(0.05f, -0.15f, 0.075f), new Vector3(250f, 270f, -30f));

                VisibleWeaponsConflictSolver(chosenPistol.Hash);

                //lastWeapon = null;
            }
        }

        private void WeaponSwapMode()
        {
            //Gun Swapping Mode
            if (PlayerChangingGun() && !Config.ControlYourCarry && !Game.Player.IsAiming && Config.EnableOnWeaponSwap)
            {
                Wait(100);
                if (TargetWpns.Contains(MC.Weapons.Current.Group))
                {
                    if (DoesExists(gunCarried))
                    {
                        VisibleWeaponsConflictSolver(lastWeapon);
                    }

                    lastWeapon = MC.Weapons.Current;
                }
                else if (TriggerWpns.Contains(MC.Weapons.Current.Group) && lastWeapon != null)
                {
                    if (DoesExists(gunCarried))
                        gunCarried.Delete();
                    if (TriggerWpns.Contains(lastWeapon.Group))
                        return;
                    float WpnLength = lastWeapon.Model.Dimensions.frontTopRight.X - lastWeapon.Model.Dimensions.rearBottomLeft.X;
                    bool longWpn = WpnLength > 0.60f;
                    if (lastWeapon.Group == WeaponGroup.SMG && !longWpn)
                        return;

                    gunCarried = CreateGunProp(lastWeapon, MC);
                    GetAttachments(MC, lastWeapon, gunCarried);
                    PedBone hand = MC.Bones[Bone.SkelLeftHand];
                    Vector3 gunBarrelPos = gunCarried.Bones["gun_barrels"].GetPositionOffset(gunCarried.Position);
                    gunCarried.AttachTo(hand, gunBarrelPos + new Vector3(0.05f, -0.15f, 0.075f), new Vector3(250f, 270f, -30f));
                }


                if (DoesExists(gunCarried) && (lastWeapon == MC.Weapons.Current))
                {
                    gunCarried.Delete();
                    lastWeapon = null;
                }

                if (DoesExists(gunCarried) && !TargetWpns.Concat(TriggerWpns).Contains(MC.Weapons.Current.Group))
                {
                    gunCarried.Delete();
                    VisibleWeaponsConflictSolver(MC.Weapons.Current);
                    VisibleWeaponsConflictSolver(lastWeapon);
                    lastWeapon = null;
                }
                if (!DoesExists(gunCarried) && !TargetWpns.Concat(TriggerWpns).Contains(MC.Weapons.Current.Group) && lastWeapon != null)
                {
                    lastWeapon = null;
                }
            }
        }

        private void Animation()
        {
            if (DoesExists(gunCarried) && !DualWielding)
            {
                if (!handAnim.ClipDictionary.IsLoaded)
                    handAnim.ClipDictionary.Request(/*500*/);
                if ((MC.IsAiming || MC.IsShooting) && !MC.IsJumping && !MC.IsRagdoll && Shootdodge == 0 && !DualWielding)
                {
                    if (Config.FirstPersonDisable && GameplayCamera.FollowPedCamViewMode == CamViewMode.FirstPerson)
                        return;

                    Game.DisableControlThisFrame(Control.Jump);
                    if (Game.IsControlPressed(Control.Aim))
                    {
                        if (MC.IsPlayingAnimation(handAnim2))
                        {
                            MC.Task.StopScriptedAnimationTask(handAnim2);
                            Yield();
                        }
                        if (!MC.IsPlayingAnimation(handAnim))
                            MC.Task.PlayAnimation(handAnim, AnimationBlendDelta.VerySlowBlendIn, AnimationBlendDelta.InstantBlendOut, -1, (AnimationFlags)48, 0f);
                        MC.SetAnimationSpeed(handAnim, 0f);
                    }
                    else
                    {
                        if (MC.IsPlayingAnimation(handAnim))
                        {
                            MC.Task.StopScriptedAnimationTask(handAnim);
                            Yield();
                        }
                        if (!MC.IsPlayingAnimation(handAnim2))
                            MC.Task.PlayAnimation(handAnim2, AnimationBlendDelta.VerySlowBlendIn, AnimationBlendDelta.InstantBlendOut, -1, (AnimationFlags)48, 0f);
                        MC.SetAnimationSpeed(handAnim2, 0f);
                    }
                    SetIK(true, MC);
                    Function.Call(Hash.SET_ENABLE_HANDCUFFS, MC, true);
                    SetIkTarget(MC);
                }
                else
                {
                    EndAnimation();
                    SetIK(false, MC);
                }
            }
            else
            {
                EndAnimation();
                if (!DualWielding)
                    SetIK(false, MC);
            }
        }

        private void EndAnimation()
        {
            if (!DualWielding)
                SetIK(false, MC);
            Function.Call(Hash.SET_ENABLE_HANDCUFFS, MC, false);
            if (MC.IsPlayingAnimation(handAnim))
                MC.Task.StopScriptedAnimationTask(handAnim);
            if (MC.IsPlayingAnimation(handAnim2))
                MC.Task.StopScriptedAnimationTask(handAnim2);
        }

        private void OptionalPassiveMode()
        {
            if (!Config.ControlYourCarry)
                return;

            if (Game.IsControlPressed(Control.VehicleHorn) && Game.IsControlPressed(Control.LookBehind))
            {
                if (!keyHeld)
                {
                    keyHeld = true;
                    heldKeyTimer = DateTime.Now; // Start timer once when key is first pressed
                }

                if ((DateTime.Now - heldKeyTimer).TotalMilliseconds >= holdKeyLength)
                {
                    if (!DoesExists(gunCarried) && TargetWpns.Contains(MC.Weapons.Current.Group)) // If weapon not already held
                    {
                        lastWeapon = MC.Weapons.Current;
                        float WpnLength = lastWeapon.Model.Dimensions.frontTopRight.X - lastWeapon.Model.Dimensions.rearBottomLeft.X;
                        bool longWpn = WpnLength > 0.60f;
                        if (lastWeapon.Group == WeaponGroup.SMG && !longWpn)
                            return;

                        gunCarried = CreateGunProp(lastWeapon, MC);
                        GetAttachments(MC, lastWeapon, gunCarried);
                        PedBone hand = MC.Bones[Bone.SkelLeftHand];
                        Vector3 gunBarrelPos = gunCarried.Bones["gun_barrels"].GetPositionOffset(gunCarried.Position);
                        gunCarried.AttachTo(hand, gunBarrelPos + new Vector3(0.05f, -0.15f, 0.075f), new Vector3(250f, 270f, -30f));

                        MC.Weapons.Select(WeaponHash.Unarmed);
                    }
                    else if (DoesExists(gunCarried))
                    {
                        gunCarried.Delete();
                        lastWeapon = null;
                        if (GameplayCamera.FollowPedCamViewMode != CamViewMode.FirstPerson && !Config.FirstPersonDisable)
                            MC.ResetWeaponMovementClipSet();
                    }
                    keyHeld = false;
                }
            }
            else
            {
                keyHeld = false;
            }

            if (DoesExists(gunCarried) && lastWeapon != null && lastWeapon == MC.Weapons.Current)
            {
                gunCarried.Delete();
                lastWeapon = null;
                if (GameplayCamera.FollowPedCamViewMode != CamViewMode.FirstPerson && !Config.FirstPersonDisable)
                    MC.ResetWeaponMovementClipSet();
            }
            if (DoesExists(gunCarried))
            {
                if (Config.HideCarriedOnReload)
                {
                    if (MC.IsReloading)
                        gunCarried.IsVisible = false;
                    else
                        gunCarried.IsVisible = true;
                }

                if (MC.Weapons.Current.Group != WeaponGroup.Pistol && MC.Weapons.Current.Group != WeaponGroup.Unarmed)
                {
                    if (!moveSet.IsLoaded)
                        moveSet.Request(/*500*/);
                    else if (GameplayCamera.FollowPedCamViewMode != CamViewMode.FirstPerson && !Config.FirstPersonDisable)
                        MC.SetWeaponMovementClipSet(moveSet);
                }
                else if (GameplayCamera.FollowPedCamViewMode != CamViewMode.FirstPerson && !Config.FirstPersonDisable)
                    MC.ResetWeaponMovementClipSet();
            }
        }
    }
}
