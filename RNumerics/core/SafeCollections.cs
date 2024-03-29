﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace RNumerics
{
	/// <summary>
	/// A simple wrapper
	/// </summary>
	public sealed class SafeCall<T>
	{
		public T data;

		public SafeCall(in T val) {
			data = val;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SafeOperation(in Action<T> opF) {
			lock (data) {
				opF(data);
			}
		}
	}

	/// <summary>
	/// A simple wrapper around a List<T> that supports multi-threaded
	/// </summary>
	public sealed class SafeList<T>
	{
		public List<T> List;

		public SafeList() {
			List = new List<T>();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SafeAdd(in T value) {
			lock (List) {
				List.Add(value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SafeOperation(in Action<List<T>> opF) {
			lock (List) {
				opF(List);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SafeRemove(in T value) {
			lock (List) {
				List.Remove(value);
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SafeAddRange(in IEnumerable<T> enumerable) {
			lock (List) {
				List.AddRange(enumerable);
			}
		}
	}


	/// <summary>
	/// A simple wrapper around a List<T> that supports multi-threaded construction.
	/// Basically intended for use within things like a Parallel.ForEach
	/// </summary>
	public sealed class SafeListBuilder<T>
	{
		public List<T> List;
		public SpinLock spinlock;

		public SafeListBuilder() {
			List = new List<T>();
			spinlock = new SpinLock();
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SafeAdd(in T value) {
			var lockTaken = false;
			while (lockTaken == false) {
				spinlock.Enter(ref lockTaken);
			}

			List.Add(value);

			spinlock.Exit();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SafeOperation(in Action<List<T>> opF) {
			var lockTaken = false;
			while (lockTaken == false) {
				spinlock.Enter(ref lockTaken);
			}

			opF(List);

			spinlock.Exit();
		}


		public List<T> Result => List;
	}


}
