using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GroundSpawner : MonoBehaviour {

	public float spawnDistance = 600;
	public GameObject groundPrefab;

	public float prefabFrequency = 32;
	
	public bool bDoSpawn = false;
	public bool bClearChildren = false;

	void Update () {
		if (bDoSpawn) {
			bDoSpawn = false;
			DropPrefabs();
		}
		if (bClearChildren) {
			bClearChildren = false;
			foreach(Transform child in transform) {
				DestroyImmediate(child.gameObject);
			}
		}
	}

	void DropPrefabs() {
		float linePos = 0f;
		Debug.Log("Placing road tiles");
		while (linePos < spawnDistance) {

			GameObject newObject = Instantiate(groundPrefab, transform);
			newObject.transform.localPosition = new Vector3(0, 0, linePos);

			linePos += prefabFrequency;
		}
	}

}
