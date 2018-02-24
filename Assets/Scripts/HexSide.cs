using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CardGridHex;

public class HexSide : MonoBehaviour
{

	public HexEdge edge1;
	public HexEdge edge2;
	public HexEdge edge3;
	public HexEdge edge4;
	public HexEdge edge5;
	public HexEdge edge6;

	public HexFaction faction;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void SetFaction(HexFaction faction)
	{
		this.faction = faction;

		switch (faction)
		{
			case HexFaction.FactionA:

				break;

			case HexFaction.FactionB:

				break;

			default:
				break;
		}
	}
}
