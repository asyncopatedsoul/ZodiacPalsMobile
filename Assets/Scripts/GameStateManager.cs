using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CardGridHex;

public class GameStateManager : MonoBehaviour
{

	GameStateHex gameState;


	public GameObject marker1;
	public GameObject marker2;
	public GameObject marker3;

	public float gridX = 5f;
	public float gridY = 5f;
	public float spacing = 1f;

	void PlaceMarker(float x, float y, GameObject marker)
	{
		//Vector3 pos = new Vector3(x, 0, y) * spacing;
		Vector3 pos = new Vector3(x, y, 0) * spacing;
		Instantiate(marker, pos, Quaternion.identity);
	}

	// Use this for initialization
	void Start()
	{

		//// start game 
		SetupGame();
		PlayGame();


	}

	void PlayGame()
	{

		Debug.Log("play game");

		gameState.beginGame();

		Debug.Log(gameState.report());

		gameState.endTurn();

		Debug.Log(gameState.report());

		var targetCard = gameState.playersById["gp"].cardsInHand[0];
		gameState.playCardToPosition(targetCard, 0);

		targetCard = gameState.playersById["az"].cardsInHand[0];
		gameState.playCardToPosition(targetCard, 1);

		Debug.Log(gameState.report());
	}


	void SetupGame()
	{
		Debug.Log("setup game");

		GameBoardHex gameboard = new GameBoardHex();

		Debug.Log("tile count: " + gameboard.tiles.Count);

		foreach (var hex in gameboard.tiles)
		{
			var x = hex.geometry.midpoint.x;
			var y = hex.geometry.midpoint.y;

			Debug.Log("tile at " + x.ToString() + "," + y.ToString());
			PlaceMarker(x, y, marker1);


			foreach (var vertex in hex.geometry.vertices)
			{
				Debug.Log("vertex at " + vertex.x.ToString() + "," + vertex.y.ToString());
				PlaceMarker(vertex.x, vertex.y, marker3);
			}
		}

		foreach (var midpoint in gameboard.uniqueMidpoints)
		{
			var x = midpoint.x;
			var y = midpoint.y;

			Debug.Log("unique midpoint at " + x.ToString() + "," + y.ToString());
			PlaceMarker(x, y, marker2);
		}

		gameState = new GameStateHex(gameboard);

		var cardJson = Resources.Load("testgame") as TextAsset;

		Debug.Log(cardJson.text);

		var jo = JObject.Parse(cardJson.text);

		foreach (var playerData in jo["players"].Values<JToken>())
		{
			var idPlayer = playerData["id"].ToObject<string>();
			var namePlayer = playerData["name"].ToObject<string>();

			var player = new Player(idPlayer, namePlayer);

			var cardsList = playerData["cards"].Values<JToken>();

			foreach (var cardData in cardsList)
			{

				var nameCard = cardData["name"].ToObject<string>();
				var idCard = cardData["id"].ToObject<string>();

				Debug.Log(string.Format("card name: {0} id: {1}", nameCard, idCard));

				var touchpoints = new List<CardTouchpoint>();

				foreach (var touchpointValue in cardData["touchpointValues"].Values<int>())
				{
					var touchpoint = new CardTouchpoint(touchpointValue);
					touchpoints.Add(touchpoint);

					Debug.Log(string.Format("touchpoint value: {0}", touchpointValue.ToString()));
				}

				var card = new Card(idCard, nameCard, touchpoints);

				player.drawCard(card);
			}

			gameState.registerPlayer(player);
		}

	}

	// Update is called once per frame
	void Update()
	{

	}
}
