using System;
using Waher.Content;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	[TypeName(TypeNameSerialization.FullName)]
	public class Structs
	{
		[ObjectId]
		public Guid ObjectId;
		public Duration Duration;

		public Structs()
		{
		}

		public override string ToString()
		{
			return this.ObjectId.ToString();
		}
	}
}
