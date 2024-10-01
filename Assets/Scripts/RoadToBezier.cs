using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using RoadArchitect;

[ExecuteInEditMode]
public class RoadToBezier : MonoBehaviour {
	public PathCreator pathCreator;
	public SplineC SplineCBase;
	public bool bGenerateSpline = false;

	void Update()
    {
		if (bGenerateSpline)
        {
			GenerateSpline();
			bGenerateSpline = false;
        }
    }

	void GenerateSpline()
    {
		//Collect our nodes from the SplineCBase....
		List<Vector3> pointArray = new List<Vector3>();
		foreach (SplineN thisNode in SplineCBase.nodes)
        {
			pointArray.Add(thisNode.pos);
        }
		Vector3[] points = pointArray.ToArray();
		pathCreator.bezierPath = new BezierPath(points, false, PathSpace.xyz);
	}
}