using CNCTool.GCode;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CNCTool.Connectivity
{
	public abstract class Communicator
	{
		public event Action StatusReceived;
		public event Action Disconnected;
		public event Action<string> ErrorReceived;

		protected Connection MachineConnection { get; private set; }
		protected Queue<string> ActiveCommands = new Queue<string>();

		public int CharBufferCount { get; protected set; } = 0;

		public Vector3 MachinePosition { get; protected set; }
		public Vector3 WorkPosition { get; protected set; }
		public string Status { get; protected set; } = "Unknown";

		public abstract void RequestStatus();
		public abstract Task<double> ProbeZ(double feed, double maxDepth);
		public abstract void Reset();

		public Communicator(Connection machineConnection)
		{
			MachineConnection = machineConnection;
			MachineConnection.Disconnected += MachineConnection_Disconnected;
		}

		private void MachineConnection_Disconnected()
		{
			if (Disconnected != null)
				Disconnected();
		}

		public int BufferSpace { get { return Properties.Settings.Default.GrblCharBuffer - CharBufferCount; } }

		public bool SendLine(string line)
		{
			if (line.Length < BufferSpace)
			{
				CharBufferCount += line.Length;
				ActiveCommands.Enqueue(line);
				MachineConnection.SendLine(line);

				return true;
			}
			else
				return false;
		}

		protected void StatusUpdate()
		{
			if (StatusReceived != null)
				StatusReceived();
		}

		protected void Error(string error)
		{
			if (ErrorReceived != null)
				ErrorReceived(error);
		}

		public void Disconnect()
		{
			MachineConnection.Disconnect();
		}
	}
}
