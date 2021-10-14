using System;
using System.Numerics;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
using Waher.Script.Objects.Sets;
using Waher.Script.Units;

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

		public static void Test(string Script, object ExpectedValue)
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
			object Result = Exp.Evaluate(v);

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
				if (Result != null)
					Assert.Fail("Expected null: " + Script);
			}
			else if (!Expected.Equals(Result))
				Assert.Fail("Expected " + Expected.ToString() + ", but got " + Result.ToString() + ": " + Script);
		}

		[TestMethod]
		public void Evaluation_Test_01_Sequences()
		{
			Test("a;b;c", c);
		}

		[TestMethod]
		public void Evaluation_Test_02_ConditionalStatements()
		{
			Test("DO ++a WHILE a<=10", 11);
			Test("WHILE a<=10 DO ++a", 11);
			Test("WHILE a<=10 : ++a", 11);
			Test("FOREACH x IN [a,b,c] DO x", c);
			Test("FOREACH x IN [a,b,c] : x", c);
			Test("FOR EACH x IN [a,b,c] DO x", c);
			Test("FOR EACH x IN [a,b,c] : x", c);
			Test("FOR a:=1 TO 10 STEP 2 DO a", 9);
			Test("FOR a:=1 TO 10 STEP 2 : a", 9);
			Test("FOR a:=1 TO 10 DO a", 10);
			Test("FOR a:=1 TO 10 : a", 10);
			Test("TRY a CATCH b FINALLY a:=0;a", 0);
			Test("a:='a';TRY a++ CATCH a:=0;a", 0);
			Test("TRY a FINALLY a:=0;a", 0);
		}

		[TestMethod]
		public void Evaluation_Test_03_Lists()
		{
			Test("a,b,c", new double[] { a, b, c });
			Test("a,b,,c", new object[] { a, b, null, c });
		}

		[TestMethod]
		public void Evaluation_Test_04_Assignments()
		{
			Test("x:=10;x+x", 20);
			Test("a+=b;a", 11);
			Test("a-=b;a", -1);
			Test("a*=b;a", 30);
			Test("b/=2;b", 3);
			Test("a^=b;a", 15625);
			Test("a&=b;a", 4);
			Test("p&&=q;p", false);
			Test("a|=b;a", 7);
			Test("q||=p;q", true);
			Test("a<<=b;a", 320);
			Test("a>>=1;a", 2);

			Test("[x,y]:=[1,2];[a,b,c,x,y]", new double[] { 5, 6, 7, 1, 2 });
			Test("[[x,y],[y,z]]:=[[a,b],[b,c]];[a,b,c,x,y,z]", new double[] { 5, 6, 7, 5, 6, 7 });
			Test("{x,y,z}:={a,b,c};x+y+z", 18);

			Test("f(x):=x^2;f(10)", 100);
			Test("f(x,y):=x*y;f(2,3)", 6);

			Test("f(x,[y]):=x*y;f(2,3)", 6);
			Test("f(x,[y]):=x*y;f(2,[3,4,5])", new double[] { 6, 8, 10 });
			Test("f(x,[y]):=x*y;f(2,[[3,4],[5,6]])", new double[,] { { 6, 8 }, { 10, 12 } });
			Test("f(x,[y]):=x*y;f(2,{3,3,4,5})", new double[] { 6, 8, 10 });

			Test("f(y[]):=y.Length;f([[1,2],[3,4]])", new double[] { 2, 2 });
			Test("f(y[]):=y.Length;f([1,2,3])", 3);
			Test("f(y[]):=y.Length;f({1,2,3})", new double[] { 1 });
			Test("f(y[]):=y.Length;f(3)", 1);

			Test("f(y[,]):=y.Rows;f(3)", 1);
			Test("f(y[,]):=y.Rows;f([1,2,3])", 1);
			Test("f(y[,]):=y.Rows;f([[1,2],[3,4]])", 2);
			Test("f(y[,]):=y.Rows;f({1,2,3})", new double[] { 1 });

			Test("f(y{}):=y union {2,3};f(3)", new object[] { 3, 2 });
			Test("f(y{}):=y union {2,3};f([1,2,3])", new object[] { 1, 2, 3 });
			Test("f(y{}):=y union {2,3};f([[1,2],[3,4]])", new object[] { new object[] { 1, 2, 3 }, new object[] { 3, 4, 2 } });
			Test("f(y{}):=y union {2,3};f({1,2,3})", new object[] { 1, 2, 3 });

			Test("Obj:={m1:a,m2:b,m3:c};Obj.m2:=a+b+c;Obj.m2", 18);
			Test("v:=[1,2,3];v[1]:=a;v", new double[] { 1, 5, 3 });
			Test("M:=[[1,2],[3,4]];M[0,1]:=a;M", new double[,] { { 1, 2 }, { 5, 4 } });
			Test("M:=[[1,2],[3,4]];M[,1]:=[a,b];M", new double[,] { { 1, 2 }, { 5, 6 } });
			Test("M:=[[1,2],[3,4]];M[1,]:=[a,b];M", new double[,] { { 1, 5 }, { 3, 6 } });
			Test("Obj:={m1:a,m2:b,m3:c};s:='m';Obj.(s+'1'):=a+b+c;Obj.m1", 18);

			// TODO: Test dynamic index, and dynamic index assignment.
		}

		[TestMethod]
		public void Evaluation_Test_05_IF()
		{
			Test("IF a<b THEN a", a);
			Test("IF a>b THEN a", null);
			Test("IF a<b THEN a ELSE b", a);
			Test("IF a>b THEN a ELSE b", b);
			Test("a<b ? a", a);
			Test("a>b ? a", null);
			Test("a<b ? a : b", a);
			Test("a ?? b", a);
			Test("null ?? b", b);

			Test("IF b<=[a,b,c] THEN a ELSE b", new double[] { b, a, a });
			Test("b<=[a,b,c] ? [1,2,3] : [4,5,6]", new double[] { 4, 2, 3 });
		}

		[TestMethod]
		public void Evaluation_Test_06_Lambda()
		{
			Test("(x->x^2)(10)", 100);
			Test("((x,y)->x*y)(2,3)", 6);

			Test("((x,[y])->x*y)(2,3)", 6);
			Test("((x,[y])->x*y)(2,[3,4,5])", new double[] { 6, 8, 10 });
			Test("((x,[y])->x*y)(2,[[3,4],[5,6]])", new double[,] { { 6, 8 }, { 10, 12 } });
			Test("((x,[y])->x*y)(2,{3,3,4,5})", new double[] { 6, 8, 10 });

			Test("(y[]->y.Length)(3)", 1);
			Test("(y[]->y.Length)([1,2,3])", 3);
			Test("(y[]->y.Length)([[1,2],[3,4]])", new double[] { 2, 2 });
			Test("(y[]->y.Length)({1,2,3})", new double[] { 1 });

			Test("(y[,]->y.Rows)(3)", 1);
			Test("(y[,]->y.Rows)([1,2,3])", 1);
			Test("(y[,]->y.Rows)([[1,2],[3,4]])", 2);
			Test("(y[,]->y.Rows)({1,2,3})", new double[] { 1 });

			Test("(y{}->y union {2,3})(3)", new object[] { 3, 2 });
			Test("(y{}->y union {2,3})([1,2,3])", new object[] { 1, 2, 3 });
			Test("(y{}->y union {2,3})([[1,2],[3,4]])", new object[] { new object[] { 1, 2, 3 }, new object[] { 3, 4, 2 } });
			Test("(y{}->y union {2,3})({1,2,3})", new object[] { 1, 2, 3 });
		}

		[TestMethod]
		public void Evaluation_Test_07_Implication()
		{
			Test("p => q", false);
			Test("q => p", true);

			Test("[p,q,r] => q", new bool[] { false, true, false });
			Test("p=>[p,q,r]", new bool[] { true, false, true });
			Test("[p,r,q]=>[p,q,r]", new bool[] { true, false, true });

			Test("[[p,q,r],[q,r,p]] => q", new bool[,] { { false, true, false }, { true, false, false } });
			Test("p=>[[p,q,r],[q,r,p]]", new bool[,] { { true, false, true }, { false, true, true } });
			Test("[[p,r,q],[r,q,p]]=>[[p,q,r],[q,r,p]]", new bool[,] { { true, false, true }, { false, true, true } });
		}

		[TestMethod]
		public void Evaluation_Test_08_Equivalence()
		{
			Test("p <=> q", false);

			Test("[p,q,r] <=> q", new bool[] { false, true, false });
			Test("p <=> [p,q,r]", new bool[] { true, false, true });
			Test("[p,r,q] <=> [p,q,r]", new bool[] { true, false, false });

			Test("[[p,q,r],[q,r,p]] <=> q", new bool[,] { { false, true, false }, { true, false, false } });
			Test("p <=> [[p,q,r],[q,r,p]]", new bool[,] { { true, false, true }, { false, true, true } });
			Test("[[p,r,q],[r,q,p]] <=> [[p,q,r],[q,r,p]]", new bool[,] { { true, false, false }, { false, false, true } });
		}

		[TestMethod]
		public void Evaluation_Test_09_OR()
		{
			Test("p || q", true);
			Test("q || p", true);
			Test("[p,q,r] || q", new bool[] { true, false, true });
			Test("p || [p,q,r]", new bool[] { true, true, true });
			Test("[p,r,q] || [p,q,r]", new bool[] { true, true, true });

			Test("a | b", 7);
			Test("b | a", 7);
			Test("[a,b,c] | b", new double[] { 7, 6, 7 });
			Test("a | [a,b,c]", new double[] { 5, 7, 7 });
			Test("[a,c,b] | [a,b,c]", new double[] { 5, 7, 7 });

			Test("p OR q", true);
			Test("q OR p", true);
			Test("[p,q,r] OR q", new bool[] { true, false, true });
			Test("p OR [p,q,r]", new bool[] { true, true, true });
			Test("[p,r,q] OR [p,q,r]", new bool[] { true, true, true });

			Test("a OR b", 7);
			Test("b OR a", 7);
			Test("[a,b,c] OR b", new double[] { 7, 6, 7 });
			Test("a OR [a,b,c]", new double[] { 5, 7, 7 });
			Test("[a,c,b] OR [a,b,c]", new double[] { 5, 7, 7 });

			Test("p XOR q", true);
			Test("q XOR p", true);
			Test("[p,q,r] XOR q", new bool[] { true, false, true });
			Test("p XOR [p,q,r]", new bool[] { false, true, false });
			Test("[p,r,q] XOR [p,q,r]", new bool[] { false, true, true });

			Test("a XOR b", 3);
			Test("b XOR a", 3);
			Test("[a,b,c] XOR b", new double[] { 3, 0, 1 });
			Test("a XOR [a,b,c]", new double[] { 0, 3, 2 });
			Test("[a,c,b] XOR [a,b,c]", new double[] { 0, 1, 1 });

			Test("p NOR q", false);
			Test("q NOR p", false);
			Test("[p,q,r] NOR q", new bool[] { false, true, false });
			Test("p NOR [p,q,r]", new bool[] { false, false, false });
			Test("[p,r,q] NOR [p,q,r]", new bool[] { false, false, false });

			Test("a NOR b", unchecked((ulong)-8));
			Test("b NOR a", unchecked((ulong)-8));
			Test("[a,b,c] NOR b", new double[] { unchecked((ulong)-8), unchecked((ulong)-7), unchecked((ulong)-8) });
			Test("a NOR [a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-8), unchecked((ulong)-8) });
			Test("[a,c,b] NOR [a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-8), unchecked((ulong)-8) });

			Test("p XNOR q", false);
			Test("q XNOR p", false);
			Test("[p,q,r] XNOR q", new bool[] { false, true, false });
			Test("p XNOR [p,q,r]", new bool[] { true, false, true });
			Test("[p,r,q] XNOR [p,q,r]", new bool[] { true, false, false });

			Test("a XNOR b", unchecked((ulong)-4));
			Test("b XNOR a", unchecked((ulong)-4));
			Test("[a,b,c] XNOR b", new double[] { unchecked((ulong)-4), unchecked((ulong)-1), unchecked((ulong)-2) });
			Test("a XNOR [a,b,c]", new double[] { unchecked((ulong)-1), unchecked((ulong)-4), unchecked((ulong)-3) });
			Test("[a,c,b] XNOR [a,b,c]", new double[] { unchecked((ulong)-1), unchecked((ulong)-2), unchecked((ulong)-2) });
		}

		[TestMethod]
		public void Evaluation_Test_10_AND()
		{
			Test("p && q", false);
			Test("q && p", false);
			Test("[p,q,r] && q", new bool[] { false, false, false });
			Test("p && [p,q,r]", new bool[] { true, false, true });
			Test("[p,r,q] && [p,q,r]", new bool[] { true, false, false });

			Test("a & b", 4);
			Test("b & a", 4);
			Test("[a,b,c] & b", new double[] { 4, 6, 6 });
			Test("a & [a,b,c]", new double[] { 5, 4, 5 });
			Test("[a,c,b] & [a,b,c]", new double[] { 5, 6, 6 });

			Test("p AND q", false);
			Test("q AND p", false);
			Test("[p,q,r] AND q", new bool[] { false, false, false });
			Test("p AND [p,q,r]", new bool[] { true, false, true });
			Test("[p,r,q] AND [p,q,r]", new bool[] { true, false, false });

			Test("a AND b", 4);
			Test("b AND a", 4);
			Test("[a,b,c] AND b", new double[] { 4, 6, 6 });
			Test("a AND [a,b,c]", new double[] { 5, 4, 5 });
			Test("[a,c,b] AND [a,b,c]", new double[] { 5, 6, 6 });

			Test("p NAND q", true);
			Test("q NAND p", true);
			Test("[p,q,r] NAND q", new bool[] { true, true, true });
			Test("p NAND [p,q,r]", new bool[] { false, true, false });
			Test("[p,r,q] NAND [p,q,r]", new bool[] { false, true, true });

			Test("a NAND b", unchecked((ulong)-5));
			Test("b NAND a", unchecked((ulong)-5));
			Test("[a,b,c] NAND b", new double[] { unchecked((ulong)-5), unchecked((ulong)-7), unchecked((ulong)-7) });
			Test("a NAND [a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-5), unchecked((ulong)-6) });
			Test("[a,c,b] NAND [a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-7), unchecked((ulong)-7) });
		}

		[TestMethod]
		public void Evaluation_Test_11_Membership()
		{
			Test("a AS System.Double", a);
			Test("a AS System.Int32", null);
			Test("[a,b] AS System.Double", null);
			Test("a AS [System.Double,System.Int32]", new object[] { a, null });

			Test("a IS System.Double", true);
			Test("a IS System.Int32", false);
			Test("[a,b] IS System.Double", false);
			Test("a IS [System.Double,System.Int32]", new bool[] { true, false });

			Test("a IN R", true);
			Test("a IN EmptySet", false);
			Test("[a,b] IN R", false);
			Test("a IN {a,b,c}", true);
			Test("a IN [a,b,c]", true);

			Test("a NOT IN R", false);
			Test("a NOTIN EmptySet", true);
			Test("[a,b] NOT IN R", true);
			Test("a NOTIN {a,b,c}", false);
			Test("a NOTIN [a,b,c]", false);
		}

		[TestMethod]
		public void Evaluation_Test_12_Comparison()
		{
			Test("a <= b", true);
			Test("a <= [a,b,c]", new bool[] { true, true, true });
			Test("[a,b,c] <= b", new bool[] { true, true, false });
			Test("[a,b,c] <= [a,c,b]", new bool[] { true, true, false });

			Test("a < b", true);
			Test("a >= b", false);
			Test("a > b", false);
			Test("a = b", false);
			Test("a == b", false);
			Test("a === b", false);
			Test("a <> b", true);
			Test("a != b", true);
			Test("s LIKE 'H.*'", true);
			Test("s NOT LIKE 'Bye.*'", true);
			Test("s NOTLIKE 'Bye.*'", true);
			Test("s UNLIKE 'Bye.*'", true);
			Test("if s LIKE \"H(?'Rest'.*)\" then Rest", "ello");
			Test("a .= b", false);
			Test("a .== b", false);
			Test("a .=== b", false);
			Test("a .<> b", true);
			Test("a .!= b", true);
			Test("a <= b <= c", true);
			Test("a <= b < c", true);
			Test("a < b <= c", true);
			Test("a < b < c", true);
			Test("a >= b >= c", false);
			Test("a > b >= c", false);
			Test("a >= b > c", false);
			Test("a > b > c", false);
		}

		[TestMethod]
		public void Evaluation_Test_13_Shift()
		{
			Test("a << b", 320);
			Test("[1,2,3] << b", new double[] { 64, 128, 192 });

			Test("7 >> 1", 3);
			Test("[7,8,9] >> 1", new double[] { 3, 4, 4 });
		}

		[TestMethod]
		public void Evaluation_Test_14_Union()
		{
			Test("{a,b,c} UNION {4,5,6}", new object[] { 5, 6, 7, 4 });
			Test("{a,b,c} ∪ {4,5,6}", new object[] { 5, 6, 7, 4 });
		}

		[TestMethod]
		public void Evaluation_Test_15_Intersection()
		{
			Test("{4,5,6} INTERSECT {a,b,c}", new object[] { 5, 6 });
			Test("{4,5,6} INTERSECTION {a,b,c}", new object[] { 5, 6 });
			Test("{4,5,6} ∩ {a,b,c}", new object[] { 5, 6 });
		}

		[TestMethod]
		public void Evaluation_Test_16_Intervals()
		{
			Test("1..10", new Objects.Sets.Interval(1, 10, true, true, null));
			Test("1..10|0.1", new Objects.Sets.Interval(1, 10, true, true, 0.1));
		}

		[TestMethod]
		public void Evaluation_Test_17_Terms()
		{
			Test("a+b", 11);
			Test("[1,2,3]+a", new double[] { 6, 7, 8 });
			Test("a+[1,2,3]", new double[] { 6, 7, 8 });
			Test("[1,2,3]+[a,b,c]", new double[] { 6, 8, 10 });

			Test("s+' '+t", "Hello Bye");
			Test("[1,2,3]+s", new object[] { "1Hello", "2Hello", "3Hello" });
			Test("s+[1,2,3]", new object[] { "Hello1", "Hello2", "Hello3" });
			Test("[1,2,3]+[s,t,s]", new object[] { "1Hello", "2Bye", "3Hello" });
			Test("[s,t,s]+[1,2,3]", new object[] { "Hello1", "Bye2", "Hello3" });
			Test("['a','b','c']+[s,t,s]", new object[] { "aHello", "bBye", "cHello" });
			Test("[s,t,s]+['a','b','c']", new object[] { "Helloa", "Byeb", "Helloc" });

			Test("10-a", 5);
			Test("[10,20,30]-a", new double[] { 5, 15, 25 });
			Test("[5,6,7]-[a,b,c]", new double[] { 0, 0, 0 });
			Test("[[10,20],[30,40]]-a", new double[,] { { 5, 15 }, { 25, 35 } });
			Test("a-[[1,1],[0,1]]", new double[,] { { 4, 4 }, { 5, 4 } });
			Test("[[a,b],[b,c]]-[[1,1],[0,1]]", new double[,] { { 4, 5 }, { 6, 6 } });

			Test("a.+b", 11);
			Test("a.+[a,b,c]", new double[] { 10, 11, 12 });
			Test("[a,b,c].+a", new double[] { 10, 11, 12 });
			Test("[a,b,c].+[a,b,c]", new double[] { 10, 12, 14 });
			Test("[[a,b],[b,c]].+[[a,b],[b,c]]", new double[,] { { 10, 12 }, { 12, 14 } });

			Test("10.-a", 5);
			Test("[10,20,30].-a", new double[] { 5, 15, 25 });
			Test("[5,6,7].-[a,b,c]", new double[] { 0, 0, 0 });
			Test("[[10,20],[30,40]].-a", new double[,] { { 5, 15 }, { 25, 35 } });
			Test("b.-[[1,2],[2,1]]", new double[,] { { 5, 4 }, { 4, 5 } });
			Test("[[a,b],[b,c]].-[[1,2],[2,1]]", new double[,] { { 4, 4 }, { 4, 6 } });

			Test("z1+2+3*i", new Complex(3, 5));
			Test("z1+z2", new Complex(4, 6));
			Test("z1-z2", new Complex(-2, -2));
			Test("z1+2", new Complex(3, 2));
			Test("2+z1", new Complex(3, 2));

			Test("[z1,z2]+2", new Complex[] { new Complex(3, 2), new Complex(5, 4) });
			Test("2+[z1,z2]", new Complex[] { new Complex(3, 2), new Complex(5, 4) });
			Test("[z1,z2]+z2", new Complex[] { new Complex(4, 6), new Complex(6, 8) });
			Test("z2+[z1,z2]", new Complex[] { new Complex(4, 6), new Complex(6, 8) });
			Test("[z1,z2]+[1,2]", new Complex[] { new Complex(2, 2), new Complex(5, 4) });
			Test("[1,2]+[z1,z2]", new Complex[] { new Complex(2, 2), new Complex(5, 4) });
			Test("[z1,z2]+[1,(2,3)]", new Complex[] { new Complex(2, 2), new Complex(5, 7) });
			Test("[1,(2,3)]+[z1,z2]", new Complex[] { new Complex(2, 2), new Complex(5, 7) });
			Test("[z1,z2]+[z1,z2]", new Complex[] { new Complex(2, 4), new Complex(6, 8) });

			Test("[[z1,z2],[z2,z1]]+2", new Complex[,] { { new Complex(3, 2), new Complex(5, 4) }, { new Complex(5, 4), new Complex(3, 2) } });
			Test("2+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(3, 2), new Complex(5, 4) }, { new Complex(5, 4), new Complex(3, 2) } });
			Test("[[z1,z2],[z2,z1]]+z2", new Complex[,] { { new Complex(4, 6), new Complex(6, 8) }, { new Complex(6, 8), new Complex(4, 6) } });
			Test("z2+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(4, 6), new Complex(6, 8) }, { new Complex(6, 8), new Complex(4, 6) } });
			Test("[[z1,z2],[z2,z1]]+[[1,2],[3,4]]", new Complex[,] { { new Complex(2, 2), new Complex(5, 4) }, { new Complex(6, 4), new Complex(5, 2) } });
			Test("[[1,2],[3,4]]+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(2, 2), new Complex(5, 4) }, { new Complex(6, 4), new Complex(5, 2) } });
			Test("[[z1,z2],[z2,z1]]+[[1,2],[3,(2,3)]]", new Complex[,] { { new Complex(2, 2), new Complex(5, 4) }, { new Complex(6, 4), new Complex(3, 5) } });
			Test("[[1,2],[3,(2,3)]]+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(2, 2), new Complex(5, 4) }, { new Complex(6, 4), new Complex(3, 5) } });
			Test("[[z1,z2],[z2,z1]]+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(2, 4), new Complex(6, 8) }, { new Complex(6, 8), new Complex(2, 4) } });
		}

		[TestMethod]
		public void Evaluation_Test_18_Factors()
		{
			Test("a*b", 30);
			Test("[1,2,3]*a", new double[] { 5, 10, 15 });
			Test("a*[1,2,3]", new double[] { 5, 10, 15 });
			Test("[1,2,3]*[a,b,c]", new double[] { 5, 12, 21 });

			Test("10/a", 2);
			Test("[10,20,30]/a", new double[] { 2, 4, 6 });
			Test("[5,6,7]/[a,b,c]", new double[] { 1, 1, 1 });
			Test("[[10,20],[30,40]]/a", new double[,] { { 2, 4 }, { 6, 8 } });
			Test("a/[[1,1],[0,1]]", new double[,] { { 5, -5 }, { 0, 5 } });
			Test("[[a,b],[b,c]]/[[1,1],[0,1]]", new double[,] { { 5, 1 }, { 6, 1 } });

			Test("a\\10", 2);
			Test("a\\[10,20,30]", new double[] { 2, 4, 6 });
			Test("[a,b,c]\\[5,6,7]", new double[] { 1, 1, 1 });
			Test("a\\[[10,20],[30,40]]", new double[,] { { 2, 4 }, { 6, 8 } });
			Test("[[1,1],[0,1]]\\a", new double[,] { { 5, -5 }, { 0, 5 } });
			Test("[[1,1],[0,1]]\\[[a,b],[b,c]]", new double[,] { { -1, -1 }, { 6, 7 } });

			Test("{4,5,6}\\{a,b,c}", new object[] { 4 });

			Test("b MOD a", 1);
			Test("b MOD [a,b,c]", new double[] { 1, 0, 6 });
			Test("[a,b,c] MOD a", new double[] { 0, 1, 2 });
			Test("[a,b,c] MOD [b,c,a]", new double[] { 5, 6, 2 });

			Test("a.*b", 30);
			Test("a.*[a,b,c]", new double[] { 25, 30, 35 });
			Test("[a,b,c].*a", new double[] { 25, 30, 35 });
			Test("[a,b,c].*[a,b,c]", new double[] { 25, 36, 49 });
			Test("[[a,b],[b,c]].*[[a,b],[b,c]]", new double[,] { { 25, 36 }, { 36, 49 } });

			Test("10./a", 2);
			Test("[10,20,30]./a", new double[] { 2, 4, 6 });
			Test("[5,6,7]./[a,b,c]", new double[] { 1, 1, 1 });
			Test("[[10,20],[30,40]]./a", new double[,] { { 2, 4 }, { 6, 8 } });
			Test("b./[[1,2],[2,1]]", new double[,] { { 6, 3 }, { 3, 6 } });
			Test("[[a,b],[b,c]]./[[1,2],[2,1]]", new double[,] { { 5, 3 }, { 3, 7 } });

			Test("a.\\10", 2);
			Test("a.\\[10,20,30]", new double[] { 2, 4, 6 });
			Test("[a,b,c].\\[5,6,7]", new double[] { 1, 1, 1 });
			Test("a.\\[[10,20],[30,40]]", new double[,] { { 2, 4 }, { 6, 8 } });
			Test("[[1,2],[2,1]].\\b", new double[,] { { 6, 3 }, { 3, 6 } });
			Test("[[1,2],[2,1]].\\[[a,b],[b,c]]", new double[,] { { 5, 3 }, { 3, 7 } });

			Test("b .MOD a", 1);
			Test("b .MOD [a,b,c]", new double[] { 1, 0, 6 });
			Test("[a,b,c] .MOD a", new double[] { 0, 1, 2 });
			Test("[a,b,c] .MOD [b,c,a]", new double[] { 5, 6, 2 });

			Test("[1,2,3] DOT [a,b,c]", 38);
			Test("[1,2,3] CROSS [a,b,c]", new double[] { -4, 8, -4 });
			Test("[1,2,3] CARTESIAN [a,b,c]", new object[]
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
		public void Evaluation_Test_18_Powers()
		{
			Test("a^b", Math.Pow(5, 6));
			Test("a²", 25);
			Test("a³", 125);

			Test("[[a,1],[1,b]]²", new double[,] { { a * a + 1, a + b }, { a + b, 1 + b * b } });
			Test("[[a,1],[1,b]]^2", new double[,] { { a * a + 1, a + b }, { a + b, 1 + b * b } });

			Test("a.^b", Math.Pow(5, 6));
			Test("[[a,1],[1,b]].^2", new double[,] { { a * a, 1 }, { 1, b * b } });
		}

		[TestMethod]
		public void Evaluation_Test_19_UnaryPrefixOperators()
		{
			Test("++a", 6);
			Test("d:=[a,b,c];++d", new double[] { 6, 7, 8 });
			Test("M:=[[a,b,c],[b,c,a],[c,a,b]];++M", new double[,] { { 6, 7, 8 }, { 7, 8, 6 }, { 8, 6, 7 } });

			Test("--a", 4);
			Test("d:=[a,b,c];--d", new double[] { 4, 5, 6 });
			Test("M:=[[a,b,c],[b,c,a],[c,a,b]];--M", new double[,] { { 4, 5, 6 }, { 5, 6, 4 }, { 6, 4, 5 } });

			Test("+a", 5);
			Test("+[a,b,c]", new double[] { 5, 6, 7 });

			Test("-a", -5);
			Test("-[a,b,c]", new double[] { -5, -6, -7 });

			Test("!p", false);
			Test("![p,q,r]", new bool[] { false, true, false });

			Test("NOT q", true);
			Test("NOT [p,q,r]", new bool[] { false, true, false });

			Test("~a", (double)unchecked((ulong)-6));
			Test("~[a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-7), unchecked((ulong)-8) });
		}

		[TestMethod]
		public void Evaluation_Test_20_UnarySuffixOperators()
		{
			Test("[a++,a]", new double[] { 5, 6 });
			Test("[a--,a]", new double[] { 5, 4 });

			Test("a%", 0.05);
			Test("a‰", 0.005);
			Test("a‱", 0.0005);
			Test("a°", Math.PI / 36);

			/*
			Test("f'(x)");   TODO
			Test("f′(x)");   TODO
			Test("f\"(x)");   TODO
			Test("f″(x)");   TODO
			Test("f‴(x)");   TODO*/

			Test("[[a,a,a],[b,b,b],[c,c,c]]T", new double[,] { { a, b, c }, { a, b, c }, { a, b, c } });
			Test("[[a,a,a],[b,b,b],[c,c,c]]H", new double[,] { { a, b, c }, { a, b, c }, { a, b, c } });
			Test("[[a,a,a],[b,b,b],[c,c,c]]†", new double[,] { { a, b, c }, { a, b, c }, { a, b, c } });

			Test("a!", 120);
			Test("a!!", 15);
		}

		[TestMethod]
		public void Evaluation_Test_21_BinarySuffixOperators()
		{
			Test("Obj:={m1:a,m2:b,m3:c};Obj.m1", a);
			Test("[a,b,c].Length", 3);
			Test("Obj:={m1:a,m2:b,m3:c};Obj.['m1','m2']", new double[] { a, b });
			Test("Obj:={m1:a,m2:b,m3:c};s:='m';Obj.(s+'1')", a);

			Test("a[]", new double[] { a });
			Test("[a,b,c,a,b,c][]", new double[] { a, b, c, a, b, c });
			Test("{a,b,c,a,b,c}[]", new double[] { a, b, c });
			Test("[[a,b],[b,c]][]", new object[] { new double[] { a, b }, new double[] { b, c } });

			Test("a[,]", new double[,] { { a } });
			Test("[a,b,c][,]", new double[,] { { a, b, c } });
			Test("{a,b,c}[,]", new double[,] { { a, b, c } });
			Test("[[a,b],[b,c]][,]", new double[,] { { a, b }, { b, c } });

			Test("a{}", new double[] { a });
			Test("[a,b,c,a,b,c]{}", new double[] { a, b, c });
			Test("{a,b,c,a,b,c}{}", new double[] { a, b, c });
			Test("[[a,b],[b,c]]{}", new object[] { new double[] { a, b }, new double[] { b, c } });

			Test("[a,b,c][1]", b);
			Test("[a,b,c][1..2]", new double[] { b, c });

			Test("[[a,b],[c,a]][0,1]", c);
			Test("[[a,b],[c,a]][0,0..1]", new double[] { a, c });

			Test("[[a,b],[c,a]][0,]", new double[] { a, c });
			Test("[[a,b],[c,a]][0..1,]", new double[,] { { a, c }, { b, a } });

			Test("[[a,b],[c,a]][,0]", new double[] { a, b });
			Test("[[a,b],[c,a]][,0..1]", new double[,] { { a, b }, { c, a } });

			Test("System", new Namespace("System"));
			Test("System.Text", new Namespace("System.Text"));
			Test("System.Text.RegularExpressions", new Namespace("System.Text.RegularExpressions"));
			Test("System.Text.RegularExpressions.Regex", typeof(System.Text.RegularExpressions.Regex));
		}

		[TestMethod]
		public void Evaluation_Test_22_ObjectExNihilo()
		{
			Dictionary<string, IElement> Obj = new Dictionary<string, IElement>()
			{
				{ "Member1", new DoubleNumber(a) },
				{ "Member2", new DoubleNumber(b) },
				{ "Member3", new DoubleNumber(c) }
			};

			Test("{Member1:a, Member2:b, Member3:c}", Obj);

			Obj["Member1"] = new StringValue("Value1");
			Obj["Member2"] = new StringValue("Value2");
			Obj["Member3"] = new StringValue("Value3");

			Test("{\"Member1\":\"Value1\", \"Member2\":\"Value2\", \"Member3\":\"Value3\"}", Obj);
		}

		[TestMethod]
		public void Evaluation_Test_23_Sets()
		{
			Test("S:={1,2,3};", new object[] { 1, 2, 3 });
			Test("S:={DO a++ WHILE a<10};", new object[] { 5, 6, 7, 8, 9 });
			Test("S:={WHILE a<10 : a++};", new object[] { 5, 6, 7, 8, 9 });
			Test("S:={FOR x:=1 TO 20 STEP 3 : x};", new object[] { 1, 4, 7, 10, 13, 16, 19 });
			Test("S:={FOREACH x IN 1..10|2 : x^2};", new object[] { 1, 9, 25, 49, 81 });
			Test("S:={FOR EACH x IN 1..10|2 : x^2};", new object[] { 1, 9, 25, 49, 81 });
		}

		[TestMethod]
		public void Evaluation_Test_24_Matrices()
		{
			Test("[[a,b,c],[b,c,a]];", new double[,] { { a, b, c }, { b, c, a } });
			Test("[DO [a++,a++,a++] WHILE a<10];", new double[,] { { 5, 6, 7 }, { 8, 9, 10 } });
			Test("[WHILE a<10 : [a++,a++,a++]];", new double[,] { { 5, 6, 7 }, { 8, 9, 10 } });
			Test("[FOR y:=1 TO 3 : [FOR x:=1 TO 3 : x=y ? 1 : 0]];", new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
			Test("[FOREACH x IN 1..10|2 : [x,x+1,x+2]];", new double[,] { { 1, 2, 3 }, { 3, 4, 5 }, { 5, 6, 7 }, { 7, 8, 9 }, { 9, 10, 11 } });
			Test("[FOR EACH x IN 1..10|2 : [x,x+1,x+2]];", new double[,] { { 1, 2, 3 }, { 3, 4, 5 }, { 5, 6, 7 }, { 7, 8, 9 }, { 9, 10, 11 } });
		}

		[TestMethod]
		public void Evaluation_Test_25_Vectors()
		{
			Test("[a,b,c];", new double[] { a, b, c });
			Test("[DO a++ WHILE a<10];", new double[] { 5, 6, 7, 8, 9 });
			Test("[WHILE a<10 : a++];", new double[] { 5, 6, 7, 8, 9 });
			Test("[FOR x:=1 TO 20 STEP 3 : x];", new double[] { 1, 4, 7, 10, 13, 16, 19 });
			Test("[FOREACH x IN 1..10|2 : x^2];", new double[] { 1, 9, 25, 49, 81 });
			Test("[FOR EACH x IN 1..10|2 : x^2];", new double[] { 1, 9, 25, 49, 81 });
		}

		[TestMethod]
		public void Evaluation_Test_26_Parenthesis()
		{
			Test("a * (b + c)", 65);
		}

		[TestMethod]
		public void Evaluation_Test_27_null()
		{
			Test("null", null);
		}

		[TestMethod]
		public void Evaluation_Test_28_StringValues()
		{
			Test("\"Hello\\r\\n\\t\\f\\b\\a\\v\\\\\\\"\\''\"", "Hello\r\n\t\f\b\a\v\\\"\''");
			Test("'Hello\\r\\n\\t\\f\\b\\a\\v\\\\\\\"\\'\"'", "Hello\r\n\t\f\b\a\v\\\"\'\"");
		}

		[TestMethod]
		public void Evaluation_Test_29_BooleanValues()
		{
			Test("true", true);
			Test("false", false);
			Test("⊤", true);
			Test("⊥", false);
		}

		[TestMethod]
		public void Evaluation_Test_30_DoubleValues()
		{
			Test("1", 1);
			Test("3.1415927", 3.1415927);
			Test("1.23e-3", 1.23e-3);
		}

		[TestMethod]
		public void Evaluation_Test_31_Constants()
		{
			Test("e", Math.E);
			Test("pi", Math.PI);
			Test("π", Math.PI);
			Test("i", Complex.ImaginaryOne);
			Test("C", ComplexNumbers.Instance);
			Test("R", RealNumbers.Instance);
			Test("Z", Integers.Instance);
			Test("EmptySet", EmptySet.Instance);
			Test("∅", EmptySet.Instance);
			Test("Now.Date", DateTime.Now.Date);
			Test("Today", DateTime.Today);
			Test("ε", double.Epsilon);
			Test("eps", double.Epsilon);
			Test("epsilon", double.Epsilon);
			Test("∞", double.PositiveInfinity);
			Test("inf", double.PositiveInfinity);
			Test("infinity", double.PositiveInfinity);
		}

		[TestMethod]
		public void Evaluation_Test_32_BinomialCoefficients()
		{
			Test("c OVER a", 21);
			Test("8 OVER [0,1,2,3,4,5,6,7,8]", new double[] { 1, 8, 28, 56, 70, 56, 28, 8, 1 });
			Test("[0,1,2,3,4,5,6,7,8] OVER 0", new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
			Test("[0,1,2,3,4,5,6,7,8] OVER [0,1,2,3,4,5,6,7,8]", new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
		}

		[TestMethod]
		public void Evaluation_Test_33_ComplexNumbers()
		{
			Test("(1,2)", new Complex(1, 2));
			Test("1+2*i", new Complex(1, 2));
		}

		[TestMethod]
		public void Evaluation_Test_34_VectorFunctions()
		{
			Test("Min([2,4,1,3])", 1);
			Test("Max([2,4,1,3])", 4);
			Test("Sum([1,2,3,4,5])", 15);
			Test("Average([1,2,3,4,5])", 3);
			Test("Avg([1,2,3,4,5])", 3);
			Test("Product([1,2,3,4,5])", 120);
			Test("Prod([1,2,3,4,5])", 120);
			Test("Variance([1,2,3,4,5])", 2);
			Test("Var([1,2,3,4,5])", 2);
			Test("StandardDeviation([1,2,3,4,5])", Math.Sqrt(2.5));
			Test("StdDev([1,2,3,4,5])", Math.Sqrt(2.5));
			Test("Median([1,2,3,4,5])", 3);
			Test("And([true,false,true])", false);
			Test("And([3,2,1])", 0);
			Test("Or([true,false,true])", true);
			Test("Or([3,2,1])", 3);
			Test("Xor([true,false,true])", false);
			Test("Xor([3,2,1])", 0);
			Test("Nand([true,false,true])", true);
			Test("Nand([3,2,1])", 0xffffffffffffffff);
			Test("Nor([true,false,true])", false);
			Test("Nor([3,2,1])", unchecked((ulong)-4));
			Test("Xnor([true,false,true])", true);
			Test("Xnor([3,2,1])", 0xffffffffffffffff);
		}

		[TestMethod]
		public void Evaluation_Test_35_AnalyticFunctions()
		{
			Test("sin(5)", Math.Sin(5));
			Test("cos(5)", Math.Cos(5));
			Test("tan(5)", Math.Tan(5));
			Test("sec(5)", 1.0 / Math.Cos(5));
			Test("csc(5)", 1.0 / Math.Sin(5));
			Test("cot(5)", 1.0 / Math.Tan(5));

			Test("round(arcsin(sin(1))*1e6)/1e6", 1);
			Test("round(arccos(cos(1))*1e6)/1e6", 1);
			Test("round(arctan(tan(1))*1e6)/1e6", 1);
			Test("round(arcsec(sec(1))*1e6)/1e6", 1);
			Test("round(arccsc(csc(1))*1e6)/1e6", 1);
			Test("round(arccot(cot(1))*1e6)/1e6", 1);
			Test("atan(3,4)", Math.Atan2(4, 3));
			Test("arctan(3,4)", Math.Atan2(4, 3));

			Test("sin(i)", Complex.Sin(Complex.ImaginaryOne));
			Test("cos(i)", Complex.Cos(Complex.ImaginaryOne));
			Test("tan(i)", Complex.Tan(Complex.ImaginaryOne));
			Test("sec(i)", 1.0 / Complex.Cos(Complex.ImaginaryOne));
			Test("csc(i)", 1.0 / Complex.Sin(Complex.ImaginaryOne));
			Test("cot(i)", 1.0 / Complex.Tan(Complex.ImaginaryOne));

			Test("round(arcsin(sin(i))*1e6)/1e6", Complex.ImaginaryOne);
			Test("round(arccos(cos(i))*1e6)/1e6", Complex.ImaginaryOne);
			Test("round(arctan(tan(i))*1e6)/1e6", Complex.ImaginaryOne);
			Test("round(arcsec(sec(i))*1e6)/1e6", Complex.ImaginaryOne);
			Test("round(arccsc(csc(i))*1e6)/1e6", Complex.ImaginaryOne);
			Test("round(arccot(cot(i))*1e6)/1e6", Complex.ImaginaryOne);

			Test("sinh(5)", Math.Sinh(5));
			Test("cosh(5)", Math.Cosh(5));
			Test("tanh(5)", Math.Tanh(5));
			Test("sech(5)", 1.0 / Math.Cosh(5));
			Test("csch(5)", 1.0 / Math.Sinh(5));
			Test("coth(5)", 1.0 / Math.Tanh(5));

			Test("round(arcsinh(sinh(1))*1e6)/1e6", 1);
			Test("round(arccosh(cosh(1))*1e6)/1e6", 1);
			Test("round(arctanh(tanh(1))*1e6)/1e6", 1);
			Test("round(arcsech(sech(1))*1e6)/1e6", 1);
			Test("round(arccsch(csch(1))*1e6)/1e6", 1);
			Test("round(arccoth(coth(1))*1e6)/1e6", 1);

			Test("sinh(i)", Complex.Sinh(Complex.ImaginaryOne));
			Test("cosh(i)", Complex.Cosh(Complex.ImaginaryOne));
			Test("tanh(i)", Complex.Tanh(Complex.ImaginaryOne));
			Test("sech(i)", 1.0 / Complex.Cosh(Complex.ImaginaryOne));
			Test("csch(i)", 1.0 / Complex.Sinh(Complex.ImaginaryOne));
			Test("coth(i)", 1.0 / Complex.Tanh(Complex.ImaginaryOne));

			Test("round(arcsinh(sinh(i))*1e6)/1e6", Complex.ImaginaryOne);
			Test("round(arccosh(cosh(i))*1e6)/1e6", Complex.ImaginaryOne);
			Test("round(arctanh(tanh(i))*1e6)/1e6", Complex.ImaginaryOne);
			Test("round(arcsech(sech(i))*1e6)/1e6", Complex.ImaginaryOne);
			Test("round(arccsch(csch(i))*1e6)/1e6", Complex.ImaginaryOne);
			Test("round(arccoth(coth(i))*1e6)/1e6", Complex.ImaginaryOne);

			Test("exp(1)", Math.E);
			Test("ln(e)", 1);
			Test("log10(10)", 1);
			Test("lg(10)", 1);
			Test("log2(2)", 1);
			Test("sqrt(4)", 2);
		}

		[TestMethod]
		public void Evaluation_Test_36_ScalarFunctions()
		{
			Test("abs(-1)", 1);
			Test("ceil(pi)", 4);
			Test("ceiling(pi)", 4);
			Test("floor(pi)", 3);
			Test("round(pi)", 3);
			Test("sign(pi)", 1);

			Test("abs(i)", 1);
			Test("ceil(pi*i)", new Complex(0, 4));
			Test("ceiling(pi*i)", new Complex(0, 4));
			Test("floor(pi*i)", new Complex(0, 3));
			Test("round(pi*i)", new Complex(0, 3));
			Test("sign(pi*i)", new Complex(0, 1));

			Test("min(a,b)", a);
			Test("max(a,b)", b);

			Test("number(a)", a);
			Test("number(true)", 1);
			Test("num(i)", Complex.ImaginaryOne);
			Test("num('100')", 100);

			Test("string(a)", "5");
			Test("string(true)", "⊤");
			Test("str(i)", "(0, 1)");
			Test("str('100')", "100");
		}

		[TestMethod]
		public void Evaluation_Test_37_ComplexFunctions()
		{
			Test("Re(2+i)", 2);
			Test("Im(2+i)", 1);
			Test("Arg(i)", Math.PI / 2);
			Test("Conjugate(2+i)", new Complex(2, -1));
			Test("Conj(2+i)", new Complex(2, -1));
			Test("round(Polar(1,pi/2)*1e6)*1e-6", Complex.ImaginaryOne);
		}

		[TestMethod]
		public void Evaluation_Test_38_Matrices()
		{
			Test("invert(2)", 0.5);
			Test("inv(2)", 0.5);
			Test("inverse(i)", new Complex(0, -1));
			Test("invert([[1,1],[0,1]])", new double[,] { { 1, -1 }, { 0, 1 } });
		}

		[TestMethod]
		public void Evaluation_Test_39_Runtime()
		{
			Test("exists(a)", true);
			Test("exists(k)", false);

			Test("f(x):=(return(x+1);x+2);f(3)", 4);
			Test("return(1);2", 1);

			Test("exists(error('hej'))", false);
			Test("exists(Exception('hej'))", false);

			Test("print('hej')", "hej");
			Test("printline('hej')", "hej");
			Test("println('hej')", "hej");

			Test("x:=10;remove(x);exists(x)", false);
			Test("x:=10;destroy(x);exists(x)", false);
			Test("x:=10;delete(x);exists(x)", false);
			Test("Create(System.String,'-',80)", new string('-', 80));
			Test("Create(System.Collections.Generic.List, System.String).GetType()", typeof(System.Collections.Generic.List<string>));
		}

		[TestMethod]
		public void Evaluation_Test_40_Strings()
		{
			Test("Evaluate('a+b')", a + b);
			Test("Eval('a+b')", a + b);
			Test("Parse('a+b')", new Expression("a+b"));
			Test("IsEmpty('a+b')", false);
			Test("Empty('')", true);
			Test("IsNotEmpty('a+b')", true);
			Test("NotEmpty('')", false);
			Test("Left('Hello',3)", "Hel");
			Test("Right('Hello',3)", "llo");
			Test("Length('Hello')", 5);
			Test("Len('Hello')", 5);
			Test("Mid('Hello',2,2)", "ll");
		}

		[TestMethod]
		public void Evaluation_Test_41_DateTime()
		{
			Test("DateTime(2016,03,05)", new DateTime(2016, 03, 05));
			Test("DateTime(2016,03,05,19,15,01)", new DateTime(2016, 03, 05, 19, 15, 01));
			Test("DateTime(2016,03,05,19,15,01,123)", new DateTime(2016, 03, 05, 19, 15, 01, 123));

			Test("TimeSpan(19,22,01)", new TimeSpan(19, 22, 01));
			Test("TimeSpan(19,22,01,123)", new TimeSpan(19, 22, 01, 123));
		}

		[TestMethod]
		public void Evaluation_Test_42_Units()
		{
			Test("10 m", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			Test("10 km", new PhysicalQuantity(10, new Unit(Prefix.Kilo, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			Test("10 mm", new PhysicalQuantity(10, new Unit(Prefix.Milli, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			Test("10 m^2", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2))));
			Test("10 m^3", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 3))));
			Test("10 m²", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2))));
			Test("10 m³", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 3))));
			Test("10 W⋅s", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("W"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), 1))));
			Test("10 W*s", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("W"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), 1))));
			Test("10 m⋅s^-1", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -1))));
			Test("10 m/s", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -1))));
			Test("10 m^2/s", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -1))));
			Test("10 m/s^2", new PhysicalQuantity(10, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -2))));
			Test("10 kg⋅m²/(A⋅s³)", new PhysicalQuantity(10, new Unit(Prefix.Kilo, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("g"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("A"), -1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -3))));

			Test("10 m + 2 km", new PhysicalQuantity(2010, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			Test("10 m < 2 km", true);
			Test("2 km < 10 m", false);
			Test("2 km - 10 m", new PhysicalQuantity(1.99, new Unit(Prefix.Kilo, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			Test("2 km * 10 m", new PhysicalQuantity(20, new Unit(Prefix.Kilo, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 2))));
			Test("2 km² / 10 m", new PhysicalQuantity(0.2, new Unit(Prefix.Kilo, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			Test("10 m / 2 s", new PhysicalQuantity(5, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1), new KeyValuePair<AtomicUnit, int>(new AtomicUnit("s"), -1))));

			Test("10 km m", new PhysicalQuantity(10000, new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			Test("10*sin(pi/6) m", new PhysicalQuantity(10 * Math.Sin(Math.PI / 6), new Unit(Prefix.None, new KeyValuePair<AtomicUnit, int>(new AtomicUnit("m"), 1))));
			Test("sin(10 W)", Math.Sin(10));
			Test("10 °C > 20 °F", true);
			Test("10 m² > 1000 inch²", true);

			Test("10 kWh", new PhysicalQuantity(10, new Unit(Prefix.Kilo, "W", "h")));
			Test("10 mph", new PhysicalQuantity(10, new Unit(Prefix.None, "SM", "h")));
			Test("10 fps", new PhysicalQuantity(10, new Unit(Prefix.None, "ft", "s")));
			Test("10 ft", new PhysicalQuantity(10, new Unit("ft")));

			Test("10 kWh J", new PhysicalQuantity(36000000, new Unit(Prefix.None, "J")));
			Test("10 kWh kJ", new PhysicalQuantity(36000, new Unit(Prefix.Kilo, "J")));
			Test("10 kWh MJ", new PhysicalQuantity(36, new Unit(Prefix.Mega, "J")));

			Test("10 V / 2 A = 5 Ohm", true);

			// TODO: Difference of temperature units -> K
			// TODO: (km)² != km²=k(m²)
			// TODO: Test all units.
		}

		[TestMethod]
		public void Evaluation_Test_43_MethodCall()
		{
			Test("DateTime(2016,3,11).AddDays(10)", new DateTime(2016, 3, 21));
			Test("DateTime(2016,3,11).AddDays(1..3)", new DateTime[] { new DateTime(2016, 3, 12), new DateTime(2016, 3, 13), new DateTime(2016, 3, 14) });
		}

		[TestMethod]
		public void Evaluation_Test_44_NullCheckSuffixOperators()
		{
			Test("null?.Test", null);
			Test("f?(1,2,3)", null);
			Test("null?[]", null);
			Test("null?{}", null);
			Test("null?[i]", null);
			Test("null?[x,y]", null);
			Test("null?[x,]", null);
			Test("null?[,y]", null);
			Test("null?[,]", null);
		}

		[TestMethod]
		public void Parsing_Test_55_ImplicitSetNotation()
		{
			Test("S:={x in Z:x>10};5 in S", false);
			Test("S:={x in Z:x>10};15 in S", true);
			Test("S:={x in Z:x>10};25 in S", true);
			Test("S:={x in Z:x>10};S2:={x in S:x<20};5 in S2", false);
			Test("S:={x in Z:x>10};S2:={x in S:x<20};15 in S2", true);
			Test("S:={x in Z:x>10};S2:={x in S:x<20};25 in S2", false);
			Test("S:=1..20;S2:={x in S:x>10};5 in S2", false);
			Test("S:=1..20;S2:={x in S:x>10};15 in S2", true);
			Test("S:=1..20;S2:={x in S:x>10};25 in S2", false);
			Test("S:={[a,b]: a>b};[1,2] in S", false);
			Test("S:={[a,b]: a>b};[2,1] in S", true);
			Test("S:={[a,b]: a>b};[2,1,0] in S", false);
			Test("S:={[a,b]: a in Z, b in Z, a>b};[1,2] in S", false);
			Test("S:={[a,b]: a in Z, b in Z, a>b};[2,1] in S", true);
			Test("S:={[a,b]: a in Z, b in Z, a>b};[2.1,1] in S", false);
			Test("S:={[a,b]: a in Z, b in Z, a>b};[2,1,0] in S", false);
			Test("S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104};count(S)", 40);
			Test("S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104};[2,13,104] in S", true);
			Test("S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104};[6,13,104] in S", false);
			Test("S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104};[2,16,104] in S", false);
			Test("S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104};[2,13,114] in S", false);
			Test("S:={v[]:count(v)>3};[1,2] in S", false);
			Test("S:={v[]:count(v)>3};[1,2,3] in S", false);
			Test("S:={v[]:count(v)>3};[1,2,3,4] in S", true);
			Test("S:={s{}:count(s)>3};{1,2} in S", false);
			Test("S:={s{}:count(s)>3};{1,2,3} in S", false);
			Test("S:={s{}:count(s)>3};{1,2,3,4} in S", true);
			Test("S:={M[,]:M.Columns>M.Rows};[[1,2],[3,4]] in S", false);
			Test("S:={M[,]:M.Columns>M.Rows};[[1,2,3],[3,4,4]] in S", true);
			Test("S:={M[,]:M.Columns>M.Rows};[[1,2],[3,4],[5,6]] in S", false);
			Test("S:={x::x>\"Hello\"};\"ABC\" in S", false);
			Test("S:={x::x>\"Hello\"};\"XYZ\" in S", true);
			Test("v:=[1,2,3,4,5,6,7,8,9,10];{x in v:x>5}[]", new double[] { 6, 7, 8, 9, 10 });
		}

		[TestMethod]
		public void Parsing_Test_56_ImplicitVectorNotation()
		{
			Test("v:=[1,2,3,4,5,6,7,8,9,10];[x in v:x>5]", new double[] { 6, 7, 8, 9, 10 });
			Test("v:=1..100;[x in v:floor(sqrt(x))^2=x]", new double[] { 1, 4, 9, 16, 25, 36, 49, 64, 81, 100 });
			Test("X:=1..10;P:=[x^2:x in X]", new double[] { 1, 4, 9, 16, 25, 36, 49, 64, 81, 100 });
		}

		[TestMethod]
		public void Parsing_Test_57_ImplicitMatrixNotation()
		{
			Test("v:=1..100;[[x,y]:x in v,(y:=floor(sqrt(x)))^2=x]", new double[,] { { 1, 1 },{ 4, 2 }, { 9, 3 }, { 16, 4 },{ 25, 5 },{ 36, 6 },{ 49, 7 },{ 64, 8 },{ 81, 9 },{ 100, 10 } });
			Test("X:=1..2;Y:=5..7;P:=[[x,y]:x in X, y in Y]", new double[,] { { 1, 5 }, { 2, 5 }, { 1, 6 }, { 2, 6 }, { 1, 7 }, { 2, 7 } });
			Test("M:=Identity(2);[Reverse(Row):Row in M]", new double[,] { { 0, 1 }, { 1, 0 } });
		}

	}
}