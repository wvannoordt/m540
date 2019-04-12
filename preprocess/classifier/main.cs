using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using m540;
using _3DSimple;
using STL_Loader_v1;
using MoreMathTools;
using TikzGraphics;

public class Program
{
	public static void Main(string[] args)
	{
		int samplecount = 20;
		List<string> failmessages = new List<string>();
		bool[] successes = new bool[samplecount];
		for (int y = 0; y < samplecount; y++)
		{
			successes[y] = true;
		}
		for (int i = 0; i < samplecount; i++)
		{
			Console.WriteLine("Trial " + i.ToString() + "\n");
			try
			{
				Timer t1 = new Timer("load");
				Timer t2 = new Timer("compute");
				t1.tic();
				StlClassifier classifier = new StlClassifier("./test.stl", 40);
				t1.toc();
				Console.WriteLine(t1.Result());
				t2.tic();
				classifier.RunClassify(get_cores());
				t2.toc();
				Console.WriteLine(t2.Result());
				StlClassificationTable table = classifier.FacetLookupTable;
				Console.WriteLine("Success");
			}
			catch (Exception e)
			{
				successes[i] = false;
				failmessages.Add("Trial 1: " + e.Message);
			}
		}
		Console.WriteLine(get_true_false(successes));
		if (failmessages.Count > 0)
		{
			File.WriteAllLines("./errors.log", failmessages.ToArray());
		}
		else
		{
			File.WriteAllLines("./errors.log", new string[] {"No failures."});
		}
	}
	private static string get_true_false(bool[] stuff)
	{
		int success = 0;
		int fail = 0;
		foreach (bool b in stuff)
		{
			if (b) success++;
			else fail++;
		}
		return "Suceess: " + success.ToString() + ", Fail: " + fail.ToString();
	}
	private static int get_cores()
	{
		int coreCount = 0;
		foreach (var item in new System.Management.ManagementObjectSearcher("Select * from Win32_Processor").Get())
		{
			coreCount += int.Parse(item["NumberOfCores"].ToString());
		}
		return coreCount;
	}
}