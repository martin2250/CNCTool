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
		GCodeParser editor_Parser = new GCodeParser();
		LinesVisual3D editor_Path = new LinesVisual3D() { Thickness=2};
		GridLinesVisual3D editor_Grid = new GridLinesVisual3D() { MajorDistance = 10, MinorDistance = 5, Center = new Point3D(0, 0, 0), Thickness = 0.03, Normal = new Vector3D(0, 0, 1), Width = 100, Length = 100 };

		public MainWindow()
		{
			InitializeComponent();

			view.Items.Add(editor_Grid);
			view.Items.Add(editor_Path);
		}
	}
}
