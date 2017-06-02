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
using Waher.Runtime.Inventory;

#if !LW
using Waher.Persistence.Files.Test.Classes;
using Waher.Persistence.Files.Test.BTreeInlineTests;

namespace Waher.Persistence.Files.Test.BTreeBlobTests
#else
using Waher.Persistence.Files;
using Waher.Persistence.FilesLW.Test.Classes;
using Waher.Persistence.FilesLW.Test.BTreeInlineTests;

namespace Waher.Persistence.FilesLW.Test.BTreeBlobTests
#endif
{
	[TestClass]
	public class BTreeTests_BLOB_16384 : BTreeTests_Inline_16384
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
