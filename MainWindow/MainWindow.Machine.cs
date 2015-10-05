using System;
using System.Collections.Generic;
using System.Windows;
using CNCTool.Connectivity;
using System.IO.Ports;
using CNCTool.Properties;
using System.Timers;
using CNCTool.GCode;
using System.Windows.Media;
using System.Windows.Input;

namespace CNCTool.MainWindow
{
	partial class MainWindow
	{
		private Communicator MachineInterface;
		private Timer MachineTimer = new Timer() { AutoReset = true };

		private static int[] BaudRates = new int[] { 4800, 9600, 19200, 28800, 38400, 57600, 115200, 230400 };
		private static int[] PositionPollRates = new int[] { 25, 50, 100, 250, 500, 1000 };

		private void InitMachine()
		{
			UpdateUiEvent += UpdateUiMachine;

			MachineTimer.Elapsed += MachineTimer_Elapsed;
		}

		private void UpdateUiMachine()
		{
			machineBtnDisconnect.Visibility = (MachineInterface == null) ? Visibility.Hidden : Visibility.Visible;
			machineGroupSettings.IsEnabled = MachineInterface == null;
			machineGroupManual.IsEnabled = MachineInterface != null;

			UpdateStatusStrip();
		}

		#region Disconnect
		private void DisconnectCleanup()
		{
			MachineTimer.Stop();

			MachineInterface = null;

			UpdateUi();
		}

		private void machineBtnDisconnect_Click(object sender, RoutedEventArgs e)
		{
			if (MachineInterface != null)
				MachineInterface.Disconnect();
		}
		#endregion

		#region Status

		private void MachineTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			MachineInterface.RequestStatus();
		}

		private void UpdateStatusStrip()
		{
			if (MachineInterface == null)
			{
				progressBarBuffer.Value = 0;
				labelPosX.Content = null;
				labelPosY.Content = null;
				labelPosZ.Content = null;
				labelStatus.Content = "Not Connected";
			}
			else
			{
				progressBarBuffer.Value = 100.0 * MachineInterface.CharBufferCount / Settings.Default.GrblCharBuffer;
				labelStatus.Content = MachineInterface.Status;
				labelPosX.Content = MachineInterface.WorkPosition.X.ToString(GCodeCommand.NumberFormat);
				labelPosY.Content = MachineInterface.WorkPosition.Y.ToString(GCodeCommand.NumberFormat);
				labelPosZ.Content = MachineInterface.WorkPosition.Z.ToString(GCodeCommand.NumberFormat);

				Vector3 ToolPos = MachineInterface.WorkPosition;
				ToolPos.Z += 5;
				editor_Tool.Origin = ToolPos.ToPoint3D();

				((RotateTransform)imageStatusUpdate.RenderTransform).Angle += 11.25;
			}
		}
		#endregion

		#region Connect
		private void machineConnect(Connection c)
		{
			try
			{
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

				MachineInterface.StatusReceived += delegate { Dispatcher.Invoke(UpdateStatusStrip); };
				MachineInterface.Disconnected += DisconnectCleanup;
				MachineInterface.ErrorReceived += (error) => { MessageBox.Show(error); };

				MachineTimer.Interval = PositionPollRates[Settings.Default.PositionPollRateIndex];

				MachineTimer.Start();
			}
			catch (Exception ex)
			{
				DisconnectCleanup();
				MessageBox.Show($"Could not connect: {ex.Message}");
			}

			UpdateUi();
		}

		private void machine_btnConnectSerial_Click(object sender, RoutedEventArgs e)
		{
			string port = (string)machine_comBox_Port.SelectedValue;

			if (string.IsNullOrWhiteSpace(port))
				return;

			Connection serial = new SerialConnection(port, BaudRates[Settings.Default.BaudRateIndex], Settings.Default.SerialDtrEnable);

			machineConnect(serial);
		}

		private void machine_comBox_Port_Loaded(object sender, RoutedEventArgs e)
		{
			machine_comBox_Port.Items.Clear();

			foreach (string portname in SerialPort.GetPortNames())
				machine_comBox_Port.Items.Add(portname);
		}

		private void machine_btnConnectRemote_Click(object sender, RoutedEventArgs e)
		{
			Connection net = new NetworkConnection(machine_textBoxRemote.Text);

			machineConnect(net);
		}
		#endregion

		#region ManualCommand
		private List<string> machineManualHistory = new List<string>();
		private int machineManualIndex = -1;

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
		#endregion
	}
}
