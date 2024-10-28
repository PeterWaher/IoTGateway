using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !LW
namespace Waher.Persistence.Files.Test.IndexInlineTests
#else
namespace Waher.Persistence.FilesLW.Test.IndexInlineTests
#endif
{
	[TestClass]
	public class DBFilesIndexTests_Inline_65536 : DBFilesIndexTests
	{
		public override int BlockSize
		{
			get
			{
				return 65536;
			}
		}
	}
}
