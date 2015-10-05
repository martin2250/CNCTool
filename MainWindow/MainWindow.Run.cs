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
		private ToolPath runToolPath;
		private int runNextLineIndex = 0;

		private Timer runSendTimer = new Timer(50) { AutoReset = true };

		private void UpdateUIRun()
		{
			runButtonStart.IsEnabled = (MachineInterface != null) && (runToolPath != null) && (runNextLineIndex < runToolPath.Count) && (!runSendTimer.Enabled);
			runButtonPause.IsEnabled = (runSendTimer.Enabled);
			runButtonStop.IsEnabled = (runToolPath != null);
			runButtonReload.IsEnabled = (!runSendTimer.Enabled) && (runToolPath != null) && (runNextLineIndex != 0);

			if (runToolPath == null)
				runLabelLine.Content = "";
			else
				runLabelLine.Content = $"{runNextLineIndex}/{runToolPath.Count}";
		}

		private void InitRun()
		{
			runSendTimer.Elapsed += RunSendTimer_Elapsed;

			UpdateUiEvent += UpdateUIRun;
		}

		private void RunSendTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (runToolPath == null || MachineInterface == null)
			{
				runSendTimer.Stop();
				UpdateUi();
				return;
			}

			if (runNextLineIndex >= runToolPath.Count)
			{
				runSendTimer.Stop();
				UpdateUi();
				MessageBox.Show("Finished");
				return;
			}

			string line = runToolPath[runNextLineIndex].GetGCode();

			if (line.Length > MachineInterface.BufferSpace)
				return;

			MachineInterface.SendLine(line);

			runNextLineIndex++;

			UpdateUi();
		}

		#region Buttons
		private void runButtonStart_Click(object sender, RoutedEventArgs e)
		{
			if (runToolPath == null)
			{
				MessageBox.Show("No Toolpath loaded");
				return;
			}

			if (runNextLineIndex >= runToolPath.Count)
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

		private void runButtonStop_Click(object sender, RoutedEventArgs e)
		{
			runSendTimer.Stop();

			runToolPath = null;

			runNextLineIndex = 0;

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
