using System;
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
		private static void MakeCompatible(ref object Left, ref object Right)
		{
			if (Left is not null && Right is not null && Left.GetType() != Right.GetType())
			{
				if (Left is long)
					Right = Convert.ToInt64(Right);
				else if (Right is long)
					Left = Convert.ToInt64(Left);
				else if (Left is int)
					Right = Convert.ToInt32(Right);
				else if (Right is int)
					Left = Convert.ToInt32(Left);
			}
		}

		public static void Same(object Expected, object Value)
		{
			if (Expected is null && Value is null)
				return;

			Assert.IsNotNull(Expected);
			Assert.IsNotNull(Value);

			MakeCompatible(ref Expected, ref Value);
			if (Expected is IComparable l)
				Assert.IsTrue(l.CompareTo(Value) == 0, "Expected " + Expected + ", but was " + Value);
			else if (Value is IComparable r)
				Assert.IsTrue(r.CompareTo(Expected) == 0, "Expected " + Expected + ", but was " + Value);
			else if (Expected is Array al && Value is Array ar)
			{
				int i, c;
				Assert.AreEqual(c = al.Length, ar.Length, "Array lengths differ.");

				for (i = 0; i < c; i++)
					Same(al.GetValue(i), ar.GetValue(i));
			}
			else
			{
				Assert.AreEqual(Expected, Value, "Values differ: " +
					Expected.ToString() + "(" + Expected.GetType().FullName + "), " +
					Value.ToString() + "(" + Value.GetType().FullName + ")");
			}
		}

		public static void NotSame(object Left, object Right)
		{
			MakeCompatible(ref Left, ref Right);
			if (Left is IComparable l)
				Assert.IsTrue(l.CompareTo(Right) != 0);
			else if (Right is IComparable r)
				Assert.IsTrue(r.CompareTo(Left) != 0);
			else
				Assert.AreNotEqual(Left, Right);
		}

		public static void Less(object Left, object Right)
		{
			MakeCompatible(ref Left, ref Right);

			if (Left is IComparable l)
				Assert.IsTrue(l.CompareTo(Right) < 0);
			else if (Right is IComparable r)
				Assert.IsTrue(r.CompareTo(Left) > 0);
			else
				Assert.Fail("Values not comparable.");
		}

		public static void Greater(object Left, object Right)
		{
			MakeCompatible(ref Left, ref Right);

			if (Left is IComparable l)
				Assert.IsTrue(l.CompareTo(Right) > 0);
			else if (Right is IComparable r)
				Assert.IsTrue(r.CompareTo(Left) < 0);
			else
				Assert.Fail("Values not comparable.");
		}

		public static void LessOrEqual(object Left, object Right)
		{
			MakeCompatible(ref Left, ref Right);

			if (Left is IComparable l)
				Assert.IsTrue(l.CompareTo(Right) <= 0);
			else if (Right is IComparable r)
				Assert.IsTrue(r.CompareTo(Left) >= 0);
			else
				Assert.Fail("Values not comparable.");
		}

		public static void GreaterOrEqual(object Left, object Right)
		{
			MakeCompatible(ref Left, ref Right);

			if (Left is IComparable l)
				Assert.IsTrue(l.CompareTo(Right) >= 0);
			else if (Right is IComparable r)
				Assert.IsTrue(r.CompareTo(Left) <= 0);
			else
				Assert.Fail("Values not comparable.");
		}
	}
}
