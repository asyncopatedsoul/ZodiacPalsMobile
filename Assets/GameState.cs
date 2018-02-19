using System;
using System.Collections.Generic;

namespace CardGridHex
{

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

	//public class GameState
	//{
	//	public IList<Player> players;
	//	public IDictionary<string, Player> playersById;
	//	public IList<RecordCardPlayed> historyCardsPlayed;

	//	public Player activePlayer;
	//	int indexActivePlayer;
	//	GameBoard gameBoard;




	//	public GameState(GameBoard gameBoard)
	//	{
	//		players = new List<Player>();
	//		playersById = new Dictionary<string, Player>();
	//		historyCardsPlayed = new List<RecordCardPlayed>();

	//		activePlayer = null;
	//		this.gameBoard = gameBoard;
	//		indexActivePlayer = 0;
	//	}

	//	public void registerPlayer(Player player)
	//	{
	//		this.players.Add(player);
	//		this.playersById.Add(player.id, player);
	//	}

	//	public void beginGame()
	//	{
	//		activePlayer = players[indexActivePlayer];
	//	}

	//	public void endTurn()
	//	{
	//		indexActivePlayer++;

	//		if (indexActivePlayer == players.Count)
	//		{
	//			indexActivePlayer = 0;
	//		}

	//		activePlayer = players[indexActivePlayer];
	//	}

	//	public void playCardToPosition(Card targetCard, int targetRow, int targetColumn)
	//	{
	//		BoardPosition targetPosition = gameBoard.getPositionAtRowAndColumn(targetRow, targetColumn);

	//		targetPosition.bindToCard(targetCard);

	//		gameBoard.cardsPlayed.Add(targetCard);

	//		var recordCardPlayed = new RecordCardPlayed(targetCard, targetPosition);
	//		historyCardsPlayed.Add(recordCardPlayed);

	//		// detect triggers card interaction



	//	}


	//	public string report()
	//	{
	//		string reportText = "";

	//		reportText += "active player:" + this.activePlayer.name;

	//		if (historyCardsPlayed.Count > 0)
	//		{

	//			var lastRecord = historyCardsPlayed[historyCardsPlayed.Count - 1];
	//			reportText += String.Format("last action: {0} to {1},{2}",
	//										lastRecord.card.name,
	//										lastRecord.position.row, lastRecord.position.column);
	//		}

	//		return reportText;
	//	}
	//}


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

	public class GameBoardHex
	{
		public List<BoardPositionHex> tiles;
		public List<InteractionPoint> interactionPoints;

		public IList<Card> cardsPlayed;

		public GameBoardHex()
		{
			this.tiles = new List<BoardPositionHex>();

			generatePositions();
			calculateInteractionPoints();
		}

		void generatePositions()
		{
			// starting at origin which is the center of a unit hexagon, tessellate 1 layer around the hexagon
			// repeat for n layers around the previous layer

			// layer 0
			var hexOrigin = new Hexagon(0, 0, 1);
			this.tiles.Add(new BoardPositionHex(hexOrigin));

			// 3 layers = 1 + 6 + 12 = 19
			var countLayers = 3;

			for (var l = 1; l <= countLayers; l++)
			{
				var hexes = Hexagon.tessellateAroundHex(hexOrigin, l);

				foreach (var hex in hexes)
				{
					this.tiles.Add(new BoardPositionHex(hex));
				}
			}

		}

