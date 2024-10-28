using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !LW
namespace Waher.Persistence.Files.Test.BTreeInlineTests
#else
namespace Waher.Persistence.FilesLW.Test.BTreeInlineTests
#endif
{
	[TestClass]
	public class DBFilesBTreeTests_Inline__8192 : DBFilesBTreeTests
	{
		public override int BlockSize
		{
			get
			{
				return 8192;
			}
		}
	}
}
