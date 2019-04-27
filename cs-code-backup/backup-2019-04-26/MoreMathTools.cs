using System;
using m540;
using _3DSimple;
using STL_Loader_v1;
namespace MoreMathTools
{
	//Class for holding miscellaneous math functions. Cleaner than storing in parent classes.
	//All should be reasonably self-explanatory.
	public static class MathTools
	{
		public static bool IsAbovePlane(Point3 p, Facet f)
		{
			if (f.Normal.K == 0) return true;
			Vector3 normal_plus = f.Normal.K > 0 ? f.Normal : -1*(f.Normal);
			Vector3 delta = new Vector3(f.V1,p);
			return normal_plus * delta > 0;
		}
		public static bool CheckAny(params bool[] conditions)
		{
			foreach (bool b in conditions) if (b) return true;
			return false;
		}
		public static bool CheckAll(params bool[] conditions)
		{
			foreach (bool b in conditions) if (!b) return false;
			return true;
		}
		public static double GetMin(params double[] X)
		{
			double xmin = double.PositiveInfinity;
			foreach (double x in X) xmin = x < xmin ? x : xmin;
			return xmin;
		}
		public static double GetMax(params double[] X)
		{
			double xmax = double.NegativeInfinity;
			foreach (double x in X) xmax = x > xmax ? x : xmax;
			return xmax;
		}
		public static bool SegmentsIntersect(Pair p1, Pair q1, Pair p2, Pair q2)
		{
			// Find the four orientations needed for general and 
			// special cases 
			int o1 = orientation(p1, q1, p2); 
			int o2 = orientation(p1, q1, q2); 
			int o3 = orientation(p2, q2, p1); 
			int o4 = orientation(p2, q2, q1); 
		  
			// General case 
			if (o1 != o2 && o3 != o4) 
				return true; 
		  
			// Special Cases 
			// p1, q1 and p2 are colinear and p2 lies on segment p1q1 
			if (o1 == 0 && onSegment(p1, p2, q1)) return true; 
		  
			// p1, q1 and q2 are colinear and q2 lies on segment p1q1 
			if (o2 == 0 && onSegment(p1, q2, q1)) return true; 
		  
			// p2, q2 and p1 are colinear and p1 lies on segment p2q2 
			if (o3 == 0 && onSegment(p2, p1, q2)) return true; 
		  
			 // p2, q2 and q1 are colinear and q1 lies on segment p2q2 
			if (o4 == 0 && onSegment(p2, q1, q2)) return true; 
		  
			return false; // Doesn't fall in any of the above cases
		}
		private static bool onSegment(Pair p, Pair q, Pair r) 
		{ 
			if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) && 
				q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y)) 
			   return true; 
		  
			return false; 
		}
		private static int orientation(Pair p, Pair q, Pair r) 
		{ 
			double val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y); 
		  
			if (val == 0) return 0;  // colinear 
		  
			return (val > 0)? 1: 2; // clock or counterclock wise 
		}
	}
	public struct Pair
	{
		private double x,y;
		public double X{get {return x;} set {x = value;}}
		public double Y{get {return y;} set {y = value;}}
		public Pair(double _X, double _Y)
		{
			x = _X; y = _Y;
		}
		public Pair(double[] xy)
		{
			x = xy[0]; y = xy[1];
		}
	}
}