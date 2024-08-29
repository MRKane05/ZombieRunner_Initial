using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour {
    private void OnTriggerEnter(Collider other)
    {
        other.gameObject.transform.position = Vector3.up;
    }
}
