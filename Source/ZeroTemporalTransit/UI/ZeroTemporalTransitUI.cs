﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using KSP.Localization;

namespace ZeroTemporalTransit.UI
{

  [KSPAddon(KSPAddon.Startup.Flight, false)]
  public class ZeroTemporalTransitUI : MonoBehaviour
  {
    #region Control Variables
    bool showInfoWindow = false;
    bool showConfirmWindow = false;

    bool yMode = false;
    bool xzMode = false;

    protected int windowID = new System.Random(3256231).Next();
    public Rect windowPos = new Rect(200f, 200f, 200f, 200f);
    protected bool initUI = false;
    protected UIResources resources;
    #endregion

    #region GUI Variables
    string jumpDistanceTitle = "";
    string jumpDispersionTitle = "";
    string jumpCostTitle = "";
    string dvToCircularizeTitle = "";

    string jumpDistance = "";
    string jumpDispersion = "";
    string jumpCost = "";
    string dvToCircularize = "";
    #endregion

    #region GUI Widgets

    #endregion

    #region Vessel Data
    ModuleZTTDrive driver;
    Vector3d currentTarget;
    Vector3d currentStart;
    CelestialBody targetBody;

    double currentDistance = 0d;
    double currentDispersion = 0d;

    PlanetariumCamera mapCamera;
    JumpTargetCursor cursor;
    #endregion

    public static ZeroTemporalTransitUI Instance { get; private set; }
    public UIResources GUIResources { get { return resources; } }

    protected virtual void InitUI()
    {
      if (Settings.DebugUIMode)
        Utils.Log("[UI]: Initializing");

      resources = new UIResources();

      jumpDistanceTitle = "Jump Distance";
      jumpDispersionTitle = "Estimated Dispersion";
      jumpCostTitle = "Energy Cost";
      dvToCircularizeTitle = "Estimated Relative Velocity";

      jumpDistance = "0 km";
      jumpDispersion = " 0 km";
      jumpCost = "0 PE";
      dvToCircularize = "0 m/s";

      initUI = true;
    }

    protected virtual void Awake()
    {
      Instance = this;
      if (Settings.DebugUIMode)
        Utils.Log("[UI]: Awake fired");
    }

    protected virtual void Start()
    {
      if (Settings.DebugUIMode)
        Utils.Log("[UI]: Start fired");

    }

    protected virtual void OnGUI()
    {
      if (Event.current.type == EventType.Repaint || Event.current.isMouse) {}
        Draw();
    }

    /// <summary>
    /// Draw the UI
    /// </summary>
    protected virtual void Draw()
    {
      if (!initUI)
        InitUI();

      if (showInfoWindow)
      {
        GUI.skin = HighLogic.Skin;
        windowPos = GUI.Window(windowID, windowPos, DrawWindow, new GUIContent(), GUIResources.GetStyle("window_main"));
      }
      if (HighLogic.LoadedSceneIsFlight)
      {

      }

    }

    /// <summary>
    /// Draw the window
    /// </summary>
    /// <param name="windowId">window ID</param>
    protected void DrawWindow(int windowId)
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label(dvToCircularizeTitle);
      GUILayout.Label(dvToCircularize);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label(jumpDistanceTitle);
      GUILayout.Label(jumpDistance);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label(jumpDispersionTitle);
      GUILayout.Label(jumpDispersion);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label(jumpCostTitle);
      GUILayout.Label(jumpCost);
      GUILayout.EndHorizontal();
    }

    void Update()
    {
      if (HighLogic.LoadedSceneIsFlight && MapView.MapIsEnabled)
      {
        if (driver != null)
        {
          if (xzMode)
          {
            UpdateWindowPosition();
            ListenForInput();
          }

          jumpCost = String.Format("{0:F1} ", driver.CalculateJumpCost(currentDistance));
          jumpDistance = String.Format("{0} m", currentDistance);
          jumpDispersion = String.Format("{0} m", currentDispersion);
        }
      }
    }
    void ListenForInput()
    {
      if (Input.GetKeyDown(Settings.UIYAxisKey))
      {
        yMode = true;
      }
      if (Input.GetKeyUp(Settings.UIYAxisKey))
      {
        yMode = false;
      }
      if (Input.GetKeyDown(Settings.UIJumpKey))
      {
        yMode = false;
        xzMode = false;
      }
    }

    void UpdateWindowPosition()
    {
      // First calculate all positions
      Vector3 vesselScaledSpacePosition = (Vector3)ScaledSpace.LocalToScaledSpace(driver.part.vessel.GetWorldPos3D());
      Vector3 zeroPlaneScaledSpacePosition = vesselScaledSpacePosition;  // This eventually needs to become the local targeted body

      Vector3 cursorScaledSpacePosition = GetCursorScaledSpacePosition(zeroPlaneScaledSpacePosition);
      Vector2 cursorUIPosition = GetCursorUIPosition();

      // Update the cursor object's position/scale
      float cursorScale = 25f;
      if (cursor != null)
      {
        cursor.Update(cursorScaledSpacePosition, cursorScale, zeroPlaneScaledSpacePosition, vesselScaledSpacePosition);
      }

      // Get the target position in world space
      currentTarget = ScaledSpace.ScaledToLocalSpace((Vector3d)cursorScaledSpacePosition);
      currentStart = vesselScaledSpacePosition;
      // Get the distance to the current jump point in world space
      currentDistance = Vector3d.Distance(currentTarget, driver.part.vessel.GetWorldPos3D());
      currentDispersion = driver.CalculateDispersion(currentDistance, currentStart, currentTarget);
      targetBody = FlightGlobals.getMainBody(currentTarget);
      // Update the info UI's position
      windowPos = new Rect(cursorUIPosition.x + 10f, cursorUIPosition.y -10f ,windowPos.width, windowPos.height);
    }

    /// <summary>
    /// Gets the ScaledSpace position of the mouse cursor with reference to a y-up zero plane
    /// </summary>
    Vector3 GetCursorScaledSpacePosition(Vector3 zeroPlanePosition)
    {

      Vector3 cursorScaledSpacePosition = zeroPlanePositiono;
      Ray ray = PlanetariumCamera.Camera.ScreenPointToRay(Input.mousePosition);
      Plane hPlane = new Plane(Vector3.up, zeroPlanePosition);
      float distance = 0f;
      if (hPlane.Raycast(ray, out distance)){
        // get the hit point:
        cursorScaledSpacePosition = ray.GetPoint(distance);
        Utils.Log(cursorScaledSpacePosition.ToString());
      }
      return cursorScaledSpacePosition;
    }

    /// <summary>
    /// Gets the UI position of the mouse cursor
    /// </summary>
    Vector2 GetCursorUIPosition()
    {
      Vector2 cursorUIPosition = Vector2.zero;
      cursorUIPosition = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
      return cursorUIPosition;
    }



    public void PlotJump(ModuleZTTDrive zttController, Vector3d storedDestination)
    {
      driver = zttController;

      MapView.EnterMapView();
      mapCamera = MapView.MapCamera;
      xzMode = true;
      cursor = new JumpTargetCursor(Vector3.zero, 0.0f);
      cursor.SetVisiblity(true);

      showInfoWindow = true;
      ScreenMessages.PostScreenMessage(new ScreenMessage(String.Format("Press [{0}] to set jump coordinates, hold [{1}] to adjust height", Settings.UIJumpKey, Settings.UIYAxisKey
        ), 5.0f, ScreenMessageStyle.UPPER_CENTER));
    }
  }
}
