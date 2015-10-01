using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CNCTool.MainWindow
{
	partial class MainWindow
	{
		private void editor_DropDownEnter(object sender, MouseEventArgs e)
		{
			editor_DropDown.Margin = new Thickness(0, -112, 0, 0);
			editor_buttonCleanUp.Visibility = Visibility.Visible;
		}

		private void editor_DropDownLeave(object sender, MouseEventArgs e)
		{
			editor_DropDown.Margin = new Thickness(0, 0, 0, -112);
			editor_buttonCleanUp.Visibility = Visibility.Hidden;
		}

		private void editor_buttonShowHeightMap_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton button = (ToggleButton)sender;

			editor_imgHeightMapCross.Visibility = ((bool)button.IsChecked) ? Visibility.Hidden : Visibility.Visible;
		}

		private void editor_buttonOpen_Click(object sender, RoutedEventArgs e)
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

		private void editor_buttonSave_Click(object sender, RoutedEventArgs e)
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
	}
}
