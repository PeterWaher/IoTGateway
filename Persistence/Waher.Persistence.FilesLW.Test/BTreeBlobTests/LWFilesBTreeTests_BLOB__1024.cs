using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !LW
using Waher.Persistence.Files.Test.BTreeInlineTests;

namespace Waher.Persistence.Files.Test.BTreeBlobTests
#else
using Waher.Persistence.FilesLW.Test.BTreeInlineTests;

namespace Waher.Persistence.FilesLW.Test.BTreeBlobTests
#endif
{
	[TestClass]
	public class DBFilesBTreeTests_BLOB__1024 : DBFilesBTreeTests_Inline__1024 
	{
		public override int MaxStringLength
		{
			get
			{
				return this.file.InlineObjectSizeLimit * 10;
			}
		}
	}
}
