using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNCTool.Util
{
	static class Extensions
	{
		public static void Raise(this Action a)
		{
			if (a != null)
				a();
		}
	}
}
