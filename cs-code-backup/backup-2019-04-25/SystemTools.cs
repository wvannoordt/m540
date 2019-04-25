using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SystemTools
{
	public static class SystemInfo
	{
		public static int GetCores()
		{
			int coreCount = 0;
			foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
			{
				coreCount += int.Parse(item["NumberOfCores"].ToString());
			}
			return coreCount;
		}
	}
}