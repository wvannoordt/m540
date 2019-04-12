using System;
using System.Collections;
using System.Collections.Generic;
using MoreMathTools;

namespace m540
{
	//Represents a triangular region on XY plane.
	public class Triangle
	{
		private Pair[] vertices;
		private double twice_area;
		public double Xmin {get;set;}
		public double Xmax {get;set;}
		public double Ymin {get;set;}
		public double Ymax {get;set;}
		public double TwiceArea {get {return twice_area;}}
		public Pair this[int i]
		{
			get{return vertices[i];}
			set{vertices[i] = value;}
		}
		public Triangle(Pair p1, Pair p2, Pair p3)
		{
			vertices = new Pair[] {p1, p2, p3};
			twice_area = Math.Abs(((vertices[0].X - vertices[1].X)*(vertices[0].Y - vertices[2].Y)) - ((vertices[0].Y - vertices[1].Y)*(vertices[0].X - vertices[2].X)));
			Xmin = MathTools.GetMin(vertices[0].X,vertices[1].X,vertices[2].X);
			Xmax = MathTools.GetMax(vertices[0].X,vertices[1].X,vertices[2].X);
			Ymin = MathTools.GetMin(vertices[0].Y,vertices[1].Y,vertices[2].Y);
			Ymax = MathTools.GetMax(vertices[0].Y,vertices[1].Y,vertices[2].Y);
		}
		public Triangle(Pair[] pairs):this(pairs[0], pairs[1], pairs[2]){}
		public bool Contains(double x, double y)
		{
			return Contains(new Pair(x, y));
		}
		public bool Contains(double[] xy)
		{
			return Contains(new Pair(xy[0], xy[1]));
		}
		public bool Contains(Pair p)
		{
			//In practice, do not create new data structures (MAY NEED TO ADDRESS THIS AT SOME POINT)
			Triangle t1 = new Triangle(p, vertices[0], vertices[1]);
			Triangle t2 = new Triangle(p, vertices[0], vertices[2]);
			Triangle t3 = new Triangle(p, vertices[1], vertices[2]);
			double sum = t1.TwiceArea + t2.TwiceArea + t3.TwiceArea;
			return Math.Abs(twice_area - sum) < 1e-6; 
		}
		public bool CheckSegmentBoundaryIntersection(Pair p1, Pair p2)
		{
			if (MathTools.SegmentsIntersect(p1, p2, vertices[0], vertices[1])) return true;
			if (MathTools.SegmentsIntersect(p1, p2, vertices[0], vertices[2])) return true;
			if (MathTools.SegmentsIntersect(p1, p2, vertices[1], vertices[2])) return true;
			return false;
		}
	}
	//"Rectangular cover" refers to the covering index set (see
	//https://wvannoordt.github.io/misc-math/unitary-cover.pdf) as computed on some GridClassifier.
	//It will refer to a list of index pairs such that the union of all corresponding grid cells is
	//The minimal cover for the triangle. Does not hold grid data as that would be redundant (though lightweight)
	public class RectangularCover
	{
		//It will be necessary that both arrays are equal in dimension.
		private int[] i_indices, j_indices;
		public int RegionCount {get {return i_indices.Length;}}
		public int[] this[int i]{ get {return new int[] {i_indices[i], j_indices[i]}; }}
		private Triangle tri;
		private GridClassifier grid;
		public Triangle CoveredTriangle {get {return tri;}}
		public GridClassifier Grid {get {return grid;}}
		public RectangularCover(Triangle T, GridClassifier map)
		{
			tri = T;
			grid = map;
			
			//O(n^2) unfortunately.
			List<int> i_indices_list = new List<int>();
			List<int> j_indices_list = new List<int>();
			//Optimization here: Precompute containment of each point in the GridClassifier object, then load in O(1). Implement if performance is horrible.
			bool[,] grid_point_interior_status = new bool[map.Count + 1, map.Count + 1];
			int[] ll = map.GetIndices(new Pair(T.Xmin, T.Ymin));
			int[] ur = map.GetIndices(new Pair(T.Xmax, T.Ymax));
			int imin = ll[0];
			int jmin = ll[1];
			int imax = ur[0];
			int jmax = ur[1];
			//Loop 1: precompute interiors
			for (int i = imin; i < imax + 1; i++)
			{
				for (int j = jmin; j < jmax + 1; j++)
				{
					grid_point_interior_status[i,j] = T.Contains(map.ComputeXY(i,j));
				}
			}
			
			//Loop 2: iterate. 
			for (int i = 0; i < 3; i++)
			{
				int[] vertexcoords = map.GetIndices(T[i]);
				i_indices_list.Add(vertexcoords[0]);
				j_indices_list.Add(vertexcoords[1]);
			}
			for (int i = imin; i < imax + 1; i++)
			{
				for (int j = jmin; j < jmax + 1; j++)
				{
					//Check corners of rectangle
					bool[] corners = 
					{
					grid_point_interior_status[i,j],//Lower left
					grid_point_interior_status[i+1,j],//Lower right
					grid_point_interior_status[i,j+1],//Upper left
					grid_point_interior_status[i+1,j+1]//Upper right
					};
					bool rectangle_has_interior_point = MathTools.CheckAny(corners);
					if (rectangle_has_interior_point)
					{
						//No more computations should occur after here.
						i_indices_list.Add(i);
						j_indices_list.Add(j);
					}
					else
					{
						//Only check three - see reference above.
						Pair[] ccw_orientation_cornerpoints = new Pair[]
						{
							new Pair(map.ComputeXY(i,j)), //Lower left
							new Pair(map.ComputeXY(i+1,j)), //Lower right
							new Pair(map.ComputeXY(i+1,j+1)), //Upper right
							new Pair(map.ComputeXY(i,j+1)) //Uppper left
						};
						if (check_three_side(ccw_orientation_cornerpoints, T))
						{
							i_indices_list.Add(i);
							j_indices_list.Add(j);
						}
					}
				}
			}
			i_indices = i_indices_list.ToArray();
			j_indices = j_indices_list.ToArray();
		}
		private bool check_three_side(Pair[] counter_clockwise_points, Triangle T)
		{
			//Messy, but necessary to minimize computations
			if (T.CheckSegmentBoundaryIntersection(counter_clockwise_points[0], counter_clockwise_points[1])) return true;
			if (T.CheckSegmentBoundaryIntersection(counter_clockwise_points[1], counter_clockwise_points[2])) return true;
			if (T.CheckSegmentBoundaryIntersection(counter_clockwise_points[2], counter_clockwise_points[3])) return true;
			return false;
		}
	}
}