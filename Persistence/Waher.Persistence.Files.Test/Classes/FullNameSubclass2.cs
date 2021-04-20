using System;

#if !LW
namespace Waher.Persistence.Files.Test.Classes
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test.Classes
#endif
{
	public class FullNameSubclass2 : FullNameBase
	{
		public string Value;
	}
}
