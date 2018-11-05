using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ZeroTemporalTransit
{
    // Main class
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class ZeroTemporalTransit : MonoBehaviour
    {
        public static ZeroTemporalTransit Instance { get; private set; }



        protected void Awake()
        {
            Instance = this;
        }

    }
}
