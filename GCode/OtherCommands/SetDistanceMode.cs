using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCTool.GCode
{
	class SetDistanceMode : GCodeCommand
	{
		public ParseDistanceMode Mode;

		public SetDistanceMode(ParseDistanceMode mode)
		{
			Mode = mode;
		}

		public override bool SkipInNormalization { get { return true; } }

		public override string GetGCode()
		{
			if (Mode == ParseDistanceMode.Absolute)
				return "G90";
			else
				return "G91";
		}
	}
}
