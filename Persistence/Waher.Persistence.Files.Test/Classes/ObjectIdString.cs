using System;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	public class ObjectIdString
	{
		[ObjectId]
		public string ObjectId;
		public int Value;

		public ObjectIdString()
		{
		}
	}
}
