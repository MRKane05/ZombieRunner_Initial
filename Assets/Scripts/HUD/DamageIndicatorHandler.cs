using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageIndicatorHandler : MonoBehaviour {
	public CanvasGroup DamageRight, DamageLeft;
	float DamageRightAlpha = 0;
	float DamageLeftAlpha = 0;

	float DamageDecayRate = 0.5f;

	void Update() {
		DamageRightAlpha = Mathf.Lerp(DamageRightAlpha, 0f, Time.deltaTime * DamageDecayRate);
		DamageLeftAlpha = Mathf.Lerp(DamageLeftAlpha, 0f, Time.deltaTime * DamageDecayRate);
		DamageRight.alpha = DamageRightAlpha;
		DamageLeft.alpha = DamageLeftAlpha;
	}

	//PROBLEM: This display is wholey substandard, but suitable for the moment
	public void TakeDamage(bool bRight) {
		if (bRight) {
			DamageRightAlpha = 1f;
		}
		else {
			DamageLeftAlpha = 1f;
		}
	}
}
