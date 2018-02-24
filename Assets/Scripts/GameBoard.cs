using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardGridHex
{

	public class GameBoardHex
	{
		public List<BoardPositionHex> tiles;
		public List<InteractionPoint> interactionPoints;

		public List<Point> uniqueMidpoints;

		public IList<Card> cardsPlayed;

		public GameBoardHex()
		{
			this.tiles = new List<BoardPositionHex>();
			this.interactionPoints = new List<InteractionPoint>();
			this.cardsPlayed = new List<Card>();

			generatePositions();
			calculateInteractionPoints();
		}

		void generatePositions()
		{
			// starting at origin which is the center of a unit hexagon, tessellate 1 layer around the hexagon
			// repeat for n layers around the previous layer

			// layer 0
			var hexOrigin = new Hexagon(0, 0, 1, false);
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

					}
					else
					{
						var midpointExists = false;

						foreach (var existingPoint in allMidpoints)
						{
							// compare midpoint equality to points in list
							// if midpoint already in list, link redundant midpoint's gameboardHex to existing midpoint's interactionPoint
							if (Point.Equals(point, existingPoint))
							{
								midpointExists = true;

								existingPoint.iPoint.tileB = point.lineSegment.shape.tile;

							}
						}

						if (midpointExists)
						{

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

			this.uniqueMidpoints = allMidpoints;

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

		public Hexagon(float x, float y, float sideLength, bool rotate90degrees)
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

			//if (Point.AlmostEqual2sComplement(rotation, 90f, 4))
			if (rotate90degrees)
			{
				Debug.Log("rotate90 degrees hexagon");

				vertex1 = new Point(x, y + (float)Math.Sqrt(3.0) * sideLength);
				vertex2 = new Point(x + 1.5f * sideLength, y + (float)Math.Sqrt(3.0) / 2f * sideLength);
				vertex3 = new Point(x + 1.5f * sideLength, y - (float)Math.Sqrt(3.0) / 2f * sideLength);
				vertex4 = new Point(x, y - (float)Math.Sqrt(3.0) * sideLength);
				vertex5 = new Point(x - 1.5f * sideLength, y - (float)Math.Sqrt(3.0) / 2f * sideLength);
				vertex6 = new Point(x - 1.5f * sideLength, y + (float)Math.Sqrt(3.0) / 2f * sideLength);
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
			this.edgeMidpoints = new List<Point>();

			calculateEdges();

			Debug.Log(String.Format("new hexagon {0} {1} {2}", this.vertices.Count, this.edges.Count, this.edgeMidpoints.Count));
		}



		void calculateEdges()
		{
			for (var v = 0; v < this.vertices.Count; v++)
			{
				if (v == this.vertices.Count - 1)
				{
					var segment = new LineSegment(this.vertices[this.vertices.Count - 1], this.vertices[0]);

					this.edges.Add(segment);
					this.edgeMidpoints.Add(segment.midpoint);
					segment.shape = this;
				}
				else
				{
					var segment = new LineSegment(this.vertices[v], this.vertices[v + 1]);

					this.edges.Add(segment);
					this.edgeMidpoints.Add(segment.midpoint);
					segment.shape = this;
				}
			}
		}

		public static List<Hexagon> tessellateAroundHex(Hexagon coreHexagon, int layerNumber)
		{
			var tessellatedHexes = new List<Hexagon>();

			// vertices of virtual hexagon at sideLength * 3 /2 * layerNumber
			// are midpoints of outer tessellated hexagons

			var virtualHexagon = new Hexagon(coreHexagon.midpoint.x, coreHexagon.midpoint.y, coreHexagon.sideLength * (float)layerNumber, true);

			foreach (var vertex in virtualHexagon.vertices)
			{
				Debug.Log("tessellated vertex " + vertex.x + "," + vertex.y);
				var hexagon = new Hexagon(vertex.x, vertex.y, coreHexagon.sideLength, false);
				tessellatedHexes.Add(hexagon);
			}

			// add layerNumber-1 hexagons along each edge of virtual hexagon 
			foreach (var edge in virtualHexagon.edges)
			{
				foreach (var point in edge.divide(layerNumber))
				{
					Debug.Log("edge division");
					var hexagon = new Hexagon(point.x, point.y, coreHexagon.sideLength, false);
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

		public static bool HasMinimalDifference(double value1, double value2, int units)
		{
			long lValue1 = BitConverter.DoubleToInt64Bits(value1);
			long lValue2 = BitConverter.DoubleToInt64Bits(value2);

			// If the signs are different, return false except for +0 and -0.
			if ((lValue1 >> 63) != (lValue2 >> 63))
			{
				if (value1 == value2)
					return true;

				return false;
			}

			long diff = Math.Abs(lValue1 - lValue2);

			if (diff <= (long)units)
				return true;

			return false;
		}

		// The example displays the following output:
		//        01 = 0.99999999999999989: True

		public static bool Equals(Point pointA, Point pointB, int maxDeltaBits = 4)
		{
			return (HasMinimalDifference(pointA.x, pointB.x, maxDeltaBits) && HasMinimalDifference(pointA.y, pointB.y, maxDeltaBits));
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
