using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCTool.GCode
{
	public abstract class GCodeCommand
	{
		public static NumberFormatInfo NumberFormat = new NumberFormatInfo() { NumberDecimalSeparator=".", NumberDecimalDigits=3 };	//prevent errors in regions that use comma as seperator

		public abstract string GetGCode();

		public abstract bool SkipInNormalization { get; }	//skip eg G20 or G90.1 to provide maximum compatibility
	}
}
