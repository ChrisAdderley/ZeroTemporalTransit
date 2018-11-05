using System;
using UnityEngine;

namespace ZeroTemporalTransit
{
    /// <summary>
    /// Constants are items that are controlled by a config file, but not modified by a player
    /// at runtime
    /// </summary>
    public static class ZTTConstants
    {
        // The energy required to transit a unit mass
        public static double energyPerMass;
        // A scaling factor for the energy needed to extend the bubble
        public static double energyRadiusScale;

        public static void Load()
        {
            ConfigNode settingsNode;

            LogUtils.Log("[Constants]: Started loading");
            if (GameDatabase.Instance.ExistsConfigNode("ZeroTemporalTransit/ZTTCONSTANTS"))
            {
                LogUtils.Log("[Constants]: Located constants file");
                settingsNode = GameDatabase.Instance.GetConfigNode("ZeroTemporalTransit/ZTTCONSTANTS");
            }
            else
            {
                LogUtils.Log("[Constants]: Couldn't find constants file, using defaults");
            }


            LogUtils.Log("[Constants]: Finished loading");
        }
    }
}
