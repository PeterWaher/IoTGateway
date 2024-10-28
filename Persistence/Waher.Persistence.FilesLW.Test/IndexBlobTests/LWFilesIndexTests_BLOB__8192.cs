using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !LW
using Waher.Persistence.Files.Test.IndexInlineTests;

namespace Waher.Persistence.Files.Test.IndexBlobTests
#else
using Waher.Persistence.FilesLW.Test.IndexInlineTests;

namespace Waher.Persistence.FilesLW.Test.IndexBlobTests
#endif
{
	[TestClass]
	public class DBFilesIndexTests_BLOB__8192 : DBFilesIndexTests_Inline__8192
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
