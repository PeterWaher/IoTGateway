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
	[ObsoleteMethod("ObsoleteProperties")]
	public class ObsoleteMethod : Simple
	{
		public ObsoleteMethod()
		{
		}

		public void ObsoleteProperties(Dictionary<string, object> _)
		{
		}
	}
}
