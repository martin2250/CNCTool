using CNCTool.Dialog;
using CNCTool.GCode;
using CNCTool.Util;
using HelixToolkit.Wpf;
using System;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CNCTool.MainWindow
{
	partial class MainWindow
	{
		ToolPath previewPath = new ToolPath();

		LinesVisual3D previewPathStraight = new LinesVisual3D() { Thickness = 3 };
		LinesVisual3D previewPathArc = new LinesVisual3D() { Thickness = 3, Color = Colors.Blue };
		LinesVisual3D previewPathRapid = new LinesVisual3D() { Thickness = 2, Color = Colors.Green };

		GridLinesVisual3D editorGrid = new GridLinesVisual3D() { MajorDistance = 10, MinorDistance = 5, Center = new Point3D(0, 0, 0), Thickness = 0.05, Normal = new Vector3D(0, 0, 1), Width = 100, Length = 100 };

		TruncatedConeVisual3D previewToolVisual = new TruncatedConeVisual3D() { Height = 5, Normal = new Vector3D(0, 0, -1), Visible = false };

		private void InitPreview()
		{
			editor_Preview.Items.Add(editorGrid);
			editor_Preview.Items.Add(previewPathStraight);
			editor_Preview.Items.Add(previewPathArc);
			editor_Preview.Items.Add(previewPathRapid);
			editor_Preview.Items.Add(previewToolVisual);

			UpdateUiEvent += UpdateUiPreview;
		}

		private void UpdateUiPreview()
		{
			buttonCanRun.IsEnabled = (MachineInterface != null);

			if (MachineInterface == null)
				buttonCanRun.IsChecked = false;

			previewToolVisual.Visible = MachineInterface != null;
		}

		private void previewButtonResetCamera_Click(object sender, RoutedEventArgs e)
		{
			editor_Preview.Camera.Position = new Point3D(5, -15, 25);
			editor_Preview.Camera.LookDirection = new Vector3D(-5, 15, -25);
			editor_Preview.Camera.UpDirection = new Vector3D(0, 0, 1);
			editor_Preview.ZoomExtents(400);
		}

		private void previewButtonInfo_Click(object sender, RoutedEventArgs e)
		{
			ToolPath path = previewPath;
			StringBuilder info = new StringBuilder();

			Bounds b = path.GetDimensions();

			info.AppendLine($"Dimensions (XY): {b.SizeX:0.###}x{b.SizeY:0.###} mm");
			info.AppendLine($"Lower left corner (XY): {b.MinX:0.###},{b.MinY:0.###} mm");
			info.AppendLine($"Travel Distance: {path.GetTravelDistance():0} mm");
			info.Append($"Total Lines: {path.Count}");

			MessageBox.Show(info.ToString());
		}

		private void previewButtonSendToEditor_Click(object sender, RoutedEventArgs e)
		{
			editor_textBoxGCode.Text = string.Join("\n", previewPath.GetLines());
			mainTabCtrl.SelectedIndex = 0;
			editor_imgTextChanged.Visibility = Visibility.Hidden;
		}

		private void previewButtonSplit_Click(object sender, RoutedEventArgs e)
		{
			TextInputDialog tid = new TextInputDialog("Split GCode: \n Segment Length (mm)");
			tid.ErrorMessage = "Invalid Number";
			tid.Validate += (s) =>
			{
				double length;

				if (!double.TryParse(s, out length))
					return false;

				return length > 0;
			};

			if (tid.ShowDialog().Value)
			{
				double length = double.Parse(tid.textBoxInput.Text);

				previewPath = previewPath.Split(length);

				editor_UpdatePreview();
			}
		}

		private void editor_UpdatePreview()
		{
			previewPathStraight.Points.Clear();
			previewPathArc.Points.Clear();
			previewPathRapid.Points.Clear();

			foreach (GCodeCommand c in previewPath)
			{
				var s = c as Straight;

				if (s != null)
				{
					if (s.Rapid)
					{
						previewPathRapid.Points.Add(s.Start.ToPoint3D());
						previewPathRapid.Points.Add(s.End.ToPoint3D());
					}
					else
					{
						previewPathStraight.Points.Add(s.Start.ToPoint3D());
						previewPathStraight.Points.Add(s.End.ToPoint3D());
					}
					continue;
				}

				var a = c as Arc;

				if (a != null)
				{
					foreach(Movement m in a.Split(Properties.Settings.Default.EditorArcSplitDistance))
					{
						previewPathArc.Points.Add(m.Start.ToPoint3D());
						previewPathArc.Points.Add(m.End.ToPoint3D());
					}
				}
			}

			Bounds b = previewPath.GetDimensions();

			editorGrid.Center = new Point3D(Math.Round((b.MinX + b.MaxX) / 20) * 10, Math.Round((b.MinY + b.MaxY) / 20) * 10, 0);

			editorGrid.Length = Math.Ceiling((b.SizeX + 20) / 20) * 20;
			editorGrid.Width = Math.Ceiling((b.SizeY + 20) / 20) * 20;

			editor_Preview.ZoomExtents(400);

			editor_imgTextChanged.Visibility = Visibility.Hidden;
		}
	}
}
