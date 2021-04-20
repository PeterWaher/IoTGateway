using System;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	public class ObjectIdByteArray
	{
		[ObjectId]
		public byte[] ObjectId;
		public int Value;

		public ObjectIdByteArray()
		{
		}
	}
}
