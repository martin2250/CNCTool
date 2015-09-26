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
			Receiver.RunWorkerCompleted += Receiver_RunWorkerCompleted;

			Receiver.RunWorkerAsync();
		}

		private void Receiver_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Disconnected();
		}

		private void Receiver_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			LineReceived((string)e.UserState);
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
			catch { }   //only raises exception when disconnected (manually or by loss of connection)
		}

		public void SendLine(string line)
		{
			ConnectionWriter.WriteLine(line);
		}

		public virtual void Disconnect()
		{
			ConnectionWriter.Close();
			ConnectionReader.Close();
		}
	}
}
