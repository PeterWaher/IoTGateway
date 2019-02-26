using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.Cluster;

namespace Waher.Networking.Cluster.Test.TestObjects
{
	public class NullableTypes
	{
		public NullableTypes()
		{
		}

		public sbyte? Int8
		{
			get;
			set;
		}

		public short? Int16
		{
			get;
			set;
		}

		public int? Int32
		{
			get;
			set;
		}

		public long? Int64
		{
			get;
			set;
		}

		public byte? UInt8
		{
			get;
			set;
		}

		public ushort? UInt16
		{
			get;
			set;
		}

		public uint? UInt32
		{
			get;
			set;
		}

		public ulong? UInt64
		{
			get;
			set;
		}

		public float? S
		{
			get;
			set;
		}

		public double? D
		{
			get;
			set;
		}

		public decimal? Dec
		{
			get;
			set;
		}

		public bool? B
		{
			get;
			set;
		}

		public char? Ch
		{
			get;
			set;
		}

		public DateTime? DT
		{
			get;
			set;
		}

		public DateTimeOffset? DTO
		{
			get;
			set;
		}

		public TimeSpan? TS
		{
			get;
			set;
		}

		public Guid? Id
		{
			get;
			set;
		}

		public NormalEnum? E1
		{
			get;
			set;
		}

		public FlagsEnum? E2
		{
			get;
			set;
		}

	}
}
