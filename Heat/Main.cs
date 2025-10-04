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
using TrailCarry;

public class Main : Script
{
    public ToggleClothing ToggleClothing { get; set; } = new ToggleClothing();
    public RelaxedDrivingStyle DrivingStyle { get; set; } = new RelaxedDrivingStyle();
    public Dryfire Dryfire { get; set; } = new Dryfire();
    public BagSystem BagSystem { get; set; } = new BagSystem();
    public HandheldWeapons HandheldWeapons { get; set; } = new HandheldWeapons();

    public Main()
    {
        HeatSettings.LoadIniFile("scripts//Heat//Heat.ini");
        if (HeatSettings.isClothingSystemEnabled) KeyUp += ToggleClothing.OnKeyUp;
        if (HeatSettings.isDrivingStyleEnabled)
        {
            Tick += DrivingStyle.OnTick;
            KeyUp += DrivingStyle.ToggleDrivingStyle;
        }
        if (HeatSettings.isDryfireEnabled) Tick += Dryfire.OnTick;
        if (HeatSettings.isBagSystemEnabled)
        {
            Tick += BagSystem.OnTick;
            Tick += new EventHandler(HandheldWeapons.OnTick);
        }
    }
}