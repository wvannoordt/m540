using System;
using System.Collections.Generic;
using m540;
using _3DSimple;
using STL_Loader_v1;
using System.Reflection;
using System.IO;

namespace InitDataTools
{
	//Tags apply to ModelNode (mass, damping coefficient, initial data, forcing terms, boundary),
	//As well as to Adjacencies (:), so each value will have to be parsed (messy)?
	//Argument types are strings, bools, doubles, ints...
	
	//Ex: 
	//tag myfile.stl
	//nodes:OnBoundary=true,MovementTerm=force_terms/mvt.csv
	//edges:EquilibriumLength=0.2
	//endtag
	
	public class Tag 
	{
		string file_argument;
		List<string> node_parameter_names, node_parameter_values;
		List<string> edge_parameter_names, edge_parameter_values;
		public int EdgeAssignmentCount {get {return edge_parameter_values.Count;}}
		public int NodeAssignmentCount {get {return node_parameter_values.Count;}}
		public string RegionFilename {get {return file_argument;}}
		public string[] GetEdgeAssignment(int i)
		{
			return new string[] {edge_parameter_names[i], edge_parameter_values[i]};
		}
		public string[] GetNodeAssignment(int i)
		{
			return new string[] {node_parameter_names[i], node_parameter_values[i]};
		}
		
		public Tag(string _file_argument, List<string> _node_param_names, List<string> _node_param_values, List<string> _edge_param_names, List<string> _edge_param_values)
		{
			file_argument = _file_argument;
			node_parameter_names = _node_param_names;
			node_parameter_values = _node_param_values;
			edge_parameter_names = _edge_param_names;
			edge_parameter_values = _edge_param_values;
			bool node_mismatch = node_parameter_names.Count != node_parameter_values.Count;
			bool edge_mismatch = edge_parameter_names.Count != edge_parameter_values.Count;
			if (node_mismatch) {throw new Exception("Error: node assignment mismatch.");}
			if (edge_mismatch) {throw new Exception("Error: edge assignment mismatch.");}
		}
		
		public static void SetProperty(object a, string propertyName, object propertyValue)
		{
			PropertyInfo prop = a.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
			if(null != prop && prop.CanWrite)
			{
				Type T1 = prop.PropertyType;
				Type T2 = propertyValue.GetType();
				prop.SetValue(a, propertyValue, null);
			}
		}
		
		
		#region io
		public override string ToString()
		{
			string output = "Tagging points on interior of " + file_argument + ":\n>> Node parameters:\n";
			for (int i = 0; i < node_parameter_names.Count; i++)
			{
				output += node_parameter_names[i] + " = " + node_parameter_values[i] + "\n";
			}
			output += ">> Edge parameters:\n";
			for (int i = 0; i < edge_parameter_names.Count; i++)
			{
				output += edge_parameter_names[i] + " = " + edge_parameter_values[i] + "\n";
			}
			return output;
		}
		public static Tag FromStringArray(params string[] lines)
		{
			if (lines.Length == 0) {throw new Exception("Error: tag cannot be empty.");}
			string filename = lines[0].Split(' ')[lines[0].Split(' ').Length - 1];
			List<string> _node_param_names = new List<string>();
			List<string> _node_param_values = new List<string>();
			List<string> _edge_param_names = new List<string>();
			List<string> _edge_param_values = new List<string>();
			for (int i = 1; i < lines.Length - 1; i++)
			{
				string line = lines[i];
				bool is_node = line.StartsWith("nodes:");
				bool is_edge = line.StartsWith("edges:");
				string[] values = line.Split(':')[1].Split(',');
				if (is_node || is_edge)
				{
					foreach (string p in values)
					{
						string name = p.Split('=')[0];
						string vval = p.Split('=')[1];
						if (is_node)
						{
							_node_param_names.Add(name);
							_node_param_values.Add(vval);
						}
						if (is_edge)
						{
							_edge_param_names.Add(name);
							_edge_param_values.Add(vval);
						}
					}
				}
			}
			return new Tag(filename, _node_param_names,_node_param_values,_edge_param_names,_edge_param_values);
		}
		public static Tag[] ExtractFromFile(string filename)
		{
			string[] filestuff = File.ReadAllLines(filename);
			List<Tag> tags = new List<Tag>();
			List<string> currenttag = new List<string>();
			bool in_tag = false;
			for (int i = 0; i < filestuff.Length; i++)
			{
				string line = filestuff[i].Trim();
				if (line.StartsWith("tag")) {in_tag = true;}
				if (line == "endtag") 
				{
					in_tag = false; 
					currenttag.Add(line);
					tags.Add(Tag.FromStringArray(currenttag.ToArray())); 
					currenttag = new List<string>();
				}
				if (in_tag)
				{
					currenttag.Add(line);
				}
			}
			return tags.ToArray();
		}
		#endregion
	}
	public class StateEvolver
	{
		private LatticeState current_state;
		public StateEvolver(LatticeState initial_data)
		{
			current_state = initial_data;
		}
		public void Step()
		{
			//This algorithm will also suck...
		}
	}
	public static class AdjacencyInitializer
	{
		public static void InitializeVietorisRips(ref LatticeState data, double search_radius)
		{
			//This algorithm is going to suck...
		}
	}
	public class LatticeState
	{
		private ModelNode[] nodes;
		
		//This is where tags are applied.
		public LatticeState(){}
		
		//These are separate because the algorithm below is generally slow!
		public void InitializeTags(PointCloud points, Tag[] tags, int cores)
		{
			StlClassifier[] body_list = BuildClassifiers(tags, cores);
			List<ModelNode> _nodes = new List<ModelNode>();
			List<int[]> relevant_tags = new List<int[]>();
			for (int i = 0; i < points.Count; i++)
			{
				
			}
		}
		
