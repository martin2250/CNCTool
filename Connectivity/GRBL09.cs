using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CNCTool.GCode;
using System.Text.RegularExpressions;
using System.Timers;

namespace CNCTool.Connectivity
{
	public class GRBL09 : Communicator
	{
		//https://www.regex101.com/r/vG0cJ8/2
		private static Regex StatusEx = new Regex(@"<(?'State'Idle|Run|Hold|Home|Alarm|Check|Door)(?:,MPos:(?'MX'-?[0-9\.]*),(?'MY'-?[0-9\.]*),(?'MZ'-?[0-9\.]*))?(?:,WPos:(?'WX'-?[0-9\.]*),(?'WY'-?[0-9\.]*),(?'WZ'-?[0-9\.]*))?(?:,Buf:(?'Buf'[0-9]*))?(?:,RX:(?'RX'[0-9]*))?(?:,Ln:(?'L'[0-9]*))?(?:,F:(?'F'[0-9\.]*))?(?:,Lim:(?'Lim'[0-1]*))?(?:,Ctl:(?'Ctl'[0-1]*))?>", RegexOptions.Compiled);
		private static Regex ProbeEx = new Regex(@"\[PRB:(?'MX'-?[0-9]+\.?[0-9]*),(?'MY'-?[0-9]+\.?[0-9]*),(?'MZ'-?[0-9]+\.?[0-9]*):(?'Success'0|1)\]", RegexOptions.Compiled);
		private static Regex ErrorEx = new Regex(@"error: Invalid gcode ID:(?'err'[0-9]+)", RegexOptions.Compiled);

		Queue<TaskCompletionSource<double>> Probes = new Queue<TaskCompletionSource<double>>();

		public GRBL09(Connection machineConnection) : base(machineConnection)
		{
			MachineConnection.LineReceived += MachineConnection_LineReceived;
		}

		private void MachineConnection_LineReceived(string line)
		{
			if (line.StartsWith("<"))
			{
				Match statusMatch = StatusEx.Match(line);

				if (!statusMatch.Success)
				{
					Console.WriteLine("Received Bad Status: '{0}'", line);
					return;
				}

				Group status = statusMatch.Groups["State"];

				if (status.Success)
				{
					Status = status.Value;
				}

				Group mx = statusMatch.Groups["MX"], my = statusMatch.Groups["MY"], mz = statusMatch.Groups["MZ"];

				if (mx.Success)
				{
					MachinePosition = new Vector3(double.Parse(mx.Value, GCodeCommand.NumberFormat), double.Parse(my.Value, GCodeCommand.NumberFormat), double.Parse(mz.Value, GCodeCommand.NumberFormat));
				}

				Group wx = statusMatch.Groups["WX"], wy = statusMatch.Groups["WY"], wz = statusMatch.Groups["WZ"];

				if (wx.Success)
				{
					WorkPosition = new Vector3(double.Parse(wx.Value, GCodeCommand.NumberFormat), double.Parse(wy.Value, GCodeCommand.NumberFormat), double.Parse(wz.Value, GCodeCommand.NumberFormat));
				}

				StatusUpdate();
			}
			else if (line.StartsWith("ok"))
			{
				if (ActiveCommands.Count > 0)
					CharBufferCount -= ActiveCommands.Dequeue().Length;
				else
					Console.WriteLine("Received ok without active command");
			}
			else if (line.StartsWith("[PRB:"))
			{
				Match probeMatch = ProbeEx.Match(line);
				Group mx = probeMatch.Groups["MX"];

				if (!probeMatch.Success || !mx.Success)
				{
					Console.WriteLine("Received Bad Probe: '{0}'", line);
					return;
				}

				double height = double.Parse(mx.Value, GCodeCommand.NumberFormat);

				height += WorkPosition.Z - MachinePosition.Z;

				if (Probes.Count > 0)
					Probes.Dequeue().SetResult(height);
			}
			else if (line.StartsWith("error"))
			{
				string command;

				if (ActiveCommands.Count > 0)
				{
					command = ActiveCommands.Dequeue();
					CharBufferCount -= command.Length;
				}
				else
					command = "No active command";

				HandleError(line, command);
			}
			else if (line.StartsWith("["))  //feedback message
			{

			}
			else if (line.StartsWith("ALARM"))
			{

			}
			else if (line.StartsWith("Grbl"))
			{
				Probes.Clear();
				ActiveCommands.Clear();
				CharBufferCount = 0;
				RequestStatus();
			}
		}

		private void HandleError(string line, string command)
		{
			new Task(delegate
			{
				Group errMatch = ErrorEx.Match(line).Groups["err"];

				string error;

				if (errMatch.Success)
				{
					int code = int.Parse(errMatch.Value);

					if (Util.GrblErrorProvider.Errors.ContainsKey(code))
					{
						error = Util.GrblErrorProvider.Errors[code];
					}
					else
					{
						error = $"Undocumented error code {code}";
					}
				}
				else
				{
					error = line;
				}

				Error(command + " : " + error);
			}).Start();
		}

		public override Task<double> ProbeZ(double feed, double maxDepth)
		{
			RequestStatus();

			TaskCompletionSource<double> tcs = new TaskCompletionSource<double>();

			Probes.Enqueue(tcs);

			SendLine(string.Format(GCodeCommand.NumberFormat, "G38.2Z-{0}F{1}", maxDepth, feed));

			return tcs.Task;
		}

		public override void Reset()
		{
			MachineConnection.Send("\u0018");
		}

		public override void RequestStatus()
		{
			MachineConnection.Send("?");
		}
	}
}
