using System;
using System.Collections.Generic;
using m540;
using _3DSimple;
using STL_Loader_v1;
using System.Reflection;
using System.IO;
using System.Threading;
using MoreMathTools;

namespace InitDataTools
{
	public enum InitAlgorithm
	{
		NAIVE,
		PRECLASSIFY
	}
	public static class AdjacencyInitializer
	{
		public static Adjacency[] InitializeEdges(ref ModelNode[] data, double parameter, InitAlgorithm algorithm)
		{
			switch (algorithm)
			{
				case InitAlgorithm.NAIVE:
				{
					return InitializeVietorisRipsNaive(ref data, parameter);
				}
				case InitAlgorithm.PRECLASSIFY:
				{
					throw new Exception("Error: Not yet implemented.");
					//return new Adjacency[]{};
				}
				default:
				{
					throw new Exception("Error: InitAlgorithm option invalid.");
				}
			}
		}
		private static Adjacency[] InitializeVietorisRipsNaive(ref ModelNode[] data, double search_radius)
		{
			
			List<Adjacency> output = new List<Adjacency>();
			//Naive implementation for now. Will need to update in due time.
			int k = 0;
			for (int i = 0; i < data.Length; i++)
			{
				for (int j = i+1; j < data.Length; j++)
				{
					if (compute_distance(data[i], data[j]) <= search_radius)
					{
						output.Add(new Adjacency(i,j));
						data[i].AddAdjacency(k);
						data[j].AddAdjacency(k);
						k++;
					}
				}
			}
			return output.ToArray();
		}
		//Uses 2-norm
		private static double compute_distance(ModelNode x, ModelNode y)
		{
			double dx = x.CurrentLocation.X - y.CurrentLocation.X;
			double dy = x.CurrentLocation.Y - y.CurrentLocation.Y;
			double dz = x.CurrentLocation.Z - y.CurrentLocation.Z;
			return Math.Sqrt(dx*dx + dy*dy + dz*dz);
		}
	}
}