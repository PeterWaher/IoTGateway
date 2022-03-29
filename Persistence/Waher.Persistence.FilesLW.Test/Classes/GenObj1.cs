using System;
using Waher.Persistence.Attributes;
using Waher.Persistence.Serialization;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	[TypeName(TypeNameSerialization.FullName)]
	public class GenObj1
	{
		[ObjectId]
		public Guid ObjectId;

		public GenericObject EmbeddedObj;

		public GenObj1()
		{
		}
	}
}
