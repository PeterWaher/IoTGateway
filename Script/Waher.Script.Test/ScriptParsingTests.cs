using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Waher.Script.Model;
using Waher.Script.Xml;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptParsingTests
	{
		private static void Test(string Script)
		{
			Console.Out.WriteLine();
			Console.Out.WriteLine(Script);
			Console.Out.WriteLine();

			Expression Exp1 = new(Script);
			Expression Exp2 = new(Script);

			Assert.AreEqual(Exp1.Root, Exp2.Root, "Script nodes from equal scripts not equal.");
			Assert.AreEqual(Exp1.Root.GetHashCode(), Exp2.Root.GetHashCode(), "Script nodes from equal scripts does not have equal hash values.");

			AssertParentNodesAndSubsexpressions(Exp1);

			Exp1.ToXml(Console.Out);
			Console.Out.WriteLine();
		}

		public static void AssertParentNodesAndSubsexpressions(Expression Exp)
		{
			Exp.ForAll((ScriptNode Node, out ScriptNode NewNode, object State) =>
			{
				if (Node.Parent is null && Node != Exp.Root)
					Assert.Fail("Parent reference not set properly");

				NewNode = null;
				return true;

			}, null, SearchMethod.TreeOrder);
		}

		[TestMethod]
		public void Parsing_Test_01_Sequences()
		{
			Test("a;b;c");
		}

		[TestMethod]
		public void Parsing_Test_02_ConditionalStatements()
		{
			Test("DO a WHILE b");
			Test("WHILE a DO b");
			Test("WHILE a : b");
			Test("FOREACH x IN S DO x");
			Test("FOREACH x IN S : x");
			Test("FOR EACH x IN S DO x");
			Test("FOR EACH x IN S : x");
			Test("FOR a:=1 TO 10 STEP 2 DO x");
			Test("FOR a:=1 TO 10 STEP 2 : x");
			Test("FOR a:=1 TO 10 DO x");
			Test("FOR a:=1 TO 10 : x");
			Test("TRY a CATCH b FINALLY c");
			Test("TRY a CATCH b");
			Test("TRY a FINALLY c");
		}

		[TestMethod]
		public void Parsing_Test_03_Lists()
		{
			Test("a,b,c,d");
			Test("a,b,,d");
		}

		[TestMethod]
		public void Parsing_Test_04_Assignments()
		{
			Test("a:=b");
			Test("[x,y]:=f(a,b,c)");
			Test("a+=b");
			Test("a-=b");
			Test("a*=b");
			Test("a/=b");
			Test("a^=b");
			Test("a&=b");
			Test("a&&=b");
			Test("a|=b");
			Test("a||=b");
			Test("a<<=b");
			Test("a>>=b");
			Test("a.b:=c");
			Test("a[b]:=c");
			Test("a[b,c]:=d");
			Test("a[,c]:=d");
			Test("a[b,]:=d");
			Test("a(b,c):=d");
			Test("a(b,[c]):=d");
			Test("a(b,c[]):=d");
			Test("a(b,c[,]):=d");
		}

		[TestMethod]
		public void Parsing_Test_05_IF()
		{
			Test("IF Condition THEN IfTrueStatement");
			Test("IF Condition THEN IfTrueStatement ELSE IfFalseStatement");
			Test("Condition ? IfTrueStatement");
			Test("Condition ? IfTrueStatement : IfFalseStatement");
			Test("Statement ?? IfNullStatement");
		}

		[TestMethod]
		public void Parsing_Test_06_Lambda()
		{
			Test("x->x^2");
			Test("(x,y)->sin(x)*exp(-1/y^2)");
			Test("(x,[y])->sin(x)*exp(-1/y^2)");
			Test("(x[],y[,])->sin(x)*exp(-1/y^2)");
			Test("(x{},y[,])->sin(x)*exp(-1/y^2)");
		}

		[TestMethod]
		public void Parsing_Test_07_Implication()
		{
			Test("a => b");
		}

		[TestMethod]
		public void Parsing_Test_08_Equivalence()
		{
			Test("a <=> b");
		}

		[TestMethod]
		public void Parsing_Test_09_OR()
		{
			Test("a | b | c");
			Test("a || b || c");
			Test("a OR b OR c");
			Test("a NOR b NOR c");
			Test("a XOR b XOR c");
			Test("a XNOR b XNOR c");
		}

		[TestMethod]
		public void Parsing_Test_10_AND()
		{
			Test("a & b & c");
			Test("a && b && c");
			Test("a AND b AND c");
			Test("a NAND b NAND c");
		}

		[TestMethod]
		public void Parsing_Test_11_Membership()
		{
			Test("a AS T");
			Test("a IS T");
			Test("a IN M");
			Test("a NOT IN M");
			Test("a NOTIN M");
		}

		[TestMethod]
		public void Parsing_Test_12_Comparison()
		{
			Test("a <= b");
			Test("a < b");
			Test("a >= b");
			Test("a > b");
			Test("a = b");
			Test("a == b");
			Test("a === b");
			Test("a <> b");
			Test("a != b");
			Test("a LIKE b");
			Test("a NOT LIKE b");
			Test("a NOTLIKE b");
			Test("a UNLIKE b");
			Test("a .= b");
			Test("a .== b");
			Test("a .=== b");
			Test("a .<> b");
			Test("a .!= b");
			Test("a <= b <= c");
			Test("a <= b < c");
			Test("a < b <= c");
			Test("a < b < c");
			Test("a >= b >= c");
			Test("a > b >= c");
			Test("a >= b > c");
			Test("a > b > c");
		}

		[TestMethod]
		public void Parsing_Test_13_Shift()
		{
			Test("a << b");
			Test("a >> b");
		}

		[TestMethod]
		public void Parsing_Test_14_Union()
		{
			Test("a UNION b");
		}

		[TestMethod]
		public void Parsing_Test_15_Intersection()
		{
			Test("a INTERSECT b");
			Test("a INTERSECTION b");
		}

		[TestMethod]
		public void Parsing_Test_16_Intervals()
		{
			Test("1..10");
			Test("1..10|0.1");
		}

		[TestMethod]
		public void Parsing_Test_17_Terms()
		{
			Test("a+b");
			Test("a-b");
			Test("a.+b");
			Test("a.-b");
		}

		[TestMethod]
		public void Parsing_Test_18_Factors()
		{
			Test("a*b");
			Test("a/b");
			Test("a\\b");
			Test("a MOD b");
			Test("a .MOD b");
			Test("a.*b");
			Test("a./b");
			Test("a.\\b");
			Test("a DOT b");
			Test("a CROSS b");
			Test("a CARTESIAN b");
		}

		[TestMethod]
		public void Parsing_Test_18_Powers()
		{
			Test("a^b");
			Test("a.^b");
			Test("a²");
			Test("a³");
		}

		[TestMethod]
		public void Parsing_Test_19_UnaryPrefixOperators()
		{
			Test("++a");
			Test("--a");
			Test("+a");
			Test("-a");
			Test("!a");
			Test("NOT a");
			Test("~a");
		}

		[TestMethod]
		public void Parsing_Test_20_UnarySuffixOperators()
		{
			Test("a++");
			Test("a--");
			Test("a%");
			Test("a‰");
			Test("a‱");
			Test("a°");
			Test("f'(x)");
			Test("f′(x)");
			Test("f\"(x)");
			Test("f″(x)");
			Test("f‴(x)");
			Test("M T");
			Test("M H");
			Test("M†");
			Test("n!");
			Test("n!!");
		}

		[TestMethod]
		public void Parsing_Test_21_BinarySuffixOperators()
		{
			Test("obj.Member");
			Test("f(a,b,c)");
			Test("a[]");
			Test("a{}");
			Test("v[i]");
			Test("M[x,y]");
			Test("M[x,]");
			Test("M[,y]");
			Test("a[,]");
		}

		[TestMethod]
		public void Parsing_Test_22_ObjectExNihilo()
		{
			Test("{Member1:Value1, Member2:Value2, MemberN:ValueN}");
			Test("{\"Member1\":\"Value1\", \"Member2\":\"Value2\", \"MemberN\":\"ValueN\"}");
		}

		[TestMethod]
		public void Parsing_Test_23_Sets()
		{
			Test("S:={1,2,3};");
			Test("S:={DO x++ WHILE X<10};");
			Test("S:={WHILE x<10 : x++};");
			Test("S:={FOR x:=1 TO 20 STEP 3 : x};");
			Test("S:={FOREACH x IN 1..10|0.1 : x^2};");
			Test("S:={FOR EACH x IN 1..10|0.1 : x^2};");
		}

		[TestMethod]
		public void Parsing_Test_24_Matrices()
		{
			Test("M:=[[1,0,0],[0,1,0],[0,0,1]];");
			Test("M:=[DO [x++,x++,x++] WHILE X<10];");
			Test("M:=[WHILE x<10 : [x++,x++,x++]];");
			Test("M:=[FOR y:=1 TO 20 : [FOR x:=1 TO 20 : x=y ? 1 : 0]];");
			Test("M:=[FOREACH x IN 1..10|0.1 : [x^2,x^3,x^4]];");
			Test("M:=[FOR EACH x IN 1..10|0.1 : [x^2,x^3,x^4]];");
		}

		[TestMethod]
		public void Parsing_Test_25_Vectors()
		{
			Test("v:=[1,2,3];");
			Test("v:=[DO x++ WHILE X<10];");
			Test("v:=[WHILE x<10 : x++];");
			Test("v:=[FOR x:=1 TO 20 STEP 3 : x];");
			Test("v:=[FOREACH x IN 1..10|0.1 : x^2];");
			Test("v:=[FOR EACH x IN 1..10|0.1 : x^2];");
		}

		[TestMethod]
		public void Parsing_Test_26_Parenthesis()
		{
			Test("a * (b + c)");
		}

		[TestMethod]
		public void Parsing_Test_27_null()
		{
			Test("null");
		}

		[TestMethod]
		public void Parsing_Test_28_StringValues()
		{
			Test("\"Hello\\r\\n\\t\\f\\b\\a\\v\\\\\\\"\\''\"\x03");
			Test("'Hello\\r\\n\\t\\f\\b\\a\\v\\\\\\\"\\'\"'\x03");
		}

		[TestMethod]
		public void Parsing_Test_29_BooleanValues()
		{
			Test("true");
			Test("false");
		}

		[TestMethod]
		public void Parsing_Test_30_DoubleValues()
		{
			Test("1");
			Test("3.1415927");
			Test("1.23e-3");
		}

		[TestMethod]
		public void Parsing_Test_31_Constants()
		{
			Test("e");
			Test("pi");
			Test("π");
		}

		[TestMethod]
		public void Parsing_Test_32_BinomialCoefficients()
		{
			Test("n OVER k");
		}

		[TestMethod]
		public void Parsing_Test_33_NullCheckSuffixOperators()
		{
			Test("obj?.Member");
			Test("f?(a,b,c)");
			Test("a?[]");
			Test("a?{}");
			Test("v?[i]");
			Test("M?[x,y]");
			Test("M?[x,]");
			Test("M?[,y]");
			Test("a?[,]");
		}

		[TestMethod]
		public void Parsing_Test_34_ImplicitSetNotation()
		{
			Test("S:={x in Z:x>10}");
			Test("S2:={x in S:x<20}");
			Test("S:=1..20;S2:={x in S:x>10}");
			Test("S:={[a,b]: a>b}");
			Test("S:={[a,b]: a in Z, b in Z, a>b}");
			Test("S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104}");
			Test("S:={v[]:count(v)>3}");
			Test("S:={s{}:count(s)>3}");
			Test("S:={M[,]:M.Columns>M.Rows}");
			Test("S:={x::x>\"Hello\"}");
			Test("v:=[1,2,3,4,5,6,7,8,9,10];{x in v:x>5}");
		}

		[TestMethod]
		public void Parsing_Test_35_ImplicitVectorNotation()
		{
			Test("v:=[1,2,3,4,5,6,7,8,9,10];[x in v:x>5]");
			Test("v:=1..100;[x in v:floor(sqrt(x))^2=x]");
			Test("X:=1..10;P:=[x^2:x in X]");
		}

		[TestMethod]
		public void Parsing_Test_36_ImplicitMatrixNotation()
		{
			Test("v:=1..100;[[x,y]:x in v,(y:=floor(sqrt(x)))^2=x]");
			Test("X:=1..10;Y:=20..30;P:=[[x,y]:x in X, y in Y]");
			Test("M:=Identity(5);[Reverse(Row):Row in M]");
		}

	}
}
