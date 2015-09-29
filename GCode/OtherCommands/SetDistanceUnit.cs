using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCTool.GCode
{
	class SetDistanceUnit : GCodeCommand
	{
		public DistanceUnit Unit;

		public SetDistanceUnit(DistanceUnit unit)
		{
			Unit = unit;
		}

		public override bool SkipInNormalization { get { return true; } }

		public override string GetGCode()
		{
			if (Unit == DistanceUnit.MM)
				return "G21";
			else
				return "G20";
		}
	}
}
