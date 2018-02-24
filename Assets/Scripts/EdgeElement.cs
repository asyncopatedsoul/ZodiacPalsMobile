using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardGridHex;

public class EdgeElement : MonoBehaviour
{

	public HexElement element = HexElement.None;

	public Transform elementMarkerRed;
	public Transform elementMarkerGreen;
	public Transform elementMarkerBlue;
	public Transform elementMarkerWhite;
	public Transform elementMarkerBlack;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	public void SetElement(HexElement element)
	{
		this.element = element;

		foreach (Transform child in transform)
		{
			child.gameObject.SetActive(false);
		}

		switch (element)
		{
			case (HexElement.Black):
				elementMarkerBlack.gameObject.SetActive(true);
				break;

			case (HexElement.White):
				elementMarkerWhite.gameObject.SetActive(true);
				break;

			case (HexElement.Red):
				elementMarkerRed.gameObject.SetActive(true);
				break;

			case (HexElement.Green):
				elementMarkerGreen.gameObject.SetActive(true);
				break;

			case (HexElement.Blue):
				elementMarkerBlue.gameObject.SetActive(true);
				break;

			default:
				break;
		}

	}
}
