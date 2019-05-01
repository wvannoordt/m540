using System;
using System.Collections.Generic;
using m540;
using _3DSimple;
using STL_Loader_v1;
using System.Reflection;
using System.IO;

namespace InitDataTools
{
  public class LatticeState // apply tags to nodes > build adjacencies > apply tags to edges
  {
    private ModelNode[] nodes;
    private Adjacency[] edges;
    private const string EDGE_FILE_NAME = "edge.csv";
    private const string NODE_FILE_NAME = "node.csv";
	public ModelNode GetNode(int i) {return nodes[i];}
	public Adjacency GetEdge(int i) {return edges[i];}
	public int NodeCount {get {return nodes.Length;}}
	public int EdgeCount {get {return edges.Length;}}
    public LatticeState(ModelNode[] _nodes, Adjacency[] _edges)
    {
      nodes = _nodes;
      edges = _edges;
    }
	public void WriteToDirectory(string dirname)
	{
		WriteToDirectory(dirname, false);
	}
    public void WriteToDirectory(string dirname, bool allow_overwrite)
    {
		if (Directory.Exists(dirname)) 
		{
			if (allow_overwrite)
			{
				runDeleteDirectory(dirname);
			}
			else
			{
				throw new Exception("Error: Directory name already exists.");
			}
		}
		string usable_dir_name = clean_name(dirname);
		string filename_edge = usable_dir_name + "/" + EDGE_FILE_NAME;
		string filename_node = usable_dir_name + "/" + NODE_FILE_NAME;
		Directory.CreateDirectory(usable_dir_name);
		WriteNodesToCsv(filename_node);
		WriteEdgesToCsv(filename_edge);
    }
    //PUT IN NEW CLASS
	private static void ClearFolder(string FolderName)
	{
		DirectoryInfo dir = new DirectoryInfo(FolderName);

		foreach (FileInfo fi in dir.GetFiles())
		{
			fi.IsReadOnly = false;
			fi.Delete();
		}

		foreach (DirectoryInfo di in dir.GetDirectories())
		{
			ClearFolder(di.FullName);
			di.Delete();
		}
	}
	private static void runDeleteDirectory(string target)
	{
		if (Directory.Exists(target))
		{
			ClearFolder(target);
			Directory.Delete(target);
		}
	}
    private string clean_name(string p)
    {
      string output = string.Empty;
      char lastchar = p[p.Length - 1] ;
      if (lastchar == '\\' || lastchar == '/')
      {
        output = remove_last(p);
      }
      else
      {
        output = p;
      }
      return output;
    }
    private string remove_last(string p)
    {
      string output = string.Empty;
      for (int i = 0; i < p.Length - 1;i++)
      {
        output += p[i];
      }
      return output;
    }
    //END PUT IN CLASS
    public void WriteNodesToCsv(string filename)
    {
		string[] fileout = new string[nodes.Length];
		for (int i = 0; i < fileout.Length; i++)
		{
			fileout[i] = nodes[i].ToSaveString();
		}
		File.WriteAllLines(filename, fileout);
    }
    public void WriteEdgesToCsv(string filename)
    {
      string[] fileout = new string[edges.Length];
      for (int i = 0; i < fileout.Length; i++)
      {
        fileout[i] = edges[i].ToSaveString();
      }
      File.WriteAllLines(filename, fileout);
    }
    #region temp
    public static LatticeState BuildUnitCube()
    {
      double mass = 1;
      Point3[] ps =
      {
        new Point3(0,0,0),
        new Point3(0,1,0),
        new Point3(1,0,0),
        new Point3(1,1,0),
        new Point3(0,0,1),
        new Point3(0,1,1),
        new Point3(1,0,1),
        new Point3(1,1,1)
      };
      Adjacency[] edges =
      {
        new Adjacency(0, 1),
        new Adjacency(1, 3),
        new Adjacency(3, 2),
        new Adjacency(2, 0),
        new Adjacency(7, 3),
        new Adjacency(6, 2),
        new Adjacency(5, 1),
        new Adjacency(4, 0),
        new Adjacency(7, 6),
        new Adjacency(6, 4),
        new Adjacency(4, 5),
        new Adjacency(7, 5)
      };
      ModelNode[] nodes = new ModelNode[ps.Length];
      for (int i = 0; i < nodes.Length; i++)
      {
        nodes[i] = new ModelNode(ps[i], mass);
      }
      return new LatticeState(nodes, edges);
    }
    #endregion
  }
}
