using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace m540
{
	public class StopWatch
	{
		DateTime pre, post;
		string task;
		double elapsed_ms;
		public StopWatch(string _task) {task = _task; elapsed_ms = -1;}
		public StopWatch():this(null){}
		public double ElapsedMilliseconds
		{
			get
			{
				return elapsed_ms;
			}
		}
		public void tic()
		{
			pre = DateTime.Now;
		}
		public void toc()
		{
			post = DateTime.Now;
			elapsed_ms = (post-pre).TotalMilliseconds;
		}
		public string Result()
		{
			if (elapsed_ms < 0) {throw new Exception("Error: Timer object has not timed any task.");}
			else
			{
				return (task ?? "elapsed time") + ": " + elapsed_ms.ToString() + " ms";
			}
		}
	}
}
