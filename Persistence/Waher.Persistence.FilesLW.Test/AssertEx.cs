using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#if !LW
namespace Waher.Persistence.Files.Test
#else
using Waher.Persistence.Files;
namespace Waher.Persistence.FilesLW.Test
#endif
{
	public static class AssertEx
	{
		public static void Less(object Left, object Right)
		{
			if (Left is IComparable l)
				Assert.IsTrue(l.CompareTo(Right) < 0);
			else if (Right is IComparable r)
				Assert.IsTrue(r.CompareTo(Left) > 0);
			else
				Assert.Fail("Values not comparable.");
		}

		public static void Greater(object Left, object Right)
		{
			if (Left is IComparable l)
				Assert.IsTrue(l.CompareTo(Right) > 0);
			else if (Right is IComparable r)
				Assert.IsTrue(r.CompareTo(Left) < 0);
			else
				Assert.Fail("Values not comparable.");
		}

		public static void LessOrEqual(object Left, object Right)
		{
			if (Left is IComparable l)
				Assert.IsTrue(l.CompareTo(Right) <= 0);
			else if (Right is IComparable r)
				Assert.IsTrue(r.CompareTo(Left) >= 0);
			else
				Assert.Fail("Values not comparable.");
		}

		public static void GreaterOrEqual(object Left, object Right)
		{
			if (Left is IComparable l)
				Assert.IsTrue(l.CompareTo(Right) >= 0);
			else if (Right is IComparable r)
				Assert.IsTrue(r.CompareTo(Left) <= 0);
			else
				Assert.Fail("Values not comparable.");
		}
	}
}
