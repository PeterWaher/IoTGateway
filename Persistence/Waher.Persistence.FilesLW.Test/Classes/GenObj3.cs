using System;
using System.Collections.Generic;
using Waher.Persistence.Attributes;
using Waher.Script.Abstraction.Elements;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	[TypeName(TypeNameSerialization.FullName)]
	public class GenObj3
	{
		[ObjectId]
		public Guid ObjectId;

		public Dictionary<string, IElement> EmbeddedObj;

		public GenObj3()
		{
		}
	}
}
