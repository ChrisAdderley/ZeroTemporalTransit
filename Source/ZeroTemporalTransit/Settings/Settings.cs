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
    public static double energyPerMass = 0d;

    // A scaling factor for the energy needed to extend the bubble (proportional to volume)
    public static double energyRadiusScale = 0.01d;
    // A scaling factor for the energy needed to move a distance
    public static double energyDistanceScale = 0.001d;
    // Amount of inaccuracy added when moving a distance
    public static double dispersionDistanceScale = 0.0001d;
    // Amount of inaccuracy added when jumping from/to a dense gravity well.
    public static double dispersionGravityScale = 1d;

    // Above this gravity value the warp drive will not work
    public static double gravityJumpThreshold = 5.0d;
    // Above this pressure value (atm) the warp drive will not work
    public static double atmosphereJumpThreshold = 0.1d;

    // How far outside the warp bubble to blow things up
    public static float bubbleOuterOffset = 1.0f;

    public static string UIJumpKey = "j";
    public static string UIYAxisKey = "left ctrl";


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
