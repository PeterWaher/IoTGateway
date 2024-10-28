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
	public class DBFilesIndexTests_BLOB_16384 : DBFilesIndexTests_Inline_16384
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
