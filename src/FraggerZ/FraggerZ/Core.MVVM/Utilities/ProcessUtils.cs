using Core.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Core.Omni.Utilities {

	public class ProcessUtils {

		///<summary>Returns whether a process with the given name is running.</summary>
		public static bool IsProcessRunning(string processName) {
			return Process.GetProcesses().Any(x => x.ProcessName.ToLower().Contains(processName));
		}

	}

}
