using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCTool.GCode
{
	public abstract class Movement : GCodeCommand
	{
		public Vector3 Start, End;
		public double? FeedRate = null;

		public Movement(Vector3 start, Vector3 end)
		{
			Start = start;
			End = end;
		}

		public abstract double Length { get; }

		public Vector3 Incremental
		{
			get
			{
				return End - Start;
			}
		}

		public abstract Vector3 Interpolate(double ratio);

		public abstract IEnumerable<Movement> Split(double length);
	}
}
