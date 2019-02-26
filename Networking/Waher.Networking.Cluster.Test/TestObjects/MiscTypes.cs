using System;
using System.Collections.Generic;
using System.Text;
using Waher.Networking.Cluster;

namespace Waher.Networking.Cluster.Test.TestObjects
{
	public enum NormalEnum
	{
		A,
		B,
		C,
		D
	}

	[Flags]
	public enum FlagsEnum
	{
		A = 1,
		B = 2,
		C = 4,
		D = 8
	}

	public class MiscTypes : IClusterMessage
	{
		public MiscTypes()
		{
		}

		public bool B
		{
			get;
			set;
		}

		public char Ch
		{
			get;
			set;
		}

		public DateTime DT
		{
			get;
			set;
		}

		public DateTimeOffset DTO
		{
			get;
			set;
		}

		public TimeSpan TS
		{
			get;
			set;
		}

		public Guid Id
		{
			get;
			set;
		}

		public NormalEnum E1
		{
			get;
			set;
		}

		public FlagsEnum E2
		{
			get;
			set;
		}

	}
}
