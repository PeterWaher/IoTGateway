using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

namespace Waher.Persistence.Files.Test.Classes
{
	[TypeName(TypeNameSerialization.FullName)]
	public class ByReference
	{
		[ObjectId]
		public Guid ObjectId;

		[ByReference]
		public Default Default;

		[ByReference]
		public Simple Simple;

		public ByReference()
		{
		}

		public override string ToString()
		{
			return this.ObjectId.ToString();
		}
	}
}
