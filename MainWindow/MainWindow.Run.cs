using CNCTool.GCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CNCTool.MainWindow
{
	partial class MainWindow
	{
		private int runNextLineIndex = 0;
		private Timer runSendTimer = new Timer(50) { AutoReset = true };

		private void UpdateUIRun()
		{
			buttonCanRun.IsEnabled = (!runSendTimer.Enabled);

			runButtonStart.IsEnabled = (MachineInterface != null) && (runNextLineIndex < previewPath.Count) && (!runSendTimer.Enabled);
			runButtonPause.IsEnabled = (runSendTimer.Enabled);
			runButtonReload.IsEnabled = (!runSendTimer.Enabled) && (runNextLineIndex != 0);

			labelLineNumberPre.Visibility = (buttonCanRun.IsChecked.Value) ? Visibility.Visible : Visibility.Hidden;
			labelLineNumber.Visibility = (buttonCanRun.IsChecked.Value) ? Visibility.Visible : Visibility.Hidden;

			UpdateLineNumber();
        }

		private void UpdateLineNumber()
		{
			labelLineNumber.Content = $"{runNextLineIndex}/{previewPath.Count}";
		}

		private void InitRun()
		{
			runSendTimer.Elapsed += RunSendTimer_Elapsed;

			UpdateUiEvent += UpdateUIRun;
		}

		private void RunSendTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (MachineInterface == null)
			{
				runSendTimer.Stop();
				UpdateUi();
				return;
			}

			if (runNextLineIndex >= previewPath.Count)
			{
				runSendTimer.Stop();
				UpdateUi();
				MessageBox.Show("Finished");
				return;
			}

			string line = previewPath[runNextLineIndex].GetGCode();

			if (line.Length > MachineInterface.BufferSpace)
				return;

			MachineInterface.SendLine(line);

			runNextLineIndex++;

			Dispatcher.Invoke(UpdateLineNumber);
		}

		#region Buttons
		private void runButtonStart_Click(object sender, RoutedEventArgs e)
		{
			if (runNextLineIndex >= previewPath.Count)
			{
				MessageBox.Show("No more lines left to send");
				return;
			}

			runSendTimer.Start();

			UpdateUi();
		}

		private void runButtonPause_Click(object sender, RoutedEventArgs e)
		{
			runSendTimer.Stop();

			UpdateUi();
		}

		private void runButtonReload_Click(object sender, RoutedEventArgs e)
		{
			runNextLineIndex = 0;

			UpdateUi();
		}
		#endregion
	}
}
