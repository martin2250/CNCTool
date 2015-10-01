using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCTool.GCode
{
	public enum ArcDirection
	{
		CW,
		CCW
	}

	public class Arc : Movement
	{
		public ArcDirection Direction;

		/// <summary>
		/// Absolute Position of Arc Center
		/// </summary>
		public Vector3 Center;

		public Arc(Vector3 start, Vector3 end, Vector3 center, ArcDirection direction) : base(start, end)
		{
			Center = center;
			Center.Z = 0;
			Direction = direction;
		}

		public override string GetGCode()
		{
			string code = (Direction == ArcDirection.CW) ? "G2" : "G3";

			if (End.X != Start.X)
				code += string.Format(NumberFormat, "X{0:0.###}", End.X);
			if (End.Y != Start.Y)
				code += string.Format(NumberFormat, "Y{0:0.###}", End.Y);
			if (End.Z != Start.Z)
				code += string.Format(NumberFormat, "Z{0:0.###}", End.Z);

			if (Center.X != Start.X)
				code += string.Format(NumberFormat, "I{0:0.###}", Center.X - Start.X);	//arc center is specified incrementally
			if (Center.Y != Start.Y)
				code += string.Format(NumberFormat, "J{0:0.###}", Center.Y - Start.Y);

			if (FeedRate.HasValue)
				code += string.Format(NumberFormat, "F{0:F0}", FeedRate);

			return code;
		}

		public double StartAngle
		{
			get
			{
				Vector3 relStart = Start - Center;
				double a = Math.Atan2(relStart.Y, relStart.X);
				return a >= 0 ? a : 2 * Math.PI + a;
			}
		}

		public double EndAngle
		{
			get
			{
				Vector3 relEnd = End - Center;
				double a = Math.Atan2(relEnd.Y, relEnd.X);
				return a >= 0 ? a : 2 * Math.PI + a;
			}
		}

		public double Stretch
		{
			get
			{
				double stretch = EndAngle -StartAngle; ;
				if (Direction == ArcDirection.CW)
				{
					if (stretch >= 0)
						stretch -= 2 * Math.PI;
				}
				else
				{
					if (stretch <= 0)
						stretch += 2 * Math.PI;
				}

				return stretch;
			}
		}

		public double Radius
		{
			get // get average between both radii
			{
				return (
					Math.Sqrt(Math.Pow(Start.X - Center.X, 2) + Math.Pow(Start.Y - Center.Y, 2)) + 
					Math.Sqrt(Math.Pow(End.X - Center.X, 2) + Math.Pow(End.Y - Center.Y, 2))
					) / 2;
			}
		}

		public override double Length
		{
			get
			{
				double circumference = Stretch * Radius;
				double height = End.X - Start.X;

				return Math.Sqrt(circumference * circumference + height * height);
			}
		}

		public override Vector3 Interpolate(double ratio)
		{
			double angle = StartAngle + Stretch * ratio;

			Vector3 pos = new Vector3(
				Radius * Math.Cos(angle),
				Radius * Math.Sin(angle),
				0);

			pos += Center;

			pos.Z = Start.Z + (End.Z - Start.Z) * ratio;

			return pos;
		}
	}
}
