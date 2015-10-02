using CNCTool.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCTool.GCode
{
	public class ToolPath : List<GCodeCommand>
	{
		private string[] file_header = new string[] {"G90", "G21", ""};

		/*
		public void SaveCommands(StreamWriter file)
		{
			file.WriteLine("G90");
			file.WriteLine("G21");
			file.WriteLine();

			foreach (GCodeCommand Command in this)
			{
				file.WriteLine(Command.GetGCode());
			}

			file.Close();
		}*/

		public string[] GetLines()
		{
			return Enumerable.Concat(
				file_header,
				this.Select((command) => { return command.GetGCode(); })
				).ToArray();
		}

		public Bounds GetDimensions()
		{
			Bounds b = new Bounds();

			foreach (var Command in this)
			{
				var MoveCommand = Command as Movement;

				if (MoveCommand == null)
					continue;

				b.ExpandTo(MoveCommand.Start.X, MoveCommand.Start.Y);
				b.ExpandTo(MoveCommand.End.X, MoveCommand.End.Y);
			}

			return b;
		}

		public double GetTravelDistance()
		{
			double d = 0;

			foreach (var Command in this)
			{
				var MoveCommand = Command as Movement;

				if (MoveCommand == null)
					continue;

				d += MoveCommand.Length;
			}
			return d;
		}

		public ToolPath Split(double length)
		{
			ToolPath split = new ToolPath();

			foreach(GCodeCommand c in this)
			{
				if(c is Movement)
				{
					split.AddRange(((Movement)c).Split(length));
				}
				else
				{
					split.Add(c);
				}
			}

			return split;
		}
	}
}