		void calculateInteractionPoints()
		{
			var allMidpoints = new List<Point>();

			// aggregate list of unique edge midpoints from all hexes

			foreach (var tile in this.tiles)
			{
				foreach (var point in tile.geometry.edgeMidpoints)
				{
					if (allMidpoints.Count == 0)
					{
						allMidpoints.Add(point);

						var iPoint = new InteractionPoint();
						point.iPoint = iPoint;
						iPoint.tileA = point.lineSegment.shape.tile;
						this.interactionPoints.Add(iPoint);

						continue;
					};

					foreach (var existingPoint in allMidpoints)
					{
						// compare midpoint equality to points in list
						// if midpoint already in list, link redundant midpoint's gameboardHex to existing midpoint's interactionPoint
						if (Point.Equals(point, existingPoint))
						{
							existingPoint.iPoint.tileB = point.lineSegment.shape.tile;
						}
						// if midpoint not already in list, add new interaction point and link new midpoint's hex to interactionPoint
						else
						{

							allMidpoints.Add(point);

							var iPoint = new InteractionPoint();
							point.iPoint = iPoint;
							iPoint.tileA = point.lineSegment.shape.tile;
							this.interactionPoints.Add(iPoint);
						}

					}
				}
			}

			//foreach (var point in allMidpoints)
			//{
			//	if (allMidpoints.Count == 0)
			//	{
			//		allMidpoints.Add(point);

			//		var iPoint = new InteractionPoint();
			//		point.iPoint = iPoint;
			//		iPoint.tileA = point.lineSegment.shape.tile;
			//		this.interactionPoints.Add(iPoint);

			//		continue;
			//	};

			//	foreach (var existingPoint in allMidpoints)
			//	{
			//		// compare midpoint equality to points in list
			//		// if midpoint already in list, link redundant midpoint's gameboardHex to existing midpoint's interactionPoint
			//		if (Point.Equals(point, existingPoint))
			//		{
			//			existingPoint.iPoint.tileB = point.lineSegment.shape.tile;
			//		}
			//		// if midpoint not already in list, add new interaction point and link new midpoint's hex to interactionPoint
			//		else
			//		{

			//			allMidpoints.Add(point);

			//			var iPoint = new InteractionPoint();
			//			point.iPoint = iPoint;
			//			iPoint.tileA = point.lineSegment.shape.tile;
			//			this.interactionPoints.Add(iPoint);
			//		}

			//	}
			//}


		}

	}

	public class BoardPositionHex
	{
		public Hexagon geometry;
		public Card activeCard;
		public int tileID;

		public List<InteractionPoint> interactionPoints;

		public BoardPositionHex(Hexagon geometry)
		{
			this.geometry = geometry;
			this.activeCard = null;

			this.interactionPoints = new List<InteractionPoint>();

			geometry.tile = this;
		}

		public void bindToCard(Card card)
		{
			this.activeCard = card;
		}
	}



	public class Hexagon
	{
		public Point midpoint;
		public IList<Point> vertices;
		public IList<LineSegment> edges;
		public IList<Point> edgeMidpoints;

		public BoardPositionHex tile;

		public float sideLength;

		public Hexagon(float x, float y, float sideLength, float rotation = 0)
		{
			this.sideLength = sideLength;

			// from midpoint x, y
			this.midpoint = new Point(x, y);

			Point vertex1;
			Point vertex2;
			Point vertex3;
			Point vertex4;
			Point vertex5;
			Point vertex6;

			if (Point.AlmostEqual2sComplement(rotation, 90f, 4))
			{
				vertex1 = new Point(x, y + sideLength);
				vertex2 = new Point(x + (float)Math.Sqrt(3.0) / 2f * sideLength, y + 0.5f * sideLength);
				vertex3 = new Point(x + (float)Math.Sqrt(3.0) / 2f * sideLength, y - 0.5f * sideLength);
				vertex4 = new Point(x, y - sideLength);
				vertex5 = new Point(x - (float)Math.Sqrt(3.0) / 2f * sideLength, y - 0.5f * sideLength);
				vertex6 = new Point(x - (float)Math.Sqrt(3.0) / 2f * sideLength, y + 0.5f * sideLength);
			}
			else
			{
				vertex1 = new Point(x - 0.5f * sideLength, y + (float)Math.Sqrt(3.0) / 2f * sideLength);
				vertex2 = new Point(x + 0.5f * sideLength, y + (float)Math.Sqrt(3.0) / 2f * sideLength);
				vertex3 = new Point(x + sideLength, y);
				vertex4 = new Point(x + 0.5f * sideLength, y - (float)Math.Sqrt(3.0) / 2f * sideLength);
				vertex5 = new Point(x - 0.5f * sideLength, y - (float)Math.Sqrt(3.0) / 2f * sideLength);
				vertex6 = new Point(x - sideLength, y);
			}

			this.vertices = new List<Point> { vertex1, vertex2, vertex3, vertex4, vertex5, vertex6 };
			this.edges = new List<LineSegment>();

			calculateEdges();
		}



		void calculateEdges()
		{
			for (var v = 0; v < this.vertices.Count; v++)
			{
				var segment = new LineSegment(this.vertices[v], this.vertices[v + 1]);
				this.edges.Add(segment);

				v++;

				if (v == this.vertices.Count)
				{
					segment = new LineSegment(this.vertices[this.vertices.Count - 1], this.vertices[0]);
					this.edges.Add(segment);

					segment.shape = this;
				}
			}
		}

