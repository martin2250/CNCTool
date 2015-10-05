using System.IO.Ports;

namespace CNCTool.Connectivity
{
	public class SerialConnection : Connection
	{
		private SerialPort port;

		public SerialConnection(string portName, int baudRate, bool Dtr)
		{
			port = new SerialPort(portName, baudRate);

			port.DtrEnable = Dtr;

			Path = $"{portName}@{baudRate} baud";
		}

		public override void Connect()
		{
			port.Open();

			Init(port.BaseStream);
		}

		public override void Disconnect()
		{
			base.Disconnect();

			port.Close();
			port.Dispose();
		}
	}
}
