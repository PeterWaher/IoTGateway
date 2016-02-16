using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Waher.Script.Test
{
	[TestFixture]
	public class ParsingTests
	{
		private void Test(string Script)
		{
			new Expression(Script);
		}

		[Test]
		public void Test_01_Sequences()
		{
			this.Test("a;b;c");
		}

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

		[Test]
		public void Test_07_Implication()
		{
			this.Test("a -> b");
			this.Test("a => b");
		}

		[Test]
		public void Test_08_Equivalence()
		{
			this.Test("a <-> b");
			this.Test("a <=> b");
		}

		[Test]
		public void Test_09_OR()
		{
			this.Test("a | b | c");
			this.Test("a || b || c");
			this.Test("a OR b OR c");
			this.Test("a NOR b NOR c");
			this.Test("a XOR b XOR c");
			this.Test("a XNOR b XNOR c");
		}

		[Test]
		public void Test_10_AND()
		{
			this.Test("a & b & c");
			this.Test("a && b && c");
			this.Test("a AND b AND c");
			this.Test("a NAND b NAND c");
		}

		[Test]
		public void Test_11_Membership()
		{
			this.Test("a AS T");
			this.Test("a IS T");
			this.Test("a IN M");
			this.Test("a NOT IN M");
			this.Test("a NOTIN M");
		}

		[Test]
		public void Test_12_Comparison()
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

		[Test]
		public void Test_18_Factors()
		{
			this.Test("a*b");
			this.Test("a/b");
			this.Test("a\\b");
			this.Test("a MOD b");
			this.Test("a.*b");
			this.Test("a./b");
			this.Test("a.\\b");
			this.Test("a DOT b");
			this.Test("a CROSS b");
			this.Test("a CARTESIAN b");
		}

		[Test]
		public void Test_18_Powers()
		{
			this.Test("a^b");
			this.Test("a.^b");
			this.Test("a²");
			this.Test("a³");
		}

		[Test]
		public void Test_19_UnaryPrefixOperators()
		{
			this.Test("++a");
			this.Test("--a");
			this.Test("+a");
			this.Test("-a");
			this.Test("!a");
			this.Test("NOT a");
			this.Test("~a");
		}

		[Test]
		public void Test_20_UnarySuffixOperators()
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

		[Test]
		public void Test_24_Matrices()
		{
			this.Test("M:=[[1,0,0],[0,1,0],[0,0,1]];");
			this.Test("M:=[DO [x++,x++,x++] WHILE X<10];");
			this.Test("M:=[WHILE x<10 : [x++,x++,x++]];");
			this.Test("M:=[FOR y:=1 TO 20 : [FOR x:=1 TO 20 : x=y ? 1 : 0]];");
			this.Test("M:=[FOREACH x IN 1..10|0.1 : [x^2,x^3,x^4]];");
			this.Test("M:=[FOR EACH x IN 1..10|0.1 : [x^2,x^3,x^4]];");
		}

		[Test]
		public void Test_25_Vectors()
		{ 
			this.Test("v:=[1,2,3];");
			this.Test("v:=[DO x++ WHILE X<10];");
			this.Test("v:=[WHILE x<10 : x++];");
			this.Test("v:=[FOR x:=1 TO 20 STEP 3 : x];");
			this.Test("v:=[FOREACH x IN 1..10|0.1 : x^2];");
			this.Test("v:=[FOR EACH x IN 1..10|0.1 : x^2];");
		}

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

		[Test]
		public void Test_31_Constants()
		{
			this.Test("e");
			this.Test("pi");
			this.Test("π");
		}

	}
}
