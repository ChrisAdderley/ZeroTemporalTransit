using System;
using UnityEngine;

namespace ZeroTemporalTransit
{
    /// <summary>
    /// Constants are items that are controlled by a config file, but not modified by a player
    /// at runtime
    /// </summary>
    public static class Settings
    {
        // The energy required to transit a unit mass
        public static double energyPerMass = 1d;
        // A scaling factor for the energy needed to extend the bubble
        public static double energyRadiusScale = 0.01d;
        // A scaling factor for the energy needed to move a distance
        public static double energyDistanceScale = 0.001d;

        public static float bubbleOuterOffset = 1.0f;

        public static double dispersionDistanceScale = 0.0001d;

        public static bool DebugUIMode = true;

        public static void Load()
        {
            ConfigNode settingsNode;

      Utils.Log("[Constants]: Started loading");
            if (GameDatabase.Instance.ExistsConfigNode("ZeroTemporalTransit/ZTTSETTINGS"))
            {
        Utils.Log("[Constants]: Located constants file");
                settingsNode = GameDatabase.Instance.GetConfigNode("ZeroTemporalTransit/ZTTSETTINGS");
            }
            else
            {
        Utils.Log("[Constants]: Couldn't find constants file, using defaults");
            }


      Utils.Log("[Constants]: Finished loading");
        }
    }
}
