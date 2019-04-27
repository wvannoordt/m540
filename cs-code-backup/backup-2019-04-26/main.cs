using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using m540;
using _3DSimple;
using STL_Loader_v1;
using MoreMathTools;
using TikzGraphics;
using InitDataTools;
using SystemTools;

public class Program
{
	public static void Main(string[] args)
	{
		StopWatch t = new StopWatch("build");
		t.tic();
		Tag[] tags = Tag.ExtractFromFile("./testtags/testtag.tg");
		PointCloud points = PointCloud.FromCsv("./testcsv/rect.csv");
		LatticeStateInitializer init = new LatticeStateInitializer(points, tags, 4, 20);
		init.InitializeNodes();
		LatticeState s = init.BuildLatticeState();
		s.WriteToDirectory("./lattice-output-test/bigtest");
		t.toc();
		Console.WriteLine("Success");
		Console.WriteLine(t.Result());
	}
	private static void error(string message)
	{
		ConsoleColor pre = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(message);
		Console.ForegroundColor = pre;
	}
}
