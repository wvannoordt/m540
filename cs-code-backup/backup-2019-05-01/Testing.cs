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
using Evolving;

public static class Testing
{
	//Class for miscellaneous test functions.
	public static void TestFullMultiGeneration()
	{
		//Should probably generalize this to a "tag case" object...
		double dx = 1.1d;
		StopWatch tagload = new StopWatch("tag load");
		tagload.tic();
		Tag[] tags = Tag.ExtractFromFile("geom-sets/cylinder-hollow/tags.tg");
		tagload.toc();
		Console.WriteLine("Loading tags...");
		Console.WriteLine(tagload.Result());
		string[] filenames = new string[tags.Length];
		int k = 0;
		foreach (Tag t in tags)
		{
			Console.WriteLine(t.ToString());
			filenames[k] = t.RegionFilename;
			Console.WriteLine(filenames[k] + "?exist=" + File.Exists(filenames[k]).ToString());
			k++;
		}
		Console.WriteLine();
		
		Console.WriteLine("Generating points...");
		StopWatch buildpoints = new StopWatch("build points");
		buildpoints.tic();
		double[] bounds = PointCloud.GetBoundsFromStls(filenames);
		PointCloud pc = PointCloud.FromBounds(bounds, dx);
		pc.WriteToCsv("./testcsv/diag.csv");
		buildpoints.toc();
		Console.WriteLine(buildpoints.Result());
		Console.WriteLine();
		
		Console.WriteLine("Initializing lattice...");
		StopWatch initlat = new StopWatch("initialize lattice");
		initlat.tic();
		LatticeStateInitializer init = new LatticeStateInitializer(pc, tags, 4, 20, 1.01*dx);
		init.InitializeNodes();
		initlat.toc();
		Console.WriteLine(initlat.Result());
		Console.WriteLine();
		
		Console.WriteLine("Building lattice...");
		StopWatch blat = new StopWatch("build lattice");
		blat.tic();
		LatticeState s = init.BuildLatticeState();
		blat.toc();
		Console.WriteLine(blat.Result());
		Console.WriteLine();
		
		StopWatch file = new StopWatch("write to file");
		Console.WriteLine("Writing to directory...");
		file.tic();
		s.WriteToDirectory("./lattice-output-test/cylgen",true);
		file.toc();
		Console.WriteLine(file.Result());
		Console.WriteLine();
		Console.WriteLine("Done.");
		Console.WriteLine();
		
		Console.WriteLine("Evolving...");
		Evolver e = new Evolver(s, 0.1, 4);
		e.EvolveAll("evolvetest", 500, true);
	}
	public static void TestPcGeneration()
	{
		StopWatch p = new StopWatch("load and bound");
		StopWatch s = new StopWatch("save");
		p.tic();
		string[] filenames = Directory.GetFiles("geom-sets/regions/cylinder", "*.stl");
		double[] bounds = PointCloud.GetBoundsFromStls(filenames);
		PointCloud pc = PointCloud.FromBounds(bounds, 0.6);
		p.toc();
		Console.WriteLine(p.Result());
		foreach (double w in bounds)
		{
			Console.WriteLine(w);
		}
		s.tic();
		pc.WriteToCsv("./testcsv/gen.csv");
		s.toc();
		Console.WriteLine(s.Result());
		
	}
	public static void TestTagging()
	{
		StopWatch t = new StopWatch("build");
		t.tic();
		Tag[] tags = Tag.ExtractFromFile("./testtags/testtag.tg");
		PointCloud points = PointCloud.FromCsv("./testcsv/rect.csv");
		LatticeStateInitializer init = new LatticeStateInitializer(points, tags, 4, 20, 1.1);
		init.InitializeNodes();
		LatticeState s = init.BuildLatticeState();
		s.WriteToDirectory("./lattice-output-test/bigtest", true);
		t.toc();
		Console.WriteLine("Success");
		Console.WriteLine(t.Result());
	}
	public static void TestAssignment()
	{
		StopWatch t1 = new StopWatch("dynamic set");
		StopWatch t2 = new StopWatch("compile set");
		Adjacency edge = new Adjacency(3, 4);
		Console.WriteLine(edge.ToDescriptionString());
		t1.tic();
		Tag.SetProperty(edge, "SpringConstant", "200");
		t1.toc();
		Console.WriteLine(edge.ToDescriptionString());
		t2.tic();
		edge.SpringConstant = 203;
		t2.toc();
		Console.WriteLine(edge.ToDescriptionString());
		Console.WriteLine(t1.Result());
		Console.WriteLine(t2.Result());
	}
}