// A module that affects the SAS level of a probe
using System;
using System.Collections.Generic;
using System.Collections;

using System.Linq;
using System.Text;
using UnityEngine;
using ZeroTemporalTransit;
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

    // Toggles the schematic warp bubble rendererer
    [KSPEvent(guiActive = true, guiName = "Visualize Bubble", active = true)]
    public void ToggleBubble()
    {
      if (schematicBubbleOn)
        StartCoroutine(DisableSchematicRenderer());
      else
        StartCoroutine(ActivateSchematicRenderer());
    }

    // Schematic bubble variables
    private bool schematicBubbleOn;
    private MeshRenderer schematicBubbleRenderer;
    private Transform schematicBubbleTransform;

    public override string GetInfo()
    {
      string info = "Provides FTL jump capability \n\n" +
          "";
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
        schematicBubbleRenderer = schematicBubbleTransform.GetComponent<MeshRenderer>();

        if (schematicBubbleTransform == null || schematicBubbleRenderer == null)
        {
          LogUtils.LogError(String.Format("[ModuleZTTDrive]: Could not find schematic bubble named {0} on part, aborting", SchematicBubbleObjectName));
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
      }

    }

    /// <summary>
    /// Attempts a warp jump
    /// </summary>
    /// <param name="destination">Destination.</param>
    public void DoWarpJump(Vector3d destination)
    {
      LogUtils.Log(String.Format("[ModuleZTTDrive]: Attempting ZTT jump"));
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
      bool resourceOk = TestResourceCost(destination);
      return resourceOk;
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
    }

    /// <summary>
    /// Destroys the parts outside warp bubble.
    /// </summary>
    /// <param name="toKill">parts to destroy</param>
    void DestroyPartsOutsideBubble(List<Part> toKill)
    {
      for (int i = 0; i < toKill.Count; i++)
      {
        LogUtils.Log(String.Format("[ModuleZTTDrive]: Part {0} is outside the warp bubble, destroying", toKill[i].partInfo.name));
        toKill[i].explode();
      }
    }

    IEnumerator PlayWarpOutEffects()
    {
      LogUtils.Log(String.Format("[ModuleZTTDrive]: Playing warp bubble effects"));
      yield return 0;
    }

    IEnumerator PlayWarpInEffects()
    {
      LogUtils.Log(String.Format("[ModuleZTTDrive]: Playing warp out effects"));
      yield return 0;
    }

    /// <summary>
    /// Tests to see if we can jump based on resources
    /// </summary>
    /// <returns><c>true</c>, if resource cost was tested, <c>false</c> otherwise.</returns>
    /// <param name="destination">Destination.</param>
    bool TestResourceCost(Vector3d destination)
    {
      double totalDistance = Vector3d.Distance(destination, part.vessel.GetWorldPos3D());
      double cost = CalculateJumpCost(totalDistance);

      LogUtils.Log(String.Format("[ModuleZTTDrive]: Jump will cost {0:F4} energy", cost));

      double amt = 0.0;
      double maxAmt = 0.0;
      part.GetConnectedResourceTotals(PartResourceLibrary.Instance.GetDefinition(ResourceName).id, out amt, out maxAmt, false);

      if (amt < cost)
      {
          ScreenMessages.PostScreenMessage(new ScreenMessage(String.Format("Aborting jump - not enough energy"), 5.0f, ScreenMessageStyle.UPPER_CENTER));
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
      LogUtils.Log(String.Format("[ModuleZTTDrive]: Playing schematic renderer in animation"));
      schematicBubbleRenderer.enabled = true;
      schematicBubbleOn = true;
      LogUtils.Log(String.Format("[ModuleZTTDrive]: Schematic renderer on"));
      yield return 0;
    }

    /// <summary>
    /// Deactivates and animates the schematic bubble
    /// </summary>
    IEnumerator DisableSchematicRenderer()
    {
      LogUtils.Log(String.Format("[ModuleZTTDrive]: Playing schematic renderer out animation"));
      schematicBubbleOn = false;
      schematicBubbleRenderer.enabled = false;
      LogUtils.Log(String.Format("[ModuleZTTDrive]: Schematic renderer off"));
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

      LogUtils.Log(String.Format("[ModuleZTTDrive]: Found {0} safe parts and {1} unsafe parts out of {2} total", includedParts.Count, unsafeParts.Count, part.vessel.parts.Count));
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
    double CalculateJumpCost(double distance)
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
    double CalculateDispersion(double distance)
    {

      double distanceCost = distance * Settings.dispersionDistanceScale;
      return distanceCost;
    }
  }
}
