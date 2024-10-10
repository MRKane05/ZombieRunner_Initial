using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDHandler : MonoBehaviour {
	private static PlayerHUDHandler instance = null;
	public static PlayerHUDHandler Instance { get { return instance; } }

	public Image StaminaBar; //What's our players stamina?

	void Awake()
	{
		if (instance)
		{
			Debug.Log("Somehow there's a duplicate PlayerHUDHandler in the scene");
			Debug.Log(gameObject.name);
			return; //cancel this
		}

		instance = this;
	}

	void Start()
	{

	}

	public void setStaminaBar(float t)
    {
		StaminaBar.fillAmount = t;
    }
}
