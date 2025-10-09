using System;
using Waher.Content;
using Waher.Events;
using Waher.Persistence;
using Waher.Persistence.Attributes;

namespace IdApp.Test.Serialization
{
	[TypeName(TypeNameSerialization.FullName)]
	public class NestedClass
	{
		[ObjectId]
		public Guid ObjectId { get; set; }

		public byte UI8 { get; set; }
		public ushort UI16 { get; set; }
		public uint UI32 { get; set; }
		public ulong UI64 { get; set; }
		public sbyte I8 { get; set; }
		public short I16 { get; set; }
		public int I32 { get; set; }
		public long I64 { get; set; }
		public string S { get; set; }
		public CaseInsensitiveString Cis { get; set; }
		public char Ch { get; set; }
		public byte[] Bin { get; set; }
		public Guid Id { get; set; }
		public TimeSpan TS { get; set; }
		public Duration D { get; set; }
		public object Null { get; set; }
		public bool B { get; set; }
		public float Fl { get; set; }
		public double Db { get; set; }
		public decimal Dc { get; set; }
		public EventType E { get; set; }
		public DateTime TP { get; set; }
		public DateTimeOffset TPO { get; set; }
		public string[] A { get; set; }
		public NestedClass Nested { get; set; }
	}
}
