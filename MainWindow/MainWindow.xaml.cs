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
using CNCTool.Dialog;

namespace CNCTool.MainWindow
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		GCodeParser editor_Parser = new GCodeParser();
		ToolPath editor_ToolPath = new ToolPath();

		LinesVisual3D editor_Path_Straight = new LinesVisual3D() { Thickness=2};
		LinesVisual3D editor_Path_Arc = new LinesVisual3D() { Thickness = 2, Color=Colors.Blue};
		LinesVisual3D editor_Path_Rapid = new LinesVisual3D() { Thickness = 1, Color=Colors.Green};

		GridLinesVisual3D editor_Grid = new GridLinesVisual3D() { MajorDistance = 10, MinorDistance = 5, Center = new Point3D(0, 0, 0), Thickness = 0.03, Normal = new Vector3D(0, 0, 1), Width = 100, Length = 100 };

		public MainWindow()
		{
			InitializeComponent();

			view.Items.Add(editor_Grid);
			view.Items.Add(editor_Path_Straight);
			view.Items.Add(editor_Path_Arc);
			view.Items.Add(editor_Path_Rapid);
		}
	}
}
