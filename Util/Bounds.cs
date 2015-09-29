using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCTool.Util
{
	public class Bounds
	{
		public double MinX { get; set; }
		public double MaxX { get; set; }
		public double MinY { get; set; }
		public double MaxY { get; set; }

		public double SizeX { get { return MaxX - MinX; } }
		public double SizeY { get { return MaxY - MinY; } }

		public Bounds()
		{
			MinX = 0;
			MaxX = 0;
			MinY = 0;
			MaxY = 0;
		}

		public Bounds(double minX, double maxX, double minY, double maxY)
		{
			MinX = minX;
			MaxX = maxX;
			MinY = minY;
			MaxY = maxY;
		}

		/// <summary>
		/// returns true if the other Bounds object is fully contained inside this one
		/// </summary>
		/// <param name="other">The other Bounds object</param>
		/// <returns></returns>
		public bool Contains(Bounds other)
		{
			bool contains = true;

			contains &= MinX <= other.MinX;
			contains &= MaxX >= other.MaxX;
			contains &= MinY <= other.MinY;
			contains &= MaxY >= other.MaxY;

			return contains;
		}

		/// <summary>
		/// Expands the Bounds to contain the given point
		/// </summary>
		/// <param name="x">X coordinate of the point</param>
		/// <param name="y">Y coordinate of the point</param>
		public void ExpandTo(double x, double y)
		{
			if (x < MinX)
				MinX = x;
			if (x > MaxX)
				MaxX = x;

			if (y < MinY)
				MinY = y;
			if (y > MaxY)
				MaxY = y;
		}

	}
}
