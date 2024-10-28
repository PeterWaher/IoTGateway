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
	public class DBFilesBTreeTests_BLOB_65536 : DBFilesBTreeTests_Inline_65536
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
