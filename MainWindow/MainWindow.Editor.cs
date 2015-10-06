using CNCTool.Dialog;
using CNCTool.GCode;
using CNCTool.Util;
using HelixToolkit.Wpf;
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
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CNCTool.MainWindow
{
	partial class MainWindow
	{
		GCodeParser editor_Parser = new GCodeParser();

		private void InitEditor()
		{
			UpdateUiEvent += UpdateUiEditor;
		}

		private void UpdateUiEditor()
		{
			editorButtonParse.IsEnabled = !buttonCanRun.IsChecked.Value;
		}

		private void editor_textBox_Changed(object sender, TextChangedEventArgs e)
		{
			editor_imgTextChanged.Visibility = Visibility.Visible;
		}

		#region FileIO
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
			catch (Exception ex)
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
		#endregion

		#region Buttons
		private void editor_ButtonSendToPreview_Click(object sender, RoutedEventArgs e)
		{
			editor_ParseGCode();

			editor_UpdatePreview();

			runNextLineIndex = 0;

			UpdateUi();
		}
		#endregion

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

			previewPath = editor_Parser.ToolPath;

			editor_labelError.Content = errors.ToString();
			editor_labelError.Visibility = errors.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
