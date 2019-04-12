using System;
using m540;
using MoreMathTools;
using System.Collections.Generic;
using System.IO;

namespace TikzGraphics
{
	public class TikzDrawing2D
	{
		//Contains framework for Tikz Visualization.
		List<string> fileBodyContents;
		
		#region nasty
		//Nasty but necessary
		private readonly string[] header = 
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
		private readonly string[] footer = 
		{
			"",
			@"%% End Content",
			"",
			@"\end{axis}",
			@"\end{tikzpicture}",
			@"\end{document}"
		};
		#endregion
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
			string[] filestuff = append(header, fileBodyContents.ToArray());
			filestuff = append(filestuff, footer);
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
		public TikzDrawing2D()
		{
			fileBodyContents = new List<string>();
		}
		private void write(string line)
		{
			fileBodyContents.Add(line);
		}
		
	}
}