using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
    public class PieVariation : FlameVariationMultipleParameters
    {
        private double slices;
        private double rotation;
        private double thickness;

		public PieVariation(ScriptNode slices, ScriptNode rotation, ScriptNode thickness, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { slices, rotation, thickness }, new ArgumentType[] { ArgumentType.Scalar,
				ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
        {
            this.slices = 0;
            this.rotation = 0;
            this.thickness = 0;
        }

		public PieVariation(double Slices, double Rotation, double Thickness, 
			ScriptNode slices, ScriptNode rotation, ScriptNode thickness, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { slices, rotation, thickness }, new ArgumentType[] { ArgumentType.Scalar,
				ArgumentType.Scalar, ArgumentType.Scalar }, Start, Length, Expression)
        {
            this.slices = Slices;
            this.rotation = Rotation;
            this.thickness = Thickness;
        }

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[] { "slices", "rotation", "thickness" };
			}
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
        {
            double Slices = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
            double Rotation = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
            double Thickness = Expression.ToDouble(Arguments[2].AssociatedObjectValue);

            return new PieVariation(Slices, Rotation, Thickness, this.Arguments[0], this.Arguments[1], this.Arguments[2],
				this.Start, this.Length, this.Expression);
		}

        public override void Operate(ref double x, ref double y)
        {
            double rnd1;
            double rnd2;
            double rnd3;

            lock (this.gen)
            {
                rnd1 = this.gen.NextDouble();
                rnd2 = this.gen.NextDouble();
                rnd3 = this.gen.NextDouble();
            }

            double t1 = System.Math.Round(this.slices * rnd1);
            double t2 = this.rotation + 2 * System.Math.PI / this.slices * (t1 + rnd2 * this.thickness);
            x = rnd3 * System.Math.Cos(t2);
            y = rnd3 * System.Math.Sin(t2);
        }

        private Random gen = new Random();

        public override string FunctionName
        {
            get { return "PieVariation"; }
        }
    }
}
