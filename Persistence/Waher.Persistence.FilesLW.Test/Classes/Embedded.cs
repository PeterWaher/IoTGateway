using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

#if NETSTANDARD1_5
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	public class Embedded
	{
		public byte Byte;
		public short Short;
		public int Int;

		public Embedded()
		{
		}
	}
}
