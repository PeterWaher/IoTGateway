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
			this.Test("a~=b");
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


	}
}
