using System;
using m540;
using MoreMathTools;
using System.Collections.Generic;
using System.IO;
using _3DSimple;
using STL_Loader_v1;

namespace TikzGraphics
{
	public class TikzDrawing
	{
		//Contains framework for Tikz Visualization.
		List<string> fileBodyContents;
		private TikzCamera camera;
		public TikzCamera Camera3D {get {return camera;} set {camera = value;}}
		public void DrawRectangularCover(RectangularCover cover)
		{
			for (int i = 0; i < cover.RegionCount; i++)
			{
				double[] coords = cover.Grid.ComputeXY(cover[i]);
				Pair p1 = new Pair(coords);
				Pair p2 = new Pair(coords[0] + cover.Grid.DeltaX, coords[1] + cover.Grid.DeltaY);
				FillRectangle(p1, p2, "blue!30!white");
			}
			DrawGrid(cover.Grid);
			DrawTriangle(cover.CoveredTriangle);
		}
		public void FillRectangle(Pair p1, Pair p2, string color)
		{
			write(@"\fill [" + color + "] "  + make_pair(p1) + " rectangle " + make_pair(p2) + ";");
		}
		public void DrawTriangle(Triangle T)
		{
			DrawLine(T[0], T[1]);
			DrawLine(T[1], T[2]);
			DrawLine(T[2], T[0]);
		}
		public void DrawLine(Pair p1, Pair p2, string color)
		{
			write(@"\draw[" + color + "] " + make_pair(p1) + " -- " + make_pair(p2) + ";");
		}
		public void DrawLine(Pair p1, Pair p2)
		{
			DrawLine(p1, p2, "black");
		}
		
		//Eww
		public void DrawCube(Pair origin, Point3 center, double radius, string color)
		{
			Point3[] cubepoints = new Point3[]
			{
				new Point3(center.X + radius, center.Y + radius, center.Z + radius),
				new Point3(center.X + radius, center.Y - radius, center.Z + radius),
				new Point3(center.X - radius, center.Y + radius, center.Z + radius),
				new Point3(center.X - radius, center.Y - radius, center.Z + radius),
				new Point3(center.X + radius, center.Y + radius, center.Z - radius),
				new Point3(center.X + radius, center.Y - radius, center.Z - radius),
				new Point3(center.X - radius, center.Y + radius, center.Z - radius),
				new Point3(center.X - radius, center.Y - radius, center.Z - radius)
			};
			DrawLine(map_3d_coords(cubepoints[0], origin), map_3d_coords(cubepoints[1], origin), color);
			DrawLine(map_3d_coords(cubepoints[1], origin), map_3d_coords(cubepoints[3], origin), color);
			DrawLine(map_3d_coords(cubepoints[3], origin), map_3d_coords(cubepoints[2], origin), color);
			DrawLine(map_3d_coords(cubepoints[2], origin), map_3d_coords(cubepoints[0], origin), color);
			
			DrawLine(map_3d_coords(cubepoints[0], origin), map_3d_coords(cubepoints[4], origin), color);
			DrawLine(map_3d_coords(cubepoints[1], origin), map_3d_coords(cubepoints[5], origin), color);
			DrawLine(map_3d_coords(cubepoints[2], origin), map_3d_coords(cubepoints[6], origin), color);
			DrawLine(map_3d_coords(cubepoints[3], origin), map_3d_coords(cubepoints[7], origin), color);
			
			DrawLine(map_3d_coords(cubepoints[4], origin), map_3d_coords(cubepoints[5], origin), color);
			DrawLine(map_3d_coords(cubepoints[5], origin), map_3d_coords(cubepoints[7], origin), color);
			DrawLine(map_3d_coords(cubepoints[7], origin), map_3d_coords(cubepoints[6], origin), color);
			DrawLine(map_3d_coords(cubepoints[6], origin), map_3d_coords(cubepoints[4], origin), color);
		}
		public void DrawCube(Pair origin, Point3 center, double radius)
		{
			DrawCube(origin, center, radius, "black");
		}
		
