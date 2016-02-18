using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Test
{
	[TestFixture]
	public class EvaluationTests
	{
		private double a = 5;
		private double b = 6;
		private double c = 7;
		private bool p = true;
		private bool q = false;
		private bool r = true;

		private void Test(string Script, object ExpectedValue)
		{
			Variables v = new Variables();
			v["a"] = a;
			v["b"] = b;
			v["c"] = c;
			v["p"] = p;
			v["q"] = q;
			v["r"] = r;
			v["s"] = "Hello";

			Expression Exp = new Expression(Script);
			object Result = Exp.Evaluate(v);

			Assert.AreEqual(ExpectedValue, Result, Script);
		}

		[Test]
		public void Test_01_Sequences()
		{
			this.Test("a;b;c", c);
		}
		/*
		[Test]
		public void Test_02_ConditionalStatements()
		{
			this.Test("DO a WHILE b");
			this.Test("WHILE a DO b");
			this.Test("WHILE a : b");
			this.Test("WHILE a b");
			this.Test("FOREACH x IN S DO x");
			this.Test("FOREACH x IN S : x");
			this.Test("FOREACH x IN S x");
			this.Test("FOR EACH x IN S DO x");
			this.Test("FOR EACH x IN S : x");
			this.Test("FOR EACH x IN S x");
			this.Test("FOR a:=1 TO 10 STEP 2 DO x");
			this.Test("FOR a:=1 TO 10 STEP 2 : x");
			this.Test("FOR a:=1 TO 10 STEP 2 x");
			this.Test("FOR a:=1 TO 10 DO x");
			this.Test("FOR a:=1 TO 10 : x");
			this.Test("FOR a:=1 TO 10 x");
			this.Test("TRY a CATCH b FINALLY c");
			this.Test("TRY a CATCH b");
			this.Test("TRY a FINALLY c");
		}

		[Test]
		public void Test_03_Lists()
		{
			this.Test("a,b,c,d");
			this.Test("a,b,,d");
		}

		[Test]
		public void Test_04_Assignments()
		{
			this.Test("a:=b");
			this.Test("[x,y]:=f(a,b,c)");
			this.Test("a+=b");
			this.Test("a-=b");
			this.Test("a*=b");
			this.Test("a/=b");
			this.Test("a^=b");
			this.Test("a&=b");
			this.Test("a&&=b");
			this.Test("a|=b");
			this.Test("a||=b");
			this.Test("a<<=b");
			this.Test("a>>=b");
			this.Test("a.b:=c");
			this.Test("a[b]:=c");
			this.Test("a[b,c]:=d");
			this.Test("a[,c]:=d");
			this.Test("a[b,]:=d");
			this.Test("a(b,c):=d");
			this.Test("a(b,[c]):=d");
			this.Test("a(b,c[]):=d");
			this.Test("a(b,c[,]):=d");
		}

		[Test]
		public void Test_05_IF()
		{
			this.Test("IF Condition THEN IfTrueStatement");
			this.Test("IF Condition THEN IfTrueStatement ELSE IfFalseStatement");
			this.Test("IF Condition IfTrueStatement");
			this.Test("IF Condition IfTrueStatement ELSE IfFalseStatement");
			this.Test("Condition ? IfTrueStatement");
			this.Test("Condition ? IfTrueStatement : IfFalseStatement");
		}

		[Test]
		public void Test_06_Lambda()
		{
			this.Test("x->x^2");
			this.Test("(x,y)=>sin(x)*exp(-1/y^2)");
			this.Test("(x,[y])=>sin(x)*exp(-1/y^2)");
			this.Test("(x[],y[,])=>sin(x)*exp(-1/y^2)");
		}
		*/
		[Test]
		public void Test_07_Implication()
		{
			this.Test("p -> q", false);
			this.Test("p => q", false);
			this.Test("q -> p", true);
			this.Test("q => p", true);

			this.Test("[p,q,r] => q", new bool[] { false, true, false });
			this.Test("p=>[p,q,r]", new bool[] { true, false, true });
			this.Test("[p,r,q]=>[p,q,r]", new bool[] { true, false, true });

			this.Test("[[p,q,r],[q,r,p]] => q", new bool[,] { { false, true, false }, { true, false, false } });
			this.Test("p=>[[p,q,r],[q,r,p]]", new bool[,] { { true, false, true }, { false, true, true } });
			this.Test("[[p,r,q],[r,q,p]]=>[[p,q,r],[q,r,p]]", new bool[,] { { true, false, true }, { false, true, true } });
		}

		[Test]
		public void Test_08_Equivalence()
		{
			this.Test("p <-> q", false);
			this.Test("p <=> q", false);

			this.Test("[p,q,r] <-> q", new bool[] { false, true, false });
			this.Test("p <=> [p,q,r]", new bool[] { true, false, true });
			this.Test("[p,r,q] <=> [p,q,r]", new bool[] { true, false, false });

			this.Test("[[p,q,r],[q,r,p]] <-> q", new bool[,] { { false, true, false }, { true, false, false } });
			this.Test("p <=> [[p,q,r],[q,r,p]]", new bool[,] { { true, false, true }, { false, true, true } });
			this.Test("[[p,r,q],[r,q,p]] <=> [[p,q,r],[q,r,p]]", new bool[,] { { true, false, false }, { false, false, true } });
		}

		[Test]
		public void Test_09_OR()
		{
			this.Test("p || q", true);
			this.Test("q || p", true);
			this.Test("[p,q,r] || q", new bool[] { true, false, true });
			this.Test("p || [p,q,r]", new bool[] { true, true, true });
			this.Test("[p,r,q] || [p,q,r]", new bool[] { true, true, true });

			this.Test("a | b", 7);
			this.Test("b | a", 7);
			this.Test("[a,b,c] | b", new double[] { 7, 6, 7 });
			this.Test("a | [a,b,c]", new double[] { 5, 7, 7 });
			this.Test("[a,c,b] | [a,b,c]", new double[] { 5, 7, 7 });

			this.Test("p OR q", true);
			this.Test("q OR p", true);
			this.Test("[p,q,r] OR q", new bool[] { true, false, true });
			this.Test("p OR [p,q,r]", new bool[] { true, true, true });
			this.Test("[p,r,q] OR [p,q,r]", new bool[] { true, true, true });

			this.Test("a OR b", 7);
			this.Test("b OR a", 7);
			this.Test("[a,b,c] OR b", new double[] { 7, 6, 7 });
			this.Test("a OR [a,b,c]", new double[] { 5, 7, 7 });
			this.Test("[a,c,b] OR [a,b,c]", new double[] { 5, 7, 7 });

			this.Test("p XOR q", true);
			this.Test("q XOR p", true);
			this.Test("[p,q,r] XOR q", new bool[] { true, false, true });
			this.Test("p XOR [p,q,r]", new bool[] { false, true, false });
			this.Test("[p,r,q] XOR [p,q,r]", new bool[] { false, true, true });

			this.Test("a XOR b", 3);
			this.Test("b XOR a", 3);
			this.Test("[a,b,c] XOR b", new double[] { 3, 0, 1 });
			this.Test("a XOR [a,b,c]", new double[] { 0, 3, 2 });
			this.Test("[a,c,b] XOR [a,b,c]", new double[] { 0, 1, 1 });

			this.Test("p NOR q", false);
			this.Test("q NOR p", false);
			this.Test("[p,q,r] NOR q", new bool[] { false, true, false });
			this.Test("p NOR [p,q,r]", new bool[] { false, false, false });
			this.Test("[p,r,q] NOR [p,q,r]", new bool[] { false, false, false });

			this.Test("a NOR b", unchecked((ulong)-8));
			this.Test("b NOR a", unchecked((ulong)-8));
			this.Test("[a,b,c] NOR b", new double[] { unchecked((ulong)-8), unchecked((ulong)-7), unchecked((ulong)-8) });
			this.Test("a NOR [a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-8), unchecked((ulong)-8) });
			this.Test("[a,c,b] NOR [a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-8), unchecked((ulong)-8) });

			this.Test("p XNOR q", false);
			this.Test("q XNOR p", false);
			this.Test("[p,q,r] XNOR q", new bool[] { false, true, false });
			this.Test("p XNOR [p,q,r]", new bool[] { true, false, true });
			this.Test("[p,r,q] XNOR [p,q,r]", new bool[] { true, false, false });

			this.Test("a XNOR b", unchecked((ulong)-4));
			this.Test("b XNOR a", unchecked((ulong)-4));
			this.Test("[a,b,c] XNOR b", new double[] { unchecked((ulong)-4), unchecked((ulong)-1), unchecked((ulong)-2) });
			this.Test("a XNOR [a,b,c]", new double[] { unchecked((ulong)-1), unchecked((ulong)-4), unchecked((ulong)-3) });
			this.Test("[a,c,b] XNOR [a,b,c]", new double[] { unchecked((ulong)-1), unchecked((ulong)-2), unchecked((ulong)-2) });
		}

		[Test]
		public void Test_10_AND()
		{
			this.Test("p && q", false);
			this.Test("q && p", false);
			this.Test("[p,q,r] && q", new bool[] { false, false, false });
			this.Test("p && [p,q,r]", new bool[] { true, false, true });
			this.Test("[p,r,q] && [p,q,r]", new bool[] { true, false, false });

			this.Test("a & b", 4);
			this.Test("b & a", 4);
			this.Test("[a,b,c] & b", new double[] { 4, 6, 6 });
			this.Test("a & [a,b,c]", new double[] { 5, 4, 5 });
			this.Test("[a,c,b] & [a,b,c]", new double[] { 5, 6, 6 });

			this.Test("p AND q", false);
			this.Test("q AND p", false);
			this.Test("[p,q,r] AND q", new bool[] { false, false, false });
			this.Test("p AND [p,q,r]", new bool[] { true, false, true });
			this.Test("[p,r,q] AND [p,q,r]", new bool[] { true, false, false });

			this.Test("a AND b", 4);
			this.Test("b AND a", 4);
			this.Test("[a,b,c] AND b", new double[] { 4, 6, 6 });
			this.Test("a AND [a,b,c]", new double[] { 5, 4, 5 });
			this.Test("[a,c,b] AND [a,b,c]", new double[] { 5, 6, 6 });

			this.Test("p NAND q", true);
			this.Test("q NAND p", true);
			this.Test("[p,q,r] NAND q", new bool[] { true, true, true });
			this.Test("p NAND [p,q,r]", new bool[] { false, true, false });
			this.Test("[p,r,q] NAND [p,q,r]", new bool[] { false, true, true });

			this.Test("a NAND b", unchecked((ulong)-5));
			this.Test("b NAND a", unchecked((ulong)-5));
			this.Test("[a,b,c] NAND b", new double[] { unchecked((ulong)-5), unchecked((ulong)-7), unchecked((ulong)-7) });
			this.Test("a NAND [a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-5), unchecked((ulong)-6) });
			this.Test("[a,c,b] NAND [a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-7), unchecked((ulong)-7) });
		}
		/*
		[Test]
		public void Test_11_Membership()
		{
			this.Test("a AS T");
			this.Test("a IS T");
			this.Test("a IN M");
			this.Test("a NOT IN M");
			this.Test("a NOTIN M");
		}
		*/
		[Test]
		public void Test_12_Comparison()
		{
			this.Test("a <= b", true);
			this.Test("a < b", true);
			this.Test("a >= b", false);
			this.Test("a > b", false);
			this.Test("a = b", false);
			this.Test("a == b", false);
			this.Test("a === b", false);
			this.Test("a <> b", true);
			this.Test("a != b", true);
			this.Test("s LIKE 'H.*'", true);
			this.Test("s NOT LIKE 'Bye.*'", true);
			this.Test("s NOTLIKE 'Bye.*'", true);
			this.Test("s UNLIKE 'Bye.*'", true);
			this.Test("a .= b", false);
			this.Test("a .== b", false);
			this.Test("a .=== b", false);
			this.Test("a .<> b", false);
			this.Test("a .!= b", false);
		}
		/*
		[Test]
		public void Test_13_Shift()
		{
			this.Test("a << b");
			this.Test("a >> b");
		}

		[Test]
		public void Test_14_Union()
		{
			this.Test("a UNION b");
		}

		[Test]
		public void Test_15_Union()
		{
			this.Test("a INTERSECT b");
			this.Test("a INTERSECTION b");
		}

		[Test]
		public void Test_16_Intervals()
		{
			this.Test("1..10");
			this.Test("1..10|0.1");
		}

		[Test]
		public void Test_17_Terms()
		{
			this.Test("a+b");
			this.Test("a-b");
			this.Test("a.+b");
			this.Test("a.-b");
		}
		*/
		[Test]
		public void Test_18_Factors()
		{
			this.Test("a*b", 30);
			this.Test("[1,2,3]*a", new double[] { 5, 10, 15 });
			this.Test("a*[1,2,3]", new double[] { 5, 10, 15 });
			this.Test("[1,2,3]*[a,b,c]", new double[] { 5, 12, 21 });

			this.Test("10/a", 2);
			this.Test("[10,20,30]/a", new double[] { 2, 4, 6 });
			this.Test("[5,6,7]/[a,b,c]", new double[] { 1, 1, 1 });
			this.Test("[[10,20],[30,40]]/a", new double[,] { { 2, 4 }, { 6, 8 } });
			this.Test("a/[[1,1],[0,1]]", new double[,] { { 5, -5 }, { 0, 5 } });
			this.Test("[[a,b],[b,c]]/[[1,1],[0,1]]", new double[,] { { 5, 1 }, { 6, 1 } });

			this.Test("a\\10", 2);
			this.Test("a\\[10,20,30]", new double[] { 2, 4, 6 });
			this.Test("[a,b,c]\\[5,6,7]", new double[] { 1, 1, 1 });
			this.Test("a\\[[10,20],[30,40]]", new double[,] { { 2, 4 }, { 6, 8 } });
			this.Test("[[1,1],[0,1]]\\a", new double[,] { { 5, -5 }, { 0, 5 } });
			this.Test("[[1,1],[0,1]]\\[[a,b],[b,c]]", new double[,] { { -1, -1 }, { 6, 7 } });

			this.Test("b MOD a", 1);
			this.Test("b MOD [a,b,c]", new double[] { 1, 0, 6 });
			this.Test("[a,b,c] MOD a", new double[] { 0, 1, 2 });
			this.Test("[a,b,c] MOD [b,c,a]", new double[] { 5, 6, 2 });

			this.Test("a.*b", 30);
			this.Test("a.*[a,b,c]", new double[] { 25, 30, 35 });
			this.Test("[a,b,c].*a", new double[] { 25, 30, 35 });
			this.Test("[a,b,c].*[a,b,c]", new double[] { 25, 36, 49 });
			this.Test("[[a,b],[b,c]].*[[a,b],[b,c]]", new double[,] { { 25, 36 }, { 36, 49 } });

			this.Test("10./a", 2);
			this.Test("[10,20,30]./a", new double[] { 2, 4, 6 });
			this.Test("[5,6,7]./[a,b,c]", new double[] { 1, 1, 1 });
			this.Test("[[10,20],[30,40]]./a", new double[,] { { 2, 4 }, { 6, 8 } });
			this.Test("b./[[1,2],[2,1]]", new double[,] { { 6, 3 }, { 3, 6 } });
			this.Test("[[a,b],[b,c]]./[[1,2],[2,1]]", new double[,] { { 5, 3 }, { 3, 7 } });

			this.Test("a.\\10", 2);
			this.Test("a.\\[10,20,30]", new double[] { 2, 4, 6 });
			this.Test("[a,b,c].\\[5,6,7]", new double[] { 1, 1, 1 });
			this.Test("a.\\[[10,20],[30,40]]", new double[,] { { 2, 4 }, { 6, 8 } });
			this.Test("[[1,2],[2,1]].\\b", new double[,] { { 6, 3 }, { 3, 6 } });
			this.Test("[[1,2],[2,1]].\\[[a,b],[b,c]]", new double[,] { { 5, 3 }, { 3, 7 } });

			this.Test("b .MOD a", 1);
			this.Test("b .MOD [a,b,c]", new double[] { 1, 0, 6 });
			this.Test("[a,b,c] .MOD a", new double[] { 0, 1, 2 });
			this.Test("[a,b,c] .MOD [b,c,a]", new double[] { 5, 6, 2 });

			this.Test("[1,2,3] DOT [a,b,c]", 38);
			this.Test("[1,2,3] CROSS [a,b,c]", new double[] { -4, 8, -4 });
			this.Test("[1,2,3] CARTESIAN [a,b,c]", new object[]
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

		[Test]
		public void Test_18_Powers()
		{
			this.Test("a^b", Math.Pow(5, 6));
			this.Test("a²", 25);
			this.Test("a³", 125);

			this.Test("[[a,1],[1,b]]²", new double[,] { { a * a + 1, a + b }, { a + b, 1 + b * b } });
			this.Test("[[a,1],[1,b]]^2", new double[,] { { a * a + 1, a + b }, { a + b, 1 + b * b } });

			this.Test("a.^b", Math.Pow(5, 6));
			this.Test("[[a,1],[1,b]].^2", new double[,] { { a * a, 1 }, { 1, b * b } });
		}

		[Test]
		public void Test_19_UnaryPrefixOperators()
		{
			this.Test("++a", 6);
			this.Test("d:=[a,b,c];++d", new double[] { 6, 7, 8 });
			this.Test("M:=[[a,b,c],[b,c,a],[c,a,b]];++M", new double[,] { { 6, 7, 8 }, { 7, 8, 6 }, { 8, 6, 7 } });

			this.Test("--a", 4);
			this.Test("d:=[a,b,c];--d", new double[] { 4, 5, 6 });
			this.Test("M:=[[a,b,c],[b,c,a],[c,a,b]];--M", new double[,] { { 4, 5, 6 }, { 5, 6, 4 }, { 6, 4, 5 } });

			this.Test("+a", 5);
			this.Test("+[a,b,c]", new double[] { 5, 6, 7 });

			this.Test("-a", -5);
			this.Test("-[a,b,c]", new double[] { -5, -6, -7 });

			this.Test("!p", false);
			this.Test("![p,q,r]", new bool[] { false, true, false });

			this.Test("NOT q", true);
			this.Test("NOT [p,q,r]", new bool[] { false, true, false });

			this.Test("~a", (double)unchecked((ulong)-6));
			this.Test("~[a,b,c]", new double[] { unchecked((ulong)-6), unchecked((ulong)-7), unchecked((ulong)-8) });
		}

		[Test]
		public void Test_20_UnarySuffixOperators()
		{
			this.Test("[a++,a]", new double[] { 5, 6 });
			this.Test("[a--,a]", new double[] { 5, 4 });

			this.Test("a%", 0.05);
			this.Test("a‰", 0.005);
			this.Test("a‱", 0.0005);
			this.Test("a°", Math.PI / 36);

			/*
			this.Test("f'(x)");
			this.Test("f′(x)");
			this.Test("f\"(x)");
			this.Test("f″(x)");
			this.Test("f‴(x)");
			this.Test("M T");
			this.Test("M H");
			this.Test("M†");
			this.Test("n!");
			this.Test("n!!");*/
		}
		/*
		[Test]
		public void Test_21_BinarySuffixOperators()
		{
			this.Test("obj.Member");
			this.Test("f(a,b,c)");
			this.Test("a[]");
			this.Test("v[i]");
			this.Test("M[x,y]");
			this.Test("M[x,]");
			this.Test("M[,y]");
			this.Test("a[,]");
		}

		[Test]
		public void Test_22_ObjectExNihilo()
		{
			this.Test("{Member1:Value1, Member2:Value2, MemberN:ValueN}");
			this.Test("{\"Member1\":\"Value1\", \"Member2\":\"Value2\", \"MemberN\":\"ValueN\"}");
		}

		[Test]
		public void Test_23_Sets()
		{
			this.Test("S:={1,2,3};");
			this.Test("S:={DO x++ WHILE X<10};");
			this.Test("S:={WHILE x<10 : x++};");
			this.Test("S:={FOR x:=1 TO 20 STEP 3 : x};");
			this.Test("S:={FOREACH x IN 1..10|0.1 : x^2};");
			this.Test("S:={FOR EACH x IN 1..10|0.1 : x^2};");
		}
		*/
		[Test]
		public void Test_24_Matrices()
		{
			this.Test("[[a,b,c],[b,c,a]];", new double[,] { { a, b, c }, { b, c, a } });
			/*this.Test("[DO [x++,x++,x++] WHILE X<10];");
			this.Test("[WHILE x<10 : [x++,x++,x++]];");
			this.Test("[FOR y:=1 TO 20 : [FOR x:=1 TO 20 : x=y ? 1 : 0]];");
			this.Test("[FOREACH x IN 1..10|0.1 : [x^2,x^3,x^4]];");
			this.Test("[FOR EACH x IN 1..10|0.1 : [x^2,x^3,x^4]];");*/
		}

		[Test]
		public void Test_25_Vectors()
		{
			this.Test("[a,b,c];", new double[] { a, b, c });
			/*this.Test("[DO x++ WHILE X<10];");
			this.Test("[WHILE x<10 : x++];");
			this.Test("[FOR x:=1 TO 20 STEP 3 : x];");
			this.Test("[FOREACH x IN 1..10|0.1 : x^2];");
			this.Test("[FOR EACH x IN 1..10|0.1 : x^2];");*/
		}
		/*
		[Test]
		public void Test_26_Parenthesis()
		{
			this.Test("a * (b + c)");
		}

		[Test]
		public void Test_27_null()
		{
			this.Test("null");
		}

		[Test]
		public void Test_28_StringValues()
		{
			this.Test("\"Hello\r\n\t\f\b\a\v\\\\\\\"\\''\"");
			this.Test("'Hello\r\n\t\f\b\a\v\\\\\\\"\\'\"'");
		}

		[Test]
		public void Test_29_BooleanValues()
		{
			this.Test("true");
			this.Test("false");
		}

		[Test]
		public void Test_30_DoubleValues()
		{
			this.Test("1");
			this.Test("3.1415927");
			this.Test("1.23e-3");
		}
*/
	}
}
