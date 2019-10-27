using System;
using System.Collections.Generic;
using System.Text;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// ASN.1 Object identifier
	/// </summary>
	public class ObjectId
	{
		private readonly int[] segments;

		/// <summary>
		/// ASN.1 Object identifier
		/// </summary>
		/// <param name="Segments">Segments</param>
		public ObjectId(params int[] Segments)
		{
			this.segments = Segments;
		}

		/// <summary>
		/// ASN.1 Object identifier
		/// </summary>
		/// <param name="Id">First part of object identifier.</param>
		/// <param name="Segments">Segments</param>
		public ObjectId(ObjectId Id, params int[] Segments)
		{
			int c = Id.segments.Length;
			int d = Segments.Length;

			this.segments = new int[c + d];

			Array.Copy(Id.segments, 0, this.segments, 0, c);
			Array.Copy(Segments, 0, this.segments, c, d);
		}

		/// <summary>
		/// Segments of Object Identifier
		/// </summary>
		public int[] Segments => this.segments;

		/// <summary>
		/// Implicit converter to an integer array
		/// </summary>
		/// <param name="Id">Identity</param>
		public static implicit operator int[](ObjectId Id) => Id.segments;

		/// <summary>
		/// Implicit converter from an integer array
		/// </summary>
		/// <param name="Segments">Segments</param>
		public static implicit operator ObjectId(int[] Segments) => new ObjectId(Segments);

		/// <summary>
		/// Implicit converter from an integer array
		/// </summary>
		/// <param name="Segments">Segments</param>
		public static implicit operator ObjectId(long[] Segments)
		{
			int i, c = Segments.Length;
			int[] Segments2 = new int[c];
			long l;

			for (i = 0; i < c; i++)
			{
				l = Segments[i];
				if (i >= int.MinValue && i <= int.MaxValue)
					Segments2[i] = (int)l;
				else
				{
					throw new ArgumentOutOfRangeException(nameof(Segments),
						  "Segments must be within the range of 32-bit integers.");
				}
			}

			return new ObjectId(Segments2);
		}
	}
}