		public static List<Hexagon> tessellateAroundHex(Hexagon coreHexagon, int layerNumber)
		{
			var tessellatedHexes = new List<Hexagon>();

			// vertices of virtual hexagon at sideLength * 3 /2 * layerNumber
			// are midpoints of outer tessellated hexagons

			var virtualHexagon = new Hexagon(coreHexagon.midpoint.x, coreHexagon.midpoint.y, coreHexagon.sideLength * 3f / 2f * (float)layerNumber, 90f);

			foreach (var vertex in virtualHexagon.vertices)
			{
				var hexagon = new Hexagon(vertex.x, vertex.y, coreHexagon.sideLength);
				tessellatedHexes.Add(hexagon);
			}

			// add layerNumber-1 hexagons along each edge of virtual hexagon 
			foreach (var edge in virtualHexagon.edges)
			{
				foreach (var point in edge.divide(layerNumber))
				{
					var hexagon = new Hexagon(point.x, point.y, coreHexagon.sideLength);
					tessellatedHexes.Add(hexagon);
				}
			}

			return tessellatedHexes;
		}

	}

	public class Point
	{
		public float x;
		public float y;

		public InteractionPoint iPoint;

		public LineSegment lineSegment;

		public Point(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public static unsafe int FloatToInt32Bits(float f)
		{
			return *((int*)&f);
		}

		public static bool AlmostEqual2sComplement(float a, float b, int maxDeltaBits)
		{
			int aInt = FloatToInt32Bits(a);
			if (aInt < 0)
				aInt = Int32.MinValue - aInt;

			int bInt = FloatToInt32Bits(b);
			if (bInt < 0)
				bInt = Int32.MinValue - bInt;

			int intDiff = Math.Abs(aInt - bInt);
			return intDiff <= (1 << maxDeltaBits);
		}

		public static bool Equals(Point pointA, Point pointB, int maxDeltaBits = 4)
		{
			return (AlmostEqual2sComplement(pointA.x, pointB.x, maxDeltaBits) && AlmostEqual2sComplement(pointA.y, pointB.y, maxDeltaBits));
		}
	}

	public class LineSegment
	{
		public Point pointA;
		public Point pointB;
		public Point midpoint;

		public Hexagon shape;

		public LineSegment(Point pointA, Point pointB)
		{
			this.pointA = pointA;
			this.pointB = pointB;

			this.midpoint = new Point((pointA.x + pointB.x) / 2f, (pointA.y + pointB.y) / 2f);

			this.pointA.lineSegment = this;
			this.pointB.lineSegment = this;
			this.midpoint.lineSegment = this;
		}

		public List<Point> divide(int countDivisions)
		{
			var divisionPoints = new List<Point>();

			if (countDivisions == 2)
			{
				divisionPoints.Add(this.midpoint);
			}
			else
			{
				for (int d = 1; d < countDivisions; d++)
				{
					var point = new Point((pointA.x + pointB.x) * (float)d / (float)countDivisions, (pointA.y + pointB.y) * (float)d / (float)countDivisions);
					divisionPoints.Add(point);
				}
			}

			return divisionPoints;
		}
	}


	public class GameBoard
	{

		public int rows;
		public int columns;

		public IList<BoardPosition> boardPositions;
		public IList<Card> cardsPlayed;

		public GameBoard(int rows, int columns)
		{
			this.rows = rows;
			this.columns = columns;

			this.boardPositions = new List<BoardPosition>();
			this.cardsPlayed = new List<Card>();

			generatePositions();
		}

		void generatePositions()
		{
			for (int r = 0; r < rows; r++)
			{
				for (int c = 0; c < columns; c++)
				{
					BoardPosition position = new BoardPosition(r, c);
					boardPositions.Add(position);
				}
			}
		}

		public BoardPosition getPositionAtRowAndColumn(int targetRow, int targetColumn)
		{
			int targetIndex = targetRow + (targetColumn * this.rows);

			return this.boardPositions[targetIndex];
		}
	}

	public class BoardPosition
	{
		public int row;
		public int column;

		public Card activeCard;

		public BoardPosition(int row, int column)
		{
			this.row = row;
			this.column = column;

			activeCard = null;
		}

		public void bindToCard(Card card)
		{
			this.activeCard = card;
		}

		//void registerAdjacentPositions()
		//{

		//}
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
