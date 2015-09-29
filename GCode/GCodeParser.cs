using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CNCTool.GCode
{
	public class GCodeParser
	{
		private static Regex GCodeSplitter = new Regex(@"([A-Z])\s*(\-?\d+\.?\d*)", RegexOptions.Compiled);

		public ParseDistanceMode DistanceMode;
		public ParseDistanceMode ArcDistanceMode;
		public DistanceUnit Units;
		public Vector3 Position;
		public ToolPath ToolPath;

		public void Reset()
		{
			DistanceMode = ParseDistanceMode.Absolute;
			ArcDistanceMode = ParseDistanceMode.Incremental;
			Units = DistanceUnit.MM;
			Position = new Vector3(0.0f, 0.0f, 0.0f);
			ToolPath = new ToolPath();
		}

		private ParseLineResult ParseLine(string line)
		{
			{	//cleanup
				int commentIndex = line.IndexOfAny(new char[] { ';', '(' });

				if (commentIndex >= 0)
					line = line.Remove(commentIndex);

				if (string.IsNullOrWhiteSpace(line))
					return new ParseLineResult(ParseLineResult.Status.NoCommand);
			}

			MatchCollection matches = GCodeSplitter.Matches(line);

			foreach(Match match in matches)
			{
				





			}



			return new ParseLineResult(ParseLineResult.Status.Error);
		}

		public struct ParseLineResult
		{
			public int ToolPathIndex;
			public Status Result;
			public string Error;

			public ParseLineResult(Status result, int toolPathIndex = -1)
			{
				ToolPathIndex = toolPathIndex;
				Result = result;
				Error = null;
			}

			public ParseLineResult(Status result, string error)
			{
				ToolPathIndex = -1;
				Result = result;
				Error = error;
			}

			public enum Status
			{
				Error,
				UnknownCommand,
				NoCommand,
				Movement,
				OtherCommand
			}
		}
	}

	public enum ParseDistanceMode
	{
		Absolute,
		Incremental
	}

	public enum DistanceUnit
	{
		MM,
		Inches
	}
}
