using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CNCTool.Connectivity;
using System.IO.Ports;
using CNCTool.Properties;
using System.Timers;
using CNCTool.GCode;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace CNCTool.MainWindow
{
	partial class MainWindow
	{
		private Communicator MachineInterface;
		private Timer MachineTimer;

		private int[] BaudRates = new int[] { 4800, 9600, 19200, 28800, 38400, 57600, 115200, 230400 };
		private int[] PositionPollRates = new int[] { 25, 50, 100, 250, 500, 1000 };

		private void machineConnect(Connection c)
		{
			try
			{
				machineGroupSettings.IsEnabled = false;
				machineBtnDisconnect.Visibility = Visibility.Visible;

				c.Connect();

				machineBtnDisconnect.Content = $"Disconnect {c.Path}";

				switch (machine_comBoxFirmware.SelectedIndex)
				{
					case 0:
						MachineInterface = new GRBL09(c);
						break;
					default:
						throw new Exception("The selected Firmware is not yet supported");
				}

				MachineInterface.StatusReceived += MachineInterface_StatusReceived;
				MachineInterface.Disconnected += MachineInterface_Disconnected;
				MachineInterface.ErrorReceived += MachineInterface_ErrorReceived;

				MachineTimer = new Timer(PositionPollRates[Settings.Default.PositionPollRateIndex]);
				MachineTimer.Elapsed += MachineTimer_Elapsed;MachineTimer.Start();
			}
			catch(Exception ex)
			{
				DisconnectCleanup();
				machineDisconnectUpdateUI();
				MessageBox.Show($"Could not connect: {ex.Message}");
			}
		}

		private void MachineInterface_ErrorReceived(string error)
		{
			MessageBox.Show(error);
		}

		private void DisconnectCleanup()
		{
			if (MachineTimer != null)
				MachineTimer.Stop();

			MachineInterface = null;
			MachineTimer = null;
		}

		private void MachineInterface_Disconnected()
		{
			DisconnectCleanup();

			Dispatcher.Invoke(machineDisconnectUpdateUI);
		}

		private void MachineTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			MachineInterface.RequestStatus();
		}

		private void MachineInterface_StatusReceived()
		{
			Dispatcher.Invoke(UpdateStatusStrip);
		}

		private void UpdateStatusStrip()
		{
			if (MachineInterface == null)
				return;

			progressBarBuffer.Value = 100.0 * MachineInterface.CharBufferCount / Settings.Default.GrblCharBuffer;
			labelStatus.Content = MachineInterface.Status;
			labelPosX.Content = MachineInterface.WorkPosition.X.ToString(GCodeCommand.NumberFormat);
			labelPosY.Content = MachineInterface.WorkPosition.Y.ToString(GCodeCommand.NumberFormat);
			labelPosZ.Content = MachineInterface.WorkPosition.Z.ToString(GCodeCommand.NumberFormat);

			((RotateTransform)imageStatusUpdate.RenderTransform).Angle += 11.25;
		}

		private void machineDisconnectUpdateUI()
		{
			machineBtnDisconnect.Visibility = Visibility.Hidden;
			machineGroupSettings.IsEnabled = true;

			progressBarBuffer.Value = 0;
			labelPosX.Content = null;
			labelPosY.Content = null;
			labelPosZ.Content = null;
			labelStatus.Content = "Not Connected";
		}

		private void machineBtnDisconnect_Click(object sender, RoutedEventArgs e)
		{
			if (MachineInterface != null)
				MachineInterface.Disconnect();
		}

		private void machine_btnConnectSerial_Click(object sender, RoutedEventArgs e)
		{
			string port = (string)machine_comBox_Port.SelectedValue;

			if (string.IsNullOrWhiteSpace(port))
				return;

			Connection serial = new SerialConnection(port, BaudRates[Settings.Default.BaudRateIndex]);

			machineConnect(serial);
		}

		private void machine_btnConnectRemote_Click(object sender, RoutedEventArgs e)
		{
			Connection net = new NetworkConnection(machine_textBoxRemote.Text);

			machineConnect(net);
		}

		private void machine_comBox_Port_Loaded(object sender, RoutedEventArgs e)
		{
			machine_comBox_Port.Items.Clear();

			foreach (string portname in SerialPort.GetPortNames())
				machine_comBox_Port.Items.Add(portname);
		}
	}
}
