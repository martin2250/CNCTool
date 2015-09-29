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
	}
}
