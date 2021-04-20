using System;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
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
