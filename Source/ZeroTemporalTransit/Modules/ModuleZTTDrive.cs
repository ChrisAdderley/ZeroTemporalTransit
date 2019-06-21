// A module that affects the SAS level of a probe
using System;
using System.Collections.Generic;
using System.Collections;

using System.Linq;
using System.Text;
using UnityEngine;
using ZeroTemporalTransit.UI;
using Contracts.Parameters;

namespace ZeroTemporalTransit
{

  public class ModuleZTTDrive : PartModule
  {
    // The radius of the bubble
    [KSPField(isPersistant = true, guiActive = true, guiName = "Bubble Radius"),
     UI_FloatRange(minValue = 5f, maxValue = 100f, stepIncrement = 1f)]
    public float BubbleRadius = 10.0f;

    // Available patterened energy tracker
    [KSPField(isPersistant = false, guiActive = true, guiName = "Available Energy")]
    public string AvailableEnergy;

    // Name of the schematic bubble object
    [KSPField(isPersistant = false)]
    public string SchematicBubbleObjectName = "";

    // Resource to use for jumps
    [KSPField(isPersistant = false)]
    public string ResourceName = "PatternedEnergy";

    [KSPField(isPersistant = true)]
    public Vector3d storedDestination = Vector3d.zero;

    [KSPField(isPersistant = true)]
    public bool hasDestination = false;

    // Toggles the schematic warp bubble rendererer
    [KSPEvent(guiActive = true, guiName = "Visualize Bubble", active = true)]
    public void ToggleBubble()
    {
      if (schematicBubbleOn)
        StartCoroutine(DisableSchematicRenderer());
      else
        StartCoroutine(ActivateSchematicRenderer());
    }

    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Plot Jump", active = true)]
    public void ClearJump()
    {
      Utils.Log(String.Format("[ModuleZTTDrive] [Event]: Clearing destination"));
      ClearDestination();
    }

    [KSPEvent(guiActive = true, guiActiveEditor = false, guiName = "Plot Jump", active = true)]
    public void PlotJump()
    {
      Utils.Log(String.Format("[ModuleZTTDrive] [Event]: Opening plotting overlay"));
      ZeroTemporalTransitUI.Instance.PlotJump(this, storedDestination);
    }

    [KSPEvent(guiActive = false, guiActiveEditor = false, guiName = "Activate Drive", active = true)]
    public void Jump()
    {
        Utils.Log(String.Format("[ModuleZTTDrive] [Event]: Starting warp jump"));
        DoWarpJump(storedDestination);
    }

    // Schematic bubble variables
    private bool schematicBubbleOn;
    private MeshRenderer schematicBubbleRenderer;
    private Transform schematicBubbleTransform;

    public void SetDestination(Vector3d destination)
    {
      Utils.Log(String.Format("[ModuleZTTDrive]: Set destination to {0}", destination));
      hasDestination = true;
      storedDestination = destination;
    }

    public void ClearDestination()
    {
      Utils.Log(String.Format("[ModuleZTTDrive]: Cleared Destination"));
      hasDestination = false;
    }

    public override string GetInfo()
    {
      string info = "Provides FTL jump capability \n\n" +
          "Generates a wormhole centered around the part of a user-defined size. Larger bubbles require more energy.";
      return info;
    }

    public override void OnStart(PartModule.StartState state)
    {
      SetUpRenderers();
      SetUpUI();
    }

    /// <summary>
    /// Setup renderers for schematic and warp bubble
    /// </summary>
    void SetUpRenderers()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        schematicBubbleTransform = part.FindModelTransform(SchematicBubbleObjectName);
        if (schematicBubbleTransform != null)
          schematicBubbleRenderer = schematicBubbleTransform.GetComponent<MeshRenderer>();

