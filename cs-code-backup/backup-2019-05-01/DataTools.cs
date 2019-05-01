using System;
using System.Collections.Generic;
using m540;
using _3DSimple;
using STL_Loader_v1;
using System.Reflection;
using System.IO;
using MoreMathTools;

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
		public void ApplyAsApplicable(ref ModelNode node)
		{
			for (int i = 0; i < node_parameter_names.Count; i++)
			{
				SetProperty(node, node_parameter_names[i], node_parameter_values[i]);
			}
		}
		public void ApplyAsApplicable(ref Adjacency edge)
		{
			for (int i = 0; i < edge_parameter_names.Count; i++)
			{
				SetProperty(edge, edge_parameter_names[i], edge_parameter_values[i]);
			}
		}
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

		public static void SetProperty(object a, string propertyName, string propertyStringValue)
		{
			try
			{
				PropertyInfo prop = a.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
				object propertyValue = Convert.ChangeType(propertyStringValue, prop.PropertyType);
				if(null != prop && prop.CanWrite)
				{
					Type T1 = prop.PropertyType;
					Type T2 = propertyValue.GetType();
					prop.SetValue(a, propertyValue, null);
				}
			}
			catch (Exception e)
			{
				string mes = "Error: could not dynamically set property " + propertyName +  " to value " + propertyStringValue + ". Exception message: ";
				throw new Exception(mes + e.Message);
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
		public static Tag FromStringArray(string[] lines)
		{
			return Tag.FromStringArray(lines, string.Empty);
		}
		private static string get_proper_filename(string first_filename, string op_extension)
		{
			string filename_clean = clean_beginning(first_filename);
			if (File.Exists(filename_clean)) return filename_clean;
			string try2 = op_extension + filename_clean;
			return try2;
		}
		private static string clean_beginning(string p)
		{
			string output = p;
			output = (output.StartsWith("\\") || output.StartsWith("/")) ? cut_front(output, 1) : output;
			output = (output.StartsWith(".\\") || output.StartsWith("./")) ? cut_front(output, 2) : output;
			return output;
		}
		private static string cut_front(string p, int a)
		{
			string output = "";
			for (int i = a; i < p.Length; i++)
			{
				output += p[i];
			}
			return output;
		}
		public static Tag FromStringArray(string[] lines, string optional_filename_extension)
		{
			if (lines.Length == 0) {throw new Exception("Error: tag cannot be empty.");}
			string filename = lines[0].Split(' ')[lines[0].Split(' ').Length - 1];
			filename = get_proper_filename(filename, optional_filename_extension);
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
						if (vval.EndsWith(".csv"))
						{
							vval = get_proper_filename(vval,optional_filename_extension);
						}
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
			string dirname = Path.GetDirectoryName(filename);
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
					tags.Add(Tag.FromStringArray(currenttag.ToArray(),  dirname + "\\"));
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
		//bounds = xmin, xmax, ymin, ymax, zmin, zmax
		public static double[] GetBoundsFromStls(string[] filenames)
		{
			double[] xmins = new double[filenames.Length];
			double[] xmaxs = new double[filenames.Length];
			double[] ymins = new double[filenames.Length];
			double[] ymaxs = new double[filenames.Length];
			double[] zmins = new double[filenames.Length];
			double[] zmaxs = new double[filenames.Length];
			int k = 0;
			foreach (string s in filenames)
			{
				double[] current_bounds = GetBoundsFromSingleStl(s);
				xmins[k] = current_bounds[0];
				xmaxs[k] = current_bounds[1];
				ymins[k] = current_bounds[2];
				ymaxs[k] = current_bounds[3];
				zmins[k] = current_bounds[4];
				zmaxs[k] = current_bounds[5];
				k++;
			}
			double xmin = MathTools.GetMin(xmins);
			double xmax = MathTools.GetMax(xmaxs);
			double ymin = MathTools.GetMin(ymins);
			double ymax = MathTools.GetMax(ymaxs);
			double zmin = MathTools.GetMin(zmins);
			double zmax = MathTools.GetMax(zmaxs);
			return new double[] {xmin, xmax, ymin, ymax, zmin, zmax};
		}
		public static double[] GetBoundsFromSingleStl(string filename)
		{
			STL body = new STL(filename);
			double xmin = double.PositiveInfinity;
			double ymin = double.PositiveInfinity;
			double zmin = double.PositiveInfinity;
			double xmax = double.NegativeInfinity;
			double ymax = double.NegativeInfinity;
			double zmax = double.NegativeInfinity;
			for (int i = 0; i < body.FacetCount; i++)
			{
				double local_xmin = MathTools.GetMin(body[i].V1.X, body[i].V2.X, body[i].V3.X);
				double local_ymin = MathTools.GetMin(body[i].V1.Y, body[i].V2.Y, body[i].V3.Y);
				double local_zmin = MathTools.GetMin(body[i].V1.Z, body[i].V2.Z, body[i].V3.Z);
				double local_xmax = MathTools.GetMax(body[i].V1.X, body[i].V2.X, body[i].V3.X);
				double local_ymax = MathTools.GetMax(body[i].V1.Y, body[i].V2.Y, body[i].V3.Y);
				double local_zmax = MathTools.GetMax(body[i].V1.Z, body[i].V2.Z, body[i].V3.Z);
				xmin = local_xmin < xmin ? local_xmin : xmin;
				ymin = local_ymin < ymin ? local_ymin : ymin;
				zmin = local_zmin < zmin ? local_zmin : zmin;
				xmax = local_xmax > xmax ? local_xmax : xmax;
				ymax = local_ymax > ymax ? local_ymax : ymax;
				zmax = local_zmax > zmax ? local_zmax : zmax;
			}
			return new double[] {xmin, xmax, ymin, ymax, zmin, zmax};
		}
		public static PointCloud FromBounds(double[] bounds, int[] counts)
		{
			double[] ds = new double[]
			{
				(bounds[1] - bounds[0]) / counts[0],
				(bounds[3] - bounds[2]) / counts[1],
				(bounds[5] - bounds[4]) / counts[2]
			};
			return PointCloud.FromBounds(bounds, ds);
		}
		public static PointCloud FromBounds(double[] bounds, int count)
		{
			double[] t = new double[] {count, count, count};
			return PointCloud.FromBounds(bounds, t);
		}
		public static PointCloud FromBounds(double[] bounds, double distance_parameter)
		{
			double[] t = new double[] {distance_parameter, distance_parameter, distance_parameter};
			return PointCloud.FromBounds(bounds, t);
		}
		public static PointCloud FromBounds(double[] bounds, double[] distance_parameters)
		{
			//Basically meshgrid, with a twist.
			if (bounds.Length != 6) {throw new Exception("Error: \"bounds\" bust be an array of 6 doubles: xmin, xmax, ymin, ymax, zmin, zmax.");}
			if (distance_parameters.Length != 3) {throw new Exception("Error: \"distance_parameters\" bust be an array of 3 doubles: dx, dy, dz.");}
			List<double[]> vecs = new List<double[]>();
			List<int> numbers = new List<int>();
			for (int i = 0; i < 3; i++)
			{
				double min = bounds[2*i];
				double max = bounds[2*i + 1];
				double delta = distance_parameters[i];
				double length = max - min;
				int count = (int)(length / delta);
				double offset = 0.5*(length%delta);
				double[] nums = new double[count];
				for (int j = 0; j < count; j++)
				{
					nums[j] = offset +  j*delta;
				}
				vecs.Add(nums);
				numbers.Add(count);
			}
			Point3[] output = new Point3[MathTools.Prod(numbers.ToArray())];
			int counter = 0;
			double[] xs = vecs[0];
			double[] ys = vecs[1];
			double[] zs = vecs[2];
			for (int i = 0; i < xs.Length; i++)
			{
				for (int j = 0; j < ys.Length; j++)
				{
					for (int k = 0; k < zs.Length; k++)
					{
						output[counter] = new Point3(bounds[0] + xs[i], bounds[2] + ys[j], bounds[4] + zs[k]);
						counter++;
					}
				}
			}
			return new PointCloud(output);
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
		public void WriteToCsv(string filename)
		{
			string[] fileout = new string[points.Length];
			for (int i = 0; i < fileout.Length; i++)
			{
				fileout[i] = points[i].X.ToString() + "," + points[i].Y.ToString() + "," + points[i].Z.ToString();
			}
			File.WriteAllLines(filename, fileout);
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
