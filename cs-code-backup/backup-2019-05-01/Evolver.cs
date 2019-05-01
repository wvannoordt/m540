using System;
using System.Collections.Generic;
using m540;
using _3DSimple;
using STL_Loader_v1;
using System.Reflection;
using System.IO;
using System.Threading;
using MoreMathTools;
using InitDataTools;

namespace Evolving
{
	public class Evolver
	{
		private volatile LatticeState currentstate;
		private int cores;
		private double delta_t;
		public Evolver(LatticeState _currentstate, double _delta_t, int _cores)
		{
			currentstate = _currentstate;
			cores = _cores;
			delta_t = _delta_t;
		}
		public void EvolveAll(string directoryname, int timecount, bool allow_overwrite)
		{
			if (Directory.Exists(directoryname) && allow_overwrite) {runDeleteDirectory(directoryname);}
			if (!Directory.Exists(directoryname)) {Directory.CreateDirectory(directoryname);}
			for (int i = 0; i < timecount; i++)
			{
				string dirname = clean_name(directoryname) + "\\timestep_" + buffermatch(i, timecount).ToString();
				step_all();
				currentstate.WriteToDirectory(dirname);
			}
		}
		private string buffermatch(int i, int count)
		{
			int buffer_count = count.ToString().Length;
			string output = "";
			while((output + i.ToString()).Length < buffer_count)
			{
				output += "0";
			}
			return output + i.ToString();
		}
		private void step_all()
		{
			for (int i = 0; i < currentstate.NodeCount; i++)
			{
				step_single(currentstate.GetNode(i), i);
			}
		}
		private void step_single(ModelNode x, int node_index)
		{
			if (!x.OnBoundary)
			{
				Vector3 current_force = get_force(x, node_index);
				x.LastVelocity = deref_vector(x.CurrentVelocity);
				x.LastLocation = deref_point(x.CurrentLocation);
				x.CurrentLocation = deref_point(x.CurrentLocation) + delta_t * x.CurrentVelocity;
				x.CurrentVelocity = deref_vector(x.CurrentVelocity) + (delta_t/ x.Mass) * current_force;
			}
		}
		private Vector3 deref_vector(Vector3 v)
		{
			return new Vector3(v.I, v.J, v.K);
		}
		private Point3 deref_point(Point3 p)
		{
			return new Point3(p.X, p.Y, p.Z);
		}
		private Vector3 get_force(ModelNode x, int node_index)
		{
			List<int> indices = x.AdjacencyIndices;
			Vector3 total_force = Vector3.Zero;
			for (int i = 0; i < x.EdgeCount; i++)
			{
				Adjacency current_edge = currentstate.GetEdge(indices[i]);
				int relevant_index = current_edge.GetRelevantIndex(node_index);
				ModelNode y = currentstate.GetNode(relevant_index);
				Vector3 direction = new Vector3(x.CurrentLocation, y.CurrentLocation);
				double eff_length = direction.Norm - current_edge.EquilibriumLength;
				double total_magnitude = current_edge.SpringConstant * eff_length;
				Vector3 current_force = (total_magnitude / direction.Norm) * direction;
				//Vector3 current_force = direction;
				total_force = total_force + current_force;
			}
			return total_force - x.DampingCoefficient * x.CurrentVelocity;
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
	}
}