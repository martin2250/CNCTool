using CNCTool.GCode;
using CNCTool.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;

namespace CNCTool.MainWindow
{
	partial class MainWindow
	{
		private void editor_ParseGCode()
		{
			editor_Parser.Reset();

			string text = editor_textBoxGCode.Text;

			StringBuilder errors = new StringBuilder();

			int lineIndex = 0;
			int lastpos = 0;
			int position = 0;

			while (true)
			{
				lineIndex++;

				position = text.IndexOf('\n', position);

				string line;

				if (position >= 0)
					line = text.Substring(lastpos, position - lastpos);
				else
					line = text.Substring(lastpos);

				try
				{
					editor_Parser.ParseLine(line);
				}
				catch (Exception ex)
				{
					if (errors.Length > 0)
						errors.Append('\n');

					errors.Append($"{lineIndex}:\t{ex.Message}");
				}

				if (position == -1)
					break;

				position++;
				lastpos = position;
			}

			editor_ToolPath = editor_Parser.ToolPath;

			editor_labelError.Content = errors.ToString();
			editor_labelError.Visibility = errors.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		private void editor_UpdatePreview()
		{
			editor_Path_Straight.Points.Clear();
			editor_Path_Arc.Points.Clear();
			editor_Path_Rapid.Points.Clear();

			foreach (GCodeCommand c in editor_ToolPath)
			{
				var s = c as Straight;

				if (s != null)
				{
					if (s.Rapid)
					{
						editor_Path_Rapid.Points.Add(s.Start.ToPoint3D());
						editor_Path_Rapid.Points.Add(s.End.ToPoint3D());
					}
					else
					{
						editor_Path_Straight.Points.Add(s.Start.ToPoint3D());
						editor_Path_Straight.Points.Add(s.End.ToPoint3D());
					}
					continue;
				}

				var a = c as Arc;

				if (a != null)
				{
					foreach(Movement m in a.Split(Properties.Settings.Default.EditorArcSplitDistance))
					{
						editor_Path_Arc.Points.Add(m.Start.ToPoint3D());
						editor_Path_Arc.Points.Add(m.End.ToPoint3D());
					}
				}
			}

			Bounds b = editor_ToolPath.GetDimensions();

			editor_Grid.Center = new Point3D(Math.Round((b.MinX + b.MaxX) / 20) * 10, Math.Round((b.MinY + b.MaxY) / 20) * 10, 0);

			editor_Grid.Length = Math.Ceiling((b.SizeX + 20) / 20) * 20;
			editor_Grid.Width = Math.Ceiling((b.SizeY + 20) / 20) * 20;

			editor_Preview.ZoomExtents(400);

			editor_imgTextChanged.Visibility = Visibility.Hidden;
		}
	}
}
