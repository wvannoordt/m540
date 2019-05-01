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
		Testing.TestFullMultiGeneration();
	}
	private static void error(string message)
	{
		ConsoleColor pre = Console.ForegroundColor;
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(message);
		Console.ForegroundColor = pre;
	}
}
