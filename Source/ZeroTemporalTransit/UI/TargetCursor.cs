using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using KSP.Localization;
using Vectrosity;

namespace ZeroTemporalTransit.UI
{
  public class JumpTargetCursor
  {
    public List<Vector3d> Vertices = new List<Vector3d>();

    private List<Vector3> vl_Vertices = new List<Vector3>();
    
    Transform cursorXform;
    Transform cursorPlaneXform;
    VectorLine billboardCircle;
    VectorLine baseMesh;
    VectorLine linkedLine;
    VectorLine originLine;
    VectorLine originLineXZ;

    public JumpTargetCursor(Vector3 position, float size)
    {
      GameObject cursor = new GameObject("JumpCursor");
      GameObject cursorPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
      GameObject.DestroyImmediate(cursorPlane.GetComponent<Collider>());
      cursorXform = cursor.GetComponent<Transform>();
      cursorXform.LookAt(Vector3.up);
      cursorPlaneXform = cursorPlane.GetComponent<Transform>();
      Mesh planeMesh = cursorPlane.GetComponent<MeshFilter>().mesh;

      cursorXform.position = Vector3.zero;
      billboardCircle = new VectorLine("JumpCursorCircle",  new List<Vector3>(100), 3f, LineType.Discrete);
      billboardCircle.MakeCircle(position, 1f);
      billboardCircle.drawTransform = cursorXform;

      baseMesh = new VectorLine("JumpCursorMesh",  new List<Vector3>(), 1.0f,  LineType.Discrete);
      baseMesh.MakeWireframe(planeMesh);
      baseMesh.drawTransform = cursorPlaneXform;

      

      cursorPlaneXform.localScale = Vector3.one*250f;
      cursorXform.localScale = Vector3.one*10f;
      baseMesh.layer = 24;
      billboardCircle.layer = 24;

      linkedLine = new VectorLine("JumpCursorMeshLine",  new List<Vector3>(2), 2.0f,LineType.Discrete);
      
      originLine = new VectorLine("JumpCursorOriginLineY",  new List<Vector3>(2), 2.0f, LineType.Discrete);
      originLineXZ = new VectorLine("JumpCursorOriginLineXZ", new List<Vector3>(2), 2.0f, LineType.Discrete);

      linkedLine.layer = 24;
      originLine.layer = 24;

      originLine.color = XKCDColors.AquaBlue;
      linkedLine.material = MapView.fetch.orbitLinesMaterial;
      originLine.material = MapView.fetch.dottedLineMaterial;
      originLineXZ.material = MapView.fetch.dottedLineMaterial;

      baseMesh.material = MapView.fetch.orbitLinesMaterial;
      billboardCircle.color = XKCDColors.AquaBlue;
      billboardCircle.material = MapView.fetch.orbitLinesMaterial;
      ///cursorPlane.GetComponent<MeshRenderer>().enabled = false;
    }

    /// <summary>
    /// Cleans up created transforms and lines
    /// </summary>
    public void DestroyCursor()
    {
      GameObject.Destroy(cursorXform.gameObject);
      GameObject.Destroy(cursorXform.gameObject);
      VectorLine.Destroy(ref linkedLine);
      VectorLine.Destroy(ref originLine);
      VectorLine.Destroy(ref originLineXZ);
      VectorLine.Destroy(ref baseMesh);
      VectorLine.Destroy(ref billboardCircle);
    }
    /// <summary>
    /// Turns the renderers on or off
    /// </summary>
    public void SetVisiblity(bool on)
    {
      linkedLine.active = on;
      originLine.active = on;
      baseMesh.active = on;
      billboardCircle.active = on;
    }

    /// <summary>
    /// Updates the cursor's position and rotation
    /// </summary>
    public void Update(Vector3 pos, float scale, Vector3 parentPos, Vector3 originPos)
    {
      UpdateBillboard(pos, scale);
      UpdateMesh(pos, scale, parentPos);
      UpdateConnections(pos, parentPos, originPos);
    }

    /// <summary>
    /// Updates the circular billboard
    /// </summary>
    void UpdateBillboard(Vector3 pos, float scale)
    {
      cursorXform.position = pos;
      //cursorXform.localScale = Vector3.one*100f;
      cursorXform.LookAt(PlanetariumCamera.Camera.transform);
      billboardCircle.Draw3D();
    }
    /// <summary>
    /// Update the wireframe zero plane mesh
    /// </summary>
    void UpdateMesh(Vector3 pos, float scale, Vector3 parentPos)
    {
      //cursorXform.position = new Vector3(pos.x, parenpostPos.y, pos.z);
      cursorPlaneXform.position = new Vector3(pos.x, parentPos.y, pos.z);
      baseMesh.Draw3D();
    }
    
    /// <summary>
    /// Updates the line connections
    /// </summary>
    void UpdateConnections(Vector3 pos,  Vector3 parentPos, Vector3 originPos)
    {
      linkedLine.points3[0] = new Vector3(pos.x, parentPos.y, pos.z);
      linkedLine.points3[1] = new Vector3(pos.x, pos.y, pos.z);
      linkedLine.Draw3D();
      originLine.points3[0] = new Vector3(pos.x, pos.y, pos.z);
      originLine.points3[1] = new Vector3(originPos.x, originPos.y, originPos.z);
      originLine.Draw3D();
      originLineXZ.points3[0] = new Vector3(pos.x, parentPos.y, pos.z);
      originLineXZ.points3[1] = new Vector3(originPos.x, originPos.y, originPos.z);
      originLineXZ.Draw3D();
    }
  }
}
