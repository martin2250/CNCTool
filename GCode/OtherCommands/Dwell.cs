using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCTool.GCode
{
	class Dwell : GCodeCommand
	{
		double Seconds;

		public Dwell(double seconds)
		{
			Seconds = seconds;
		}

		public override string GetGCode()
		{
			return string.Format(NumberFormat, "G4P{0}", Seconds);
		}
	}
}
