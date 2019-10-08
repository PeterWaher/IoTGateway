using System;
using System.Collections;
using System.Collections.Generic;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// Generic array.
	/// </summary>
	/// <typeparam name="T">Element type.</typeparam>
	public class Array<T> : IEnumerable<T>
	{
		private readonly T[] elements;

		/// <summary>
		/// Implicit type converter to a typed array.
		/// </summary>
		/// <param name="A">Object instance</param>
		public static implicit operator T[](Array<T> A) => A.elements;

		/// <summary>
		/// Implicit type converter to an untyped array.
		/// </summary>
		/// <param name="A">Object instance</param>
		public static implicit operator Array(Array<T> A) => A.elements;

		/// <summary>
		/// Implicit type converter to a typed array.
		/// </summary>
		/// <param name="A">Array</param>
		public static implicit operator Array<T>(T[] A) => new Array<T>(A);

		/// <summary>
		/// Generic array.
		/// </summary>
		/// <param name="Length">Length of array.</param>
		public Array(int Length)
		{
			if (Length < 0)
				throw new ArgumentException("Length must be non-negative.", nameof(Length));
		}

		/// <summary>
		/// Generic array.
		/// </summary>
		/// <param name="Elements">Elements</param>
		public Array(T[] Elements)
		{
			this.elements = Elements;
		}

		/// <summary>
		/// Gets the rank (number of dimensions) of the System.Array. For example, a one-dimensional
		/// array returns 1, a two-dimensional array returns 2, and so on.
		/// </summary>
		/// <returns>
		/// The rank (number of dimensions) of the System.Array.
		/// </returns>
		public int Rank => 1;

		/// <summary>
		/// Gets a 32-bit integer that represents the total number of elements in all the
		/// dimensions of the System.Array.
		/// </summary>
		/// <returns>
		/// A 32-bit integer that represents the total number of elements in all the dimensions
		/// of the System.Array; zero if there are no elements in the array.
		/// </returns>
		/// <exception cref="System.OverflowException">
		/// The array is multidimensional and contains more than System.Int32.MaxValue elements.
		/// </exception>
		public int Length => this.elements.Length;

		/// <summary>
		/// Internal array of elements.
		/// </summary>
		public T[] Elements => this.elements;

		/// <summary>
		/// Gets or sets elements of the array.
		/// </summary>
		/// <param name="Index">Zero-based index into the array.</param>
		/// <returns>Element of array.</returns>
		public T this[int Index]
		{
			get => this.elements[Index];
			set => this.elements[Index] = value;
		}

		/// <summary>
		/// Creates a shallow copy of the System.Array.
		/// </summary>
		/// <returns>
		/// A shallow copy of the System.Array.
		/// </returns>
		public object Clone()
		{
			return new Array<T>((T[])this.elements.Clone());
		}

		/// <summary>
		/// Copies all the elements of the current one-dimensional array to the specified
		/// one-dimensional array starting at the specified destination array index. The
		/// index is specified as a 32-bit integer.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional array that is the destination of the elements copied from
		/// the current array.
		/// </param>
		/// <param name="index">
		/// A 32-bit integer that represents the index in array at which copying begins.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// array is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// index is less than the lower bound of array.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// array is multidimensional.-or-The number of elements in the source array is greater
		/// than the available number of elements from index to the end of the destination
		/// array.
		/// </exception>
		/// <exception cref="System.ArrayTypeMismatchException">
		/// The type of the source System.Array cannot be cast automatically to the type
		/// of the destination array.
		/// </exception>
		/// <exception cref="System.RankException">
		/// The source array is multidimensional.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		/// At least one element in the source System.Array cannot be cast to the type of
		/// destination array.
		/// </exception>
		public void CopyTo(Array<T> array, int index)
		{
			this.elements.CopyTo(array.elements, index);
		}

		/// <summary>
		/// Returns an System.Collections.IEnumerator for the System.Array.
		/// </summary>
		/// <returns>
		/// An System.Collections.IEnumerator for the System.Array.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.elements.GetEnumerator();
		}

		/// <summary>
		/// Returns an System.Collections.IEnumerator for the System.Array.
		/// </summary>
		/// <returns>
		/// An System.Collections.IEnumerator for the System.Array.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			return new ElementEnumerator(this.elements.GetEnumerator());
		}

		private class ElementEnumerator : IEnumerator<T>
		{
			private readonly IEnumerator e;

			public ElementEnumerator(IEnumerator Enumerator)
			{
				this.e = Enumerator;
			}

			public T Current => (T)this.e.Current;

			object IEnumerator.Current => this.e.Current;

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				return this.e.MoveNext();
			}

			public void Reset()
			{
				this.e.Reset();
			}
		}

		/// <summary>
		/// Gets a 32-bit integer that represents the number of elements in the specified
		/// dimension of the System.Array.
		/// </summary>
		/// <param name="dimension">
		/// A zero-based dimension of the System.Array whose length needs to be determined.
		/// </param>
		/// <returns>
		/// A 32-bit integer that represents the number of elements in the specified dimension.
		/// </returns>
		/// <exception cref="System.IndexOutOfRangeException">
		/// dimension is less than zero.-or-dimension is equal to or greater than System.Array.Rank.
		/// </exception>
		public int GetLength(int dimension)
		{
			return this.elements.GetLength(dimension);
		}

		/// <summary>
		/// Gets the index of the first element of the specified dimension in the array.
		/// </summary>
		/// <param name="dimension">
		/// A zero-based dimension of the array whose starting index needs to be determined.
		/// </param>
		/// <returns>
		/// The index of the first element of the specified dimension in the array.
		/// </returns>
		/// <exception cref="System.IndexOutOfRangeException">
		/// dimension is less than zero.-or-dimension is equal to or greater than System.Array.Rank.
		/// </exception>
		public int GetLowerBound(int dimension)
		{
			return this.elements.GetLowerBound(dimension);
		}

		/// <summary>
		/// Gets the index of the last element of the specified dimension in the array.
		/// </summary>
		/// <param name="dimension">
		/// A zero-based dimension of the array whose upper bound needs to be determined.
		/// </param>
		/// <returns>
		/// The index of the last element of the specified dimension in the array, or -1
		/// if the specified dimension is empty.
		/// </returns>
		/// <exception cref="System.IndexOutOfRangeException">
		/// dimension is less than zero.-or-dimension is equal to or greater than System.Array.Rank.
		/// </exception>
		public int GetUpperBound(int dimension)
		{
			return this.elements.GetUpperBound(dimension);
		}

		/// <summary>
		/// Gets the value at the specified position in the one-dimensional System.Array.
		/// The index is specified as a 32-bit integer.
		/// </summary>
		/// <param name="index">
		/// A 32-bit integer that represents the position of the System.Array element to
		/// get.
		/// </param>
		/// <returns>
		/// The value at the specified position in the one-dimensional System.Array.
		/// </returns>
		/// <exception cref="System.ArgumentException">
		/// The current System.Array does not have exactly one dimension.
		/// </exception>
		/// <exception cref="System.IndexOutOfRangeException">
		/// index is outside the range of valid indexes for the current System.Array.
		/// </exception>
		public object GetValue(int index)
		{
			return this.elements.GetValue(index);
		}

		/// <summary>
		/// Gets the value at the specified position in the multidimensional System.Array.
		/// The indexes are specified as an array of 32-bit integers.
		/// </summary>
		/// <param name="indices">
		/// A one-dimensional array of 32-bit integers that represent the indexes specifying
		/// the position of the System.Array element to get.
		/// </param>
		/// <returns>
		/// The value at the specified position in the multidimensional System.Array.
		/// </returns>
		/// <exception cref="System.ArgumentNullException">
		/// indices is null.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The number of dimensions in the current System.Array is not equal to the number
		/// of elements in indices.
		/// </exception>
		/// <exception cref="System.IndexOutOfRangeException">
		/// Any element in indices is outside the range of valid indexes for the corresponding
		/// dimension of the current System.Array.
		/// </exception>
		public object GetValue(params int[] indices)
		{
			return this.elements.GetValue(indices);
		}

		/// <summary>
		/// Initializes every element of the value-type System.Array by calling the default
		/// constructor of the value type.
		/// </summary>
		public void Initialize()
		{
			this.elements.Initialize();
		}

		/// <summary>
		/// Sets a value to the element at the specified position in the multidimensional
		/// System.Array. The indexes are specified as an array of 32-bit integers.
		/// </summary>
		/// <param name="value">
		/// The new value for the specified element.
		/// </param>
		/// <param name="indices">
		/// A one-dimensional array of 32-bit integers that represent the indexes specifying
		/// the position of the element to set.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		/// indices is null.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		/// The number of dimensions in the current System.Array is not equal to the number
		/// of elements in indices.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		/// value cannot be cast to the element type of the current System.Array.
		/// </exception>
		/// <exception cref="System.IndexOutOfRangeException">
		/// Any element in indices is outside the range of valid indexes for the corresponding
		/// dimension of the current System.Array.
		/// </exception>
		public void SetValue(object value, params int[] indices)
		{
			this.elements.SetValue(value, indices);
		}

		/// <summary>
		/// Sets a value to the element at the specified position in the one-dimensional
		/// System.Array. The index is specified as a 32-bit integer.
		/// </summary>
		/// <param name="value">
		/// The new value for the specified element.
		/// </param>
		/// <param name="index">
		/// A 32-bit integer that represents the position of the System.Array element to
		/// set.
		/// </param>
		/// <exception cref="System.ArgumentException">
		/// The current System.Array does not have exactly one dimension.
		/// </exception>
		/// <exception cref="System.InvalidCastException">
		/// value cannot be cast to the element type of the current System.Array.
		/// </exception>
		/// <exception cref="System.IndexOutOfRangeException">
		/// index is outside the range of valid indexes for the current System.Array.
		/// </exception>
		public void SetValue(object value, int index)
		{
			this.elements.SetValue(value, index);
		}
	}
}
