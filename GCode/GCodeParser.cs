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
		public GCodeParser()
		{
			Reset();
		}

		private static Regex GCodeSplitter = new Regex(@"([A-Z])\s*(\-?\d+\.?\d*)", RegexOptions.Compiled);

		public ParseDistanceMode DistanceMode;
		public ParseDistanceMode ArcDistanceMode;
		public DistanceUnit Unit;
		public Vector3 Position;
		public ToolPath ToolPath;
		public int LastGCode;

		public void Reset()
		{
			DistanceMode = ParseDistanceMode.Absolute;
			ArcDistanceMode = ParseDistanceMode.Incremental;
			Unit = DistanceUnit.MM;
			Position = new Vector3(0.0f, 0.0f, 0.0f);
			ToolPath = new ToolPath();
			LastGCode = -1;
		}

		private static char[] PositionWords = new char[] { 'X', 'Y', 'Z', 'I', 'J', 'F' };

		/// <summary>
		/// 
		/// </summary>
		/// <param name="line"></param>
		/// <returns>index of created movement in toolpath</returns>
		public int ParseLine(string line)
		{
			{   //cleanup
				int commentIndex = line.IndexOfAny(new char[] { ';', '(' });

				if (commentIndex >= 0)
					line = line.Remove(commentIndex);

				if (string.IsNullOrWhiteSpace(line))
					return -1;

				line = line.ToUpperInvariant();
			}

			double? x = null, y = null, z = null, i = null, j = null, r = null, f = null, p = null;

			int LineMovementCode = -1;

			MatchCollection matches = GCodeSplitter.Matches(line);

			foreach (Match match in matches)
			{
				char Letter = match.Groups[1].Value[0];
				double Arg = double.Parse(match.Groups[2].Value, GCodeCommand.NumberFormat);  //no tryparse, the regex should only accept valid numbers (except for out of range)

				if (Letter == 'M')
				{
					if (LineMovementCode != -1)
						throw new Exception("GCode with parameters must be the last in a line");

					ToolPath.Add(new MCode(Arg));
					continue;
				}

				if (Letter == 'G')
				{
					if (LineMovementCode != -1)
						throw new Exception("GCode with parameters must be the last in a line");

					if (Arg == (int)Arg && (int)Arg <= 4 && (int)Arg >= 0)
					{
						LineMovementCode = (int)Arg;
						LastGCode = LineMovementCode;
						continue;
					}

					if (Arg == 20)
					{
						Unit = DistanceUnit.Inches;
						continue;
					}

					if (Arg == 21)
					{
						Unit = DistanceUnit.MM;
						continue;
					}

					if (Arg == 90)
					{
						DistanceMode = ParseDistanceMode.Absolute;
						continue;
					}

					if (Arg == 90.1)
					{
						ArcDistanceMode = ParseDistanceMode.Absolute;
						continue;
					}

					if (Arg == 91)
					{
						DistanceMode = ParseDistanceMode.Incremental;
						continue;
					}

					if (Arg == 91.1)
					{
						ArcDistanceMode = ParseDistanceMode.Incremental;
						continue;
					}

					continue;
					//throw new Exception($"Unrecognized GCode 'G{Arg}'");
				}

				if (PositionWords.Contains(Letter))
				{
					if (LineMovementCode == -1)
						LineMovementCode = LastGCode;

					if (Unit == DistanceUnit.Inches)
						Arg *= 25.4;
				}

				if (Letter == 'X')
				{ x = Arg; continue; }
				if (Letter == 'Y')
				{ y = Arg; continue; }
				if (Letter == 'Z')
				{ z = Arg; continue; }
				if (Letter == 'I')
				{ i = Arg; continue; }
				if (Letter == 'J')
				{ j = Arg; continue; }
				if (Letter == 'R')
				{ r = Arg; continue; }
				if (Letter == 'F')
				{ f = Arg; continue; }
				if (Letter == 'P')
				{ p = Arg; continue; }


				throw new Exception($"Unrecognized word '{Letter}'");
			}

			if (LineMovementCode == -1)
				return - 1;

			if (LineMovementCode == 4)
			{
				if (p.HasValue)
				{
					if ((x ?? y ?? z ?? i ?? j ?? r ?? f) != null)
						throw new Exception("Unused word in block");

					ToolPath.Add(new Dwell(p.Value));
				}
				else
					throw new Exception("Missing 'P' command word");
			}

			Vector3 EndPosition = new Vector3(Position);

			if (x.HasValue)
			{
				EndPosition.X = (DistanceMode == ParseDistanceMode.Absolute) ? x.Value : EndPosition.X + x.Value;
			}

			if (y.HasValue)
			{
				EndPosition.Y = (DistanceMode == ParseDistanceMode.Absolute) ? y.Value : EndPosition.Y + y.Value;
			}

			if (z.HasValue)
			{
				EndPosition.Z = (DistanceMode == ParseDistanceMode.Absolute) ? z.Value : EndPosition.Z + z.Value;
			}

			if (Position == EndPosition)
				return -1;


			switch (LineMovementCode)
			{
				case 0:
					{
						if ((i ?? j ?? r ?? p ?? f) != null)
							throw new Exception("Unused word in block");

						Straight s = new Straight(Position, EndPosition, true);

						ToolPath.Add(s);

						break;
					}

				case 1:
					{
						if ((i ?? j ?? r ?? p) != null)
							throw new Exception("Unused word in block");

						Straight s = new Straight(Position, EndPosition, false);
						s.FeedRate = f;

						ToolPath.Add(s);

						break;
					}
				case 2:
				case 3:
					{
						if(p.HasValue)
							throw new Exception("Unused word in block");

						Vector3 center = new Vector3(Position);

						if (i.HasValue)
						{
							center.X = (ArcDistanceMode == ParseDistanceMode.Absolute) ? i.Value : Position.X + i.Value;
						}

						if (j.HasValue)
						{
							center.Y = (ArcDistanceMode == ParseDistanceMode.Absolute) ? j.Value : Position.Y + j.Value;
						}

						if (r.HasValue)
						{
							if ((i ?? j).HasValue)
								throw new Exception("Both IJ and R notation used");

							Vector3 Parallel = EndPosition - Position;
							Vector3 Perpendicular = Parallel.CrossProduct(new Vector3(0, 0, 1));

							double PerpLength = Math.Sqrt((r.Value * r.Value) - (Parallel.Magnitude * Parallel.Magnitude / 4));

							if (LineMovementCode == 3 ^ r.Value < 0)
								PerpLength = -PerpLength;

							Perpendicular *= PerpLength / Perpendicular.Magnitude;

							center = Position + (Parallel / 2) + Perpendicular;
						}

						if (center == Position)
							throw new Exception("Center of arc undefined");

						Arc a = new Arc(Position, EndPosition, center, LineMovementCode == 2 ? ArcDirection.CW : ArcDirection.CCW);
						a.FeedRate = f;

						ToolPath.Add(a);

						break;
					}
			}

			Position = EndPosition;

			return ToolPath.Count - 1;
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
