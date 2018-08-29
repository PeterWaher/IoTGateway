using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Waher.Security.EllipticCurves.Test
{
    [TestClass]
    public class Arithmetic
    {
        [TestMethod]
        public void Test_01_Double()
        {
			NistP192 Curve = new NistP192();

			PointOnCurve P = new PointOnCurve(1, 1);
			PointOnCurve Q = Curve.Double(P);
        }
	}
}
