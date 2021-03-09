using System;
using Waher.Persistence.Attributes;

namespace Waher.Runtime.Settings.Test
{
	[TypeName(TypeNameSerialization.FullName)]
	public class TestObject
	{
		public string S;
		public double D;
		public int I;
	}
}
