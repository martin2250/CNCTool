using CNCTool.GCode;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
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
		public MainWindow()
		{
			InitializeComponent();

			var a = new Arc(new Vector3(0, 1, 0), new Vector3(1, 0, 1), new Vector3(), ArcDirection.CW);

			LinesVisual3D lv = new LinesVisual3D();

			Vector3 lastend = a.Interpolate(0);

			for (double x = 0.1; x <= 10; x += 0.1)
			{
				Vector3 point = a.Interpolate(x);
				lv.Points.Add(new Point3D(lastend.X, lastend.Y, lastend.Z));
				lv.Points.Add(new Point3D(point.X, point.Y, point.Z));
				lastend = point;
			}

			view.Items.Add(lv);
		}
	}
}
