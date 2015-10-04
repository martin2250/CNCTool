using System;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace CNCTool.Connectivity
{
	public abstract class Connection
	{
		public event Action<string> LineReceived;
		public event Action Disconnected;

		public string Path { get; protected set; }

		protected StreamReader ConnectionReader;
		protected StreamWriter ConnectionWriter;

		BackgroundWorker Receiver;

		public abstract void Connect();

		protected void Init(Stream stream)
		{
			ConnectionReader = new StreamReader(stream, Encoding.ASCII) { };
			ConnectionWriter = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true, NewLine = "\n" };

			Receiver = new BackgroundWorker() { WorkerReportsProgress = true };

			Receiver.DoWork += Receiver_DoWork;
			Receiver.ProgressChanged += Receiver_ProgressChanged;

			Receiver.RunWorkerAsync();
		}

		private void Receiver_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			Action<string> temp = LineReceived;
			if (temp != null)
			{
				temp((string)e.UserState);
			}
		}

		private void Receiver_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				while (true)
				{
					string line = ConnectionReader.ReadLine();

					if (!string.IsNullOrWhiteSpace(line))
						Receiver.ReportProgress(0, line);
				}
			}
			catch	//only raises exception when disconnected (manually or by loss of connection)
			{
				Disconnect();
			}   
		}

		public void SendLine(string line)
		{
			Send(line + "\n");
		}

		public void Send(string message)
		{
			try
			{
				ConnectionWriter.Write(message);
			}
			catch
			{
				Disconnect();
			}
		}

		public virtual void Disconnect()
		{
			if (Disconnected != null)
			{
				Disconnected();
			}
			try
			{
				if (ConnectionWriter != null)
					ConnectionWriter.Close();
				if (ConnectionReader != null)
					ConnectionReader.Close();
			}
			catch { }	//possibly device not responding error
		}
	}
}
