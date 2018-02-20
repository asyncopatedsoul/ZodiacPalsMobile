using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardGridHex
{

	public class GameBoardHex
	{
		public List<BoardPositionHex> tiles;
		public List<InteractionPoint> interactionPoints;

		public IList<Card> cardsPlayed;

		public GameBoardHex()
		{
			this.tiles = new List<BoardPositionHex>();

			generatePositions();
			//calculateInteractionPoints();
		}

		void generatePositions()
		{
			// starting at origin which is the center of a unit hexagon, tessellate 1 layer around the hexagon
			// repeat for n layers around the previous layer

			// layer 0
			var hexOrigin = new Hexagon(0, 0, 1);
			this.tiles.Add(new BoardPositionHex(hexOrigin));

			// 3 layers = 1 + 6 + 12 = 19
			var countLayers = 2;

			for (var l = 1; l <= countLayers; l++)
			{
				Debug.Log("tessellating layer " + l);
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

			Debug.Log("new hexagon " + this.vertices.Count + " " + this.edges.Count);
		}



		void calculateEdges()
		{
			for (var v = 0; v < this.vertices.Count; v++)
			{
				if (v == this.vertices.Count-1)
				{
					var segment = new LineSegment(this.vertices[this.vertices.Count - 1], this.vertices[0]);
					this.edges.Add(segment);

					segment.shape = this;
				}
				else
				{
					var segment = new LineSegment(this.vertices[v], this.vertices[v + 1]);
					this.edges.Add(segment);
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
				Debug.Log("tessellated vertex");
				var hexagon = new Hexagon(vertex.x, vertex.y, coreHexagon.sideLength);
				tessellatedHexes.Add(hexagon);
			}

			// add layerNumber-1 hexagons along each edge of virtual hexagon 
			foreach (var edge in virtualHexagon.edges)
			{
				foreach (var point in edge.divide(layerNumber))
				{
					Debug.Log("edge division");
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

}
