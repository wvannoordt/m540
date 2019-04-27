using System;
using System.Collections.Generic;
using _3DSimple;
namespace InitDataTools
{
  public class ModelNode
	{
		string force_csv_file, mvt_csv_file;
		private Point3 current_location;
		private Point3 last_location;
		private Vector3 current_velocity, previous_velocity;
		public string ForceTerm{get {return force_csv_file;} set {force_csv_file = value;}}//tag set
		public string MovementTerm{get {return mvt_csv_file;} set {mvt_csv_file = value;}}//tag set
		public double Mass {get {return mass;} set {mass = value;}}//tag set
		public double DampingCoefficient {get {return damping_coefficient;} set {damping_coefficient = value;}}//tag set
		private double mass, damping_coefficient;
		private bool on_boundary;
		public bool OnBoundary {get {return on_boundary;} set {on_boundary = value;}}//tag set
		public Point3 CurrentLocation {get {return current_location;} set {current_location = value;}}
		public Point3 LastLocation {get {return last_location;} set {last_location = value;}}
    public string ToSaveString()
    {
      return current_location.X.ToString() + "," + current_location.Y.ToString() + "," + current_location.Z.ToString();
    }
		public ModelNode(Point3 _current_location, Vector3 _current_velocity, bool _on_boundary, double _mass)
		{
			mass = _mass;
			current_location = _current_location;
			current_velocity = _current_velocity;
			previous_velocity = Vector3.Zero;
			on_boundary = _on_boundary;
			force_csv_file = null;
			mvt_csv_file = null;
			last_location = Point3.Origin; //Assume that upon initialization, nothing is known about previous position/velocity.
		}
		public ModelNode(Point3 _current_location, double _mass):this(_current_location, Vector3.Zero, false, _mass){}
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
    public string ToSaveString()
    {
      return endindex.ToString() + "," + rootindex.ToString() + "," + equilibrium_length.ToString() + "," + spring_constant.ToString() + "," + broken.ToString();
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
