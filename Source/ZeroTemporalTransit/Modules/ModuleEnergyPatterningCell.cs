// A module that affects the SAS level of a probe
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ZeroTemporalTransit
{

    public class ModuleEnergyPatterningCell : PartModule
    {

        public override string GetInfo()
        {
            string toRet = "Stores energy ";
            return toRet;
        }

    }
}
