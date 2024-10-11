using PathCreation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ClutterPropPosition
{
	public Vector3 pos = Vector3.zero;
	public Vector3 rot = Vector3.zero;
	public string objectName = "";	//Don't feel this will be useful
	public GameObject clutterObject;

	public void toPosition()
    {
		pos = clutterObject.transform.position;
		rot = clutterObject.transform.eulerAngles;
    }
	public string toString()
    {
		string outString = "";
		outString += "pos:" + pos.ToString() + "\n";
		outString += "rot:" + rot.ToString() + "\n";
		return outString;
    }
}


//Just a script to drop thins down along a line and try to flesh out levels to see how they feel
[ExecuteInEditMode]
public class RandomHighwaySpawner : MonoBehaviour {
	public List<ClutterPropPosition> clutterProps = new List<ClutterPropPosition>();
	public bool bDoSpawn = false;	//Spawns all of our props
	public bool bWritePositions = false;	//Writes positions to a txt file after doing a physics drop. This will also remove the rigidbodies
	public bool bSetPositions = false;	//Reads in the "dropped" positions after hitting play
	public bool bClearChildren = false;
	public List<GameObject> PrefabObjects = new List<GameObject>();
	public float SpawnLength = 500;
	public Range SpawnWidth = new Range(3, 7);
	public Range DropHeight = new Range(3, 15); //What height will the props be spawned in at before doing the "drop"
	public PathCreator pathCreator;	//The path that we'll spawn clutter along
	public Vector2 SpawnFrequency = new Vector2(3, 5);	//in metres


	// Update is called once per frame
	void Update () {
		if (bDoSpawn) {
			bDoSpawn = false;
			DropPrefabs();
		}
		if (bClearChildren) {
			bClearChildren = false;

			StartCoroutine(DoClearChildren());
		}
		if (bWritePositions)
        {
			bWritePositions = false;
			ExportCurrentPositions();
        }
		if (bSetPositions)
        {
			bSetPositions = false;
			ImportAndSetPositions();
        }
	}

	void ExportCurrentPositions()
    {
		string PosRotString = "";
		for (int i=0; i<clutterProps.Count; i++)
        {
			PosRotString += i.ToString() + "\n";
			clutterProps[i].toPosition();
			PosRotString += clutterProps[i].toString();
        }
		
		string path = Application.streamingAssetsPath + "/ClutterPositions.txt";
		//Create File if it doesn't exist
		if (!File.Exists(path))
		{
			File.WriteAllText(path, PosRotString);
		}
	}

	Vector3 Vec3FromString(string vstring)
	{
		//Remove our brackets
		vstring = vstring.Replace('(', ' ');
		vstring = vstring.Replace(')', ' ');
		//Debug.Log(vstring);
		string[] values = vstring.Split(',');
		float vecX = float.Parse(values[0].Trim());
		float vecY = float.Parse(values[1].Trim());
		float vecZ = float.Parse(values[2].Trim());
        //Debug.Log("X: " + vecX + " Y: " + vecY + " Z: " + vecZ);
        return new Vector3(vecX, vecY, vecZ);
	}

	void ImportAndSetPositions()
    {
		string filePath = Application.streamingAssetsPath + "/ClutterPositions.txt";


		// Open the file and read each line
		using (StreamReader reader = new StreamReader(filePath))
		{
			string line;
			int currentIndex = -1;
			Vector3 currentPos = Vector3.zero;
			Vector3 currentRot = Vector3.zero;
			while ((line = reader.ReadLine()) != null)
			{
				// Process the data on each line
				if (!line.Contains("pos") && !line.Contains("rot")) { //this is our index. We'll want to add our current entry and then set it
					if (currentIndex >=0)
                    {
						clutterProps[currentIndex].pos = currentPos;
						clutterProps[currentIndex].rot = currentRot;
						//Debug.Log("Set prop position: " + currentIndex);
						clutterProps[currentIndex].clutterObject.transform.position = currentPos;
						clutterProps[currentIndex].clutterObject.transform.eulerAngles = currentRot;
						DestroyImmediate(clutterProps[currentIndex].clutterObject.GetComponent<Rigidbody>());	//Remove our rigidbody
					}
					currentIndex = int.Parse(line);
				} else if (line.Contains("pos")) {
					//We want to trim out the first part of our line and use the second
					string[] data = line.Split(':');
					currentPos = Vec3FromString(data[1]);

				}
				else if (line.Contains("rot"))
				{
					//We want to trim out the first part of our line and use the second
					string[] data = line.Split(':');
					currentRot = Vec3FromString(data[1]);
				}
			}
		}
		


		

		/*
		using (StreamReader sr = new StreamReader(path))
		{

			string[] Readfile = new string[File.ReadAllLines(path).Length];
			string line;

			int t = 0;
			// Read and display lines from the file until the end of 
			// the file is reached.
			while ((line = sr.ReadLine()) != null)
			{
				Debug.Log(Readfile[t]);

				t += 1;
			}
		}*/
	}

