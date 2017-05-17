using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

namespace Waher.Persistence.Files.Test.Classes
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
