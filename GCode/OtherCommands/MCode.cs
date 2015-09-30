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
		public double Code { get; set; }

		public MCode(double code)
		{
			Code = code;
		}

		public override string GetGCode()
		{
			return string.Format(NumberFormat, "M{0}", Code);
		}
	}
}
