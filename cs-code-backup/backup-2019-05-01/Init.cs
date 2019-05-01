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
  //Storing initialization data in separate worker class because c# doesn't have a way to deallocate memory
  public class LatticeStateInitializer
  {
    private volatile bool[] thread_signin;
    private volatile StlClassifier[] body_list;
    private volatile PointCloud points;
	private double adjacencyradius;
    private int cores, searchresolution;
    private volatile List<ModelNode>[] par_ext_model_nodes; //Captures all model nodes with a relevant tag.
    private volatile List<int[]>[] par_ext_relevant_indices;
	private Tag[] all_tags;

    //Computes relevant tags for each node in parallel
    private void ComputeRelevantsAsync(int process_count)
    {
		Thread[] threads = new Thread[process_count];
		thread_signin = new bool[process_count];
		par_ext_model_nodes = new List<ModelNode>[process_count];
		par_ext_relevant_indices = new List<int[]>[process_count];
		for (int i = 0; i < process_count; i++)
		{
			par_ext_model_nodes[i] = new List<ModelNode>();
			par_ext_relevant_indices[i] = new List<int[]>();
			//Lambda expressions pass by reference. Dereferencing is necessary here.
			int dereference_i = i;
			threads[i] = new Thread (() => ComputeRelevantSingleProcess(dereference_i, process_count));
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
    }
    //Computes the indices of tags that apply to points.
    private void ComputeRelevantSingleProcess(int process_num, int process_count)
    {
      thread_signin[process_num] = false;
        for (int i = process_num; i < points.Count; i+= process_count)
        {
          ModelNode current_node = new ModelNode(points[i], 1);
          List<int> current_relevants = new List<int>();
          for (int p = 0; p < body_list.Length; p++)
          {
            if (body_list[p].IsInteriorPoint(points[i]))
            {
              current_relevants.Add(p);
            }
          }
          int[] relevants = current_relevants.ToArray();
          if (relevants.Length > 0) //remove all oints with no applicable tags
          {
            par_ext_model_nodes[process_num].Add(current_node);
            par_ext_relevant_indices[process_num].Add(relevants);
          }
        }
        thread_signin[process_num] = true;
    }
    public LatticeStateInitializer(PointCloud _points, Tag[] tags, int _cores, int _searchresolution, double vr_radius)
    {
      cores = _cores;
      points = _points;
      searchresolution = _searchresolution;
	  adjacencyradius = vr_radius;
	  all_tags = tags;
      //Body list automatically has an elementwise correspondence with tags.
      body_list = BuildClassifiers(tags, cores, searchresolution);
    }
    //Build nodes using tags passed to this object
    public void InitializeNodes()
    {
        ComputeRelevantsAsync(cores);
    }
    public LatticeState BuildLatticeState()
    {
	  InitAlgorithm algorithm = InitAlgorithm.PRECLASSIFY;
      List<ModelNode> built_nodes = new List<ModelNode>();
      for (int i = 0; i < cores; i++)
      {
        List<ModelNode> current_list = par_ext_model_nodes[i];
        foreach(ModelNode current_node in current_list)
        {
          built_nodes.Add(current_node);
        }
      }
	  ModelNode[] ar_built_nodes = built_nodes.ToArray();
	  Adjacency[] ar_built_edges;
	  try
	  {
		ar_built_edges = AdjacencyInitializer.InitializeEdges(ref ar_built_nodes, adjacencyradius, algorithm);
	  }
	  catch
	  {
		ar_built_edges = AdjacencyInitializer.InitializeEdges(ref ar_built_nodes, adjacencyradius, InitAlgorithm.NAIVE);
	  }
	  List<int[]> relevants = CombineRelevants();
	  
	  //VERY SLOW, needs re-writing if time.
	  ApplyAllTags(ref ar_built_nodes, ref ar_built_edges,  ref relevants, ref all_tags);
      return new LatticeState(ar_built_nodes, ar_built_edges);
    }
	private void ApplyAllTags(ref ModelNode[] all_nodes, ref Adjacency[] all_edges, ref List<int[]> node_relevants, ref Tag[] tags)
	{
		//Apply node properties
		for (int i = 0; i < all_nodes.Length; i++)
		{
			ModelNode current_node = all_nodes[i];
			int[] relevants = node_relevants[i];
			for (int k = 0; k < relevants.Length; k++)
			{
				int tagindex = relevants[k];
				Tag current_tag = tags[tagindex];
				current_tag.ApplyAsApplicable(ref current_node);
			}
		}
		
		//apply edge properties
		for (int i = 0; i < all_edges.Length; i++)
		{
			Adjacency current_edge = all_edges[i];
			int[] relevants_a = node_relevants[current_edge.RootIndex];
			int[] relevants_b = node_relevants[current_edge.EndIndex];
			for (int k = 0; k < relevants_a.Length; k++)
			{
				int tagindex = relevants_a[k];
				Tag current_tag = tags[tagindex];
				current_tag.ApplyAsApplicable(ref current_edge);
			}
			for (int k = 0; k < relevants_b.Length; k++)
			{
				int tagindex = relevants_b[k];
				Tag current_tag = tags[tagindex];
				current_tag.ApplyAsApplicable(ref current_edge);
			}
		}
	}
	private List<int[]> CombineRelevants()
	{
		List<int[]> output = new List<int[]>();
		for (int i = 0; i < cores; i++)
		{
			List<int[]> current_list = par_ext_relevant_indices[i];
			foreach(int[] current_array in current_list)
			{
				output.Add(current_array);
			}
		}
		return output;
	}

    //No parallelization, RunClassify is parallelized.
    private StlClassifier[] BuildClassifiers(Tag[] tags, int cores, int searchresolution)
    {
      StlClassifier[] output = new StlClassifier[tags.Length];
      for (int i = 0; i < tags.Length; i++)
      {
        StlClassifier current_classifier = new StlClassifier(tags[i].RegionFilename, searchresolution);

        //oof this is a lot
        current_classifier.RunClassify(cores);
        output[i] = current_classifier;
      }
      return output;
    }
  }
}
