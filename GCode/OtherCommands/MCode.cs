using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCTool.GCode
{
	class MCode : GCodeCommand
	{
		public int Code { get; set; }

		public override bool SkipInNormalization { get { return false; } }

		public MCode(int code)
		{
			Code = code;
		}

		public override string GetGCode()
		{
			return $"M{Code}";
		}
	}
}