		//don't want to parallelize this since each iteration is already parallelized.
		private StlClassifier[] BuildClassifiers(Tag[] tags, int cores)
		{
			StlClassifier[] output = new StlClassifier[tags.Length];
			for (int i = 0; i < tags.Length; i++)
			{
				//StlClassifier current_classifier = new StlClassifier()
			}
			return null;
		}
	}
	public class PointCloud //PointCloud >> tags >> modelnodes (Lattice State)
	{
		private Point3[] points;
		public int Count
		{
			get {return points.Length;}
		}
		public PointCloud(Point3[] _points)
		{
			points = _points;
		}
		public Point3 this[int i]
		{
			get {return points[i];}
			set {points[i] = value;}
		}
		public static PointCloud FromCsv(string filename)
		{
			string[] filestuff = File.ReadAllLines(filename);
			Point3[] input_points = new Point3[filestuff.Length];
			for (int i = 0; i < input_points.Length; i++)
			{
				Point3 p;
				if (TryParsePoint(filestuff[i], out p))
				{
					input_points[i] = p;
				}
				else
				{
					throw new Exception("Error: cannot parse line " + i + " (" + filestuff[i] + ") in file " + filename + ".");
				}
			}
			return new PointCloud(input_points);
		}
		private static bool TryParsePoint(string s, out Point3 output)
		{
			output = null;
			double x,y,z;
			string[] spt = s.Split(',');
			if (!(double.TryParse(spt[0], out x) && double.TryParse(spt[1], out y) && double.TryParse(spt[2], out z)))
			{
				return false;
			}
			else
			{
				output = new Point3(x, y, z);
				return true;
			}
		}
	}
	public class ModelNode
	{
		string force_csv_file, mvt_csv_file;
		private Point3 current_location;
		private Point3 last_location;
		private Vector3 current_velocity, previous_velocity;
		private List<Adjacency> adjacencies;
		public string ForceTerm{get {return force_csv_file;} set {force_csv_file = value;}}//tag set
		public string MovementTerm{get {return mvt_csv_file;} set {mvt_csv_file = value;}}//tag set
		public int AdjacencyCount {get {return adjacencies.Count;}}
		public List<Adjacency> Adjacencies {get {return adjacencies;}}
		public double Mass {get {return mass;} set {mass = value;}}//tag set
		public double DampingCoefficient {get {return damping_coefficient;} set {damping_coefficient = value;}}//tag set
		private double mass, damping_coefficient;
		private bool on_boundary;
		public bool OnBoundary {get {return on_boundary;} set {on_boundary = value;}}//tag set
		public Point3 CurrentLocation {get {return current_location;} set {current_location = value;}}
		public Point3 LastLocation {get {return last_location;} set {last_location = value;}}
		public Adjacency this[int i]{get {return adjacencies[i];}}
		
		public ModelNode(Point3 _current_location, Vector3 _current_velocity, bool _on_boundary, double _mass)
		{
			mass = _mass;
			adjacencies = new List<Adjacency>();
			current_location = _current_location;
			current_velocity = _current_velocity;
			previous_velocity = Vector3.Zero;
			on_boundary = _on_boundary;
			force_csv_file = null;
			mvt_csv_file = null;
			last_location = Point3.Origin; //Assume that upon initialization, nothing is known about previous position/velocity.
		}
		public ModelNode(Point3 _current_location, double _mass):this(_current_location, Vector3.Zero, false, _mass){}
		public void AddAdjacency(Adjacency a)
		{
			adjacencies.Add(a);
		}
	}
	public class Adjacency
	{
		//Add nonlinearity effects?
		private int endindex, rootindex;
		private double spring_constant, equilibrium_length, force_limit, sp_damping_coefficient;
		private bool broken;
		private const double EQ_LENGTH_DEFAULT = 1;
		private const double FORCE_LIMIT_DEFAULT = double.PositiveInfinity;
		private const double SPRING_CONST_DEFAULT = 1;
		private const double SP_DAMPING_DEFAULT = 0;
		public bool Broken {get {return broken;}}
		public int RootIndex {get {return rootindex;} set {rootindex = value;}}
		public int EndIndex {get {return endindex;} set {endindex = value;}}
		public double SpringConstant {get {return spring_constant;} set {spring_constant = value;}}//tag set
		public double EquilibriumLength {get {return equilibrium_length;} set {equilibrium_length = value;}}//tag set
		public double ForceLimit {get {return force_limit;} set {force_limit = value;}}//tag set
		public double SpringDampingCoefficient {get {return sp_damping_coefficient;} set {sp_damping_coefficient = value;}}//tag set
		public Adjacency(int _rootindex, int _endindex, double _spring_constant, double _equilibrium_length, double _sp_damping_coefficient, double _force_limit)
		{
			endindex = _endindex;
			rootindex = _rootindex;
			spring_constant = _spring_constant;
			equilibrium_length = _equilibrium_length;
			force_limit = _force_limit;
			sp_damping_coefficient = _sp_damping_coefficient;
			broken = false;
		}
		public void Break()
		{
			broken = true;
		}
		public void Fix()
		{
			broken = false;
		}
		public Adjacency(int i1, int i2, double k, double r, double b):this(i1, i2, k, r, b, FORCE_LIMIT_DEFAULT){}
		public Adjacency(int i1, int i2):this(i1, i2, SPRING_CONST_DEFAULT, EQ_LENGTH_DEFAULT, SP_DAMPING_DEFAULT, FORCE_LIMIT_DEFAULT){}
	}
}