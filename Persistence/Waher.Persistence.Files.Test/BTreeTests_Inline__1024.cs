using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using NUnit.Framework;
using Waher.Content;
using Waher.Persistence.Files.Test.Classes;
using Waher.Persistence.Files.Serialization;
using Waher.Persistence.Files.Statistics;
using Waher.Script;

namespace Waher.Persistence.Files.Test
{
	[TestFixture]
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
