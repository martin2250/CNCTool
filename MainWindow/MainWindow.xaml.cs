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
		ToolPath editor_ToolPath = new ToolPath();

		LinesVisual3D editor_Path_Straight = new LinesVisual3D() { Thickness = 2 };
		LinesVisual3D editor_Path_Arc = new LinesVisual3D() { Thickness = 2, Color = Colors.Blue };
		LinesVisual3D editor_Path_Rapid = new LinesVisual3D() { Thickness = 1, Color = Colors.Green };

		GridLinesVisual3D editor_Grid = new GridLinesVisual3D() { MajorDistance = 10, MinorDistance = 5, Center = new Point3D(0, 0, 0), Thickness = 0.03, Normal = new Vector3D(0, 0, 1), Width = 100, Length = 100 };

		public MainWindow()
		{
			InitializeComponent();

			editor_Preview.Items.Add(editor_Grid);
			editor_Preview.Items.Add(editor_Path_Straight);
			editor_Preview.Items.Add(editor_Path_Arc);
			editor_Preview.Items.Add(editor_Path_Rapid);
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			Properties.Settings.Default.Save();
		}

		private void machineSendManual()
		{
			if (MachineInterface == null)
				return;

			string text = machineTextBoxCommand.Text;

			if (MachineInterface.BufferSpace < text.Length)
				MessageBox.Show("Not enough space in buffer");
			else
			{
				MachineInterface.SendLine(text);

				if (machineManualIndex > -1)
					machineManualHistory.RemoveAt(machineManualIndex);

				machineManualIndex = -1;

				machineManualHistory.Insert(0, text);
				machineTextBoxCommand.Text = "";
			}
		}

		private void machineBtnSend_Click(object sender, RoutedEventArgs e)
		{
			machineSendManual();
		}

		private List<string> machineManualHistory = new List<string>();
		private int machineManualIndex = -1;

		private void machineTextBoxCommand_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				e.Handled = true;
				machineSendManual();
				return;
			}

			if (e.Key == Key.Down)
			{
				e.Handled = true;

				if (machineManualIndex > -1)
				{
					machineManualIndex--;

					if (machineManualIndex > -1)
						machineTextBoxCommand.Text = machineManualHistory[machineManualIndex];
					else
						machineTextBoxCommand.Text = "";
                }
				return;
			}

			if (e.Key == Key.Up)
			{
				e.Handled = true;

				if (machineManualIndex < machineManualHistory.Count - 1)
				{
					machineManualIndex++;
					machineTextBoxCommand.Text = machineManualHistory[machineManualIndex];
				}

				return;
			}
		}
	}
}
