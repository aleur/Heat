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

public class Main : Script
{
    public ToggleClothing ToggleClothing { get; set; }
    public RelaxedDrivingStyle DrivingStyle { get; set; }
    public Dryfire Dryfire { get; set; }
    public BagSystem BagSystem { get; set; }

    public Main()
    {
        HeatSettings.LoadIniFile("scripts//Heat//Heat.ini");
        if (true) ToggleClothing = new ToggleClothing();
        if (HeatSettings.isDrivingStyleEnabled.Equals("True")) DrivingStyle = new RelaxedDrivingStyle();
        if (HeatSettings.isDryfireEnabled.Equals("True")) Dryfire = new Dryfire();
        if (HeatSettings.isBagSystemEnabled.Equals("True")) BagSystem = new BagSystem();
    }
}