using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Just a script to drop thins down along a line and try to flesh out levels to see how they feel
[ExecuteInEditMode]
public class RandomHighwaySpawner : MonoBehaviour {
	public bool bDoSpawn = false;
	public bool bClearChildren = false;
	public List<GameObject> PrefabObjects = new List<GameObject>();
	public float SpawnLength = 500;
	public Vector2 SpawnWidth = new Vector2(3, 7);

	public Vector2 SpawnFrequency = new Vector2(3, 5);	//in metres


	// Update is called once per frame
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
		//So logically our highway has two sides, and goes for a set length. To kick off with we need to be spawning along these
		float Lane_LeftCrawl = 0;
		float Lane_RightCrawl = 0;

		while (Lane_LeftCrawl < SpawnLength) {
			Lane_LeftCrawl += Random.RandomRange(SpawnFrequency.x, SpawnFrequency.y);
			//A new spawn position X
			float SpawnX = -Random.RandomRange(SpawnWidth.x, SpawnWidth.y);

			GameObject newObject = Instantiate(PrefabObjects[Random.RandomRange(0, PrefabObjects.Count)], transform);
			newObject.transform.localPosition = new Vector3(SpawnX, 0, Lane_LeftCrawl);

			newObject.transform.eulerAngles = new Vector3(0, 45 + Random.value * 90, 0);
		}

		while (Lane_RightCrawl < SpawnLength) {
			Lane_RightCrawl += Random.RandomRange(SpawnFrequency.x, SpawnFrequency.y);
			//A new spawn position X
			float SpawnX = Random.RandomRange(SpawnWidth.x, SpawnWidth.y);

			GameObject newObject = Instantiate(PrefabObjects[Random.RandomRange(0, PrefabObjects.Count)], transform);
			newObject.transform.localPosition = new Vector3(SpawnX, 0, Lane_RightCrawl);

			newObject.transform.eulerAngles = new Vector3(0, 225 + Random.value * 90, 0);
		}
	}
}
