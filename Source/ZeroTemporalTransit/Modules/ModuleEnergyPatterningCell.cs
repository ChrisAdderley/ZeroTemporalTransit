// A module that affects the SAS level of a probe
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;

namespace ZeroTemporalTransit
{

  public class ModuleEnergyPatterningCell : ModuleResourceConverter
  {
    // Rate at which decay occurs (per second)
    [KSPField(isPersistant = false)]
    public float DecayRate = 0.0f;

    // Resource to decay
    [KSPField(isPersistant = false)]
    public string DecayResource = "PatternedEnergy";

    // Status string
    [KSPField(isPersistant = false, guiActive = true, guiName = "Energy Storage")]
    public string StorageStatus;

    public override string GetInfo()
    {
        return Localizer.Format("#LOC_ZTT_ModuleEnergyPatterningCell_ModuleInfo")
            + base.GetInfo();
    }
    public override string GetModuleDisplayName()
    {
        return Localizer.Format("#LOC_ZTT_ModuleEnergyPatterningCell_ModuleName");
    }

    public override void OnStart(PartModule.StartState state)
    {

      if (state != PartModule.StartState.Editor)
        this.part.force_activate();
        
      base.OnStart(state);
      Fields["StorageStatus"].guiName = Localizer.Format("#LOC_ZTT_ModuleEnergyPatterningCell_Status_Title");
    }

    public override void OnFixedUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight)
      {
        double lossAmt = DecayRate * TimeWarp.fixedDeltaTime;
        double amt = this.part.RequestResource(DecayResource,  lossAmt);
        if (amt <= 0.00001d)
        {
          StorageStatus = Localizer.Format("#LOC_ZTT_ModuleEnergyPatterningCell_Status_None");
        } else
        {
          if (base.ModuleIsActive())
          {
            StorageStatus = Localizer.Format("#LOC_ZTT_ModuleEnergyPatterningCell_Status_Ok");
          } else
          {
            StorageStatus = Localizer.Format("#LOC_ZTT_ModuleEnergyPatterningCell_Status_Unstable", String.Format("{0:F1}", DecayRate));
          }
        }
      }
      base.OnFixedUpdate();
    }


  }
}
