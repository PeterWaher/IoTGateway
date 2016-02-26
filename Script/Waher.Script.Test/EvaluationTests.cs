using System;
using System.Numerics;
using System.Collections.Generic;
using NUnit.Framework;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Objects;
using Waher.Script.Objects.Matrices;
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
            v["z1"] = new Complex(1, 2);
            v["z2"] = new Complex(3, 4);

            Expression Exp = new Expression(Script);
            object Result = Exp.Evaluate(v);

            if (Result is BooleanMatrix)
                Result = ((BooleanMatrix)Result).Values;
            else if (Result is DoubleMatrix)
                Result = ((DoubleMatrix)Result).Values;
            else if (Result is ComplexMatrix)
                Result = ((ComplexMatrix)Result).Values;
            else if (Result is ObjectMatrix)
                Result = ((ObjectMatrix)Result).Values;

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

            this.Test("[x,y]:=[1,2];[a,b,c,x,y]", new double[] { 5, 6, 7, 1, 2 });
            this.Test("[[x,y],[y,z]]:=[[a,b],[b,c]];[a,b,c,x,y,z]", new double[] { 5, 6, 7, 5, 6, 7 });
            this.Test("{x,y,z}:={a,b,c};x+y+z", 18);

            this.Test("f(x):=x^2;f(10)", 100);
            this.Test("f(x,y):=x*y;f(2,3)", 6);

            this.Test("f(x,[y]):=x*y;f(2,3)", 6);
            this.Test("f(x,[y]):=x*y;f(2,[3,4,5])", new double[] { 6, 8, 10 });
            this.Test("f(x,[y]):=x*y;f(2,[[3,4],[5,6]])", new double[,] { { 6, 8 }, { 10, 12 } });
            this.Test("f(x,[y]):=x*y;f(2,{3,3,4,5})", new double[] { 6, 8, 10 });

            this.Test("f(y[]):=y.Length;f([[1,2],[3,4]])", new double[] { 2, 2 });
            this.Test("f(y[]):=y.Length;f([1,2,3])", 3);
            this.Test("f(y[]):=y.Length;f({1,2,3})", new double[] { 1 });
            this.Test("f(y[]):=y.Length;f(3)", 1);

            this.Test("f(y[,]):=y.Rows;f(3)", 1);
            this.Test("f(y[,]):=y.Rows;f([1,2,3])", 1);
            this.Test("f(y[,]):=y.Rows;f([[1,2],[3,4]])", 2);
            this.Test("f(y[,]):=y.Rows;f({1,2,3})", new double[] { 1 });

            this.Test("f(y{}):=y union {2,3};f(3)", new object[] { 3, 2 });
            this.Test("f(y{}):=y union {2,3};f([1,2,3])", new object[] { 1, 2, 3 });
            this.Test("f(y{}):=y union {2,3};f([[1,2],[3,4]])", new object[] { new object[] { 1, 2, 3 }, new object[] { 3, 4, 2 } });
            this.Test("f(y{}):=y union {2,3};f({1,2,3})", new object[] { 1, 2, 3 });

            this.Test("Obj:={m1:a,m2:b,m3:c};Obj.m2:=a+b+c;Obj.m2", 18);
            this.Test("v:=[1,2,3];v[1]:=a;v", new double[] { 1, 5, 3 });
            this.Test("M:=[[1,2],[3,4]];M[0,1]:=a;M", new double[,] { { 1, 2 }, { 5, 4 } });
            this.Test("M:=[[1,2],[3,4]];M[,1]:=[a,b];M", new double[,] { { 1, 2 }, { 5, 6 } });
            this.Test("M:=[[1,2],[3,4]];M[1,]:=[a,b];M", new double[,] { { 1, 5 }, { 3, 6 } });
            this.Test("Obj:={m1:a,m2:b,m3:c};s:='m';Obj.(s+'1'):=a+b+c;Obj.m1", 18);

            // TODO: Test dynamic index, and dynamic index assignment.
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

        [Test]
        public void Test_06_Lambda()
        {
            this.Test("(x->x^2)(10)", 100);
            this.Test("((x,y)->x*y)(2,3)", 6);

            this.Test("((x,[y])->x*y)(2,3)", 6);
            this.Test("((x,[y])->x*y)(2,[3,4,5])", new double[] { 6, 8, 10 });
            this.Test("((x,[y])->x*y)(2,[[3,4],[5,6]])", new double[,] { { 6, 8 }, { 10, 12 } });
            this.Test("((x,[y])->x*y)(2,{3,3,4,5})", new double[] { 6, 8, 10 });

            this.Test("(y[]->y.Length)(3)", 1);
            this.Test("(y[]->y.Length)([1,2,3])", 3);
            this.Test("(y[]->y.Length)([[1,2],[3,4]])", new double[] { 2, 2 });
            this.Test("(y[]->y.Length)({1,2,3})", new double[] { 1 });

            this.Test("(y[,]->y.Rows)(3)", 1);
            this.Test("(y[,]->y.Rows)([1,2,3])", 1);
            this.Test("(y[,]->y.Rows)([[1,2],[3,4]])", 2);
            this.Test("(y[,]->y.Rows)({1,2,3})", new double[] { 1 });

            this.Test("(y{}->y union {2,3})(3)", new object[] { 3, 2 });
            this.Test("(y{}->y union {2,3})([1,2,3])", new object[] { 1, 2, 3 });
            this.Test("(y{}->y union {2,3})([[1,2],[3,4]])", new object[] { new object[] { 1, 2, 3 }, new object[] { 3, 4, 2 } });
            this.Test("(y{}->y union {2,3})({1,2,3})", new object[] { 1, 2, 3 });
        }

        [Test]
        public void Test_07_Implication()
        {
            this.Test("p => q", false);
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
            this.Test("p <=> q", false);

            this.Test("[p,q,r] <=> q", new bool[] { false, true, false });
            this.Test("p <=> [p,q,r]", new bool[] { true, false, true });
            this.Test("[p,r,q] <=> [p,q,r]", new bool[] { true, false, false });

            this.Test("[[p,q,r],[q,r,p]] <=> q", new bool[,] { { false, true, false }, { true, false, false } });
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

        [Test]
        public void Test_11_Membership()
        {
            this.Test("a AS System.Double", a);
            this.Test("a AS System.Int32", null);
            this.Test("[a,b] AS System.Double", null);
            this.Test("a AS [System.Double,System.Int32]", new object[] { a, null });

            this.Test("a IS System.Double", true);
            this.Test("a IS System.Int32", false);
            this.Test("[a,b] IS System.Double", false);
            this.Test("a IS [System.Double,System.Int32]", new bool[] { true, false });

            this.Test("a IN R", true);
            this.Test("a IN EmptySet", false);
            this.Test("[a,b] IN R", false);
            this.Test("a IN [R,EmptySet]", new bool[] { true, false });

            this.Test("a NOT IN R", false);
            this.Test("a NOTIN EmptySet", true);
            this.Test("[a,b] NOT IN R", true);
            this.Test("a NOTIN [R,EmptySet]", new bool[] { false, true });
        }

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
            this.Test("if s LIKE \"H(?'Rest'.*)\" then Rest", "ello");
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

            this.Test("z1+2+3*i", new Complex(3, 5));
            this.Test("z1+z2", new Complex(4, 6));
            this.Test("z1-z2", new Complex(-2, -2));
            this.Test("z1+2", new Complex(3, 2));
            this.Test("2+z1", new Complex(3, 2));

            this.Test("[z1,z2]+2", new Complex[] { new Complex(3, 2), new Complex(5, 4) });
            this.Test("2+[z1,z2]", new Complex[] { new Complex(3, 2), new Complex(5, 4) });
            this.Test("[z1,z2]+z2", new Complex[] { new Complex(4, 6), new Complex(6, 8) });
            this.Test("z2+[z1,z2]", new Complex[] { new Complex(4, 6), new Complex(6, 8) });
            this.Test("[z1,z2]+[1,2]", new Complex[] { new Complex(2, 2), new Complex(5, 4) });
            this.Test("[1,2]+[z1,z2]", new Complex[] { new Complex(2, 2), new Complex(5, 4) });
            this.Test("[z1,z2]+[1,(2,3)]", new Complex[] { new Complex(2, 2), new Complex(5, 7) });
            this.Test("[1,(2,3)]+[z1,z2]", new Complex[] { new Complex(2, 2), new Complex(5, 7) });
            this.Test("[z1,z2]+[z1,z2]", new Complex[] { new Complex(2, 4), new Complex(6, 8) });

            this.Test("[[z1,z2],[z2,z1]]+2", new Complex[,] { { new Complex(3, 2), new Complex(5, 4) }, { new Complex(5, 4), new Complex(3, 2) } });
            this.Test("2+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(3, 2), new Complex(5, 4) }, { new Complex(5, 4), new Complex(3, 2) } });
            this.Test("[[z1,z2],[z2,z1]]+z2", new Complex[,] { { new Complex(4, 6), new Complex(6, 8) }, { new Complex(6, 8), new Complex(4, 6) } });
            this.Test("z2+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(4, 6), new Complex(6, 8) }, { new Complex(6, 8), new Complex(4, 6) } });
            this.Test("[[z1,z2],[z2,z1]]+[[1,2],[3,4]]", new Complex[,] { { new Complex(2, 2), new Complex(5, 4) }, { new Complex(6, 4), new Complex(5, 2) } });
            this.Test("[[1,2],[3,4]]+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(2, 2), new Complex(5, 4) }, { new Complex(6, 4), new Complex(5, 2) } });
            this.Test("[[z1,z2],[z2,z1]]+[[1,2],[3,(2,3)]]", new Complex[,] { { new Complex(2, 2), new Complex(5, 4) }, { new Complex(6, 4), new Complex(3, 5) } });
            this.Test("[[1,2],[3,(2,3)]]+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(2, 2), new Complex(5, 4) }, { new Complex(6, 4), new Complex(3, 5) } });
            this.Test("[[z1,z2],[z2,z1]]+[[z1,z2],[z2,z1]]", new Complex[,] { { new Complex(2, 4), new Complex(6, 8) }, { new Complex(6, 8), new Complex(2, 4) } });
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
            this.Test("Obj:={m1:a,m2:b,m3:c};s:='m';Obj.(s+'1')", a);

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
            this.Test("i", Complex.ImaginaryOne);
            this.Test("C", ComplexNumbers.Instance);
            this.Test("R", RealNumbers.Instance);
            this.Test("EmptySet", EmptySet.Instance);
            this.Test("∅", EmptySet.Instance);
            this.Test("Now.Date", DateTime.Now.Date);
            this.Test("Today", DateTime.Today);
            this.Test("ε", double.Epsilon);
            this.Test("eps", double.Epsilon);
            this.Test("epsilon", double.Epsilon);
            this.Test("∞", double.PositiveInfinity);
            this.Test("inf", double.PositiveInfinity);
            this.Test("infinity", double.PositiveInfinity);
        }

        [Test]
        public void Test_32_BinomialCoefficients()
        {
            this.Test("c OVER a", 21);
            this.Test("8 OVER [0,1,2,3,4,5,6,7,8]", new double[] { 1, 8, 28, 56, 70, 56, 28, 8, 1 });
            this.Test("[0,1,2,3,4,5,6,7,8] OVER 0", new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            this.Test("[0,1,2,3,4,5,6,7,8] OVER [0,1,2,3,4,5,6,7,8]", new double[] { 1, 1, 1, 1, 1, 1, 1, 1, 1 });
        }

        [Test]
        public void Test_33_ComplexNumbers()
        {
            this.Test("(1,2)", new Complex(1, 2));
            this.Test("1+2*i", new Complex(1, 2));
        }

        [Test]
        public void Test_34_VectorFunctions()
        {
            this.Test("Min([2,4,1,3])", 1);
            this.Test("Max([2,4,1,3])", 4);
            this.Test("Sum([1,2,3,4,5])", 15);
            this.Test("Average([1,2,3,4,5])", 3);
            this.Test("Avg([1,2,3,4,5])", 3);
            this.Test("Product([1,2,3,4,5])", 120);
            this.Test("Prod([1,2,3,4,5])", 120);
            this.Test("Variance([1,2,3,4,5])", 2);
            this.Test("Var([1,2,3,4,5])", 2);
            this.Test("StandardDeviation([1,2,3,4,5])", Math.Sqrt(2.5));
            this.Test("StdDev([1,2,3,4,5])", Math.Sqrt(2.5));
            this.Test("Median([1,2,3,4,5])", 3);
            this.Test("And([true,false,true])", false);
            this.Test("And([3,2,1])", 0);
            this.Test("Or([true,false,true])", true);
            this.Test("Or([3,2,1])", 3);
            this.Test("Xor([true,false,true])", false);
            this.Test("Xor([3,2,1])", 0);
            this.Test("Nand([true,false,true])", true);
            this.Test("Nand([3,2,1])", 0xffffffffffffffff);
            this.Test("Nor([true,false,true])", false);
            this.Test("Nor([3,2,1])", unchecked((ulong)-4));
            this.Test("Xnor([true,false,true])", true);
            this.Test("Xnor([3,2,1])", 0xffffffffffffffff);
        }

        [Test]
        public void Test_35_AnalyticFunctions()
        {
            this.Test("sin(5)", Math.Sin(5));
            this.Test("cos(5)", Math.Cos(5));
            this.Test("tan(5)", Math.Tan(5));
            this.Test("sec(5)", 1.0 / Math.Cos(5));
            this.Test("csc(5)", 1.0 / Math.Sin(5));
            this.Test("cot(5)", 1.0 / Math.Tan(5));

            this.Test("round(arcsin(sin(1))*1e6)/1e6", 1);
            this.Test("round(arccos(cos(1))*1e6)/1e6", 1);
            this.Test("round(arctan(tan(1))*1e6)/1e6", 1);
            this.Test("round(arcsec(sec(1))*1e6)/1e6", 1);
            this.Test("round(arccsc(csc(1))*1e6)/1e6", 1);
            this.Test("round(arccot(cot(1))*1e6)/1e6", 1);
            this.Test("atan(3,4)", Math.Atan2(4, 3));
            this.Test("arctan(3,4)", Math.Atan2(4, 3));

            this.Test("sin(i)", Complex.Sin(Complex.ImaginaryOne));
            this.Test("cos(i)", Complex.Cos(Complex.ImaginaryOne));
            this.Test("tan(i)", Complex.Tan(Complex.ImaginaryOne));
            this.Test("sec(i)", 1.0 / Complex.Cos(Complex.ImaginaryOne));
            this.Test("csc(i)", 1.0 / Complex.Sin(Complex.ImaginaryOne));
            this.Test("cot(i)", 1.0 / Complex.Tan(Complex.ImaginaryOne));

            this.Test("round(arcsin(sin(i))*1e6)/1e6", Complex.ImaginaryOne);
            this.Test("round(arccos(cos(i))*1e6)/1e6", Complex.ImaginaryOne);
            this.Test("round(arctan(tan(i))*1e6)/1e6", Complex.ImaginaryOne);
            this.Test("round(arcsec(sec(i))*1e6)/1e6", Complex.ImaginaryOne);
            this.Test("round(arccsc(csc(i))*1e6)/1e6", Complex.ImaginaryOne);
            this.Test("round(arccot(cot(i))*1e6)/1e6", Complex.ImaginaryOne);

            this.Test("sinh(5)", Math.Sinh(5));
            this.Test("cosh(5)", Math.Cosh(5));
            this.Test("tanh(5)", Math.Tanh(5));
            this.Test("sech(5)", 1.0 / Math.Cosh(5));
            this.Test("csch(5)", 1.0 / Math.Sinh(5));
            this.Test("coth(5)", 1.0 / Math.Tanh(5));

            this.Test("round(arcsinh(sinh(1))*1e6)/1e6", 1);
            this.Test("round(arccosh(cosh(1))*1e6)/1e6", 1);
            this.Test("round(arctanh(tanh(1))*1e6)/1e6", 1);
            this.Test("round(arcsech(sech(1))*1e6)/1e6", 1);
            this.Test("round(arccsch(csch(1))*1e6)/1e6", 1);
            this.Test("round(arccoth(coth(1))*1e6)/1e6", 1);

            this.Test("sinh(i)", Complex.Sinh(Complex.ImaginaryOne));
            this.Test("cosh(i)", Complex.Cosh(Complex.ImaginaryOne));
            this.Test("tanh(i)", Complex.Tanh(Complex.ImaginaryOne));
            this.Test("sech(i)", 1.0 / Complex.Cosh(Complex.ImaginaryOne));
            this.Test("csch(i)", 1.0 / Complex.Sinh(Complex.ImaginaryOne));
            this.Test("coth(i)", 1.0 / Complex.Tanh(Complex.ImaginaryOne));

            this.Test("round(arcsinh(sinh(i))*1e6)/1e6", Complex.ImaginaryOne);
            this.Test("round(arccosh(cosh(i))*1e6)/1e6", Complex.ImaginaryOne);
            this.Test("round(arctanh(tanh(i))*1e6)/1e6", Complex.ImaginaryOne);
            this.Test("round(arcsech(sech(i))*1e6)/1e6", Complex.ImaginaryOne);
            this.Test("round(arccsch(csch(i))*1e6)/1e6", Complex.ImaginaryOne);
            this.Test("round(arccoth(coth(i))*1e6)/1e6", Complex.ImaginaryOne);

            this.Test("exp(1)", Math.E);
            this.Test("ln(e)", 1);
            this.Test("log10(10)", 1);
            this.Test("lg(10)", 1);
            this.Test("log2(2)", 1);
            this.Test("sqrt(4)", 2);
        }

        [Test]
        public void Test_36_ScalarFunctions()
        {
            this.Test("abs(-1)", 1);
            this.Test("ceil(pi)", 4);
            this.Test("ceiling(pi)", 4);
            this.Test("floor(pi)", 3);
            this.Test("round(pi)", 3);
            this.Test("sign(pi)", 1);

            this.Test("abs(i)", 1);
            this.Test("ceil(pi*i)", new Complex(0, 4));
            this.Test("ceiling(pi*i)", new Complex(0, 4));
            this.Test("floor(pi*i)", new Complex(0, 3));
            this.Test("round(pi*i)", new Complex(0, 3));
            this.Test("sign(pi*i)", new Complex(0, 1));

            this.Test("min(a,b)", a);
            this.Test("max(a,b)", b);

            this.Test("number(a)", a);
            this.Test("number(true)", 1);
            this.Test("num(i)", Complex.ImaginaryOne);
            this.Test("num('100')", 100);

            this.Test("string(a)", "5");
            this.Test("string(true)", "True");
            this.Test("str(i)", "(0, 1)");
            this.Test("str('100')", "100");
        }

        [Test]
        public void Test_37_ComplexFunctions()
        {
            this.Test("Re(2+i)", 2);
            this.Test("Im(2+i)", 1);
            this.Test("Arg(i)", Math.PI / 2);
            this.Test("Conjugate(2+i)", new Complex(2, -1));
            this.Test("Conj(2+i)", new Complex(2, -1));
            this.Test("round(Polar(1,pi/2)*1e6)*1e-6", Complex.ImaginaryOne);
        }

        [Test]
        public void Test_38_Matrices()
        {
            this.Test("invert(2)", 0.5);
            this.Test("inv(2)", 0.5);
            this.Test("inverse(i)", new Complex(0, -1));
            this.Test("invert([[1,1],[0,1]])", new double[,] { { 1, -1 }, { 0, 1 } });
        }

        [Test]
        public void Test_39_Runtime()
        {
            this.Test("exists(a)", true);
            this.Test("exists(k)", false);

            this.Test("f(x):=(return(x+1);x+2);f(3)", 4);
            this.Test("return(1);2", 1);

            this.Test("exists(error('hej'))", false);
            this.Test("exists(Exception('hej'))", false);

            this.Test("print('hej')", "hej");
            this.Test("printline('hej')", "hej");
            this.Test("println('hej')", "hej");

            this.Test("x:=10;remove(x);exists(x)", false);
            this.Test("x:=10;destroy(x);exists(x)", false);
            this.Test("x:=10;delete(x);exists(x)", false);
            this.Test("Create(System.String,'-',80)", new string('-', 80));
            this.Test("Create(System.Collections.Generic.List, System.String)", new System.Collections.Generic.List<string>());
        }
    }
}