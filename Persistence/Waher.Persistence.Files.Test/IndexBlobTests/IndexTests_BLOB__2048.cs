using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Content;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Statistics;
using Waher.Script;

#if !LW
using Waher.Persistence.Files.Test.Classes;
using Waher.Persistence.Files.Test.IndexInlineTests;

namespace Waher.Persistence.Files.Test.IndexBlobTests
#else
using Waher.Persistence.Files;
using Waher.Persistence.FilesLW.Test.Classes;
using Waher.Persistence.FilesLW.Test.IndexInlineTests;

namespace Waher.Persistence.FilesLW.Test.IndexBlobTests
#endif
{
	[TestClass]
	public class IndexTests_BLOB__2048 : IndexTests_Inline__2048
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
