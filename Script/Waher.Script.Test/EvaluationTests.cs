using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Objects.Sets;

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
            v["t"] = "Bye";

            Expression Exp = new Expression(Script);
            object Result = Exp.Evaluate(v);

            if (Result is Dictionary<string, IElement> && ExpectedValue is Dictionary<string, IElement>)
            {
                Dictionary<string, IElement> R = (Dictionary<string, IElement>)Result;
                Dictionary<string, IElement> E = (Dictionary<string, IElement>)ExpectedValue;

                Assert.AreEqual(E.Count, R.Count, Script);

                foreach (KeyValuePair<string, IElement> P in E)
                {
                    Assert.IsTrue(R.ContainsKey(P.Key), Script);
                    Assert.AreEqual(P.Value.AssociatedObjectValue, R[P.Key].AssociatedObjectValue, Script);
                }
            }
            else
                Assert.AreEqual(ExpectedValue, Result, Script);
        }

        [Test]
        public void Test_01_Sequences()
        {
            this.Test("a;b;c", c);
        }

        [Test]
        public void Test_02_ConditionalStatements()
        {
            this.Test("DO ++a WHILE a<=10", 11);
            this.Test("WHILE a<=10 DO ++a", 11);
            this.Test("WHILE a<=10 : ++a", 11);
            this.Test("WHILE a<=10 ++a", 11);
            this.Test("FOREACH x IN [a,b,c] DO x", c);
            this.Test("FOREACH x IN [a,b,c] : x", c);
            this.Test("FOREACH x IN [a,b,c] x", c);
            this.Test("FOR EACH x IN [a,b,c] DO x", c);
            this.Test("FOR EACH x IN [a,b,c] : x", c);
            this.Test("FOR EACH x IN [a,b,c] x", c);
            this.Test("FOR a:=1 TO 10 STEP 2 DO a", 9);
            this.Test("FOR a:=1 TO 10 STEP 2 : a", 9);
            this.Test("FOR a:=1 TO 10 STEP 2 a", 9);
            this.Test("FOR a:=1 TO 10 DO a", 10);
            this.Test("FOR a:=1 TO 10 : a", 10);
            this.Test("FOR a:=1 TO 10 a", 10);
            this.Test("TRY a CATCH b FINALLY a:=0;a", 0);
            this.Test("a:='a';TRY a++ CATCH a:=0;a", 0);
            this.Test("TRY a FINALLY a:=0;a", 0);
        }

        [Test]
        public void Test_03_Lists()
        {
            this.Test("a,b,c", new double[] { a, b, c });
            this.Test("a,b,,c", new object[] { a, b, null, c });
        }

        [Test]
        public void Test_04_Assignments()
        {
            this.Test("x:=10;x+x", 20);
            //this.Test("[x,y]:=f(a,b,c)");
            this.Test("a+=b;a", 11);
            this.Test("a-=b;a", -1);
            this.Test("a*=b;a", 30);
            this.Test("b/=2;b", 3);
            this.Test("a^=b;a", 15625);
            this.Test("a&=b;a", 4);
            this.Test("p&&=q;p", false);
            this.Test("a|=b;a", 7);
            this.Test("q||=p;q", true);
            this.Test("a<<=b;a", 320);
            this.Test("a>>=1;a", 2);
            /*this.Test("a.b:=c");  TODO
			this.Test("a[b]:=c");   TODO
			this.Test("a[b,c]:=d");   TODO
			this.Test("a[,c]:=d");   TODO
			this.Test("a[b,]:=d");   TODO
			this.Test("a(b,c):=d");   TODO
			this.Test("a(b,[c]):=d");   TODO
			this.Test("a(b,c[]):=d");   TODO
			this.Test("a(b,c[,]):=d");   TODO */
        }

        [Test]
        public void Test_05_IF()
        {
            this.Test("IF a<b THEN a", a);
            this.Test("IF a>b THEN a", null);
            this.Test("IF a<b THEN a ELSE b", a);
            this.Test("IF a>b THEN a ELSE b", b);
            this.Test("IF a<b a", a);
            this.Test("IF a>b a", null);
            this.Test("IF a<b a ELSE b", a);
            this.Test("IF a>b a ELSE b", b);
            this.Test("a<b ? a", a);
            this.Test("a>b ? a", null);
            this.Test("a<b ? a : b", a);
            this.Test("a>b ? a : b", b);

            this.Test("IF b<=[a,b,c] THEN a ELSE b", new double[] { b, a, a });
            this.Test("b<=[a,b,c] ? [1,2,3] : [4,5,6]", new double[] { 4, 2, 3 });
        }
        /*
		[Test]
		public void Test_06_Lambda()
		{
			this.Test("x->x^2");   TODO
			this.Test("(x,y)=>sin(x)*exp(-1/y^2)");   TODO
			this.Test("(x,[y])=>sin(x)*exp(-1/y^2)");   TODO
			this.Test("(x[],y[,])=>sin(x)*exp(-1/y^2)");   TODO
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
			this.Test("a AS T");   TODO
			this.Test("a IS T");   TODO
			this.Test("a IN M");   TODO
			this.Test("a NOT IN M");   TODO
			this.Test("a NOTIN M");   TODO
		}
		*/
        [Test]
        public void Test_12_Comparison()
        {
            this.Test("a <= b", true);
            this.Test("a <= [a,b,c]", new bool[] { true, true, true });
            this.Test("[a,b,c] <= b", new bool[] { true, true, false });
            this.Test("[a,b,c] <= [a,c,b]", new bool[] { true, true, false });

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
            this.Test("a .<> b", true);
            this.Test("a .!= b", true);
        }

        [Test]
        public void Test_13_Shift()
        {
            this.Test("a << b", 320);
            this.Test("[1,2,3] << b", new double[] { 64, 128, 192 });

            this.Test("7 >> 1", 3);
            this.Test("[7,8,9] >> 1", new double[] { 3, 4, 4 });
        }

        [Test]
        public void Test_14_Union()
        {
            this.Test("{a,b,c} UNION {4,5,6}", new object[] { 5, 6, 7, 4 });
            this.Test("{a,b,c} ∪ {4,5,6}", new object[] { 5, 6, 7, 4 });
        }

        [Test]
        public void Test_15_Intersection()
        {
            this.Test("{4,5,6} INTERSECT {a,b,c}", new object[] { 5, 6 });
            this.Test("{4,5,6} INTERSECTION {a,b,c}", new object[] { 5, 6 });
            this.Test("{4,5,6} ∩ {a,b,c}", new object[] { 5, 6 });
        }

        [Test]
        public void Test_16_Intervals()
        {
            this.Test("1..10", new Objects.Sets.Interval(1, 10, true, true, null));
            this.Test("1..10|0.1", new Objects.Sets.Interval(1, 10, true, true, 0.1));
        }

        [Test]
        public void Test_17_Terms()
        {
            this.Test("a+b", 11);
            this.Test("[1,2,3]+a", new double[] { 6, 7, 8 });
            this.Test("a+[1,2,3]", new double[] { 6, 7, 8 });
            this.Test("[1,2,3]+[a,b,c]", new double[] { 6, 8, 10 });

            this.Test("s+' '+t", "Hello Bye");
            this.Test("[1,2,3]+s", new object[] { "1Hello", "2Hello", "3Hello" });
            this.Test("s+[1,2,3]", new object[] { "Hello1", "Hello2", "Hello3" });
            this.Test("[1,2,3]+[s,t,s]", new object[] { "1Hello", "2Bye", "3Hello" });
            this.Test("[s,t,s]+[1,2,3]", new object[] { "Hello1", "Bye2", "Hello3" });
            this.Test("['a','b','c']+[s,t,s]", new object[] { "aHello", "bBye", "cHello" });
            this.Test("[s,t,s]+['a','b','c']", new object[] { "Helloa", "Byeb", "Helloc" });

            this.Test("10-a", 5);
            this.Test("[10,20,30]-a", new double[] { 5, 15, 25 });
            this.Test("[5,6,7]-[a,b,c]", new double[] { 0, 0, 0 });
            this.Test("[[10,20],[30,40]]-a", new double[,] { { 5, 15 }, { 25, 35 } });
            this.Test("a-[[1,1],[0,1]]", new double[,] { { 4, 4 }, { 5, 4 } });
            this.Test("[[a,b],[b,c]]-[[1,1],[0,1]]", new double[,] { { 4, 5 }, { 6, 6 } });

            this.Test("a.+b", 11);
            this.Test("a.+[a,b,c]", new double[] { 10, 11, 12 });
            this.Test("[a,b,c].+a", new double[] { 10, 11, 12 });
            this.Test("[a,b,c].+[a,b,c]", new double[] { 10, 12, 14 });
            this.Test("[[a,b],[b,c]].+[[a,b],[b,c]]", new double[,] { { 10, 12 }, { 12, 14 } });

            this.Test("10.-a", 5);
            this.Test("[10,20,30].-a", new double[] { 5, 15, 25 });
            this.Test("[5,6,7].-[a,b,c]", new double[] { 0, 0, 0 });
            this.Test("[[10,20],[30,40]].-a", new double[,] { { 5, 15 }, { 25, 35 } });
            this.Test("b.-[[1,2],[2,1]]", new double[,] { { 5, 4 }, { 4, 5 } });
            this.Test("[[a,b],[b,c]].-[[1,2],[2,1]]", new double[,] { { 4, 4 }, { 4, 6 } });
        }

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

            this.Test("{4,5,6}\\{a,b,c}", new object[] { 4 });

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
			this.Test("f'(x)");   TODO
			this.Test("f′(x)");   TODO
			this.Test("f\"(x)");   TODO
			this.Test("f″(x)");   TODO
			this.Test("f‴(x)");   TODO*/

            this.Test("[[a,a,a],[b,b,b],[c,c,c]]T", new double[,] { { a, b, c }, { a, b, c }, { a, b, c } });
            this.Test("[[a,a,a],[b,b,b],[c,c,c]]H", new double[,] { { a, b, c }, { a, b, c }, { a, b, c } });
            this.Test("[[a,a,a],[b,b,b],[c,c,c]]†", new double[,] { { a, b, c }, { a, b, c }, { a, b, c } });

            this.Test("a!", 120);
            this.Test("a!!", 15);
        }

        [Test]
        public void Test_21_BinarySuffixOperators()
        {
            this.Test("Obj:={m1:a,m2:b,m3:c};Obj.m1", a);
            this.Test("[a,b,c].Length", 3);
            this.Test("Obj:={m1:a,m2:b,m3:c};Obj.['m1','m2']", new double[] { a, b });

            this.Test("a[]", new double[] { a });
            this.Test("[a,b,c,a,b,c][]", new double[] { a, b, c, a, b, c });
            this.Test("{a,b,c,a,b,c}[]", new double[] { a, b, c });
            this.Test("[[a,b],[b,c]][]", new object[] { new double[] { a, b }, new double[] { b, c } });

            this.Test("a[,]", new double[,] { { a } });
            this.Test("[a,b,c][,]", new double[,] { { a, b, c } });
            this.Test("{a,b,c}[,]", new double[,] { { a, b, c } });
            this.Test("[[a,b],[b,c]][,]", new double[,] { { a, b }, { b, c } });

            this.Test("a{}", new double[] { a });
            this.Test("[a,b,c,a,b,c]{}", new double[] { a, b, c });
            this.Test("{a,b,c,a,b,c}{}", new double[] { a, b, c });
            this.Test("[[a,b],[b,c]]{}", new object[] { new double[] { a, b }, new double[] { b, c } });

            //this.Test("f(a,b,c)");   TODO

            this.Test("[a,b,c][1]", b);
            this.Test("[a,b,c][1..2]", new double[] { b, c });

            this.Test("[[a,b],[c,a]][0,1]", c);
            this.Test("[[a,b],[c,a]][0,0..1]", new double[] { a, c });

            this.Test("[[a,b],[c,a]][0,]", new double[] { a, c });
            this.Test("[[a,b],[c,a]][0..1,]", new double[,] { { a, c }, { b, a } });

            this.Test("[[a,b],[c,a]][,0]", new double[] { a, b });
            this.Test("[[a,b],[c,a]][,0..1]", new double[,] { { a, b }, { c, a } });

            this.Test("System", new Namespace("System"));
            this.Test("System.Text", new Namespace("System.Text"));
            this.Test("System.Text.RegularExpressions", new Namespace("System.Text.RegularExpressions"));
            this.Test("System.Text.RegularExpressions.Regex", typeof(System.Text.RegularExpressions.Regex));
        }

        [Test]
        public void Test_22_ObjectExNihilo()
        {
            Dictionary<string, IElement> Obj = new Dictionary<string, IElement>();
            Obj["Member1"] = new DoubleNumber(a);
            Obj["Member2"] = new DoubleNumber(b);
            Obj["Member3"] = new DoubleNumber(c);

            this.Test("{Member1:a, Member2:b, Member3:c}", Obj);

            Obj["Member1"] = new StringValue("Value1");
            Obj["Member2"] = new StringValue("Value2");
            Obj["Member3"] = new StringValue("Value3");

            this.Test("{\"Member1\":\"Value1\", \"Member2\":\"Value2\", \"Member3\":\"Value3\"}", Obj);
        }

        [Test]
        public void Test_23_Sets()
        {
            this.Test("S:={1,2,3};", new object[] { 1, 2, 3 });
            this.Test("S:={DO a++ WHILE a<10};", new object[] { 5, 6, 7, 8, 9 });
            this.Test("S:={WHILE a<10 : a++};", new object[] { 5, 6, 7, 8, 9 });
            this.Test("S:={FOR x:=1 TO 20 STEP 3 : x};", new object[] { 1, 4, 7, 10, 13, 16, 19 });
            this.Test("S:={FOREACH x IN 1..10|2 : x^2};", new object[] { 1, 9, 25, 49, 81 });
            this.Test("S:={FOR EACH x IN 1..10|2 : x^2};", new object[] { 1, 9, 25, 49, 81 });
        }

        [Test]
        public void Test_24_Matrices()
        {
            this.Test("[[a,b,c],[b,c,a]];", new double[,] { { a, b, c }, { b, c, a } });
            this.Test("[DO [a++,a++,a++] WHILE a<10];", new double[,] { { 5, 6, 7 }, { 8, 9, 10 } });
            this.Test("[WHILE a<10 : [a++,a++,a++]];", new double[,] { { 5, 6, 7 }, { 8, 9, 10 } });
            this.Test("[FOR y:=1 TO 3 : [FOR x:=1 TO 3 : x=y ? 1 : 0]];", new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
            this.Test("[FOREACH x IN 1..10|2 : [x,x+1,x+2]];", new double[,] { { 1, 2, 3 }, { 3, 4, 5 }, { 5, 6, 7 }, { 7, 8, 9 }, { 9, 10, 11 } });
            this.Test("[FOR EACH x IN 1..10|2 : [x,x+1,x+2]];", new double[,] { { 1, 2, 3 }, { 3, 4, 5 }, { 5, 6, 7 }, { 7, 8, 9 }, { 9, 10, 11 } });
        }

        [Test]
        public void Test_25_Vectors()
        {
            this.Test("[a,b,c];", new double[] { a, b, c });
            this.Test("[DO a++ WHILE a<10];", new double[] { 5, 6, 7, 8, 9 });
            this.Test("[WHILE a<10 : a++];", new double[] { 5, 6, 7, 8, 9 });
            this.Test("[FOR x:=1 TO 20 STEP 3 : x];", new double[] { 1, 4, 7, 10, 13, 16, 19 });
            this.Test("[FOREACH x IN 1..10|2 : x^2];", new double[] { 1, 9, 25, 49, 81 });
            this.Test("[FOR EACH x IN 1..10|2 : x^2];", new double[] { 1, 9, 25, 49, 81 });
        }

        [Test]
        public void Test_26_Parenthesis()
        {
            this.Test("a * (b + c)", 65);
        }

        [Test]
        public void Test_27_null()
        {
            this.Test("null", null);
        }

        [Test]
        public void Test_28_StringValues()
        {
            this.Test("\"Hello\\r\\n\\t\\f\\b\\a\\v\\\\\\\"\\''\"", "Hello\r\n\t\f\b\a\v\\\"\''");
            this.Test("'Hello\\r\\n\\t\\f\\b\\a\\v\\\\\\\"\\'\"'", "Hello\r\n\t\f\b\a\v\\\"\'\"");
        }

        [Test]
        public void Test_29_BooleanValues()
        {
            this.Test("true", true);
            this.Test("false", false);
        }

        [Test]
        public void Test_30_DoubleValues()
        {
            this.Test("1", 1);
            this.Test("3.1415927", 3.1415927);
            this.Test("1.23e-3", 1.23e-3);
        }

        [Test]
        public void Test_31_Constants()
        {
            this.Test("e", Math.E);
            this.Test("pi", Math.PI);
            this.Test("π", Math.PI);
            this.Test("R", RealNumbers.Instance);
            this.Test("EmptySet", EmptySet.Instance);
            this.Test("∅", EmptySet.Instance);
        }

        [Test]
        public void Test_32_BinomialCoefficients()
        {
            this.Test("c OVER a", 21);
            this.Test("8 OVER [0,1,2,3,4,5,6,7,8]", new double[] { 1, 8, 28, 56, 70, 56, 28, 8, 1 });
            this.Test("[0,1,2,3,4,5,6,7,8] OVER 0", new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            this.Test("[0,1,2,3,4,5,6,7,8] OVER [0,1,2,3,4,5,6,7,8]", new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        }

    }
}
