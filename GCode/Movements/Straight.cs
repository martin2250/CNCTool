using System.Globalization;

namespace CNCTool.GCode
{
	public class Straight : Movement
	{
		public bool Rapid;

		public Straight(Vector3 start, Vector3 end, bool rapid) : base(start, end)
		{
			Rapid = rapid;
		}

		public override string GetGCode()
		{
			string code = Rapid ? "G0" : "G1";

			if(End.X != Start.X)
				code += string.Format(NumberFormat, "X{0:0.###}", End.X);
			if (End.Y != Start.Y)
				code += string.Format(NumberFormat, "Y{0:0.###}", End.Y);
			if (End.Z != Start.Z)
				code += string.Format(NumberFormat, "Z{0:0.###}", End.Z);
			if (FeedRate.HasValue)
				code += string.Format(NumberFormat, "F{0:F0}", FeedRate);
			return code;
		}

		public override double Length
		{
			get
			{
				return (End - Start).Magnitude;
			}
		}

		public override Vector3 Interpolate(double ratio)
		{
			return Vector3.Interpolate(Start, End, ratio, true);
		}
	}
}