		public void DrawGrid(GridClassifier g)
		{
			int n = g.Count;
			for (int i = 0; i < n + 1; i++)
			{
				DrawLine(new Pair(g.ComputeXY(i, 0)), new Pair(g.ComputeXY(i, n)));
				DrawLine(new Pair(g.ComputeXY(0, i)), new Pair(g.ComputeXY(n, i)));
			}
		}
		public void Save(string outputfilename)
		{
			string[] filestuff = append(TikzBanner.GetHeader(), fileBodyContents.ToArray());
			filestuff = append(filestuff, TikzBanner.GetFooter());
			File.WriteAllLines(outputfilename, filestuff);
		}
		
		
		private string[] append(string[] a, string[] b)
		{
			int n = a.Length + b.Length;
			string[] c = new string[n];
			int k = 0;
			foreach (string s in a) {c[k] = s; k++;}
			foreach (string s in b) {c[k] = s; k++;}
			return c;
		}
		private Pair map_3d_coords(Point3 P)
		{
			return map_3d_coords(P, new Pair(0,0));
		}
		private Pair map_3d_coords(Point3 P, Pair origin_2d)
		{
			double dx = P.X - camera.OrbitFocus.X;
			double dy = P.Y - camera.OrbitFocus.Y;
			double dz = P.Z - camera.OrbitFocus.Z;
			double zoom = camera.Zoom;
			double pX = ((dy * camera.SinZenith) - dx * camera.CosZenith) * zoom;
            double pY = ((dz * camera.CosAzimuth) - (dx * camera.SinZenith * camera.SinAzimuth) - (dy * camera.SinAzimuth * camera.CosZenith)) * zoom;
			return new Pair(origin_2d.X + pX, origin_2d.Y + pY);
		}
		private string combine(string[] p)
		{
			string output = "";
			if (p.Length > 0)
			{
				output += p[0];
				for (int i = 1; i < p.Length; i++)
				{
					output += "," + p[i];
				}
			}
			return output;
		}
		private string make_pair(Pair p)
		{
			return "(" + p.X.ToString() + "," + p.Y.ToString() + ")";
		}
		public TikzDrawing()
		{
			fileBodyContents = new List<string>();
			camera = new TikzCamera();
		}
		private void write(string line)
		{
			fileBodyContents.Add(line);
		}
		public void RotateAzimuth(double change_radians)
		{
			camera.Azimuth += change_radians;
		}
		public void RotateZenith(double change_radians)
		{
			camera.Zenith += change_radians;
		}
		
	}
	public static class TikzBanner 
	{
		//Awful
		private static readonly string[] header = 
		{
			@"\documentclass{standalone}",
			@"\usepackage{etex}",
			@"\PassOptionsToPackage{rgb}{xcolor}",
			@"\usepackage{tikz}",
			@"\usepackage{pgfplots}",
			@"\usepackage{amsmath}",
			@"\usepackage{tkz-fct}",
			@"\usepackage{makecell}",
			@"\usepackage{mathrsfs}",
			@"\usepackage{pifont}",
			@"\usepackage{textcomp}",
			@"\newcommand{\xmax}{10}",
			@"\newcommand{\ymax}{10}",
			@"\usetikzlibrary{shapes.geometric,arrows,backgrounds,calc,trees,decorations.pathmorphing,patterns,3d,arrows.meta,decorations.text,}",
			@"\tikzset{>={Stealth[width=1.2mm,length=1.2mm]},box/.style={draw=none,fill=none,align=center,execute at begin node=\setlength{\baselineskip}{9pt}}}",
			@"\begin{document}",
			@"\pgfplotsset{compat=1.11,axis line style={white}}",
			@"\begin{tikzpicture}",
			@"\begin{axis}[clip=false,ticks=none,xmin=0,xmax=\xmax,ymin=0,ymax=\ymax,scale mode=scale uniformly,width=3.5in, height=3.5in]",
			"",
			@"%% Begin Content",
			""
		};
		private static readonly string[] footer = 
		{
			"",
			@"%% End Content",
			"",
			@"\end{axis}",
			@"\end{tikzpicture}",
			@"\end{document}"
		};
		public static string[] GetHeader()
		{
			return header;
		}
		public static string[] GetFooter()
		{
			return footer;
		}
	}
	public class TikzCamera
	{
		private double zenith, azimuth, zoom;//radians by default
		private double cos_azimuth,sin_azimuth,cos_zenith,sin_zenith;
		private Point3 orbit_focus;
		public double Zenith {get {return zenith;} set {zenith = value; update_sin_cos();}}
		public double CosZenith {get {return cos_zenith;}}
		public double SinZenith {get {return sin_zenith;}}
		public double Azimuth {get {return azimuth;} set {azimuth = value; update_sin_cos();}}
		public double CosAzimuth {get {return cos_azimuth;}}
		public double SinAzimuth {get {return sin_azimuth;}}
		public double Zoom {get {return zoom;} set {zoom = value;}}
		public Point3 OrbitFocus {get {return orbit_focus;} set {orbit_focus = value;}}
		public TikzCamera(double _zenith, double _azimuth, double _zoom, Point3 _orbit_focus)
		{
			zenith = _zenith;
			azimuth = _azimuth;
			zoom = _zoom;
			orbit_focus = _orbit_focus;
			update_sin_cos();
		}
		private void update_sin_cos()
		{
			cos_azimuth = Math.Cos(azimuth);
			sin_azimuth = Math.Sin(azimuth);
			cos_zenith = Math.Cos(zenith);
			sin_zenith = Math.Sin(zenith);
		}
		public TikzCamera():this(0.25*Math.PI, 0.25*Math.PI, 1, new Point3(0,0,0)){}
	}
}