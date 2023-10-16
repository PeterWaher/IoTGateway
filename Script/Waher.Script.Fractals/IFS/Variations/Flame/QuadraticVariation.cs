using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.Flame
{
	public class QuadraticVariation : FlameVariationMultipleParameters
	{
		private readonly double[] cx;
		private readonly double[] cy;

		public QuadraticVariation(ScriptNode Coefficients, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Coefficients }, argumentTypes1Matrix, Start, Length, Expression)
		{
			this.cx = new double[6];
			this.cy = new double[6];
		}

		public QuadraticVariation(ScriptNode XCoefficients, ScriptNode YCoefficients, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { XCoefficients, YCoefficients }, new ArgumentType[] { ArgumentType.Vector, ArgumentType.Vector },
				  Start, Length, Expression)
		{
			this.cx = new double[6];
			this.cy = new double[6];
		}

		public QuadraticVariation(ScriptNode cx1, ScriptNode cx2, ScriptNode cx3,
			ScriptNode cx4, ScriptNode cx5, ScriptNode cx6,
			ScriptNode cy1, ScriptNode cy2, ScriptNode cy3,
			ScriptNode cy4, ScriptNode cy5, ScriptNode cy6,
			int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { cx1, cx2, cx3, cx4, cx5, cx6, cy1, cy2, cy3, cy4, cy5, cy6 },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
					  ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
					  ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar,
					  ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
			this.cx = new double[6];
			this.cy = new double[6];
		}

		public QuadraticVariation(double cx1, double cx2, double cx3, double cx4, double cx5, double cx6,
			double cy1, double cy2, double cy3, double cy4, double cy5, double cy6,
			ScriptNode[] Arguments, ArgumentType[] ArgumentTypes, int Start, int Length, Expression Expression)
			: base(Arguments, ArgumentTypes, Start, Length, Expression)
		{
			this.cx = new double[] { cx1, cx2, cx3, cx4, cx5, cx6 };
			this.cy = new double[] { cy1, cy2, cy3, cy4, cy5, cy6 };
		}

		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			switch (Arguments.Length)
			{
				case 12:
					break;

				case 1:
					if (!(Arguments[0] is IMatrix M) || M.Rows != 2 || M.Columns != 6)
						throw new ScriptRuntimeException("Expected a 2x6 matrix.", this);

					Arguments = new IElement[]
					{
						M.GetElement(0, 0),
						M.GetElement(1, 0),
						M.GetElement(2, 0),
						M.GetElement(3, 0),
						M.GetElement(4, 0),
						M.GetElement(5, 0),
						M.GetElement(0, 1),
						M.GetElement(1, 1),
						M.GetElement(2, 1),
						M.GetElement(3, 1),
						M.GetElement(4, 1),
						M.GetElement(5, 1)
					};
					break;

				case 2:
					if (!(Arguments[0] is IVector CX) || CX.Dimension != 6 ||
						!(Arguments[1] is IVector CY) || CY.Dimension != 6)
					{
						throw new ScriptRuntimeException("Expected vectors of 6 dimensions.", this);
					}

					Arguments = new IElement[]
					{
						CX.GetElement(0),
						CX.GetElement(1),
						CX.GetElement(2),
						CX.GetElement(3),
						CX.GetElement(4),
						CX.GetElement(5),
						CY.GetElement(0),
						CY.GetElement(1),
						CY.GetElement(2),
						CY.GetElement(3),
						CY.GetElement(4),
						CY.GetElement(5)
					};
					break;

				default:
					throw new ScriptRuntimeException("Invalid number of arguments.", this);
			}

			double cx1 = Expression.ToDouble(Arguments[0].AssociatedObjectValue);
			double cx2 = Expression.ToDouble(Arguments[1].AssociatedObjectValue);
			double cx3 = Expression.ToDouble(Arguments[2].AssociatedObjectValue);
			double cx4 = Expression.ToDouble(Arguments[3].AssociatedObjectValue);
			double cx5 = Expression.ToDouble(Arguments[4].AssociatedObjectValue);
			double cx6 = Expression.ToDouble(Arguments[5].AssociatedObjectValue);
			double cy1 = Expression.ToDouble(Arguments[6].AssociatedObjectValue);
			double cy2 = Expression.ToDouble(Arguments[7].AssociatedObjectValue);
			double cy3 = Expression.ToDouble(Arguments[8].AssociatedObjectValue);
			double cy4 = Expression.ToDouble(Arguments[9].AssociatedObjectValue);
			double cy5 = Expression.ToDouble(Arguments[10].AssociatedObjectValue);
			double cy6 = Expression.ToDouble(Arguments[11].AssociatedObjectValue);

			return new QuadraticVariation(cx1, cx2, cx3, cx4, cx5, cx6, cy1, cy2, cy3, cy4, cy5, cy6, 
				this.Arguments, this.ArgumentTypes, Start, Length, Expression);
		}

		public override string[] DefaultArgumentNames
		{
			get
			{
				return new string[]
				{
					"cx1", "cx2", "cx2", "cx4", "cx5", "cx6",
					"cx7", "cx8", "cx9", "cx10", "cx11", "cx12"
				};
			}
		}

		public override void Operate(ref double x, ref double y)
		{
			double x2 = x * x;
			double y2 = y * y;
			double xy = x * y;

			double NextX = this.cx[0] + this.cx[1] * x + this.cx[2] * x2 +
				this.cx[3] * xy + this.cx[4] * y + this.cx[5] * y2;

			y = this.cy[0] + this.cy[1] * x + this.cy[2] * x2 +
				this.cy[3] * xy + this.cy[4] * y + this.cy[5] * y2;
			x = NextX;
		}

		public override string FunctionName => nameof(QuadraticVariation);
	}
}