        if (schematicBubbleTransform == null || schematicBubbleRenderer == null)
        {
          Utils.LogWarning(String.Format("[ModuleZTTDrive]: Could not find schematic bubble named {0} on part, aborting", SchematicBubbleObjectName));
        }
      }
    }
    /// <summary>
    /// Setup the user interface components and localization
    /// </summary>
    void SetUpUI()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        Events["ToggleBubble"].guiName = "Visualize Bubble";
        Events["Jump"].guiName = "Activate Drive";
        Fields["AvailableEnergy"].guiName = "Available Energy";
        Fields["BubbleRadius"].guiName = "Bubble Radius";
      }
    }

    public void FixedUpdate()
    {
      if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedSceneIsEditor)
      {
        HandleSchematicBubble();
      }
      if (HighLogic.LoadedSceneIsFlight)
      {
        double amt = 0.0;
        double maxAmt = 0.0;
        part.GetConnectedResourceTotals(PartResourceLibrary.Instance.GetDefinition(ResourceName).id, out amt, out maxAmt, false);

        AvailableEnergy = amt.ToString();

        if (hasDestination)
        {
          Events["Jump"].guiActive = true;
          Events["ClearJump"].guiActive = true;
        }
        else
        {
          Events["Jump"].guiActive = false;
          Events["ClearJump"].guiActive = false;
        }
      }

    }

    /// <summary>
    /// Attempts a warp jump
    /// </summary>
    /// <param name="destination">Destination.</param>
    public void DoWarpJump(Vector3d destination)
    {
      Utils.Log(String.Format("[ModuleZTTDrive]: Attempting ZTT jump"));
      if (TestJumpConditions(destination))
        InitiateJump(destination);
    }

    /// <summary>
    /// Tests if we can jump to a target
    /// </summary>
    /// <returns><c>true</c>, if jump conditions passed, <c>false</c> otherwise.</returns>
    /// <param name="destination">Destination.</param>
    public bool TestJumpConditions(Vector3d destination)
    {
      if (!TestResourceCost(destination))
        return false;
      if (!TestAtmosphere())
        return false;
      if (!TestGravity())
        return false;
      return true;
    }

    /// <summary>
    /// Initiates the warp jump to the destination
    /// </summary>
    /// <param name="destination">Destination.</param>
    public void InitiateJump(Vector3d destination)
    {
      DestroyPartsOutsideBubble(GetUnsafeParts());
      StartCoroutine(PlayWarpOutEffects());
      StartCoroutine(PlayWarpInEffects());
      DisplaceVessel(destination)
    }


    void DisplaceVessel(Vector3d destination)
    {
      //Vector3d solarRefVel = vessel.orbit.GetFrameVel ();
      //vessel.SetPosition(destination);
      //vessel.SetWorldVelocity(solarRefVel);

      // Calculate target location and dispersion
      Vector3d currentDistance = Vector3d.Distance(destination, vessel.GetWorldPos3D());
      double currentDispersion = CalculateDispersion(currentDistance, vessel.GetWorldPos3D(), destination);
      Vector3d dispersion = (Vector3d)(UnityEngine.Random.insideUnitSphere /2.0 ) * currentDispersion;

      CelestialBody targetBody = FlightGlobals.getMainBody(destination + dispersion);
      CelestialBody currentBody = orbitDriver.orbit.referenceBody;

      Utils.Log(String.Format("[ModuleZTTDrive]: Jump summary: \n - Start: {0} ({1})\n - Target: {2} ({3})\n - Distance: {4:F0}\n - Dispersion: {5:F0}\n - Final coordinates: {6}", currentBody.name, vessel.GetWorldPos3D(), targetBody.name, destination, currentDistance, currentDispersion, destination + dispersion));

      OrbitDriver orbitDriver = vessel.orbitDriver;
      Orbit orbit = orbitDriver.orbit;

      // This code is inspired by several projects related to teleports
      Orbit newOrbit = new Orbit(orbit.inclination, orbit.eccentricity, orbit.semiMajorAxis, orbit.LAN, orbit.argumentOfPeriapsis, orbit.meanAnomalyAtEpoch, orbit.epoch, orbit.referenceBody);
      newOrbit.UpdateFromStateVectors(destination, orbit.GetFrameVel(), targetBody, Planetarium.GetUniversalTime());

      vessel.Landed = false;
      vessel.Splashed = false;
      vessel.landedAt = string.Empty;

      OrbitPhysicsManager.HoldVesselUnpack(60);

      List<Vessel> allVessels = FlightGlobals.Vessels;
      foreach (Vessel v in allVessels.AsEnumerable())
      {
        if (v.packed == false)
          v.GoOnRails();
      }
      
      orbit.inclination = newOrbit.inclination;
      orbit.eccentricity = newOrbit.eccentricity;
      orbit.semiMajorAxis = newOrbit.semiMajorAxis;
      orbit.LAN = newOrbit.LAN;
      orbit.argumentOfPeriapsis = newOrbit.argumentOfPeriapsis;
      orbit.meanAnomalyAtEpoch = newOrbit.meanAnomalyAtEpoch;
      orbit.epoch = newOrbit.epoch;
      orbit.referenceBody = newOrbit.referenceBody;
      orbit.Init();
      orbit.UpdateFromUT(Planetarium.GetUniversalTime());

      if (orbit.referenceBody != newOrbit.referenceBody)
        orbitDriver.OnReferenceBodyChange?.Invoke(newOrbit.referenceBody);

      vessel.orbitDriver.pos = vessel.orbit.pos.xzy;
      vessel.orbitDriver.vel = vessel.orbit.vel;

      if (vessel.orbitDriver.orbit.referenceBody != currentBody)
        GameEvents.onVesselSOIChanged.Fire(new GameEvents.HostedFromToAction<Vessel, CelestialBody>(vessel, currentBody, vessel.orbitDriver.orbit.referenceBody));

    }
    /// <summary>
    /// Destroys the parts outside warp bubble.
    /// </summary>
    /// <param name="toKill">parts to destroy</param>
    void DestroyPartsOutsideBubble(List<Part> toKill)
    {
      for (int i = 0; i < toKill.Count; i++)
      {
        Utils.Log(String.Format("[ModuleZTTDrive]: Part {0} is outside the warp bubble, destroying", toKill[i].partInfo.name));
        toKill[i].explode();
      }
    }

    IEnumerator PlayWarpOutEffects()
    {
      Utils.Log(String.Format("[ModuleZTTDrive]: Playing warp bubble effects"));
      yield return 0;
    }

    IEnumerator PlayWarpInEffects()
    {
      Utils.Log(String.Format("[ModuleZTTDrive]: Playing warp out effects"));
      yield return 0;
    }

    /// <summary>
    /// Tests to see if we can jump based on resources
    /// </summary>
    /// <returns><c>true</c>, if resource cost test was passed, <c>false</c> otherwise.</returns>
    /// <param name="destination">Destination.</param>
    bool TestResourceCost(Vector3d destination)
    {
      double totalDistance = Vector3d.Distance(destination, part.vessel.GetWorldPos3D());
      double cost = CalculateJumpCost(totalDistance);

      Utils.Log(String.Format("[ModuleZTTDrive]: Jump will cost {0:F4} energy", cost));

      double amt = 0.0;
      double maxAmt = 0.0;
      part.GetConnectedResourceTotals(PartResourceLibrary.Instance.GetDefinition(ResourceName).id, out amt, out maxAmt, false);

      if (amt < cost)
      {
        Utils.Log(String.Format("[ModuleZTTDrive]: Resource test was failed (has {0:F2} PE, needs {1:F2} PE)", amt, cost));
        ScreenMessages.PostScreenMessage(new ScreenMessage(String.Format("Aborting jump - not enough energy"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      return true;
    }

    /// <summary>
    /// Tests to see if we can jump based on gravity gradient
    /// </summary>
    /// <returns><c>true</c>, if gravity test was passed, <c>false</c> otherwise.</returns>
    bool TestGravity()
    {
      CelestialBody thisBody = vessel.mainBody;
      double startGrav = thisBody.gravParameter/( Math.Pow(thisBody.GetAltitude(vessel.GetWorldPos3D()), 2));
      if (startGrav > Settings.gravityJumpThreshold)
      {
        Utils.Log(String.Format("[ModuleZTTDrive]: Gravity test was failed (local {0:F2} m/s-2, threshold is {1:F2} m/s-2)", startGrav, Settings.gravityJumpThreshold));
        ScreenMessages.PostScreenMessage(new ScreenMessage(String.Format("Aborting jump - local gravity gradient is too high"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      return true;
    }

    /// <summary>
    /// Tests to see if we can jump based on atmosphere
    /// </summary>
    /// <returns><c>true</c>, if atmosphere test was passed, <c>false</c> otherwise.</returns>
    bool TestAtmosphere()
    {
      CelestialBody thisBody = vessel.mainBody;
      double pressureAtm = thisBody.GetPressureAtm(thisBody.GetAltitude(vessel.GetWorldPos3D()));
      if (pressureAtm > Settings.atmosphereJumpThreshold)
      {
        Utils.Log(String.Format("[ModuleZTTDrive]: Atmo test was failed (local {0:F2} atm, threshold is {1:F2} atm)", pressureAtm, Settings.atmosphereJumpThreshold));
        ScreenMessages.PostScreenMessage(new ScreenMessage(String.Format("Aborting jump - Cannot jump in dense atmosphere"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
        return false;
      }
      return true;
    }

    #region SchematicBubble methods

    /// <summary>
    /// Functions for handling the schematic bubble
    /// </summary>
    void HandleSchematicBubble()
    {
      if (schematicBubbleTransform != null && schematicBubbleOn)
      {
        schematicBubbleTransform.localScale = Vector3.Lerp(schematicBubbleTransform.localScale, Vector3.one * BubbleRadius, TimeWarp.fixedDeltaTime);
        schematicBubbleTransform.Rotate(TimeWarp.fixedDeltaTime * new Vector3(0f, 1f, 0f), Space.Self);
      }
    }

    /// <summary>
    /// Activates and animates the schematic bubble
    /// </summary>
    IEnumerator ActivateSchematicRenderer()
    {
      Utils.Log(String.Format("[ModuleZTTDrive]: Playing schematic renderer in animation"));
      schematicBubbleRenderer.enabled = true;
      schematicBubbleOn = true;
      Utils.Log(String.Format("[ModuleZTTDrive]: Schematic renderer on"));
      yield return 0;
    }

    /// <summary>
    /// Deactivates and animates the schematic bubble
    /// </summary>
    IEnumerator DisableSchematicRenderer()
    {
      Utils.Log(String.Format("[ModuleZTTDrive]: Playing schematic renderer out animation"));
      schematicBubbleOn = false;
      schematicBubbleRenderer.enabled = false;
      Utils.Log(String.Format("[ModuleZTTDrive]: Schematic renderer off"));
      yield return 0;
    }
    #endregion

    /// <summary>
    /// Gets a list of the parts that will be damaged by the bubble
    /// </summary>
    /// <returns>The unsafe parts.</returns>
    List<Part> GetUnsafeParts()
    {
      List<Part> includedParts = GetSafeParts();
      List<Part> unsafeParts = new List<Part>();
      for (int i = 0; i < part.vessel.parts.Count; i++)
      {
        if (!includedParts.Contains(part.vessel.parts[i]))
        {
            unsafeParts.Add(part.vessel.parts[i]);
        }
      }

      Utils.Log(String.Format("[ModuleZTTDrive]: Found {0} safe parts and {1} unsafe parts out of {2} total", includedParts.Count, unsafeParts.Count, part.vessel.parts.Count));
      return unsafeParts;
    }

    /// <summary>
    /// Gets a list of parts that are not going to be damaged by the bubble
    /// </summary>
    /// <returns>The safe parts.</returns>
    List<Part> GetSafeParts()
    {
      List<Part> enclosedOrTouchingInner = GetPartsInSphere(BubbleRadius);
      List<Part> enclosedOrTouchingOuter = GetPartsInSphere(BubbleRadius + Settings.bubbleOuterOffset);

      List<Part> includedParts = new List<Part>();
      for (int i = 0; i < enclosedOrTouchingInner.Count; i++)
      {
        if (!enclosedOrTouchingOuter.Contains(enclosedOrTouchingInner[i]))
        {
            includedParts.Add(enclosedOrTouchingInner[i]);
        }
      }
      return includedParts;

    }

    /// <summary>
    /// Gets a set of Parts in a sphere of a given radius
    /// </summary>
    /// <returns>The Parts in thes phere.</returns>
    /// <param name="radius">Radius.</param>
    List<Part> GetPartsInSphere(float radius)
    {
      Collider[] hits = Physics.OverlapSphere(part.transform.position, radius);
      List<Part> overlappedParts = new List<Part>();

      for (int j = 0; j < hits.Length; j++)
      {
        for (int i = 0; i < part.vessel.parts.Count; i++)
        {
          if (hits[j].attachedRigidbody == part.vessel.parts[i].Rigidbody)
          {
            if (!overlappedParts.Contains(part.vessel.parts[i]))
            {
              overlappedParts.Add(part.vessel.parts[i]);
            }
          }
        }
      }
      return overlappedParts;

    }
    /// <summary>
    /// Get the cost of a jump given the distance
    /// </summary>
    /// <returns>The cost of the jump</returns>
    /// <param name="distance">The distance in m</param>
    public double CalculateJumpCost(double distance)
    {
      double massCost = part.vessel.totalMass * Settings.energyPerMass;
      double bubbleSizeCost = (4.0 / 3.0f) * Math.PI * Math.Pow(BubbleRadius, 3) * Settings.energyRadiusScale;
      double distanceCost = (massCost + bubbleSizeCost) * distance * Settings.energyDistanceScale;
      return distanceCost;
    }

    /// <summary>
    /// Get the random dispersion of a jump in m
    /// </summary>
    /// <returns>The randomness in the jump in m</returns>
    /// <param name="distance">The distance in m</param>
    public double CalculateDispersion(double distance, Vector3d startPos, Vector3d endPos)
    {
      CelestialBody startBody = FlightGlobals.getMainBody(startPos);
      CelestialBody endBody = FlightGlobals.getMainBody(endPos);
      double startGrav = startBody.gravParameter/( Math.Pow(startBody.GetAltitude(startPos), 2));
      double endGrav = endBody.gravParameter/( Math.Pow(endBody.GetAltitude(endPos), 2));
      double gravCost = (startGrav + endGrav) * Settings.dispersionGravityScale;

      double distanceCost = distance * Settings.dispersionDistanceScale;
      return distanceCost + gravCost;
    }
  }
}
