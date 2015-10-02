using CNCTool.GCode;
using CNCTool.Util;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace CNCTool.MainWindow
{
	partial class MainWindow
	{
		private void editor_DropDownEnter(object sender, MouseEventArgs e)
		{
			StackPanel dropDown = sender as StackPanel;
			int height = (dropDown.Children.Count - 2) * 56;
			dropDown.Margin = new Thickness(0, -height, 0, 0);
			editor_buttonCleanUp.Visibility = Visibility.Visible;
			editor_btnDropDown_Other.Visibility = Visibility.Collapsed;
		}

		private void editor_DropDownLeave(object sender, MouseEventArgs e)
		{
			StackPanel dropDown = sender as StackPanel;
			int height = (dropDown.Children.Count - 1) * 56;
			dropDown.Margin = new Thickness(0, 0, 0, -height);
			editor_buttonCleanUp.Visibility = Visibility.Hidden;
			editor_btnDropDown_Other.Visibility = Visibility.Visible;
		}

		private void editor_btnShowHeightMap_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton button = (ToggleButton)sender;

			editor_imgHeightMapCross.Visibility = ((bool)button.IsChecked) ? Visibility.Hidden : Visibility.Visible;
		}

		private void editor_btnOpen_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog()
			{
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
				Filter = "GCode Files|*.nc;*.tap;*.ngc;*.txt|All Files|*.*",
				CheckFileExists = true
			};

			ofd.FileOk += editor_OpenFileOk;

			ofd.ShowDialog();
		}

		private void editor_OpenFileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				editor_textBoxGCode.Text = File.ReadAllText(((OpenFileDialog)sender).FileName);
			}
			catch(Exception ex)
			{
				Console.WriteLine("Error opening file: {0}", ex.ToString());
				MessageBox.Show("Could not open File");
			}
		}

		private void editor_btnSave_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog()
			{
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
				Filter = "GCode Files|*.nc;*.tap;*.ngc;*.txt|All Files|*.*",
				FileName = "toolpath"
			};

			sfd.FileOk += editor_SaveFileOk;

			sfd.ShowDialog();
		}

		private void editor_SaveFileOk(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				File.WriteAllText(((SaveFileDialog)sender).FileName, editor_textBoxGCode.Text);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error saving file: {0}", ex.ToString());
				MessageBox.Show("Could not save File");
			}
		}

		private void editor_btnResetCamera_Click(object sender, RoutedEventArgs e)
		{
			view.Camera.Position = new Point3D(5, -15, 25);
			view.Camera.LookDirection = new Vector3D(-5, 15, -25);
			view.Camera.UpDirection = new Vector3D(0, 0, 1);
			view.ZoomExtents(400);
		}

		private void editor_btnInfo_Click(object sender, RoutedEventArgs e)
		{
			ToolPath path = editor_ToolPath;
			StringBuilder info = new StringBuilder();

			Bounds b = path.GetDimensions();

			info.AppendLine($"Dimensions (XY): {b.SizeX:0.###}x{b.SizeY:0.###} mm");
			info.AppendLine($"Lower left corner (XY): {b.MinX:0.###},{b.MinY:0.###} mm");
			info.AppendLine($"Travel Distance: {path.GetTravelDistance():0} mm");
			info.Append($"Total Lines: {path.Count}");

			MessageBox.Show(info.ToString());
		}

		//TODO: replace false with 'can apply'
		private void editor_btnApplyHeightMap_Load(object sender, RoutedEventArgs e)
		{
			((Button)sender).IsEnabled = false;
		}

		private void editor_btnUpdatePreview_Click(object sender, RoutedEventArgs e)
		{
			editor_ParseGCode();

			editor_UpdatePreview();
		}

		private void editor_btnCleanUp_Click(object sender, RoutedEventArgs e)
		{
			editor_textBoxGCode.Text = string.Join("\n", editor_ToolPath.GetLines());
		}

		private void editor_textBox_Changed(object sender, TextChangedEventArgs e)
		{
			editor_imgTextChanged.Visibility = Visibility.Visible;
		}

		private void editor_btnSplit_Click(object sender, RoutedEventArgs e)
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

				editor_ToolPath = editor_ToolPath.Split(length);

				editor_textBoxGCode.Text = string.Join("\n", editor_ToolPath.GetLines());

				editor_UpdatePreview();
			}
		}
	}
}
