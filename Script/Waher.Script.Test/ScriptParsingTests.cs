using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Runtime.Inventory;

namespace Waher.Script.Test
{
	[TestClass]
	public class ScriptParsingTests
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext Context)
		{
			Types.Initialize(typeof(Expression).Assembly,
				typeof(Graphs.Graph).Assembly,
				typeof(System.Text.RegularExpressions.Regex).Assembly);
		}

		private void Test(string Script)
		{
			Expression Exp1 = new Expression(Script);
			Expression Exp2 = new Expression(Script);

			Assert.AreEqual(Exp1.Root, Exp2.Root, "Script nodes from equal scripts not equal.");
			Assert.AreEqual(Exp1.Root.GetHashCode(), Exp2.Root.GetHashCode(), "Script nodes from equal scripts does not have equal hash values.");
		}

		[TestMethod]
		public void Parsing_Test_01_Sequences()
		{
			this.Test("a;b;c");
		}

		[TestMethod]
		public void Parsing_Test_02_ConditionalStatements()
		{
			this.Test("DO a WHILE b");
			this.Test("WHILE a DO b");
			this.Test("WHILE a : b");
			this.Test("FOREACH x IN S DO x");
			this.Test("FOREACH x IN S : x");
			this.Test("FOR EACH x IN S DO x");
			this.Test("FOR EACH x IN S : x");
			this.Test("FOR a:=1 TO 10 STEP 2 DO x");
			this.Test("FOR a:=1 TO 10 STEP 2 : x");
			this.Test("FOR a:=1 TO 10 DO x");
			this.Test("FOR a:=1 TO 10 : x");
			this.Test("TRY a CATCH b FINALLY c");
			this.Test("TRY a CATCH b");
			this.Test("TRY a FINALLY c");
		}

		[TestMethod]
		public void Parsing_Test_03_Lists()
		{
			this.Test("a,b,c,d");
			this.Test("a,b,,d");
		}

		[TestMethod]
		public void Parsing_Test_04_Assignments()
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

		[TestMethod]
		public void Parsing_Test_05_IF()
		{
			this.Test("IF Condition THEN IfTrueStatement");
			this.Test("IF Condition THEN IfTrueStatement ELSE IfFalseStatement");
			this.Test("Condition ? IfTrueStatement");
			this.Test("Condition ? IfTrueStatement : IfFalseStatement");
			this.Test("Statement ?? IfNullStatement");
		}

		[TestMethod]
		public void Parsing_Test_06_Lambda()
		{
			this.Test("x->x^2");
			this.Test("(x,y)->sin(x)*exp(-1/y^2)");
			this.Test("(x,[y])->sin(x)*exp(-1/y^2)");
			this.Test("(x[],y[,])->sin(x)*exp(-1/y^2)");
			this.Test("(x{},y[,])->sin(x)*exp(-1/y^2)");
		}

		[TestMethod]
		public void Parsing_Test_07_Implication()
		{
			this.Test("a => b");
		}

		[TestMethod]
		public void Parsing_Test_08_Equivalence()
		{
			this.Test("a <=> b");
		}

		[TestMethod]
		public void Parsing_Test_09_OR()
		{
			this.Test("a | b | c");
			this.Test("a || b || c");
			this.Test("a OR b OR c");
			this.Test("a NOR b NOR c");
			this.Test("a XOR b XOR c");
			this.Test("a XNOR b XNOR c");
		}

		[TestMethod]
		public void Parsing_Test_10_AND()
		{
			this.Test("a & b & c");
			this.Test("a && b && c");
			this.Test("a AND b AND c");
			this.Test("a NAND b NAND c");
		}

		[TestMethod]
		public void Parsing_Test_11_Membership()
		{
			this.Test("a AS T");
			this.Test("a IS T");
			this.Test("a IN M");
			this.Test("a NOT IN M");
			this.Test("a NOTIN M");
		}

		[TestMethod]
		public void Parsing_Test_12_Comparison()
		{
			this.Test("a <= b");
			this.Test("a < b");
			this.Test("a >= b");
			this.Test("a > b");
			this.Test("a = b");
			this.Test("a == b");
			this.Test("a === b");
			this.Test("a <> b");
			this.Test("a != b");
			this.Test("a LIKE b");
			this.Test("a NOT LIKE b");
			this.Test("a NOTLIKE b");
			this.Test("a UNLIKE b");
			this.Test("a .= b");
			this.Test("a .== b");
			this.Test("a .=== b");
			this.Test("a .<> b");
			this.Test("a .!= b");
		}

		[TestMethod]
		public void Parsing_Test_13_Shift()
		{
			this.Test("a << b");
			this.Test("a >> b");
		}

		[TestMethod]
		public void Parsing_Test_14_Union()
		{
			this.Test("a UNION b");
		}

		[TestMethod]
		public void Parsing_Test_15_Intersection()
		{
			this.Test("a INTERSECT b");
			this.Test("a INTERSECTION b");
		}

		[TestMethod]
		public void Parsing_Test_16_Intervals()
		{
			this.Test("1..10");
			this.Test("1..10|0.1");
		}

		[TestMethod]
		public void Parsing_Test_17_Terms()
		{
			this.Test("a+b");
			this.Test("a-b");
			this.Test("a.+b");
			this.Test("a.-b");
		}

		[TestMethod]
		public void Parsing_Test_18_Factors()
		{
			this.Test("a*b");
			this.Test("a/b");
			this.Test("a\\b");
			this.Test("a MOD b");
			this.Test("a .MOD b");
			this.Test("a.*b");
			this.Test("a./b");
			this.Test("a.\\b");
			this.Test("a DOT b");
			this.Test("a CROSS b");
			this.Test("a CARTESIAN b");
		}

		[TestMethod]
		public void Parsing_Test_18_Powers()
		{
			this.Test("a^b");
			this.Test("a.^b");
			this.Test("a²");
			this.Test("a³");
		}

		[TestMethod]
		public void Parsing_Test_19_UnaryPrefixOperators()
		{
			this.Test("++a");
			this.Test("--a");
			this.Test("+a");
			this.Test("-a");
			this.Test("!a");
			this.Test("NOT a");
			this.Test("~a");
		}

		[TestMethod]
		public void Parsing_Test_20_UnarySuffixOperators()
		{
			this.Test("a++");
			this.Test("a--");
			this.Test("a%");
			this.Test("a‰");
			this.Test("a‱");
			this.Test("a°");
			this.Test("f'(x)");
			this.Test("f′(x)");
			this.Test("f\"(x)");
			this.Test("f″(x)");
			this.Test("f‴(x)");
			this.Test("M T");
			this.Test("M H");
			this.Test("M†");
			this.Test("n!");
			this.Test("n!!");
		}

		[TestMethod]
		public void Parsing_Test_21_BinarySuffixOperators()
		{
			this.Test("obj.Member");
			this.Test("f(a,b,c)");
			this.Test("a[]");
			this.Test("a{}");
			this.Test("v[i]");
			this.Test("M[x,y]");
			this.Test("M[x,]");
			this.Test("M[,y]");
			this.Test("a[,]");
		}

		[TestMethod]
		public void Parsing_Test_22_ObjectExNihilo()
		{
			this.Test("{Member1:Value1, Member2:Value2, MemberN:ValueN}");
			this.Test("{\"Member1\":\"Value1\", \"Member2\":\"Value2\", \"MemberN\":\"ValueN\"}");
		}

		[TestMethod]
		public void Parsing_Test_23_Sets()
		{
			this.Test("S:={1,2,3};");
			this.Test("S:={DO x++ WHILE X<10};");
			this.Test("S:={WHILE x<10 : x++};");
			this.Test("S:={FOR x:=1 TO 20 STEP 3 : x};");
			this.Test("S:={FOREACH x IN 1..10|0.1 : x^2};");
			this.Test("S:={FOR EACH x IN 1..10|0.1 : x^2};");
		}

		[TestMethod]
		public void Parsing_Test_24_Matrices()
		{
			this.Test("M:=[[1,0,0],[0,1,0],[0,0,1]];");
			this.Test("M:=[DO [x++,x++,x++] WHILE X<10];");
			this.Test("M:=[WHILE x<10 : [x++,x++,x++]];");
			this.Test("M:=[FOR y:=1 TO 20 : [FOR x:=1 TO 20 : x=y ? 1 : 0]];");
			this.Test("M:=[FOREACH x IN 1..10|0.1 : [x^2,x^3,x^4]];");
			this.Test("M:=[FOR EACH x IN 1..10|0.1 : [x^2,x^3,x^4]];");
		}

		[TestMethod]
		public void Parsing_Test_25_Vectors()
		{
			this.Test("v:=[1,2,3];");
			this.Test("v:=[DO x++ WHILE X<10];");
			this.Test("v:=[WHILE x<10 : x++];");
			this.Test("v:=[FOR x:=1 TO 20 STEP 3 : x];");
			this.Test("v:=[FOREACH x IN 1..10|0.1 : x^2];");
			this.Test("v:=[FOR EACH x IN 1..10|0.1 : x^2];");
		}

		[TestMethod]
		public void Parsing_Test_26_Parenthesis()
		{
			this.Test("a * (b + c)");
		}

		[TestMethod]
		public void Parsing_Test_27_null()
		{
			this.Test("null");
		}

		[TestMethod]
		public void Parsing_Test_28_StringValues()
		{
			this.Test("\"Hello\\r\\n\\t\\f\\b\\a\\v\\\\\\\"\\''\"\x03");
			this.Test("'Hello\\r\\n\\t\\f\\b\\a\\v\\\\\\\"\\'\"'\x03");
		}

		[TestMethod]
		public void Parsing_Test_29_BooleanValues()
		{
			this.Test("true");
			this.Test("false");
		}

		[TestMethod]
		public void Parsing_Test_30_DoubleValues()
		{
			this.Test("1");
			this.Test("3.1415927");
			this.Test("1.23e-3");
		}

		[TestMethod]
		public void Parsing_Test_31_Constants()
		{
			this.Test("e");
			this.Test("pi");
			this.Test("π");
		}

		[TestMethod]
		public void Parsing_Test_32_BinomialCoefficients()
		{
			this.Test("n OVER k");
		}

		[TestMethod]
		public void Parsing_Test_33_NullCheckSuffixOperators()
		{
			this.Test("obj?.Member");
			this.Test("f?(a,b,c)");
			this.Test("a?[]");
			this.Test("a?{}");
			this.Test("v?[i]");
			this.Test("M?[x,y]");
			this.Test("M?[x,]");
			this.Test("M?[,y]");
			this.Test("a?[,]");
		}

		[TestMethod]
		public void Parsing_Test_34_ImplicitSetNotation()
		{
			this.Test("S:={x in Z:x>10}");
			this.Test("S2:={x in S:x<20}");
			this.Test("S:=1..20;S2:={x in S:x>10}");
			this.Test("S:={[a,b]: a>b}");
			this.Test("S:={[a,b]: a in Z, b in Z, a>b}");
			this.Test("S:={[a,b,c]:a in 1..2, b in 10..13, c in 100..104}");
			this.Test("S:={v[]:count(v)>3}");
			this.Test("S:={s{}:count(s)>3}");
			this.Test("S:={M[,]:M.Columns>M.Rows}");
			this.Test("S:={x::x>\"Hello\"}");
			this.Test("v:=[1,2,3,4,5,6,7,8,9,10];{x in v:x>5}");
		}

		[TestMethod]
		public void Parsing_Test_35_ImplicitVectorNotation()
		{
			this.Test("v:=[1,2,3,4,5,6,7,8,9,10];[x in v:x>5]");
			this.Test("v:=1..100;[x in v:floor(sqrt(x))^2=x]");
			this.Test("X:=1..10;P:=[x^2:x in X]");
		}

		[TestMethod]
		public void Parsing_Test_36_ImplicitMatrixNotation()
		{
			this.Test("v:=1..100;[[x,y]:x in v,(y:=floor(sqrt(x)))^2=x]");
			this.Test("X:=1..10;Y:=20..30;P:=[[x,y]:x in X, y in Y]");
			this.Test("M:=Identity(5);[Reverse(Row):Row in M]");
		}

	}
}
