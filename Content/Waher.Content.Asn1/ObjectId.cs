using System;
using System.Numerics;
using System.Text;

namespace Waher.Content.Asn1
{
	/// <summary>
	/// ASN.1 Object identifier
	/// </summary>
	public class ObjectId
	{
		private readonly uint[] segments;
		private readonly string oid;

		/// <summary>
		/// ASN.1 Object identifier
		/// </summary>
		/// <param name="Oid">Object Identifier</param>
		public ObjectId(string Oid)
		{
			string[] Segments = Oid.Split('.');
			int i, c = Segments.Length;
			this.segments = new uint[c];
			this.oid = Oid;

			for (i = 0; i < c; i++)
				this.segments[i] = uint.Parse(Segments[i]);
		}

		/// <summary>
		/// ASN.1 Object identifier
		/// </summary>
		/// <param name="Segments">Segments</param>
		public ObjectId(params uint[] Segments)
		{
			this.segments = Segments;
			this.oid = ToString(Segments);
		}

		private static string ToString(uint[] Segments)
		{
			StringBuilder sb = new StringBuilder();
			bool First = true;

			foreach (uint Segment in Segments)
			{
				if (First)
					First = false;
				else
					sb.Append('.');

				sb.Append(Segment.ToString());
			}

			return sb.ToString();
		}

		/// <summary>
		/// ASN.1 Object identifier
		/// </summary>
		/// <param name="Segments">Segments</param>
		public ObjectId(params BigInteger[] Segments)
			: this(ToUIntArray(Segments))
		{
		}

		private static uint[] ToUIntArray(BigInteger[] Segments)
		{
			int i, c = Segments.Length;
			uint[] Result = new uint[c];

			for (i = 0; i < c; i++)
			{
				if (Segments[i] >= 0 && Segments[i] <= uint.MaxValue)
					Result[i] = (uint)Segments[i];
				else
					throw new ArgumentOutOfRangeException(nameof(Segments));
			}

			return Result;
		}

		/// <summary>
		/// ASN.1 Object identifier
		/// </summary>
		/// <param name="Id">First part of object identifier.</param>
		/// <param name="Segments">Segments</param>
		public ObjectId(ObjectId Id, params uint[] Segments)
		{
			int c = Id.segments.Length;
			int d = Segments.Length;

			this.segments = new uint[c + d];

			Buffer.BlockCopy(Id.segments, 0, this.segments, 0, c);
			Buffer.BlockCopy(Segments, 0, this.segments, c, d);
		
			this.oid = ToString(this.segments);
		}

		/// <summary>
		/// ASN.1 Object identifier
		/// </summary>
		/// <param name="Id">First part of object identifier.</param>
		/// <param name="Segments">Segments</param>
		public ObjectId(ObjectId Id, params BigInteger[] Segments)
			: this(Id, ToUIntArray(Segments))
		{
		}

		/// <summary>
		/// String-representation of object identifier.
		/// </summary>
		/// <returns>String value</returns>
		public override string ToString() => this.oid;

		/// <summary>
		/// Segments of Object Identifier
		/// </summary>
		public uint[] Segments => this.segments;

		/// <summary>
		/// String-representation of object identifier.
		/// </summary>
		public string Oid => this.oid;

		/// <summary>
		/// Implicit converter to an integer array
		/// </summary>
		/// <param name="Id">Identity</param>
		public static implicit operator uint[](ObjectId Id) => Id.segments;

		/// <summary>
		/// Implicit converter from an integer array
		/// </summary>
		/// <param name="Segments">Segments</param>
		public static implicit operator ObjectId(uint[] Segments) => new ObjectId(Segments);

		/// <summary>
		/// Implicit converter from an integer array
		/// </summary>
		/// <param name="Segments">Segments</param>
		public static implicit operator ObjectId(BigInteger[] Segments)
		{
			int i, c = Segments.Length;
			uint[] Segments2 = new uint[c];
			BigInteger l;

			for (i = 0; i < c; i++)
			{
				l = Segments[i];
				if (l >= uint.MinValue && l <= uint.MaxValue)
					Segments2[i] = (uint)l;
				else
				{
					throw new ArgumentOutOfRangeException(nameof(Segments),
						"Segments must be within the range of 32-bit integers.");
				}
			}

			return new ObjectId(Segments2);
		}

		/// <summary>
		/// Implicit converter from an integer array
		/// </summary>
		/// <param name="Segments">Segments</param>
		public static implicit operator ObjectId(int[] Segments)
		{
			int i, c = Segments.Length;
			uint[] Segments2 = new uint[c];
			int l;

			for (i = 0; i < c; i++)
			{
				l = Segments[i];
				if (l >= 0)
					Segments2[i] = (uint)l;
				else
				{
					throw new ArgumentOutOfRangeException(nameof(Segments),
						"Segments must be unsigned 32-bit integers.");
				}
			}

			return new ObjectId(Segments2);
		}

		/// <summary>
		/// Implicit converter from an integer array
		/// </summary>
		/// <param name="Segments">Segments</param>
		public static implicit operator ObjectId(long[] Segments)
		{
			int i, c = Segments.Length;
			uint[] Segments2 = new uint[c];
			long l;

			for (i = 0; i < c; i++)
			{
				l = Segments[i];
				if (l >= uint.MinValue && l <= uint.MaxValue)
					Segments2[i] = (uint)l;
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
