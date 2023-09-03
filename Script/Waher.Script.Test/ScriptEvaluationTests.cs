using System;
using System.Numerics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.Sets;
using Waher.Script.Units;
using System.Threading.Tasks;
using Waher.Script.Xml;
using Waher.Content;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptEvaluationTests
	{
		internal const double a = 5;
		internal const double b = 6;
		internal const double c = 7;
		internal const bool p = true;
		internal const bool q = false;
		internal const bool r = true;
		internal const string s = "Hello";
		internal const string t = "Bye";
		internal static readonly Complex z1 = new Complex(1, 2);
		internal static readonly Complex z2 = new Complex(3, 4);

		public static async Task Test(string Script, object ExpectedValue)
		{
			Variables v = new Variables()
			{
				{ "a", a },
				{ "b", b },
				{ "c", c },
				{ "p", p },
				{ "q", q },
				{ "r", r },
				{ "s", s },
				{ "t", t },
				{ "z1", z1 },
				{ "z2", z2 }
			};

			Expression Exp = new Expression(Script);
			object Result = await Exp.EvaluateAsync(v);

			if (Result is BooleanMatrix BM)
				Result = BM.Values;
			else if (Result is DoubleMatrix DM)
				Result = DM.Values;
			else if (Result is ComplexMatrix CM)
				Result = CM.Values;
			else if (Result is ObjectMatrix OM)
				Result = OM.Values;

			if (Result is Dictionary<string, IElement> R &&
				ExpectedValue is Dictionary<string, IElement> E)
			{
				AssertEqual(E.Count, R.Count, Script);

				foreach (KeyValuePair<string, IElement> P in E)
				{
					Assert.IsTrue(R.ContainsKey(P.Key), Script);
					AssertEqual(P.Value.AssociatedObjectValue, R[P.Key].AssociatedObjectValue, Script);
				}
			}
			else
				AssertEqual(ExpectedValue, Result, Script);

			ScriptParsingTests.AssertParentNodesAndSubsexpressions(Exp);

			Console.Out.WriteLine();
			Exp.ToXml(Console.Out);
			Console.Out.WriteLine();
		}

		public static void AssertEqual(object Expected, object Result, string Script)
		{
			if (Result is Array ResultArray && Expected is Array ExpectedArray)
			{
				int j, d = ExpectedArray.Rank;
				if (d != ResultArray.Rank)
					Assert.Fail("Array ranks mismatch: " + Script);

				switch (d)
				{
					case 1:
						int i, c = ExpectedArray.Length;
						if (c != ResultArray.Length)
							Assert.Fail("Array lengths mismatch: " + Script);

						for (i = 0; i < c; i++)
							AssertEqual(ExpectedArray.GetValue(i), ResultArray.GetValue(i), Script);
						break;

					case 2:
						d = ExpectedArray.GetLength(0);
						if (d != ResultArray.GetLength(0))
							Assert.Fail("Array lengths mismatch: " + Script);

						c = ExpectedArray.GetLength(1);
						if (c != ResultArray.GetLength(1))
							Assert.Fail("Array lengths mismatch: " + Script);

						for (j = 0; j < d; j++)
						{
							for (i = 0; i < c; i++)
								AssertEqual(ExpectedArray.GetValue(j, i), ResultArray.GetValue(j, i), Script);
						}
						break;

					default:
						Assert.Fail("Usupported rank: " + d.ToString());
						break;
				}
			}
			else if (Result is double d)
			{
				if (Convert.ToDouble(Expected) != d)
					Assert.Fail("Expected " + Expected.ToString() + ", but got " + Result.ToString() + ": " + Script);
			}
			else if (Expected is null)
			{
				if (!(Result is null))
					Assert.Fail("Expected null: " + Script);
			}
			else if (!Expected.Equals(Result))
				Assert.Fail("Expected " + Expected.ToString() + ", but got " + Result.ToString() + ": " + Script);
		}

		[TestMethod]
		public async Task Evaluation_Test_01_Sequences()
		{
			await Test("a;b;c", c);
		}

		[TestMethod]
		public async Task Evaluation_Test_02_ConditionalStatements()
		{
			await Test("DO ++a WHILE a<=10", 11);
			await Test("WHILE a<=10 DO ++a", 11);
			await Test("WHILE a<=10 : ++a", 11);
			await Test("FOREACH x IN [a,b,c] DO x", c);
			await Test("FOREACH x IN [a,b,c] : x", c);
			await Test("FOR EACH x IN [a,b,c] DO x", c);
			await Test("FOR EACH x IN [a,b,c] : x", c);
			await Test("FOR a:=1 TO 10 STEP 2 DO a", 9);
			await Test("FOR a:=1 TO 10 STEP 2 : a", 9);
			await Test("FOR a:=1 TO 10 DO a", 10);
			await Test("FOR a:=1 TO 10 : a", 10);
			await Test("TRY a CATCH b FINALLY a:=0;a", 0);
			await Test("a:='a';TRY a++ CATCH a:=0;a", 0);
			await Test("TRY a FINALLY a:=0;a", 0);
		}

		[TestMethod]
		public async Task Evaluation_Test_03_Lists()
		{
			await Test("a,b,c", new double[] { a, b, c });
			await Test("a,b,,c", new object[] { a, b, null, c });
		}

		[TestMethod]
		public async Task Evaluation_Test_04_Assignments()
		{
			await Test("x:=10;x+x", 20);
			await Test("a+=b;a", 11);
			await Test("a-=b;a", -1);
			await Test("a*=b;a", 30);
			await Test("b/=2;b", 3);
			await Test("a^=b;a", 15625);
			await Test("a&=b;a", 4);
			await Test("p&&=q;p", false);
			await Test("a|=b;a", 7);
			await Test("q||=p;q", true);
			await Test("a<<=b;a", 320);
			await Test("a>>=1;a", 2);

			await Test("[x,y]:=[1,2];[a,b,c,x,y]", new double[] { 5, 6, 7, 1, 2 });
			await Test("[[x,y],[y,z]]:=[[a,b],[b,c]];[a,b,c,x,y,z]", new double[] { 5, 6, 7, 5, 6, 7 });
			await Test("{x,y,z}:={a,b,c};x+y+z", 18);

			await Test("f(x):=x^2;f(10)", 100);
			await Test("f(x,y):=x*y;f(2,3)", 6);

			await Test("f(x,[y]):=x*y;f(2,3)", 6);
			await Test("f(x,[y]):=x*y;f(2,[3,4,5])", new double[] { 6, 8, 10 });
			await Test("f(x,[y]):=x*y;f(2,[[3,4],[5,6]])", new double[,] { { 6, 8 }, { 10, 12 } });
			await Test("f(x,[y]):=x*y;f(2,{3,3,4,5})", new double[] { 6, 8, 10 });

			await Test("f(y[]):=y.Length;f([[1,2],[3,4]])", new double[] { 2, 2 });
			await Test("f(y[]):=y.Length;f([1,2,3])", 3);
			await Test("f(y[]):=y.Length;f({1,2,3})", new double[] { 1 });
			await Test("f(y[]):=y.Length;f(3)", 1);

			await Test("f(y[,]):=y.Rows;f(3)", 1);
			await Test("f(y[,]):=y.Rows;f([1,2,3])", 1);
			await Test("f(y[,]):=y.Rows;f([[1,2],[3,4]])", 2);
			await Test("f(y[,]):=y.Rows;f({1,2,3})", new double[] { 1 });

			await Test("f(y{}):=y union {2,3};f(3)", new object[] { 3, 2 });
			await Test("f(y{}):=y union {2,3};f([1,2,3])", new object[] { 1, 2, 3 });
			await Test("f(y{}):=y union {2,3};f([[1,2],[3,4]])", new object[] { new object[] { 1, 2, 3 }, new object[] { 3, 4, 2 } });
			await Test("f(y{}):=y union {2,3};f({1,2,3})", new object[] { 1, 2, 3 });

			await Test("Obj:={m1:a,m2:b,m3:c};Obj.m2:=a+b+c;Obj.m2", 18);
			await Test("v:=[1,2,3];v[1]:=a;v", new double[] { 1, 5, 3 });
			await Test("M:=[[1,2],[3,4]];M[0,1]:=a;M", new double[,] { { 1, 2 }, { 5, 4 } });
			await Test("M:=[[1,2],[3,4]];M[,1]:=[a,b];M", new double[,] { { 1, 2 }, { 5, 6 } });
			await Test("M:=[[1,2],[3,4]];M[1,]:=[a,b];M", new double[,] { { 1, 5 }, { 3, 6 } });
			await Test("Obj:={m1:a,m2:b,m3:c};s:='m';Obj.(s+'1'):=a+b+c;Obj.m1", 18);

			// TODO: Test dynamic index, and dynamic index assignment.
		}

		[TestMethod]
		public async Task Evaluation_Test_05_IF()
		{
			await Test("IF a<b THEN a", a);
			await Test("IF a>b THEN a", null);
			await Test("IF a<b THEN a ELSE b", a);
			await Test("IF a>b THEN a ELSE b", b);
			await Test("a<b ? a", a);
			await Test("a>b ? a", null);
			await Test("a<b ? a : b", a);
			await Test("a ?? b", a);
			await Test("null ?? b", b);

			await Test("IF b<=[a,b,c] THEN a ELSE b", new double[] { b, a, a });
			await Test("b<=[a,b,c] ? [1,2,3] : [4,5,6]", new double[] { 4, 2, 3 });
		}

		[TestMethod]
		public async Task Evaluation_Test_06_Lambda()
		{
			await Test("(x->x^2)(10)", 100);
			await Test("((x,y)->x*y)(2,3)", 6);

			await Test("((x,[y])->x*y)(2,3)", 6);
			await Test("((x,[y])->x*y)(2,[3,4,5])", new double[] { 6, 8, 10 });
			await Test("((x,[y])->x*y)(2,[[3,4],[5,6]])", new double[,] { { 6, 8 }, { 10, 12 } });
			await Test("((x,[y])->x*y)(2,{3,3,4,5})", new double[] { 6, 8, 10 });

			await Test("(y[]->y.Length)(3)", 1);
			await Test("(y[]->y.Length)([1,2,3])", 3);
			await Test("(y[]->y.Length)([[1,2],[3,4]])", new double[] { 2, 2 });
			await Test("(y[]->y.Length)({1,2,3})", new double[] { 1 });

			await Test("(y[,]->y.Rows)(3)", 1);
			await Test("(y[,]->y.Rows)([1,2,3])", 1);
			await Test("(y[,]->y.Rows)([[1,2],[3,4]])", 2);
			await Test("(y[,]->y.Rows)({1,2,3})", new double[] { 1 });

			await Test("(y{}->y union {2,3})(3)", new object[] { 3, 2 });
			await Test("(y{}->y union {2,3})([1,2,3])", new object[] { 1, 2, 3 });
			await Test("(y{}->y union {2,3})([[1,2],[3,4]])", new object[] { new object[] { 1, 2, 3 }, new object[] { 3, 4, 2 } });
			await Test("(y{}->y union {2,3})({1,2,3})", new object[] { 1, 2, 3 });
		}

		[TestMethod]
		public async Task Evaluation_Test_07_Implication()
		{
			await Test("p => q", false);
			await Test("q => p", true);

			await Test("[p,q,r] => q", new bool[] { false, true, false });
			await Test("p=>[p,q,r]", new bool[] { true, false, true });
			await Test("[p,r,q]=>[p,q,r]", new bool[] { true, false, true });

			await Test("[[p,q,r],[q,r,p]] => q", new bool[,] { { false, true, false }, { true, false, false } });
			await Test("p=>[[p,q,r],[q,r,p]]", new bool[,] { { true, false, true }, { false, true, true } });
			await Test("[[p,r,q],[r,q,p]]=>[[p,q,r],[q,r,p]]", new bool[,] { { true, false, true }, { false, true, true } });
		}

		[TestMethod]
		public async Task Evaluation_Test_08_Equivalence()
		{
			await Test("p <=> q", false);

			await Test("[p,q,r] <=> q", new bool[] { false, true, false });
			await Test("p <=> [p,q,r]", new bool[] { true, false, true });
			await Test("[p,r,q] <=> [p,q,r]", new bool[] { true, false, false });

			await Test("[[p,q,r],[q,r,p]] <=> q", new bool[,] { { false, true, false }, { true, false, false } });
			await Test("p <=> [[p,q,r],[q,r,p]]", new bool[,] { { true, false, true }, { false, true, true } });
			await Test("[[p,r,q],[r,q,p]] <=> [[p,q,r],[q,r,p]]", new bool[,] { { true, false, false }, { false, false, true } });
		}

		[TestMethod]
		public async Task Evaluation_Test_09_OR()
		{
			await Test("p || q", true);
			await Test("q || p", true);
			await Test("[p,q,r] || q", new bool[] { true, false, true });
			await Test("p || [p,q,r]", new bool[] { true, true, true });
			await Test("[p,r,q] || [p,q,r]", new bool[] { true, true, true });

			await Test("a | b", 7);
			await Test("b | a", 7);
			await Test("[a,b,c] | b", new double[] { 7, 6, 7 });
			await Test("a | [a,b,c]", new double[] { 5, 7, 7 });
			await Test("[a,c,b] | [a,b,c]", new double[] { 5, 7, 7 });

			await Test("p OR q", true);
			await Test("q OR p", true);
			await Test("[p,q,r] OR q", new bool[] { true, false, true });
			await Test("p OR [p,q,r]", new bool[] { true, true, true });
			await Test("[p,r,q] OR [p,q,r]", new bool[] { true, true, true });

			await Test("a OR b", 7);
			await Test("b OR a", 7);
			await Test("[a,b,c] OR b", new double[] { 7, 6, 7 });
			await Test("a OR [a,b,c]", new double[] { 5, 7, 7 });
			await Test("[a,c,b] OR [a,b,c]", new double[] { 5, 7, 7 });

			await Test("p XOR q", true);
			await Test("q XOR p", true);
			await Test("[p,q,r] XOR q", new bool[] { true, false, true });
			await Test("p XOR [p,q,r]", new bool[] { false, true, false });
			await Test("[p,r,q] XOR [p,q,r]", new bool[] { false, true, true });

			await Test("a XOR b", 3);
			await Test("b XOR a", 3);
			await Test("[a,b,c] XOR b", new double[] { 3, 0, 1 });
			await Test("a XOR [a,b,c]", new double[] { 0, 3, 2 });
			await Test("[a,c,b] XOR [a,b,c]", new double[] { 0, 1, 1 });

			await Test("p NOR q", false);
			await Test("q NOR p", false);
			await Test("[p,q,r] NOR q", new bool[] { false, true, false });
			await Test("p NOR [p,q,r]", new bool[] { false, false, false });
			await Test("[p,r,q] NOR [p,q,r]", new bool[] { false, false, false });

			await Test("a NOR b", unchecked((ulong)-8));
			await Test("b NOR a", unchecked((ulong)-8));
			await Test("[a,b,c] NOR b", new double[] { unchecked((ulong)-8), unchecked((ulong)-7), unchecked((ulong)-8) });
			await Test("a NOR [a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-8), unchecked((ulong)-8) });
			await Test("[a,c,b] NOR [a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-8), unchecked((ulong)-8) });

			await Test("p XNOR q", false);
			await Test("q XNOR p", false);
			await Test("[p,q,r] XNOR q", new bool[] { false, true, false });
			await Test("p XNOR [p,q,r]", new bool[] { true, false, true });
			await Test("[p,r,q] XNOR [p,q,r]", new bool[] { true, false, false });

			await Test("a XNOR b", unchecked((ulong)-4));
			await Test("b XNOR a", unchecked((ulong)-4));
			await Test("[a,b,c] XNOR b", new double[] { unchecked((ulong)-4), unchecked((ulong)-1), unchecked((ulong)-2) });
			await Test("a XNOR [a,b,c]", new double[] { unchecked((ulong)-1), unchecked((ulong)-4), unchecked((ulong)-3) });
			await Test("[a,c,b] XNOR [a,b,c]", new double[] { unchecked((ulong)-1), unchecked((ulong)-2), unchecked((ulong)-2) });
		}

		[TestMethod]
		public async Task Evaluation_Test_10_AND()
		{
			await Test("p && q", false);
			await Test("q && p", false);
			await Test("[p,q,r] && q", new bool[] { false, false, false });
			await Test("p && [p,q,r]", new bool[] { true, false, true });
			await Test("[p,r,q] && [p,q,r]", new bool[] { true, false, false });

			await Test("a & b", 4);
			await Test("b & a", 4);
			await Test("[a,b,c] & b", new double[] { 4, 6, 6 });
			await Test("a & [a,b,c]", new double[] { 5, 4, 5 });
			await Test("[a,c,b] & [a,b,c]", new double[] { 5, 6, 6 });

			await Test("p AND q", false);
			await Test("q AND p", false);
			await Test("[p,q,r] AND q", new bool[] { false, false, false });
			await Test("p AND [p,q,r]", new bool[] { true, false, true });
			await Test("[p,r,q] AND [p,q,r]", new bool[] { true, false, false });

			await Test("a AND b", 4);
			await Test("b AND a", 4);
			await Test("[a,b,c] AND b", new double[] { 4, 6, 6 });
			await Test("a AND [a,b,c]", new double[] { 5, 4, 5 });
			await Test("[a,c,b] AND [a,b,c]", new double[] { 5, 6, 6 });

			await Test("p NAND q", true);
			await Test("q NAND p", true);
			await Test("[p,q,r] NAND q", new bool[] { true, true, true });
			await Test("p NAND [p,q,r]", new bool[] { false, true, false });
			await Test("[p,r,q] NAND [p,q,r]", new bool[] { false, true, true });

			await Test("a NAND b", unchecked((ulong)-5));
			await Test("b NAND a", unchecked((ulong)-5));
			await Test("[a,b,c] NAND b", new double[] { unchecked((ulong)-5), unchecked((ulong)-7), unchecked((ulong)-7) });
			await Test("a NAND [a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-5), unchecked((ulong)-6) });
			await Test("[a,c,b] NAND [a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-7), unchecked((ulong)-7) });
		}

		[TestMethod]
		public async Task Evaluation_Test_11_Membership()
		{
			await Test("a AS System.Double", a);
			await Test("a AS System.Int32", null);
			await Test("[a,b] AS System.Double", null);
			await Test("a AS [System.Double,System.Int32]", new object[] { a, null });

			await Test("a IS System.Double", true);
			await Test("a IS System.Int32", false);
			await Test("[a,b] IS System.Double", false);
			await Test("a IS [System.Double,System.Int32]", new bool[] { true, false });

			await Test("a IN R", true);
			await Test("a IN EmptySet", false);
			await Test("[a,b] IN R", false);
			await Test("a IN {a,b,c}", true);
			await Test("a IN [a,b,c]", true);

			await Test("a NOT IN R", false);
			await Test("a NOTIN EmptySet", true);
			await Test("[a,b] NOT IN R", true);
			await Test("a NOTIN {a,b,c}", false);
			await Test("a NOTIN [a,b,c]", false);

			await Test("System.Double INHERITS System.Double", true);
			await Test("a INHERITS System.Double", true);
			await Test("Waher.Runtime.Inventory.Grade INHERITS System.Enum", true);
			await Test("Waher.Runtime.Inventory.Grade.Ok INHERITS System.Enum", true);

			await Test("Identity(2) matches [[e11,e12],[e21,e22]] ? [e11,e12,e21,e22]", new object[] { 1, 0, 0, 1 });
		}

		[TestMethod]
		public async Task Evaluation_Test_12_Comparison()
		{
			await Test("a <= b", true);
			await Test("a <= [a,b,c]", new bool[] { true, true, true });
			await Test("[a,b,c] <= b", new bool[] { true, true, false });
			await Test("[a,b,c] <= [a,c,b]", new bool[] { true, true, false });

			await Test("a < b", true);
			await Test("a >= b", false);
			await Test("a > b", false);
			await Test("a = b", false);
			await Test("a == b", false);
			await Test("a === b", false);
			await Test("a <> b", true);
			await Test("a != b", true);
			await Test("s LIKE 'H.*'", true);
			await Test("s NOT LIKE 'Bye.*'", true);
			await Test("s NOTLIKE 'Bye.*'", true);
			await Test("s UNLIKE 'Bye.*'", true);
			await Test("if s LIKE \"H(?'Rest'.*)\" then Rest", "ello");
			await Test("a .= b", false);
			await Test("a .== b", false);
			await Test("a .=== b", false);
			await Test("a .<> b", true);
			await Test("a .!= b", true);
			await Test("a <= b <= c", true);
			await Test("a <= b < c", true);
			await Test("a < b <= c", true);
			await Test("a < b < c", true);
			await Test("a >= b >= c", false);
			await Test("a > b >= c", false);
			await Test("a >= b > c", false);
			await Test("a > b > c", false);
		}

		[TestMethod]
		public async Task Evaluation_Test_13_Shift()
		{
			await Test("a << b", 320);
			await Test("[1,2,3] << b", new double[] { 64, 128, 192 });

			await Test("7 >> 1", 3);
			await Test("[7,8,9] >> 1", new double[] { 3, 4, 4 });
		}

		[TestMethod]
		public async Task Evaluation_Test_14_Union()
		{
			await Test("{a,b,c} UNION {4,5,6}", new object[] { 5, 6, 7, 4 });
			await Test("{a,b,c} ∪ {4,5,6}", new object[] { 5, 6, 7, 4 });
		}

		[TestMethod]
		public async Task Evaluation_Test_15_Intersection()
		{
			await Test("{4,5,6} INTERSECT {a,b,c}", new object[] { 5, 6 });
			await Test("{4,5,6} INTERSECTION {a,b,c}", new object[] { 5, 6 });
			await Test("{4,5,6} ∩ {a,b,c}", new object[] { 5, 6 });
		}

		[TestMethod]
		public async Task Evaluation_Test_16_Intervals()
		{
			await Test("1..10", new Interval(1, 10, true, true, null));
			await Test("1..10|0.1", new Interval(1, 10, true, true, 0.1));
		}

		[TestMethod]
		public async Task Evaluation_Test_17_Terms()
		{
			await Test("a+b", 11);
			await Test("[1,2,3]+a", new double[] { 6, 7, 8 });
			await Test("a+[1,2,3]", new double[] { 6, 7, 8 });
			await Test("[1,2,3]+[a,b,c]", new double[] { 6, 8, 10 });

			await Test("s+' '+t", "Hello Bye");
			await Test("[1,2,3]+s", new object[] { "1Hello", "2Hello", "3Hello" });
			await Test("s+[1,2,3]", new object[] { "Hello1", "Hello2", "Hello3" });
			await Test("[1,2,3]+[s,t,s]", new object[] { "1Hello", "2Bye", "3Hello" });
			await Test("[s,t,s]+[1,2,3]", new object[] { "Hello1", "Bye2", "Hello3" });
			await Test("['a','b','c']+[s,t,s]", new object[] { "aHello", "bBye", "cHello" });
			await Test("[s,t,s]+['a','b','c']", new object[] { "Helloa", "Byeb", "Helloc" });

			await Test("10-a", 5);
			await Test("[10,20,30]-a", new double[] { 5, 15, 25 });
			await Test("[5,6,7]-[a,b,c]", new double[] { 0, 0, 0 });
			await Test("[[10,20],[30,40]]-a", new double[,] { { 5, 15 }, { 25, 35 } });
			await Test("a-[[1,1],[0,1]]", new double[,] { { 4, 4 }, { 5, 4 } });
			await Test("[[a,b],[b,c]]-[[1,1],[0,1]]", new double[,] { { 4, 5 }, { 6, 6 } });

			await Test("a.+b", 11);
			await Test("a.+[a,b,c]", new double[] { 10, 11, 12 });
			await Test("[a,b,c].+a", new double[] { 10, 11, 12 });
			await Test("[a,b,c].+[a,b,c]", new double[] { 10, 12, 14 });
			await Test("[[a,b],[b,c]].+[[a,b],[b,c]]", new double[,] { { 10, 12 }, { 12, 14 } });

			await Test("10.-a", 5);
			await Test("[10,20,30].-a", new double[] { 5, 15, 25 });
			await Test("[5,6,7].-[a,b,c]", new double[] { 0, 0, 0 });
			await Test("[[10,20],[30,40]].-a", new double[,] { { 5, 15 }, { 25, 35 } });
			await Test("b.-[[1,2],[2,1]]", new double[,] { { 5, 4 }, { 4, 5 } });
			await Test("[[a,b],[b,c]].-[[1,2],[2,1]]", new double[,] { { 4, 4 }, { 4, 6 } });

			await Test("z1+2+3*i", new Complex(3, 5));
			await Test("z1+z2", new Complex(4, 6));
			await Test("z1-z2", new Complex(-2, -2));
			await Test("z1+2", new Complex(3, 2));
			await Test("2+z1", new Complex(3, 2));

			await Test("[z1,z2]+2", new Complex[] { new Complex(3, 2), new Complex(5, 4) });
			await Test("2+[z1,z2]", new Complex[] { new Complex(3, 2), new Complex(5, 4) });
			await Test("[z1,z2]+z2", new Complex[] { new Complex(4, 6), new Complex(6, 8) });
			await Test("z2+[z1,z2]", new Complex[] { new Complex(4, 6), new Complex(6, 8) });
			await Test("[z1,z2]+[1,2]", new Complex[] { new Complex(2, 2), new Complex(5, 4) });
			await Test("[1,2]+[z1,z2]", new Complex[] { new Complex(2, 2), new Complex(5, 4) });
			await Test("[z1,z2]+[1,(2,3)]", new Complex[] { new Complex(2, 2), new Complex(5, 7) });
			await Test("[1,(2,3)]+[z1,z2]", new Complex[] { new Complex(2, 2), new Complex(5, 7) });
			await Test("[z1,z2]+[z1,z2]", new Complex[] { new Complex(2, 4), new Complex(6, 8) });

			await Test("[[z1,z2],[z2,z1]]+2", new Complex[,] { { new Complex(3, 2), new Complex(5, 4) }, { new Complex(5, 4), new Complex(3, 2) } });
			await Test("2+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(3, 2), new Complex(5, 4) }, { new Complex(5, 4), new Complex(3, 2) } });
			await Test("[[z1,z2],[z2,z1]]+z2", new Complex[,] { { new Complex(4, 6), new Complex(6, 8) }, { new Complex(6, 8), new Complex(4, 6) } });
			await Test("z2+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(4, 6), new Complex(6, 8) }, { new Complex(6, 8), new Complex(4, 6) } });
			await Test("[[z1,z2],[z2,z1]]+[[1,2],[3,4]]", new Complex[,] { { new Complex(2, 2), new Complex(5, 4) }, { new Complex(6, 4), new Complex(5, 2) } });
			await Test("[[1,2],[3,4]]+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(2, 2), new Complex(5, 4) }, { new Complex(6, 4), new Complex(5, 2) } });
			await Test("[[z1,z2],[z2,z1]]+[[1,2],[3,(2,3)]]", new Complex[,] { { new Complex(2, 2), new Complex(5, 4) }, { new Complex(6, 4), new Complex(3, 5) } });
			await Test("[[1,2],[3,(2,3)]]+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(2, 2), new Complex(5, 4) }, { new Complex(6, 4), new Complex(3, 5) } });
			await Test("[[z1,z2],[z2,z1]]+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(2, 4), new Complex(6, 8) }, { new Complex(6, 8), new Complex(2, 4) } });
		}

		[TestMethod]
		public async Task Evaluation_Test_18_Factors()
		{
			await Test("a*b", 30);
			await Test("[1,2,3]*a", new double[] { 5, 10, 15 });
			await Test("a*[1,2,3]", new double[] { 5, 10, 15 });
			await Test("[1,2,3]*[a,b,c]", new double[] { 5, 12, 21 });

			await Test("10/a", 2);
			await Test("[10,20,30]/a", new double[] { 2, 4, 6 });
			await Test("[5,6,7]/[a,b,c]", new double[] { 1, 1, 1 });
			await Test("[[10,20],[30,40]]/a", new double[,] { { 2, 4 }, { 6, 8 } });
			await Test("a/[[1,1],[0,1]]", new double[,] { { 5, -5 }, { 0, 5 } });
			await Test("[[a,b],[b,c]]/[[1,1],[0,1]]", new double[,] { { 5, 1 }, { 6, 1 } });

			await Test("a\\10", 2);
			await Test("a\\[10,20,30]", new double[] { 2, 4, 6 });
			await Test("[a,b,c]\\[5,6,7]", new double[] { 1, 1, 1 });
			await Test("a\\[[10,20],[30,40]]", new double[,] { { 2, 4 }, { 6, 8 } });
			await Test("[[1,1],[0,1]]\\a", new double[,] { { 5, -5 }, { 0, 5 } });
			await Test("[[1,1],[0,1]]\\[[a,b],[b,c]]", new double[,] { { -1, -1 }, { 6, 7 } });

			await Test("{4,5,6}\\{a,b,c}", new object[] { 4 });

			await Test("b MOD a", 1);
			await Test("b MOD [a,b,c]", new double[] { 1, 0, 6 });
			await Test("[a,b,c] MOD a", new double[] { 0, 1, 2 });
			await Test("[a,b,c] MOD [b,c,a]", new double[] { 5, 6, 2 });

			await Test("a.*b", 30);
			await Test("a.*[a,b,c]", new double[] { 25, 30, 35 });
			await Test("[a,b,c].*a", new double[] { 25, 30, 35 });
			await Test("[a,b,c].*[a,b,c]", new double[] { 25, 36, 49 });
			await Test("[[a,b],[b,c]].*[[a,b],[b,c]]", new double[,] { { 25, 36 }, { 36, 49 } });

			await Test("10./a", 2);
			await Test("[10,20,30]./a", new double[] { 2, 4, 6 });
			await Test("[5,6,7]./[a,b,c]", new double[] { 1, 1, 1 });
			await Test("[[10,20],[30,40]]./a", new double[,] { { 2, 4 }, { 6, 8 } });
			await Test("b./[[1,2],[2,1]]", new double[,] { { 6, 3 }, { 3, 6 } });
			await Test("[[a,b],[b,c]]./[[1,2],[2,1]]", new double[,] { { 5, 3 }, { 3, 7 } });

			await Test("a.\\10", 2);
			await Test("a.\\[10,20,30]", new double[] { 2, 4, 6 });
			await Test("[a,b,c].\\[5,6,7]", new double[] { 1, 1, 1 });
			await Test("a.\\[[10,20],[30,40]]", new double[,] { { 2, 4 }, { 6, 8 } });
			await Test("[[1,2],[2,1]].\\b", new double[,] { { 6, 3 }, { 3, 6 } });
			await Test("[[1,2],[2,1]].\\[[a,b],[b,c]]", new double[,] { { 5, 3 }, { 3, 7 } });

			await Test("b .MOD a", 1);
			await Test("b .MOD [a,b,c]", new double[] { 1, 0, 6 });
			await Test("[a,b,c] .MOD a", new double[] { 0, 1, 2 });
			await Test("[a,b,c] .MOD [b,c,a]", new double[] { 5, 6, 2 });

			await Test("[1,2,3] DOT [a,b,c]", 38);
			await Test("[1,2,3] CROSS [a,b,c]", new double[] { -4, 8, -4 });
			await Test("[1,2,3] CARTESIAN [a,b,c]", new object[]
			{
				new double[] { 1, 5 },
				new double[] { 1, 6 },
				new double[] { 1, 7 },
				new double[] { 2, 5 },
				new double[] { 2, 6 },
				new double[] { 2, 7 },
				new double[] { 3, 5 },
				new double[] { 3, 6 },
				new double[] { 3, 7 }
			});
		}

		[TestMethod]
		public async Task Evaluation_Test_18_Powers()
		{
			await Test("a^b", Math.Pow(5, 6));
			await Test("a²", 25);
			await Test("a³", 125);

			await Test("[[a,1],[1,b]]²", new double[,] { { a * a + 1, a + b }, { a + b, 1 + b * b } });
			await Test("[[a,1],[1,b]]^2", new double[,] { { a * a + 1, a + b }, { a + b, 1 + b * b } });

			await Test("a.^b", Math.Pow(5, 6));
			await Test("[[a,1],[1,b]].^2", new double[,] { { a * a, 1 }, { 1, b * b } });
		}

		[TestMethod]
		public async Task Evaluation_Test_19_UnaryPrefixOperators()
		{
			await Test("++a", 6);
			await Test("d:=[a,b,c];++d", new double[] { 6, 7, 8 });
			await Test("M:=[[a,b,c],[b,c,a],[c,a,b]];++M", new double[,] { { 6, 7, 8 }, { 7, 8, 6 }, { 8, 6, 7 } });

			await Test("--a", 4);
			await Test("d:=[a,b,c];--d", new double[] { 4, 5, 6 });
			await Test("M:=[[a,b,c],[b,c,a],[c,a,b]];--M", new double[,] { { 4, 5, 6 }, { 5, 6, 4 }, { 6, 4, 5 } });

			await Test("+a", 5);
			await Test("+[a,b,c]", new double[] { 5, 6, 7 });

			await Test("-a", -5);
			await Test("-[a,b,c]", new double[] { -5, -6, -7 });

			await Test("!p", false);
			await Test("![p,q,r]", new bool[] { false, true, false });

			await Test("NOT q", true);
			await Test("NOT [p,q,r]", new bool[] { false, true, false });

			await Test("~a", (double)unchecked((ulong)-6));
			await Test("~[a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-7), unchecked((ulong)-8) });
		}

		[TestMethod]
		public async Task Evaluation_Test_20_UnarySuffixOperators()
		{
			await Test("[a++,a]", new double[] { 5, 6 });
			await Test("[a--,a]", new double[] { 5, 4 });

			await Test("a%", 0.05);
			await Test("a‰", 0.005);
			await Test("a‱", 0.0005);
			await Test("a°", Math.PI / 36);

			/*
			await Test("f'(x)");   TODO
			await Test("f′(x)");   TODO
			await Test("f\"(x)");   TODO
			await Test("f″(x)");   TODO
			await Test("f‴(x)");   TODO*/

			await Test("[[a,a,a],[b,b,b],[c,c,c]]T", new double[,] { { a, b, c }, { a, b, c }, { a, b, c } });
			await Test("[[a,a,a],[b,b,b],[c,c,c]]H", new double[,] { { a, b, c }, { a, b, c }, { a, b, c } });
			await Test("[[a,a,a],[b,b,b],[c,c,c]]†", new double[,] { { a, b, c }, { a, b, c }, { a, b, c } });

			await Test("a!", 120);
			await Test("a!!", 15);
		}

		[TestMethod]
		public async Task Evaluation_Test_21_BinarySuffixOperators()
		{
			await Test("Obj:={m1:a,m2:b,m3:c};Obj.m1", a);
			await Test("[a,b,c].Length", 3);
			await Test("Obj:={m1:a,m2:b,m3:c};Obj.['m1','m2']", new double[] { a, b });
			await Test("Obj:={m1:a,m2:b,m3:c};s:='m';Obj.(s+'1')", a);

			await Test("a[]", new double[] { a });
			await Test("[a,b,c,a,b,c][]", new double[] { a, b, c, a, b, c });
			await Test("{a,b,c,a,b,c}[]", new double[] { a, b, c });
			await Test("[[a,b],[b,c]][]", new object[] { new double[] { a, b }, new double[] { b, c } });

			await Test("a[,]", new double[,] { { a } });
			await Test("[a,b,c][,]", new double[,] { { a, b, c } });
			await Test("{a,b,c}[,]", new double[,] { { a, b, c } });
			await Test("[[a,b],[b,c]][,]", new double[,] { { a, b }, { b, c } });

			await Test("a{}", new double[] { a });
			await Test("[a,b,c,a,b,c]{}", new double[] { a, b, c });
			await Test("{a,b,c,a,b,c}{}", new double[] { a, b, c });
			await Test("[[a,b],[b,c]]{}", new object[] { new double[] { a, b }, new double[] { b, c } });

			await Test("[a,b,c][1]", b);
			await Test("[a,b,c][1..2]", new double[] { b, c });

			await Test("[[a,b],[c,a]][0,1]", c);
			await Test("[[a,b],[c,a]][0,0..1]", new double[] { a, c });

			await Test("[[a,b],[c,a]][0,]", new double[] { a, c });
			await Test("[[a,b],[c,a]][0..1,]", new double[,] { { a, c }, { b, a } });

			await Test("[[a,b],[c,a]][,0]", new double[] { a, b });
			await Test("[[a,b],[c,a]][,0..1]", new double[,] { { a, b }, { c, a } });

			await Test("System", new Namespace("System"));
			await Test("System.Text", new Namespace("System.Text"));
			await Test("System.Text.RegularExpressions", new Namespace("System.Text.RegularExpressions"));
			await Test("System.Text.RegularExpressions.Regex", typeof(System.Text.RegularExpressions.Regex));
		}

		[TestMethod]
		public async Task Evaluation_Test_22_ObjectExNihilo()
		{
			Dictionary<string, IElement> Obj = new Dictionary<string, IElement>()
			{
				{ "Member1", new DoubleNumber(a) },
				{ "Member2", new DoubleNumber(b) },
				{ "Member3", new DoubleNumber(c) }
			};

			await Test("{Member1:a, Member2:b, Member3:c}", Obj);

			Obj["Member1"] = new StringValue("Value1");
			Obj["Member2"] = new StringValue("Value2");
			Obj["Member3"] = new StringValue("Value3");

			await Test("{\"Member1\":\"Value1\", \"Member2\":\"Value2\", \"Member3\":\"Value3\"}", Obj);
		}

		[TestMethod]
		public async Task Evaluation_Test_23_Sets()
		{
			await Test("S:={1,2,3};", new object[] { 1, 2, 3 });
			await Test("S:={DO a++ WHILE a<10};", new object[] { 5, 6, 7, 8, 9 });
			await Test("S:={WHILE a<10 : a++};", new object[] { 5, 6, 7, 8, 9 });
			await Test("S:={FOR x:=1 TO 20 STEP 3 : x};", new object[] { 1, 4, 7, 10, 13, 16, 19 });
			await Test("S:={FOREACH x IN 1..10|2 : x^2};", new object[] { 1, 9, 25, 49, 81 });
			await Test("S:={FOR EACH x IN 1..10|2 : x^2};", new object[] { 1, 9, 25, 49, 81 });
		}

		[TestMethod]
		public async Task Evaluation_Test_24_Matrices()
		{
			await Test("[[a,b,c],[b,c,a]];", new double[,] { { a, b, c }, { b, c, a } });
			await Test("[DO [a++,a++,a++] WHILE a<10];", new double[,] { { 5, 6, 7 }, { 8, 9, 10 } });
			await Test("[WHILE a<10 : [a++,a++,a++]];", new double[,] { { 5, 6, 7 }, { 8, 9, 10 } });
			await Test("[FOR y:=1 TO 3 : [FOR x:=1 TO 3 : x=y ? 1 : 0]];", new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
			await Test("[FOREACH x IN 1..10|2 : [x,x+1,x+2]];", new double[,] { { 1, 2, 3 }, { 3, 4, 5 }, { 5, 6, 7 }, { 7, 8, 9 }, { 9, 10, 11 } });
			await Test("[FOR EACH x IN 1..10|2 : [x,x+1,x+2]];", new double[,] { { 1, 2, 3 }, { 3, 4, 5 }, { 5, 6, 7 }, { 7, 8, 9 }, { 9, 10, 11 } });
		}

		[TestMethod]
		public async Task Evaluation_Test_25_Vectors()
		{
			await Test("[a,b,c];", new double[] { a, b, c });
			await Test("[DO a++ WHILE a<10];", new double[] { 5, 6, 7, 8, 9 });
			await Test("[WHILE a<10 : a++];", new double[] { 5, 6, 7, 8, 9 });
			await Test("[FOR x:=1 TO 20 STEP 3 : x];", new double[] { 1, 4, 7, 10, 13, 16, 19 });
			await Test("[FOREACH x IN 1..10|2 : x^2];", new double[] { 1, 9, 25, 49, 81 });
			await Test("[FOR EACH x IN 1..10|2 : x^2];", new double[] { 1, 9, 25, 49, 81 });
		}

		[TestMethod]
		public async Task Evaluation_Test_26_Parenthesis()
		{
			await Test("a * (b + c)", 65);
		}

		[TestMethod]
		public async Task Evaluation_Test_27_null()
		{
			await Test("null", null);
		}

		[TestMethod]
		public async Task Evaluation_Test_28_StringValues()
		{
			await Test("\"Hello\\r\\n\\t\\f\\b\\a\\v\\\\\\\"\\''\"", "Hello\r\n\t\f\b\a\v\\\"\''");
			await Test("'Hello\\r\\n\\t\\f\\b\\a\\v\\\\\\\"\\'\"'", "Hello\r\n\t\f\b\a\v\\\"\'\"");
		}

		[TestMethod]
		public async Task Evaluation_Test_29_BooleanValues()
		{
			await Test("true", true);
			await Test("false", false);
			await Test("⊤", true);
			await Test("⊥", false);
		}

		[TestMethod]
		public async Task Evaluation_Test_30_DoubleValues()
		{
			await Test("1", 1);
			await Test("3.1415927", 3.1415927);
			await Test("1.23e-3", 1.23e-3);
		}

		[TestMethod]
		public async Task Evaluation_Test_31_Constants()
		{
			await Test("e", Math.E);
			await Test("pi", Math.PI);
			await Test("π", Math.PI);
			await Test("i", Complex.ImaginaryOne);
			await Test("C", ComplexNumbers.Instance);
			await Test("R", RealNumbers.Instance);
			await Test("Z", Integers.Instance);
			await Test("EmptySet", EmptySet.Instance);
			await Test("∅", EmptySet.Instance);
			await Test("Now.Date", DateTime.Now.Date);
			await Test("Today", DateTime.Today);
			await Test("ε", double.Epsilon);
			await Test("eps", double.Epsilon);
			await Test("epsilon", double.Epsilon);
			await Test("∞", double.PositiveInfinity);
			await Test("inf", double.PositiveInfinity);
			await Test("infinity", double.PositiveInfinity);
		}

		[TestMethod]
		public async Task Evaluation_Test_32_BinomialCoefficients()
		{
			await Test("c OVER a", 21);
			await Test("8 OVER [0,1,2,3,4,5,6,7,8]", new double[] { 1, 8, 28, 56, 70, 56, 28, 8, 1 });
			await Test("[0,1,2,3,4,5,6,7,8] OVER 0", new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
			await Test("[0,1,2,3,4,5,6,7,8] OVER [0,1,2,3,4,5,6,7,8]", new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
		}

		[TestMethod]
		public async Task Evaluation_Test_33_ComplexNumbers()
		{
			await Test("(1,2)", new Complex(1, 2));
			await Test("1+2*i", new Complex(1, 2));
		}

		[TestMethod]
		public async Task Evaluation_Test_34_VectorFunctions()
		{
			await Test("Min([2,4,1,3])", 1);
			await Test("Max([2,4,1,3])", 4);
			await Test("Sum([1,2,3,4,5])", 15);
			await Test("Average([1,2,3,4,5])", 3);
			await Test("Avg([1,2,3,4,5])", 3);
			await Test("Product([1,2,3,4,5])", 120);
			await Test("Prod([1,2,3,4,5])", 120);
			await Test("Variance([1,2,3,4,5])", 2);
			await Test("Var([1,2,3,4,5])", 2);
			await Test("StandardDeviation([1,2,3,4,5])", Math.Sqrt(2.5));
			await Test("StdDev([1,2,3,4,5])", Math.Sqrt(2.5));
			await Test("Median([1,2,3,4,5])", 3);
			await Test("And([true,false,true])", false);
			await Test("And([3,2,1])", 0);
			await Test("Or([true,false,true])", true);
			await Test("Or([3,2,1])", 3);
			await Test("Xor([true,false,true])", false);
			await Test("Xor([3,2,1])", 0);
			await Test("Nand([true,false,true])", true);
			await Test("Nand([3,2,1])", 0xffffffffffffffff);
			await Test("Nor([true,false,true])", false);
			await Test("Nor([3,2,1])", unchecked((ulong)-4));
			await Test("Xnor([true,false,true])", true);
			await Test("Xnor([3,2,1])", 0xffffffffffffffff);
			await Test("Contains(1..10,5)", true);
			await Test("Contains(1..10,11)", false);
			await Test("IndexOf(1..10,5)", 4);
			await Test("IndexOf(1..10,11)", -1);
			await Test("LastIndexOf(1..10,5)", 4);
			await Test("LastIndexOf(1..10,11)", -1);
		}

		[TestMethod]
		public async Task Evaluation_Test_35_AnalyticFunctions()
		{
			await Test("sin(5)", Math.Sin(5));
			await Test("cos(5)", Math.Cos(5));
			await Test("tan(5)", Math.Tan(5));
			await Test("sec(5)", 1.0 / Math.Cos(5));
			await Test("csc(5)", 1.0 / Math.Sin(5));
			await Test("cot(5)", 1.0 / Math.Tan(5));

			await Test("round(arcsin(sin(1))*1e6)/1e6", 1);
			await Test("round(arccos(cos(1))*1e6)/1e6", 1);
			await Test("round(arctan(tan(1))*1e6)/1e6", 1);
			await Test("round(arcsec(sec(1))*1e6)/1e6", 1);
			await Test("round(arccsc(csc(1))*1e6)/1e6", 1);
			await Test("round(arccot(cot(1))*1e6)/1e6", 1);
			await Test("atan(3,4)", Math.Atan2(4, 3));
			await Test("arctan(3,4)", Math.Atan2(4, 3));

			await Test("sin(i)", Complex.Sin(Complex.ImaginaryOne));
			await Test("cos(i)", Complex.Cos(Complex.ImaginaryOne));
			await Test("tan(i)", Complex.Tan(Complex.ImaginaryOne));
			await Test("sec(i)", 1.0 / Complex.Cos(Complex.ImaginaryOne));
			await Test("csc(i)", 1.0 / Complex.Sin(Complex.ImaginaryOne));
			await Test("cot(i)", 1.0 / Complex.Tan(Complex.ImaginaryOne));

			await Test("round(arcsin(sin(i))*1e6)/1e6", Complex.ImaginaryOne);
			await Test("round(arccos(cos(i))*1e6)/1e6", Complex.ImaginaryOne);
			await Test("round(arctan(tan(i))*1e6)/1e6", Complex.ImaginaryOne);
			await Test("round(arcsec(sec(i))*1e6)/1e6", Complex.ImaginaryOne);
			await Test("round(arccsc(csc(i))*1e6)/1e6", Complex.ImaginaryOne);
			await Test("round(arccot(cot(i))*1e6)/1e6", Complex.ImaginaryOne);

			await Test("sinh(5)", Math.Sinh(5));
			await Test("cosh(5)", Math.Cosh(5));
			await Test("tanh(5)", Math.Tanh(5));
			await Test("sech(5)", 1.0 / Math.Cosh(5));
			await Test("csch(5)", 1.0 / Math.Sinh(5));
			await Test("coth(5)", 1.0 / Math.Tanh(5));

			await Test("round(arcsinh(sinh(1))*1e6)/1e6", 1);
			await Test("round(arccosh(cosh(1))*1e6)/1e6", 1);
			await Test("round(arctanh(tanh(1))*1e6)/1e6", 1);
			await Test("round(arcsech(sech(1))*1e6)/1e6", 1);
			await Test("round(arccsch(csch(1))*1e6)/1e6", 1);
			await Test("round(arccoth(coth(1))*1e6)/1e6", 1);

			await Test("sinh(i)", Complex.Sinh(Complex.ImaginaryOne));
			await Test("cosh(i)", Complex.Cosh(Complex.ImaginaryOne));
			await Test("tanh(i)", Complex.Tanh(Complex.ImaginaryOne));
			await Test("sech(i)", 1.0 / Complex.Cosh(Complex.ImaginaryOne));
			await Test("csch(i)", 1.0 / Complex.Sinh(Complex.ImaginaryOne));
			await Test("coth(i)", 1.0 / Complex.Tanh(Complex.ImaginaryOne));

			await Test("round(arcsinh(sinh(i))*1e6)/1e6", Complex.ImaginaryOne);
			await Test("round(arccosh(cosh(i))*1e6)/1e6", Complex.ImaginaryOne);
			await Test("round(arctanh(tanh(i))*1e6)/1e6", Complex.ImaginaryOne);
			await Test("round(arcsech(sech(i))*1e6)/1e6", Complex.ImaginaryOne);
			await Test("round(arccsch(csch(i))*1e6)/1e6", Complex.ImaginaryOne);
			await Test("round(arccoth(coth(i))*1e6)/1e6", Complex.ImaginaryOne);

			await Test("exp(1)", Math.E);
			await Test("ln(e)", 1);
			await Test("log10(10)", 1);
			await Test("lg(10)", 1);
			await Test("log2(2)", 1);
			await Test("sqrt(4)", 2);
		}

		[TestMethod]
		public async Task Evaluation_Test_36_ScalarFunctions()
		{
			await Test("abs(-1)", 1);
			await Test("ceil(pi)", 4);
			await Test("ceiling(pi)", 4);
			await Test("floor(pi)", 3);
			await Test("round(pi)", 3);
			await Test("sign(pi)", 1);

			await Test("abs(i)", 1);
			await Test("ceil(pi*i)", new Complex(0, 4));
			await Test("ceiling(pi*i)", new Complex(0, 4));
			await Test("floor(pi*i)", new Complex(0, 3));
			await Test("round(pi*i)", new Complex(0, 3));
			await Test("sign(pi*i)", new Complex(0, 1));

			await Test("min(a,b)", a);
			await Test("max(a,b)", b);

			await Test("number(a)", a);
			await Test("number(true)", 1);
			await Test("num(i)", Complex.ImaginaryOne);
			await Test("num('100')", 100);

			await Test("string(a)", "5");
			await Test("string(true)", "⊤");
			await Test("str(i)", "(0, 1)");
			await Test("str('100')", "100");
		}

		[TestMethod]
		public async Task Evaluation_Test_37_ComplexFunctions()
		{
			await Test("Re(2+i)", 2);
			await Test("Im(2+i)", 1);
			await Test("Arg(i)", Math.PI / 2);
			await Test("Conjugate(2+i)", new Complex(2, -1));
			await Test("Conj(2+i)", new Complex(2, -1));
			await Test("round(Polar(1,pi/2)*1e6)*1e-6", Complex.ImaginaryOne);
		}

		[TestMethod]
		public async Task Evaluation_Test_38_Matrices()
		{
			await Test("invert(2)", 0.5);
			await Test("inv(2)", 0.5);
			await Test("inverse(i)", new Complex(0, -1));
			await Test("invert([[1,1],[0,1]])", new double[,] { { 1, -1 }, { 0, 1 } });
		}

		[TestMethod]
		public async Task Evaluation_Test_39_Runtime()
		{
			await Test("exists(a)", true);
			await Test("exists(k)", false);

			await Test("f(x):=(return(x+1);x+2);f(3)", 4);
			await Test("return(1);2", 1);

			await Test("exists(error('hej'))", false);
			await Test("exists(Exception('hej'))", false);

			await Test("print('hej')", "hej");
			await Test("printline('hej')", "hej");
			await Test("println('hej')", "hej");

			await Test("x:=10;remove(x);exists(x)", false);
			await Test("x:=10;destroy(x);exists(x)", false);
			await Test("x:=10;delete(x);exists(x)", false);
			await Test("Create(System.String,'-',80)", new string('-', 80));
			await Test("Create(List, System.String).GetType()", typeof(List<string>));
		}

		[TestMethod]
		public async Task Evaluation_Test_40_Strings()
		{
			await Test("Evaluate('a+b')", a + b);
			await Test("Eval('a+b')", a + b);
			await Test("Parse('a+b')", new Expression("a+b"));
			await Test("IsEmpty('a+b')", false);
			await Test("Empty('')", true);
			await Test("IsNotEmpty('a+b')", true);
			await Test("NotEmpty('')", false);
			await Test("Left('Hello',3)", "Hel");
			await Test("Right('Hello',3)", "llo");
			await Test("Length('Hello')", 5);
			await Test("Len('Hello')", 5);
			await Test("Mid('Hello',2,2)", "ll");
			await Test("StartsWith('Hello','He')", true);
			await Test("StartsWith('World','He')", false);
			await Test("EndsWith('Hello','lo')", true);
			await Test("EndsWith('World','lo')", false);
			await Test("Contains('Hello','el')", true);
			await Test("Contains('World','el')", false);
			await Test("IndexOf('Hello','el')", 1);
			await Test("IndexOf('World','el')", -1);
			await Test("LastIndexOf('Hello','el')", 1);
			await Test("LastIndexOf('World','el')", -1);
			await Test("Split('Hello World','l')", new string[] { "He", string.Empty, "o Wor", "d" });
			await Test("UpperCase('Hello')", "HELLO");
			await Test("LowerCase('Hello')", "hello");
			await Test("Trim(' Hello ')", "Hello");
			await Test("TrimStart(' Hello ')", "Hello ");
			await Test("TrimEnd(' Hello ')", " Hello");
			await Test("PadLeft('Hello',10)", "     Hello");
			await Test("PadRight('Hello',10)", "Hello     ");
		}

		[TestMethod]
		public async Task Evaluation_Test_41_DateTime()
		{
			await Test("DateTime(2016,03,05)", new DateTime(2016, 03, 05));
			await Test("DateTime(2016,03,05,19,15,01)", new DateTime(2016, 03, 05, 19, 15, 01));
			await Test("DateTime(2016,03,05,19,15,01,123)", new DateTime(2016, 03, 05, 19, 15, 01, 123));

			await Test("TimeSpan(19,22,01)", new TimeSpan(19, 22, 01));
			await Test("TimeSpan(19,22,01,123)", new TimeSpan(19, 22, 01, 123));
		}

		[TestMethod]
		public async Task Evaluation_Test_42_Units()
		{
			await Test("10 m", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			await Test("10 km", new PhysicalQuantity(10, new Unit(Prefix.Kilo, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			await Test("10 mm", new PhysicalQuantity(10, new Unit(Prefix.Milli, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			await Test("10 m^2", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2))));
			await Test("10 m^3", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 3))));
			await Test("10 m²", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2))));
			await Test("10 m³", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 3))));
			await Test("10 W⋅s", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("W"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), 1))));
			await Test("10 W*s", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("W"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), 1))));
			await Test("10 m⋅s^-1", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -1))));
			await Test("10 m/s", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -1))));
			await Test("10 m^2/s", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -1))));
			await Test("10 m/s^2", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -2))));
			await Test("10 kg⋅m²/(A⋅s³)", new PhysicalQuantity(10, new Unit(Prefix.Kilo, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("g"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("A"), -1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -3))));

			await Test("10 m + 2 km", new PhysicalQuantity(2010, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			await Test("10 m < 2 km", true);
			await Test("2 km < 10 m", false);
			await Test("2 km - 10 m", new PhysicalQuantity(1.99, new Unit(Prefix.Kilo, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			await Test("2 km * 10 m", new PhysicalQuantity(20, new Unit(Prefix.Kilo, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2))));
			await Test("2 km² / 10 m", new PhysicalQuantity(0.2, new Unit(Prefix.Kilo, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			await Test("10 m / 2 s", new PhysicalQuantity(5, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -1))));

			await Test("10 km m", new PhysicalQuantity(10000, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			await Test("10*sin(pi/6) m", new PhysicalQuantity(10 * Math.Sin(Math.PI / 6), new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			await Test("sin(10 W)", Math.Sin(10));
			await Test("10 °C > 20 °F", true);
			await Test("10 m² > 1000 inch²", true);

			await Test("10 kWh", new PhysicalQuantity(10, new Unit(Prefix.Kilo, "W", "h")));
			await Test("10 mph", new PhysicalQuantity(10, new Unit(Prefix.None, "SM", "h")));
			await Test("10 fps", new PhysicalQuantity(10, new Unit(Prefix.None, "ft", "s")));
			await Test("10 ft", new PhysicalQuantity(10, new Unit("ft")));

			await Test("10 kWh J", new PhysicalQuantity(36000000, new Unit(Prefix.None, "J")));
			await Test("10 kWh kJ", new PhysicalQuantity(36000, new Unit(Prefix.Kilo, "J")));
			await Test("10 kWh MJ", new PhysicalQuantity(36, new Unit(Prefix.Mega, "J")));

			await Test("10 V / 2 A = 5 Ohm", true);

			// TODO: Difference of temperature units -> K
			// TODO: (km)² != km²=k(m²)
			// TODO: Test all units.
		}

		[TestMethod]
		public async Task Evaluation_Test_43_MethodCall()
		{
			await Test("DateTime(2016,3,11).AddDays(10)", new DateTime(2016, 3, 21));
			await Test("DateTime(2016,3,11).AddDays(1..3)", new DateTime[] { new DateTime(2016, 3, 12), new DateTime(2016, 3, 13), new DateTime(2016, 3, 14) });
		}

		[TestMethod]
		public async Task Evaluation_Test_44_NullCheckSuffixOperators()
		{
			await Test("null?.Test", null);
			await Test("f?(1,2,3)", null);
			await Test("null?[]", null);
			await Test("null?{}", null);
			await Test("null?[i]", null);
			await Test("null?[x,y]", null);
			await Test("null?[x,]", null);
			await Test("null?[,y]", null);
			await Test("null?[,]", null);
		}

		[TestMethod]
		public async Task Evaluation_Test_55_ImplicitSetNotation()
		{
			await Test("S:={x in Z:x>10};5 in S", false);
			await Test("S:={x in Z:x>10};15 in S", true);
			await Test("S:={x in Z:x>10};25 in S", true);
			await Test("S:={x in Z:x>10};S2:={x in S:x<20};5 in S2", false);
			await Test("S:={x in Z:x>10};S2:={x in S:x<20};15 in S2", true);
			await Test("S:={x in Z:x>10};S2:={x in S:x<20};25 in S2", false);
			await Test("S:=1..20;S2:={x in S:x>10};5 in S2", false);
			await Test("S:=1..20;S2:={x in S:x>10};15 in S2", true);
			await Test("S:=1..20;S2:={x in S:x>10};25 in S2", false);
			await Test("S:={[a,b]: a>b};[1,2] in S", false);
			await Test("S:={[a,b]: a>b};[2,1] in S", true);
			await Test("S:={[a,b]: a>b};[2,1,0] in S", false);
			await Test("S:={[a,b]: a in Z, b in Z, a>b};[1,2] in S", false);
			await Test("S:={[a,b]: a in Z, b in Z, a>b};[2,1] in S", true);
			await Test("S:={[a,b]: a in Z, b in Z, a>b};[2.1,1] in S", false);
			await Test("S:={[a,b]: a in Z, b in Z, a>b};[2,1,0] in S", false);
			await Test("S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104};count(S)", 40);
			await Test("S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104};[2,13,104] in S", true);
			await Test("S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104};[6,13,104] in S", false);
			await Test("S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104};[2,16,104] in S", false);
			await Test("S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104};[2,13,114] in S", false);
			await Test("S:={v[]:count(v)>3};[1,2] in S", false);
			await Test("S:={v[]:count(v)>3};[1,2,3] in S", false);
			await Test("S:={v[]:count(v)>3};[1,2,3,4] in S", true);
			await Test("S:={s{}:count(s)>3};{1,2} in S", false);
			await Test("S:={s{}:count(s)>3};{1,2,3} in S", false);
			await Test("S:={s{}:count(s)>3};{1,2,3,4} in S", true);
			await Test("S:={M[,]:M.Columns>M.Rows};[[1,2],[3,4]] in S", false);
			await Test("S:={M[,]:M.Columns>M.Rows};[[1,2,3],[3,4,4]] in S", true);
			await Test("S:={M[,]:M.Columns>M.Rows};[[1,2],[3,4],[5,6]] in S", false);
			await Test("S:={x::x>\"Hello\"};\"ABC\" in S", false);
			await Test("S:={x::x>\"Hello\"};\"XYZ\" in S", true);
			await Test("v:=[1,2,3,4,5,6,7,8,9,10];{x in v:x>5}[]", new double[] { 6, 7, 8, 9, 10 });
		}

		[TestMethod]
		public async Task Evaluation_Test_56_ImplicitVectorNotation()
		{
			await Test("v:=[1,2,3,4,5,6,7,8,9,10];[x in v:x>5]", new double[] { 6, 7, 8, 9, 10 });
			await Test("v:=1..100;[x in v:floor(sqrt(x))^2=x]", new double[] { 1, 4, 9, 16, 25, 36, 49, 64, 81, 100 });
			await Test("X:=1..10;P:=[x^2:x in X]", new double[] { 1, 4, 9, 16, 25, 36, 49, 64, 81, 100 });
		}

		[TestMethod]
		public async Task Evaluation_Test_57_ImplicitMatrixNotation()
		{
			await Test("v:=1..100;[[x,y]:x in v,(y:=floor(sqrt(x)))^2=x]", new double[,] { { 1, 1 }, { 4, 2 }, { 9, 3 }, { 16, 4 }, { 25, 5 }, { 36, 6 }, { 49, 7 }, { 64, 8 }, { 81, 9 }, { 100, 10 } });
			await Test("X:=1..2;Y:=5..7;P:=[[x,y]:x in X, y in Y]", new double[,] { { 1, 5 }, { 2, 5 }, { 1, 6 }, { 2, 6 }, { 1, 7 }, { 2, 7 } });
			await Test("M:=Identity(2);[Reverse(Row):Row in M]", new double[,] { { 0, 1 }, { 1, 0 } });
		}

		[TestMethod]
		public async Task Evaluation_Test_58_Measurements()
		{
			await Test("10 m +- 1cm", new Measurement(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1)), 0.01));

			await Test("(10 m +- 1cm) + (2 km +- 10m)", new Measurement(2010, new Unit(Prefix.None, "m"), 10.01));
			await Test("(10 m +- 1cm) * (2 s +- 100ms)", new Measurement(20, new Unit(Prefix.None, "m", "s"), 1.02));
			await Test("(10 m +- 1cm) / (2 s +- 100ms)", new Measurement(5, new Unit(Prefix.None, new KeyValuePair<string, int>("m", 1), new KeyValuePair<string, int>("s", -1)), 0.255));
		}

		[TestMethod]
		public async Task Evaluation_Test_59_NamedOperators()
		{
			await Test("DateTime(2000,1,2)+TimeSpan(1,2,3)", new DateTime(2000, 1, 2, 1, 2, 3));
			await Test("DateTime(2000,1,2)+TimeSpan(1,2,3)", new DateTime(2000, 1, 2, 1, 2, 3));

			await Test("DateTime(2000,1,2)+Duration('PT1H')", new DateTime(2000, 1, 2, 1, 0, 0));
			await Test("DateTime(2000,1,2)+Duration('PT1H')", new DateTime(2000, 1, 2, 1, 0, 0));

			await Test("DateTime(2000,1,2)-TimeSpan(1,2,3)", new DateTime(2000, 1, 1, 22, 57, 57));
			await Test("DateTime(2000,1,2)-TimeSpan(1,2,3)", new DateTime(2000, 1, 1, 22, 57, 57));

			await Test("DateTime(2000,1,2)-Duration('PT1H')", new DateTime(2000, 1, 1, 23, 0, 0));
			await Test("DateTime(2000,1,2)-Duration('PT1H')", new DateTime(2000, 1, 1, 23, 0, 0));
		}

		[TestMethod]
		public async Task Evaluation_Test_60_ScalarMultiplication()
		{
			await Test("Duration('P1D')*1000", Duration.FromDays(1000));
			await Test("1000*Duration('P1D')", Duration.FromDays(1000));
			await Test("Duration('P1D')*-1000", Duration.FromDays(-1000));
			await Test("(-1000)*Duration('P1D')", Duration.FromDays(-1000));
			await Test("Duration('P1D')*#1000", Duration.FromDays(1000));
			await Test("#1000*Duration('P1D')", Duration.FromDays(1000));
			await Test("Duration('P1D')*-#1000", Duration.FromDays(-1000));
			await Test("(-#1000)*Duration('P1D')", Duration.FromDays(-1000));
		}

		[TestMethod]
		public async Task Evaluation_Test_61_Remove()
		{
			Dictionary<string, IElement> Obj = new Dictionary<string, IElement>()
			{
				{ "Member1", new DoubleNumber(a) },
				{ "Member3", new DoubleNumber(c) }
			};

			await Test("Obj:={Member1:a, Member2:b, Member3:c};Remove(Obj.Member2);Obj", Obj);
			await Test("Obj:={Member1:a, Member2:b, Member3:c};Remove(Obj.Member2)", true);
			await Test("Remove(a);exists(a)", false);
			await Test("Remove(a)", a);
		}

		[TestMethod]
		public async Task Evaluation_Test_62_Duration()
		{
			await Test("Duration('PT10H30M')", new Duration(false, 0, 0, 0, 10, 30, 0));
			await Test("Duration(1,2,3,4,5,6)", new Duration(false, 1, 2, 3, 4, 5, 6));
			await Test("Duration(-1,-2,-3,-4,-5,-6)", new Duration(true, 1, 2, 3, 4, 5, 6));
			await Test("{'Duration':Duration(x)}:={'Duration':'PT10H30M'};x", new Duration(false, 0, 0, 0, 10, 30, 0));
		}

	}
}