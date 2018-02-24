using System;
using System.Collections.Generic;

namespace CardGridHex
{

	public enum HexElement
	{
		None,
		Red,
		Green,
		Blue,
		White,
		Black
	}

	public enum HexFaction
	{
		FactionA,
		FactionB,
		FactionC,
		FactionD,
		FactionE,
		FactionF
	}

	public class RecordCardPlayed
	{
		public Card card;
		public BoardPositionHex position;

		public RecordCardPlayed(Card card, BoardPositionHex position)
		{
			this.card = card;
			this.position = position;
		}
	}


	public class GameStateHex
	{
		public IList<Player> players;
		public IDictionary<string, Player> playersById;
		public IList<RecordCardPlayed> historyCardsPlayed;

		public Player activePlayer;
		int indexActivePlayer;
		GameBoardHex gameBoard;


		public GameStateHex(GameBoardHex gameBoard)
		{
			players = new List<Player>();
			playersById = new Dictionary<string, Player>();
			historyCardsPlayed = new List<RecordCardPlayed>();

			activePlayer = null;
			this.gameBoard = gameBoard;
			indexActivePlayer = 0;
		}

		public void registerPlayer(Player player)
		{
			this.players.Add(player);
			this.playersById.Add(player.id, player);
		}

		public void beginGame()
		{
			activePlayer = players[indexActivePlayer];
		}

		public void endTurn()
		{
			indexActivePlayer++;

			if (indexActivePlayer == players.Count)
			{
				indexActivePlayer = 0;
			}

			activePlayer = players[indexActivePlayer];
		}

		public void playCardToPosition(Card targetCard, int targetTilePosition)
		{
			BoardPositionHex targetTile = gameBoard.tiles[targetTilePosition];

			targetTile.bindToCard(targetCard);

			gameBoard.cardsPlayed.Add(targetCard);

			var recordCardPlayed = new RecordCardPlayed(targetCard, targetTile);
			historyCardsPlayed.Add(recordCardPlayed);

			// detect triggers card interaction

		}


		public string report()
		{
			string reportText = "";

			reportText += "active player:" + this.activePlayer.name;

			if (historyCardsPlayed.Count > 0)
			{

				var lastRecord = historyCardsPlayed[historyCardsPlayed.Count - 1];
				reportText += String.Format("last action: {0} to {1}",
											lastRecord.card.name,
											lastRecord.position.tileID);
			}

			return reportText;
		}
	}


	public class CardTouchpoint
	{
		public int powerValue;

		public CardTouchpoint(int powerValue)
		{
			this.powerValue = powerValue;
		}
	}

	public class Card
	{
		public String id;
		public String name;

		IList<CardTouchpoint> touchpoints;
		Player owner;
		Player controller;

		public Card(String id, String name, IList<CardTouchpoint> touchpoints)
		{
			this.id = id;
			this.name = name;
			this.touchpoints = touchpoints;
		}
	}

	public class InteractionPoint
	{
		// point where two cards interact, along adjacent edges of tile geometries

		public BoardPositionHex tileA;
		public BoardPositionHex tileB;

		public InteractionPoint()
		{
			this.tileA = null;
			this.tileB = null;
		}

		public InteractionPoint(BoardPositionHex tileA, BoardPositionHex tileB)
		{
			this.tileA = tileA;
			this.tileB = tileB;
		}

		public Boolean hasNeighbors()
		{
			return (tileA.activeCard != null && tileB.activeCard != null);
		}
	}












	public class Player
	{
		public String id;
		public String name;

		public readonly IList<Card> cardsInHand;


		public Player(string id, string name)
		{
			this.id = id;
			this.name = name;

			this.cardsInHand = new List<Card>();
		}

		public void drawCard(Card card)
		{
			cardsInHand.Add(card);
		}
	}

	public class CardInteraction
	{
		Card cardA;
		Card cardB;
		CardTouchpoint sideA;
		CardTouchpoint sideB;


		Card winner;
		Card loser;

		public CardInteraction(Card cardA, CardTouchpoint sideA, Card cardB, CardTouchpoint sideB)
		{
			this.cardA = cardA;
			this.cardB = cardB;
		}

		public Card resolve()
		{
			if (sideA.powerValue > sideB.powerValue)
			{
				winner = cardA;
				loser = cardB;
			}
			else
			{
				winner = cardB;
				loser = cardA;
			}

			return winner;
		}

		public String report()
		{
			return String.Format("{0} {1}", winner.name, loser.name);
		}
	}

}
