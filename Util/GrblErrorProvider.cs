using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CNCTool.Util
{
	static class GrblErrorProvider
	{
		public static Dictionary<int, string> Errors;

		static GrblErrorProvider()
		{
			Errors = new Dictionary<int, string>();

			Regex LineParser = new Regex(@"([0-9]+)\t([^\n^\r]*)");     //test here https://www.regex101.com/r/hO5zI1/2

			MatchCollection mc = LineParser.Matches(Properties.Resources.GrblErrors);

			foreach(Match m in mc)
			{
				int errorNo;

				if(int.TryParse(m.Groups[1].Value, out errorNo))
				{
					Errors.Add(errorNo, m.Groups[2].Value);
				}
			}

			Console.WriteLine("Loaded GRBL Error Database");
		}
	}
}
