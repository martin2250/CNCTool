using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCTool.Connection
{
	interface IConnection
	{
		event Action<string> LineReceived;
	}
}
