using System;

using MessagePack;
namespace RNumerics
{
	// interval [a,b] on Real line. 
	//   TODO: should check that a <= b !!
	[MessagePackObject]
	public struct Interval1d
	{
		[Key(0)]
		public double a;
		[Key(1)]
		public double b;

		public Interval1d(double f) { a = b = f; }
		public Interval1d(double x, double y) { a = x; b = y; }
		public Interval1d(double[] v2) { a = v2[0]; b = v2[1]; }
		public Interval1d(float f) { a = b = f; }
		public Interval1d(float x, float y) { a = x; b = y; }
		public Interval1d(float[] v2) { a = v2[0]; b = v2[1]; }
		public Interval1d(Interval1d copy) { a = copy.a; b = copy.b; }

		[IgnoreMember]
		static public readonly Interval1d Zero = new(0.0f, 0.0f);
		[IgnoreMember]
		static public readonly Interval1d Empty = new(double.MaxValue, -double.MaxValue);
		[IgnoreMember]
		static public readonly Interval1d Infinite = new(-double.MaxValue, double.MaxValue);


		public static Interval1d Unsorted(double x, double y) {
			return (x < y) ? new Interval1d(x, y) : new Interval1d(y, x);
		}

		[IgnoreMember]
		public double this[int key]
		{
			get => (key == 0) ? a : b;
			set {
				if (key == 0) { a = value; }
				else {
					b = value;
				}
			}
		}


		[IgnoreMember]
		public double LengthSquared => (a - b) * (a - b);
		[IgnoreMember]
		public double Length => b - a;
		[IgnoreMember]
		public bool IsConstant => b == a;

		[IgnoreMember]
		public double Center => (b + a) * 0.5;

		public void Contain(double d) {
			if (d < a) {
				a = d;
			}

			if (d > b) {
				b = d;
			}
		}

		public bool Contains(double d) {
			return d >= a && d <= b;
		}


		public bool Overlaps(Interval1d o) {
			return !(o.a > b || o.b < a);
		}

		public double SquaredDist(Interval1d o) {
			return b < o.a ? (o.a - b) * (o.a - b) : a > o.b ? (a - o.b) * (a - o.b) : 0;
		}
		public double Dist(Interval1d o) {
			return b < o.a ? o.a - b : a > o.b ? a - o.b : 0;
		}

		public Interval1d IntersectionWith(ref Interval1d o) {
			return o.a > b || o.b < a ? Interval1d.Empty : new Interval1d(Math.Max(a, o.a), Math.Min(b, o.b));
		}

		/// <summary>
		/// clamp value f to interval [a,b]
		/// </summary>
		public double Clamp(double f) {
			return (f < a) ? a : (f > b) ? b : f;
		}

		/// <summary>
		/// interpolate between a and b using value t in range [0,1]
		/// </summary>
		public double Interpolate(double t) {
			return ((1 - t) * a) + (t * b);
		}

		/// <summary>
		/// Convert value into (clamped) t value in range [0,1]
		/// </summary>
		public double GetT(double value) {
			return value <= a ? 0 : value >= b ? 1 : a == b ? 0.5 : (value - a) / (b - a);
		}

		public void Set(Interval1d o) {
			a = o.a;
			b = o.b;
		}
		public void Set(double fA, double fB) {
			a = fA;
			b = fB;
		}



		public static Interval1d operator -(Interval1d v) => new(-v.a, -v.b);


		public static Interval1d operator +(Interval1d a, double f) => new(a.a + f, a.b + f);
		public static Interval1d operator -(Interval1d a, double f) => new(a.a - f, a.b - f);

		public static Interval1d operator *(Interval1d a, double f) => new(a.a * f, a.b * f);


		public override string ToString() {
			return string.Format("[{0:F8},{1:F8}]", a, b);
		}


	}
}
