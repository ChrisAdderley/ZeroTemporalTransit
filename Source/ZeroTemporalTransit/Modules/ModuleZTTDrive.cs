// A module that affects the SAS level of a probe
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ZeroTemporalTransit;

namespace ZeroTemporalTransit
{

    public class ModuleZTTDrive : PartModule
    {
        // The radius of the bubble
        public float BubbleRadius = 10.0f;

        public string Bu
        public override string GetInfo()
        {
            string info = "Provides FTL jump capability \n\n" +
                "";
            return info;
        }

        double CalculateJumpCost()
        {
            double massCost = part.vessel.totalMass * ZTTConstants.energyPerMass;

            return 0d;
        }

    }
}
