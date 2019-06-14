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


    public JumpTargetCursor(Vector3 position, float size)
    {
      GameObject cursor = new GameObject("JumpCursor");
      GameObject cursorPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
      GameObject.DestroyImmediate(cursorPlane.GetComponent<Collider>());
      cursorXform = cursor.GetComponent<Transform>();
      cursorPlaneXform = cursorPlane.GetComponent<Transform>();
      Mesh planeMesh = cursorPlane.GetComponent<MeshFilter>().mesh;

      billboardCircle = new VectorLine("JumpCursorCircle",  new List<Vector3>(100), 50.0f);
      billboardCircle.MakeCircle(position, size);
      billboardCircle.drawTransform = cursorXform;

      baseMesh = new VectorLine("JumpCursorMesh",  new List<Vector3>(), 50.0f,  LineType.Discrete);
      baseMesh.MakeWireframe(planeMesh);
      baseMesh.drawTransform = cursorPlaneXform;

      cursorPlaneXform.localScale = Vector3.one*100f;
      baseMesh.layer = 24;

      linkedLine = new VectorLine("JumpCursorMeshLine",  new List<Vector3>(2), 50.0f);
      originLine = new VectorLine("JumpCursorOriginLine",  new List<Vector3>(2), 50.0f);

      ///cursorPlane.GetComponent<MeshRenderer>().enabled = false;
    }

    /// <summary>
    /// Cleans up created transforms and lines
    /// </summary>
    public void DestroyCursor()
    {
      GameObject.Destroy(cursorXform);
      GameObject.Destroy(cursorXform);
      VectorLine.Destroy(ref linkedLine);
      VectorLine.Destroy(ref originLine);
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
      cursorXform.localScale = scale * Vector3.one;
      cursorXform.LookAt(PlanetariumCamera.Camera.transform);
      billboardCircle.Draw3D();
    }
    /// <summary>
    /// Update the wireframe zero plane mesh
    /// </summary>
    void UpdateMesh(Vector3 pos, float scale, Vector3 parentPos)
    {
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
    }
  }
}
