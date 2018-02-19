using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CardGrid;

public class GameStateManager : MonoBehaviour
{

	// Use this for initialization
	void Start()
	{



		Debug.Log("start game");

		GameBoard gameboard = new GameBoard(3, 3);

		GameState gameState = new GameState(gameboard);

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

		// start game 

		gameState.beginGame();

		Debug.Log(gameState.report());

		gameState.endTurn();

		Debug.Log(gameState.report());

		var targetCard = gameState.playersById["gp"].cardsInHand[0];
		gameState.playCardToPosition(targetCard, 0, 0);

		targetCard = gameState.playersById["az"].cardsInHand[0];
		gameState.playCardToPosition(targetCard, 0, 1);

		Debug.Log(gameState.report());
	}

	// Update is called once per frame
	void Update()
	{

	}
}
