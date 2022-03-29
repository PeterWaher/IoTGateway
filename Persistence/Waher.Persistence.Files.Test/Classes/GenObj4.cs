using System;
using System.Collections.Generic;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	[TypeName(TypeNameSerialization.FullName)]
	public class GenObj4
	{
		[ObjectId]
		public Guid ObjectId;

		public KeyValuePair<string, object>[] EmbeddedObj;

		public GenObj4()
		{
		}
	}
}
