using System;
using MoreMathTools;

namespace m540
{
	//Defines a discrete grid on a bounded, rectangular domain.
	//Purpose is to return a cell-index pair for a given point.
	public class GridClassifier
	{
		private int discrete_count;
		//[xmin xmax ymin ymax]
		private double[] XY_bounds;
		private double deltax, deltay;
		private double Lx, Ly;
		public double DeltaX {get {return deltax;}}
		public double DeltaY {get {return deltay;}}
		bool allow_illegal_access = false;
		public int Count {get {return discrete_count;}}
		public double[] Bounds {get {return XY_bounds;}}
		public GridClassifier(int _discrete_count, double[] _XY_bounds)
		{
			discrete_count = _discrete_count;
			XY_bounds = _XY_bounds;
			Lx = XY_bounds[1] - XY_bounds[0];
			Ly = XY_bounds[3] - XY_bounds[2];
			deltax = Lx/discrete_count;
			deltay = Ly/discrete_count;
			if (Lx < 0 || Ly < 0) {throw new Exception("Error: at least one domain dimension length is negative.");}
		}
		public double[] ComputeXY(int[] ij)
		{
			return ComputeXY(ij[0], ij[1]);
		}
		public double[] ComputeXY(int i, int j)
		{
			//ij = (i, j) = zero-based, cartesian coordinates.
			//note that i,j UP TO discrete_count IS LEGAL.
			if (!allow_illegal_access)
			{
				if (i < 0 || i > discrete_count)
				{
					throw new Exception("Error: illegal access: i = " + i.ToString() + ", which lies outside [0," + discrete_count.ToString() +"].");
				}
				if (j < 0 || j > discrete_count)
				{
					throw new Exception("Error: illegal access: j = " + j.ToString() + ", which lies outside [0," + discrete_count.ToString() +"].");
				}
			}
			double[] output = 
			{
				XY_bounds[0] + i*deltax,
				XY_bounds[2] + j*deltay
			};
			return output;
		}
		public void SetAllowIllegalAccess(bool bvalue)
		{
			allow_illegal_access = bvalue;
		}
		public int[] GetIndices(double x, double y)
		{
			if (!allow_illegal_access)
			{
				if ((x < XY_bounds[0] || x > XY_bounds[1])) 
				{
					throw new Exception("Error: illegal classification: x = " + x.ToString() + ", which lies outside [" + XY_bounds[0].ToString() + ", " + XY_bounds[1].ToString() +"].");
				}
				if ((y < XY_bounds[2] || y > XY_bounds[3]))
				{
					throw new Exception("Error: illegal classification: y = " + y.ToString() + ", which lies outside [" + XY_bounds[2].ToString() + ", " + XY_bounds[3].ToString() +"].");
				}
			}
			int[] output = new int[] {(int)(discrete_count*(x - XY_bounds[0]) / Lx), (int)(discrete_count*(y - XY_bounds[2]) / Ly)};
			if (!allow_illegal_access)
			{
				if (output[0] >= discrete_count) {output[0] = discrete_count-1;}
				if (output[1] >= discrete_count) {output[1] = discrete_count-1;}
			}
			return output;
		}
		public int[] GetIndices(Pair p)
		{
			return GetIndices(p.X, p.Y);
		}
	}
}