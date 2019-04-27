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
}
