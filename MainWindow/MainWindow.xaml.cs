using CNCTool.GCode;
using CNCTool.Util;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CNCTool.MainWindow
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		GCodeParser parser = new GCodeParser();
		LinesVisual3D path = new LinesVisual3D();
		GridLinesVisual3D glv3d = new GridLinesVisual3D() { MajorDistance = 10, MinorDistance = 1, Center = new Point3D(0, 0, 0), Thickness = 0.02, Normal = new Vector3D(0, 0, 1), Width = 100, Length = 100 };

		public MainWindow()
		{
			InitializeComponent();
			view.Items.Add(glv3d);
			view.Items.Add(path);

			editor_textBoxGCode.Text = Properties.Resources.ShapeokoLogo;
		}

		private void buttonUpdateEditPreview_Click(object sender, RoutedEventArgs e)
		{
			parser.Reset();

			foreach (string line in editor_textBoxGCode.Text.Split('\n'))
			{
				try
				{
					parser.ParseLine(line);
				}
				catch { }
			}

			path.Points.Clear();

			foreach (GCodeCommand c in parser.ToolPath)
			{
				var s = c as Straight; // Movement;

				if (s != null)
				{
					path.Points.Add(s.Start.ToPoint3D());
					path.Points.Add(s.End.ToPoint3D());
					continue;
				}

				//not working
				var a = c as Arc;

				if (a != null)
				{
					Vector3 LastEnd = a.Start;
					for (double x = 0; x <= 1; x += 0.1)
					{
						Vector3 point = a.Interpolate(x);
						path.Points.Add(LastEnd.ToPoint3D());
						path.Points.Add(point.ToPoint3D());
						LastEnd = point;
					}
				}
			}

			Bounds b = parser.ToolPath.GetDimensions();

			glv3d.Center = new Point3D((b.MinX + b.MaxX) / 2, (b.MinY + b.MaxY) / 2, 0);

			glv3d.Length = Math.Ceiling(b.SizeX / 5) * 5;
			glv3d.Width = Math.Ceiling(b.SizeY / 5) * 5;

			view.ZoomExtents(400);
		}
	}
}
