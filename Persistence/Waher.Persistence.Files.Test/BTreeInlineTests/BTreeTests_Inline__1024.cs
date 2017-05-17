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

#if NETSTANDARD1_5
using Waher.Persistence.Files.Test.Classes;

namespace Waher.Persistence.Files.Test.BTreeInlineTests
#else
using Waher.Persistence.Files;
using Waher.Persistence.FilesLW.Test.Classes;

namespace Waher.Persistence.FilesLW.Test.BTreeInlineTests
#endif
{
	[TestClass]
	public class BTreeTests_Inline__1024 : BTreeTests
	{
		public override int BlockSize
		{
			get
			{
				return 1024;
			}
		}
	}
}
