using System;
using System.Threading;
using _3DSimple;
using STL_Loader_v1;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using MoreMathTools;

namespace m540
{
	public class StlClassifier
	{
		//Intentionally not holding stl model in memory until the classification is made.
		private int bin_count;
		private string stl_location;
		private GridClassifier classifier;
		private double[] bounds; //xmin xmax ymin ymax
		private StlClassificationTable lookup_table;
		
		//Store separately to avoid crashing.
		private volatile StlClassificationTable[] volatile_tables;
		public StlClassificationTable FacetLookupTable
		{
			get 
			{
				if (!lookup_table.IsEmpty)
				{
					return lookup_table;
				}
				else
				{
					throw new Exception("Error: Classification table access attempt before computation.");
				}
			}
		}
		//Global access for async classifier
		private volatile STL temp_facet_data;
		
		//This variable defines a way for each thread in the async classifier to "check-in"
		private volatile bool[] thread_signin;
		
		
		public StlClassifier(string _stl_location, int _bin_count)
		{
			//build bounds...
			bin_count = _bin_count;
			stl_location = _stl_location;
			temp_facet_data = new STL(_stl_location);
			bounds = get_bounds();
			classifier = new GridClassifier(bin_count, bounds);
			lookup_table = new StlClassificationTable(bin_count);
		}
		
		private double[] get_bounds()
		{
			//Run in single thread, probably better performance than multithreading. Also stupid.
			double xmin = double.PositiveInfinity;
			double xmax = double.NegativeInfinity;
			double ymin = double.PositiveInfinity;
			double ymax = double.NegativeInfinity;
			for (int i = 0; i < temp_facet_data.FacetCount; i++)
			{
				double localXmax = get_max(temp_facet_data[i].V1.X, temp_facet_data[i].V2.X, temp_facet_data[i].V3.X);
				double localXmin = get_min(temp_facet_data[i].V1.X, temp_facet_data[i].V2.X, temp_facet_data[i].V3.X);
				double localYmax = get_max(temp_facet_data[i].V1.Y, temp_facet_data[i].V2.Y, temp_facet_data[i].V3.Y);
				double localYmin = get_min(temp_facet_data[i].V1.Y, temp_facet_data[i].V2.Y, temp_facet_data[i].V3.Y);
				xmin = localXmin < xmin ? localXmin : xmin;
				xmax = localXmax > xmax ? localXmax : xmax;
				ymin = localYmin < ymin ? localYmin : ymin;
				ymax = localYmax > ymax ? localYmax : ymax;
			}
			return new double[] {xmin, xmax, ymin, ymax};
		}
		
		private double get_max(params double[] values)
		{
			double maxValue = double.NegativeInfinity;
			foreach (double xi in values) maxValue = xi > maxValue ? xi : maxValue;
			return maxValue;
		}
		private double get_min(params double[] values)
		{
			double minValue = double.PositiveInfinity;
			foreach (double xi in values) minValue = xi < minValue ? xi : minValue;
			return minValue;
		}
		
		//test function for asynchronous classification. Will need to be modified to return some classification object.
		//This function is not run by default.
		public void RunClassify(int process_count)
		{
			//Declare and initialize volatile table array.
			volatile_tables = new StlClassificationTable[process_count];
			for (int i = 0; i < process_count; i++)
			{
				volatile_tables[i] = new StlClassificationTable(bin_count);
			}
			
			//Populates the volatile array
			process_stl_async(stl_location, process_count);
		}
		
		//Loads stl,  makes computations ("classify" each facet), then removes the facet data from memory.
		private void process_stl_async(string stl_location, int process_count)
		{			
			//Build all threads, start. (This snippet may be worth saving?)
			Thread[] threads = new Thread[process_count];
			thread_signin = new bool[process_count];
			for (int i = 0; i < process_count; i++)
			{
				//Lambda expressions pass by reference. Dereferencing is necessary here.
				int dereference_i = i;
				
				threads[i] = new Thread (() => process_stl_singleprocess(dereference_i, process_count));
			}
			foreach (Thread t in threads)
			{
				t.Start();
			}
			//Wait for all threads to complete.
			while (!MathTools.CheckAll(thread_signin))
			{
				Thread.Sleep(1);
			}
			temp_facet_data = null;	
			
			//Combine tables and reduce.
			lookup_table = StlClassificationTable.CombineReduce(volatile_tables);			
		}
		private void process_stl_singleprocess(int process_number, int process_count)
		{
			for (int i = process_number; i < temp_facet_data.FacetCount; i+=process_count)
			{
				//This is the Hard-working part. Classify each facet using a RectangularCover
				//On the projection of the facet to the x-y plane.
				Triangle xyProjection = project_xy(temp_facet_data[i]);
				RectangularCover facet_cover = new RectangularCover(xyProjection, classifier);
				map_cover_volatile(facet_cover, i, process_number);
			}
			thread_signin[process_number] = true;
		}
		private void map_cover_volatile(RectangularCover cover, int index, int process_id)
		{
			if (cover.RegionCount > 0)
			{
				for (int i = 0; i < cover.RegionCount; i++)
				{
					int[] indices = cover[i];
					volatile_tables[process_id].AddEntry(indices[0], indices[1], index);
				}
			}
		}
		private Triangle project_xy(Facet f)
		{
			Pair[] pairs = new Pair[]
			{
				new Pair(f.V1.X, f.V1.Y),
				new Pair(f.V2.X, f.V2.Y),
				new Pair(f.V3.X, f.V3.Y)
			};
			return new Triangle(pairs);
		}
	}
	
	public class StlClassificationTable
	{
		private List<int>[,] table;
		private int count;
		public int Count {get {return count;}}
		bool is_empty;
		public bool IsEmpty {get {return is_empty;}}
		public StlClassificationTable(int _count)
		{
			table = new List<int>[_count, _count];
			count = _count;
			is_empty = true;
			for (int i = 0; i < count; i++)
			{
				for (int j = 0; j < count; j++)
				{
					table[i,j] = new List<int>();
				}
			}
		}
		public List<int> this[int i, int j]
		{
			get {return table[i,j];}
			set {table[i,j] = value; is_empty = false;}
		}
		public static StlClassificationTable CombineReduce(StlClassificationTable[] tables)
		{
			if (tables.Length == 0) {throw new Exception("Error: no tables to combine.");}
			int n = tables[0].Count;
			StlClassificationTable output = new StlClassificationTable(tables[0].Count);
			for (int i = 0; i < n; i++)
			{
				for (int j = 0; j < n; j++)
				{
					List<int> entry = new List<int>();
					for (int p = 0; p < tables.Length; p++)
					{
						List<int> this_table = (tables[p])[i,j];
						for (int y = 0; y < this_table.Count; y++)
						{
							entry.Add(this_table[y]);
						}
					}
					output[i,j] = entry;
				}
			}
			output.Reduce();
			return output;
		}
		public void AddEntry(int i, int j, int index)
		{
			table[i,j].Add(index);
			is_empty = false;
		}
		public void Reduce()
		{
			for (int i = 0; i < count; i++)
			{
				for (int j = 0; j < count; j++)
				{
					table[i,j] =  table[i,j].Distinct().ToList();
				}
			}
		}
	}
}
