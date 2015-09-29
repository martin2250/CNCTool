using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCTool.GCode
{
	class SetArcCenterDistanceMode : GCodeCommand
	{
		public ParseDistanceMode Mode;

		public SetArcCenterDistanceMode(ParseDistanceMode mode)
		{
			Mode = mode;
		}

		public override bool SkipInNormalization { get { return true; } }

		public override string GetGCode()
		{
			if (Mode == ParseDistanceMode.Absolute)
				return "G90.1";
			else
				return "G91.1";
		}
	}
}
