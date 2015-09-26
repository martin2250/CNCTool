using System;
using System.Net.Sockets;
using System.Net;

namespace CNCTool.Connectivity
{
	public class NetworkConnection : Connection
	{
		private TcpClient Client;

		public NetworkConnection(string address)
		{
			Client = new TcpClient();
			Path = address;
		}

		public override void Connect()
		{
			Client.Connect(Util.ParseIPEndPoint(Path));

			Init(Client.GetStream());
		}

		public override void Disconnect()
		{
			base.Disconnect();

			Client.Close();
		}

		private static class Util
		{
			public static IPEndPoint ParseIPEndPoint(string text)
			{
				Uri uri;
				if (Uri.TryCreate(text, UriKind.Absolute, out uri))
					return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port < 0 ? 0 : uri.Port);
				if (Uri.TryCreate(String.Concat("tcp://", text), UriKind.Absolute, out uri))
					return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port < 0 ? 0 : uri.Port);
				if (Uri.TryCreate(String.Concat("tcp://", String.Concat("[", text, "]")), UriKind.Absolute, out uri))
					return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port < 0 ? 0 : uri.Port);
				throw new Exception("Failed to parse text to IPEndPoint");
			}
		}
	}
}