    IEnumerator DoClearChildren()
    {
		clutterProps.Clear();
		clutterProps = new List<ClutterPropPosition>();
		while (gameObject.transform.childCount > 0)
        {
			foreach (Transform child in transform)
			{
				DestroyImmediate(child.gameObject);
			}
		}
		yield return null;
    }

	void DropPrefabs() {
		//So logically our highway has two sides, and goes for a set length. To kick off with we need to be spawning along these
		float Lane_LeftCrawl = 0;
		float Lane_RightCrawl = 0;

		//I might simplify everything to just dump junk everywhere
		while (Lane_LeftCrawl < SpawnLength) {
			Lane_LeftCrawl += UnityEngine.Random.Range(SpawnFrequency.x, SpawnFrequency.y);
			//A new spawn position X
			float SpawnX = -SpawnWidth.GetRandom();

			GameObject newObject = Instantiate(PrefabObjects[UnityEngine.Random.Range(0, PrefabObjects.Count)], transform);

			Vector3 linePos = pathCreator.path.GetPointAtDistance(Lane_LeftCrawl);
			Vector3 forward = pathCreator.path.GetDirectionAtDistance(Lane_LeftCrawl);

			newObject.transform.localPosition = linePos + Quaternion.AngleAxis(90f, Vector3.up) * forward * SpawnX + Vector3.up * DropHeight.GetRandom();//new Vector3(SpawnX, 0, Lane_LeftCrawl);

			newObject.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(-60, 60), 45 + UnityEngine.Random.value * 90, UnityEngine.Random.Range(-60, 60));

			Rigidbody newRigid = newObject.AddComponent<Rigidbody>();
			newRigid.mass = 10f;

			ClutterPropPosition newClutterProp = new ClutterPropPosition();
			newClutterProp.clutterObject = newObject;
			clutterProps.Add(newClutterProp);
		}

		while (Lane_RightCrawl < SpawnLength)
		{
			Lane_RightCrawl += UnityEngine.Random.Range(SpawnFrequency.x, SpawnFrequency.y);
			//A new spawn position X
			float SpawnX = SpawnWidth.GetRandom();

			GameObject newObject = Instantiate(PrefabObjects[UnityEngine.Random.Range(0, PrefabObjects.Count)], transform);
			Vector3 linePos = pathCreator.path.GetPointAtDistance(Lane_RightCrawl);
			Vector3 forward = pathCreator.path.GetDirectionAtDistance(Lane_RightCrawl);

			newObject.transform.localPosition = linePos + Quaternion.AngleAxis(90f, Vector3.up) * forward * SpawnX + Vector3.up * DropHeight.GetRandom();//new Vector3(SpawnX, 0, Lane_LeftCrawl);

			newObject.transform.eulerAngles = new Vector3(UnityEngine.Random.Range(-60, 60), 45 + UnityEngine.Random.value * 90, UnityEngine.Random.Range(-60, 60));

			Rigidbody newRigid = newObject.AddComponent<Rigidbody>();
			newRigid.mass = 10f;

			ClutterPropPosition newClutterProp = new ClutterPropPosition();
			newClutterProp.clutterObject = newObject;
			clutterProps.Add(newClutterProp);
		}

	}
}
