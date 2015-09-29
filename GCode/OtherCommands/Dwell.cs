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

		public override bool SkipInNormalization { get { return true; } }

		public override string GetGCode()
		{
			return $"G4P{Seconds:0.#}";
		}
	}
}
