﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RNumerics
{
	public class PolyLine2d : IEnumerable<Vector2d>
	{
		protected List<Vector2d> vertices;
		public int Timestamp;

		public PolyLine2d()
		{
			vertices = new List<Vector2d>();
			Timestamp = 0;
		}

		public PolyLine2d(in PolyLine2d copy)
		{
			vertices = new List<Vector2d>(copy.vertices);
			Timestamp = 0;
		}

		public PolyLine2d(in Polygon2d copy, in bool bDuplicateFirstLast)
		{
			vertices = new List<Vector2d>(copy.VerticesItr(bDuplicateFirstLast));
			Timestamp = 0;
		}

		public PolyLine2d(in IList<Vector2d> copy)
		{
			vertices = new List<Vector2d>(copy);
			Timestamp = 0;
		}

		public PolyLine2d(in IEnumerable<Vector2d> copy)
		{
			vertices = new List<Vector2d>(copy);
			Timestamp = 0;
		}

		public PolyLine2d(in Vector2d[] v)
		{
			vertices = new List<Vector2d>(v);
			Timestamp = 0;
		}
		public PolyLine2d(in VectorArray2d v)
		{
			vertices = new List<Vector2d>(v.AsVector2d());
			Timestamp = 0;
		}


		public Vector2d this[in int key]
		{
			get => vertices[key];
			set { vertices[key] = value; Timestamp++; }
		}

		public Vector2d Start => vertices[0];
		public Vector2d End => vertices[vertices.Count - 1];


		public ReadOnlyCollection<Vector2d> Vertices => vertices.AsReadOnly();

		public int VertexCount => vertices.Count;

		public  void AppendVertex(in Vector2d v)
		{
			vertices.Add(v);
			Timestamp++;
		}

		public  void AppendVertices(in IEnumerable<Vector2d> v)
		{
			vertices.AddRange(v);
			Timestamp++;
		}


		public  void Reverse()
		{
			vertices.Reverse();
			Timestamp++;
		}


		public Vector2d GetTangent(in int i)
		{
			return i == 0
				? (vertices[1] - vertices[0]).Normalized
				: i == vertices.Count - 1
				? (vertices[vertices.Count - 1] - vertices[vertices.Count - 2]).Normalized
				: (vertices[i + 1] - vertices[i - 1]).Normalized;
		}

		public Vector2d GetNormal(in int i)
		{
			return GetTangent(i).Perp;
		}


		public AxisAlignedBox2d GetBounds()
		{
			if (vertices.Count == 0) {
				return AxisAlignedBox2d.Empty;
			}

			var box = new AxisAlignedBox2d(vertices[0]);
			for (var i = 1; i < vertices.Count; ++i) {
				box.Contain(vertices[i]);
			}

			return box;
		}
		public AxisAlignedBox2d Bounds => GetBounds();


		public double DistanceSquared(in Vector2d point)
		{
			var fNearestSqr = double.MaxValue;
			for (var i = 0; i < vertices.Count - 1; ++i)
			{
				var seg = new Segment2d(vertices[i], vertices[i + 1]);
				var d = seg.DistanceSquared(point);
				if (d < fNearestSqr) {
					fNearestSqr = d;
				}
			}
			return fNearestSqr;
		}


		public Segment2d Segment(in int iSegment)
		{
			return new Segment2d(vertices[iSegment], vertices[iSegment + 1]);
		}

		public IEnumerable<Segment2d> SegmentItr()
		{
			for (var i = 0; i < vertices.Count - 1; ++i) {
				yield return new Segment2d(vertices[i], vertices[i + 1]);
			}
		}

		public IEnumerator<Vector2d> GetEnumerator()
		{
			return vertices.GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator()
		{
			return vertices.GetEnumerator();
		}


		[System.Obsolete("This method name is confusing. Will remove in future. Use ArcLength instead")]
		public double Length => ArcLength;
		public double ArcLength
		{
			get
			{
				double fLength = 0;
				var N = vertices.Count;
				for (var i = 0; i < N - 1; ++i) {
					fLength += vertices[i].Distance(vertices[i + 1]);
				}

				return fLength;
			}
		}


		/// <summary>
		/// Offset each point by dist along vertex normal direction (ie tangent-perp)
		/// </summary>
		public void VertexOffset(in double dist)
		{
			var newv = new Vector2d[vertices.Count];
			for (var k = 0; k < vertices.Count; ++k) {
				newv[k] = vertices[k] + (dist * GetNormal(k));
			}

			for (var k = 0; k < vertices.Count; ++k)
            {
                vertices[k] = newv[k];
            }
		}


		/// <summary>
		/// make polyline shorter by dist length, by removing from front
		/// </summary>
		public bool TrimStart(in double dist)
		{
			var NV = vertices.Count;
			var vi = 0;
			var next_len = vertices[vi].Distance(vertices[vi + 1]);
			double accum_len = 0;
			while (vi < NV - 2 && (accum_len + next_len) < dist)
			{
				accum_len += next_len;
				vi++;
				next_len = vertices[vi].Distance(vertices[vi + 1]);
			}
			if (vi == NV - 2 && (accum_len + next_len) <= dist) {
				return false;
			}

			var t = (dist - accum_len) / next_len;
			var pt = Segment(vi).PointBetween(t);
			if (vi > 0) {
				vertices.RemoveRange(0, vi);
			}

			vertices[0] = pt;
			return true;
		}

		/// <summary>
		/// make polyline shorter by dist length, by removing from end
		/// </summary>
		public bool TrimEnd(in double dist)
		{
			var NV = vertices.Count;
			var vi = NV - 1;
			var next_len = vertices[vi].Distance(vertices[vi - 1]);
			double accum_len = 0;
			while (vi > 1 && (accum_len + next_len) < dist)
			{
				accum_len += next_len;
				vi--;
				next_len = vertices[vi].Distance(vertices[vi - 1]);
			}
			if (vi == 1 && (accum_len + next_len) <= dist) {
				return false;
			}

			var t = (dist - accum_len) / next_len;
			var pt = Segment(vi - 1).PointBetween(1 - t);
			if (vi < NV - 1) {
				vertices.RemoveRange(vi, NV - 1 - vi);
			}

			vertices[vi] = pt;
			return true;
		}

		/// <summary>
		/// make polyline shorter by removing each_end_dist from start and end
		/// </summary>
		public bool Trim(in double each_end_dist)
		{
			return ArcLength >= 2 * each_end_dist
&& TrimEnd(each_end_dist) != false
&& TrimStart(each_end_dist);
		}



		// Polyline simplification (converted from Polyogon2d.simplifyDP)
		// code adapted from: http://softsurfer.com/Archive/algorithm_0205/algorithm_0205.htm
		// simplifyDP():
		//  This is the Douglas-Peucker recursive simplification routine
		//  It just marks vertices that are part of the simplified polyline
		//  for approximating the polyline subchain v[j] to v[k].
		//    Input:  tol = approximation tolerance
		//            v[] = polyline array of vertex points
		//            j,k = indices for the subchain v[j] to v[k]
		//    Output: mk[] = array of markers matching vertex array v[]
		protected static void SimplifyDP(in double tol, in Vector2d[] v, in int j, in int k, in bool[] mk)
		{
			if (k <= j + 1) // there is nothing to simplify
{
				return;
			}

			// check for adequate approximation by segment S from v[j] to v[k]
			var maxi = j;          // index of vertex farthest from S
			double maxd2 = 0;         // distance squared of farthest vertex
			var tol2 = tol * tol;  // tolerance squared
			var S = new Segment2d(v[j], v[k]);    // segment from v[j] to v[k]

			// test each vertex v[i] for max distance from S
			// Note: this works in any dimension (2D, 3D, ...)
			for (var i = j + 1; i < k; i++)
			{
				var dv2 = S.DistanceSquared(v[i]);
				if (dv2 <= maxd2) {
					continue;
				}
				// v[i] is a new max vertex
				maxi = i;
				maxd2 = dv2;
			}
			if (maxd2 > tol2)
			{       // error is worse than the tolerance
					// split the polyline at the farthest vertex from S
				mk[maxi] = true;      // mark v[maxi] for the simplified polyline
									  // recursively simplify the two subpolylines at v[maxi]
				SimplifyDP(tol, v, j, maxi, mk);  // polyline v[j] to v[maxi]
				SimplifyDP(tol, v, maxi, k, mk);  // polyline v[maxi] to v[k]
			}
			// else the approximation is OK, so ignore intermediate vertices
			return;
		}



		public void Simplify(in double clusterTol = 0.0001,
							  in double lineDeviationTol = 0.01)
		{
			var n = vertices.Count;

			int i, k, pv;            // misc counters
			var vt = new Vector2d[n];  // vertex buffer
			var mk = new bool[n];
			for (i = 0; i < n; ++i)     // marker buffer
{
				mk[i] = false;
			}

			// STAGE 1.  Vertex Reduction within tolerance of prior vertex cluster
			var clusterTol2 = clusterTol * clusterTol;
			vt[0] = vertices[0];              // start at the beginning
			for (i = k = 1, pv = 0; i < n; i++)
			{
				if ((vertices[i] - vertices[pv]).LengthSquared < clusterTol2) {
					continue;
				}

				vt[k++] = vertices[i];
				pv = i;
			}
			if (pv < n - 1) {
				vt[k++] = vertices[n - 1];      // finish at the end
			}

			// STAGE 2.  Douglas-Peucker polyline simplification
			if (lineDeviationTol > 0)
			{
				mk[0] = mk[k - 1] = true;       // mark the first and last vertices
				SimplifyDP(lineDeviationTol, vt, 0, k - 1, mk);
			}
			else
			{
				for (i = 0; i < k; ++i) {
					mk[i] = true;
				}
			}

			// copy marked vertices back to this polygon
			vertices = new List<Vector2d>();
			for (i = 0; i < k; ++i)
			{
				if (mk[i]) {
					vertices.Add(vt[i]);
				}
			}
			Timestamp++;

			return;
		}


		public PolyLine2d Transform(in ITransform2 xform)
		{
			var N = vertices.Count;
			for (var k = 0; k < N; ++k)
            {
                vertices[k] = xform.TransformP(vertices[k]);
            }
			return this;
		}



		static public PolyLine2d MakeBoxSpiral(in Vector2d center, in double len, in double spacing)
		{
			var pline = new PolyLine2d();
			pline.AppendVertex(center);

			var c = center;
			c.x += spacing / 2;
			pline.AppendVertex(c);
			c.y += spacing;
			pline.AppendVertex(c);
			var accum = (spacing / 2) + spacing;

			var w = spacing / 2;
			var h = spacing;

			var sign = -1.0;
			while (accum < len)
			{
				w += spacing;
				c.x += sign * w;
				pline.AppendVertex(c);
				accum += w;

				h += spacing;
				c.y += sign * h;
				pline.AppendVertex(c);
				accum += h;

				sign *= -1.0;
			}

			return pline;
		}



	}




	/// <summary>
	/// Wrapper for a PolyLine2d that provides minimal IParametricCurve2D interface
	/// </summary>
	public sealed class PolyLine2DCurve : IParametricCurve2d
	{
		public PolyLine2d Polyline;

		public bool IsClosed => false;

		// can call SampleT in range [0,ParamLength]
		public double ParamLength => Polyline.VertexCount;
		public Vector2d SampleT(in double t)
		{
			var i = (int)t;
			if (i >= Polyline.VertexCount - 1) {
				return Polyline[Polyline.VertexCount - 1];
			}

			var a = Polyline[i];
			var b = Polyline[i + 1];
			var alpha = t - (double)i;
			return ((1.0 - alpha) * a) + (alpha * b);
		}
		public Vector2d TangentT(in double t)
		{
			throw new NotImplementedException("Polygon2dCurve.TangentT");
		}

		public bool HasArcLength => true;
		public double ArcLength => Polyline.ArcLength;

		public Vector2d SampleArcLength(in double a)
		{
			throw new NotImplementedException("Polygon2dCurve.SampleArcLength");
		}

		public void Reverse()
		{
			Polyline.Reverse();
		}

		public IParametricCurve2d Clone()
		{
			return new PolyLine2DCurve() { Polyline = new PolyLine2d(Polyline) };
		}

		public bool IsTransformable => true;
		public void Transform(in ITransform2 xform)
		{
			Polyline.Transform(xform);
		}
	}

}
