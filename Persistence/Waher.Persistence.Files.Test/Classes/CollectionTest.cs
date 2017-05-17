using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Waher.Persistence.Attributes;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	[CollectionName("Test")]
	public class CollectionTest
	{
		public string S1;
		public string S2;
		public string S3;

		public CollectionTest()
		{
		}
	}
}
