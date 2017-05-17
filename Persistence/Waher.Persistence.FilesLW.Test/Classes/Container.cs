using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

namespace Waher.Persistence.Files.Test.Classes
{
	public class Container
	{
		[ObjectId]
		public Guid ObjectId;
		public Embedded Embedded;
		public Embedded EmbeddedNull;
		public Embedded[] MultipleEmbedded;
		public Embedded[] MultipleEmbeddedNullable;
		public Embedded[] MultipleEmbeddedNull;
	}
}
