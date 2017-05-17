using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

namespace Waher.Persistence.Files.Test.Classes
{
	[TypeName(TypeNameSerialization.FullName)]
	public abstract class FullNameBase
	{
		[ObjectId]
		public Guid ObjectId;
		public string Name;
	}
}
