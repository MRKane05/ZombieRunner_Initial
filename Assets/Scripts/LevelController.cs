using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour {
	private static LevelController instance = null;
	public static LevelController Instance { get { return instance; } }

	public PathCreator pathCreator;

	void Awake()
	{
		if (instance)
		{
			Debug.Log("Somehow there's a duplicate LevelController in the scene");
			Debug.Log(gameObject.name);
			return; //cancel this
		}

		instance = this;
	}

	public Vector3 GetEnemyDropPoint(LayerMask redropMask)
	{
		bool bFoundDropPoint = false;
		int cycles = 0;
		while (!bFoundDropPoint && cycles < 30)
		{
			//PROBLEM: This'll need re-worked once we've got curves
			//float baseRandZ = PC_FPSController.Instance.gameObject.transform.position.z + Random.RandomRange(30f, 40f);
			//float baseRandX = Random.RandomRange(-10f, 10f);

			float RandomCurveDistance = PC_FPSController.Instance.bestDistance + Random.RandomRange(30f, 40f);
			Vector3 CurveDropPoint = pathCreator.path.GetPointAtDistance(RandomCurveDistance);
			Vector3 CurveDirection = pathCreator.path.GetDirectionAtDistance(RandomCurveDistance);
			CurveDropPoint += Quaternion.AngleAxis(90f, Vector3.up) * CurveDirection * Random.RandomRange(-10f, 10f);
			//Move our curve point up so we're elevated for the hit
			CurveDropPoint += Vector3.up * 30f;

			//Do a raycast down to see if we hit the ground and not a vehicle, then plonk our enemy here
			RaycastHit hit;
			// Does the ray intersect any objects excluding the player layer
			if (Physics.Raycast(CurveDropPoint, -Vector3.up, out hit, 50f, redropMask))
			{
				Debug.DrawRay(CurveDropPoint, -Vector3.up * hit.distance, Color.yellow);
				if (hit.collider.gameObject.tag == "Ground")
				{   //We're good
					Collider[] hitColliders = Physics.OverlapSphere(hit.point, 1f);
					bool bClearDropArea = true;
					foreach (var hitCollider in hitColliders)
					{
						if (hitCollider.gameObject.tag != "Ground")
                        {
							bClearDropArea = false;
                        }
					}
					if (bClearDropArea)
					{
						return hit.point;
						bFoundDropPoint = true;
					}
				}
			}
			else
			{
				//This shouldn't have happened...
				Debug.DrawRay(CurveDropPoint, -Vector3.up * 30, Color.red);
			}
		}
		return Vector3.zero; //This is a failed drop. This needs a handler
	}
}
