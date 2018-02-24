using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgePower : MonoBehaviour {

	public int power = 1;

	public List<Transform> powerIndicators;

	// Use this for initialization
	void Start () {
		SetPower(power);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetPower(int power)
	{
		this.power = power;

		transform.GetChild(power - 1).gameObject.SetActive(true);
	}
}
