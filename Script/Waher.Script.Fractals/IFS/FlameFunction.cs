using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SkiaSharp;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;
using Waher.Script.Objects.VectorSpaces;
using Waher.Script.Objects.Matrices;

namespace Waher.Script.Fractals.IFS
{
    public class FlameFunction
    {
		private readonly DoubleNumber xv, yv;
		private readonly ComplexNumber zv;
		private readonly IElement[] complexParameters;
		private readonly IElement[] doubleParameters;
        private readonly double[] homogeneousTransform;
        private readonly ScriptNode node;
        private IFlameVariation[] variations;
        private ILambdaExpression[] variations2;
        private double[] variationWeights;
        private double[] variation2Weights;
        private bool[] variation2IsReal;
        private readonly List<IFlameVariation> variationsList = new List<IFlameVariation>();
        private readonly List<ILambdaExpression> variations2List = new List<ILambdaExpression>();
        private readonly List<double> variationsWeightList = new List<double>();
        private readonly List<double> variations2WeightList = new List<double>();
        private readonly List<bool> isReal = new List<bool>();
        private SKColor color = SKColors.Red;
        private double weight = 1.0;
        private int nrVariations = 0;
        private int nrVariations2 = 0;
        private object lastVariation = null;
        private double red = 1.0;
        private double green = 0.0;
        private double blue = 0.0;
        private readonly bool hasProjection;
        private bool hasVariations;
        private bool isTransparent = false;

        public FlameFunction(DoubleMatrix M, ScriptNode Node)
        {
			double[,] E = M.Values;

            this.homogeneousTransform = new double[9];
            this.node = Node;
			this.xv = new DoubleNumber(0);
			this.yv = new DoubleNumber(0);
			this.zv = new ComplexNumber(0);
			this.doubleParameters = new IElement[] { xv, yv };
			this.complexParameters = new IElement[] { zv };

			if (M.Columns == 2 && M.Rows == 2)
            {
                this.homogeneousTransform[0] = (double)E[0, 0];
                this.homogeneousTransform[1] = (double)E[0, 1];
                this.homogeneousTransform[2] = 0;
                this.homogeneousTransform[3] = (double)E[1, 0];
                this.homogeneousTransform[4] = (double)E[1, 1];
                this.homogeneousTransform[5] = 0;
                this.homogeneousTransform[6] = 0;
                this.homogeneousTransform[7] = 0;
                this.homogeneousTransform[8] = 1;
                this.hasProjection = false;
            }
            else if (M.Columns == 3 && M.Rows == 3)
            {
                this.homogeneousTransform[0] = (double)E[0, 0];
                this.homogeneousTransform[1] = (double)E[0, 1];
                this.homogeneousTransform[2] = (double)E[0, 2];
                this.homogeneousTransform[3] = (double)E[1, 0];
                this.homogeneousTransform[4] = (double)E[1, 1];
                this.homogeneousTransform[5] = (double)E[1, 2];
                this.homogeneousTransform[6] = (double)E[2, 0];
                this.homogeneousTransform[7] = (double)E[2, 1];
                this.homogeneousTransform[8] = (double)E[2, 2];

                this.hasProjection = this.homogeneousTransform[6] != 0 ||
                    this.homogeneousTransform[7] != 0 ||
                    this.homogeneousTransform[8] != 1;
            }
            else
                throw new ScriptRuntimeException("Linear transformation must be a 2D or homogeneous 2D transformation.", Node);
        }

        public void Add(IFlameVariation Variation)
        {
            this.variationsList.Add(Variation);
            this.variationsWeightList.Add(1);
            this.lastVariation = Variation;
        }

        public void Add(ILambdaExpression Variation)
        {
            switch (Variation.NrArguments)
            {
                case 1:
                    this.isReal.Add(false);
                    break;

                case 2:
                    this.isReal.Add(true);
                    break;

                default:
                    throw new ScriptRuntimeException("Only lambda expressions taking 2 real-valued parmeters or 1 complex-valued parameter are allowed.", this.node);
            }

            this.variations2List.Add(Variation);
            this.variations2WeightList.Add(1);
            this.lastVariation = Variation;
        }

        public void SetWeight(double Weight)
        {
            if (this.lastVariation is IFlameVariation)
                this.variationsWeightList[this.variationsWeightList.Count - 1] = Weight;

            else if (this.lastVariation is ILambdaExpression)
                this.variations2WeightList[this.variations2WeightList.Count - 1] = Weight;

            else
                this.weight = Weight;
        }

        public void SetColor(SKColor Color)
        {
            this.color = Color;
            this.red = this.color.Red / 255.0;
            this.green = this.color.Green / 255.0;
            this.blue = this.color.Blue / 255.0;
            this.isTransparent = (this.color.Alpha == 0);
        }

        public void SetColorHsl(double H, double S, double L)
        {
            this.color = SKColors.Black;
            this.red = H / 360.0;
            this.green = S / 100.0;
            this.blue = L / 100.0;
            this.isTransparent = false;
        }

        internal void DefinitionDone()
        {
            this.variations = this.variationsList.ToArray();
            this.variationWeights = this.variationsWeightList.ToArray();
            this.nrVariations = this.variations.Length;

            this.variations2 = this.variations2List.ToArray();
            this.variation2Weights = this.variations2WeightList.ToArray();
            this.nrVariations2 = this.variation2Weights.Length;
            this.variation2IsReal = this.isReal.ToArray();

            this.hasVariations = this.nrVariations > 0 || this.nrVariations2 > 0;

            int c = this.variations.Length;
            int i;

            for (i = 0; i < c; i++)
                this.variations[i].Initialize(this.homogeneousTransform, this.variationWeights[i]);
        }

        public double Weight { get { return this.weight; } }

        internal bool Operate(FlameState point, Variables Variables)
        {
            double x = point.x;
            double y = point.y;

            double x2 = this.homogeneousTransform[0] * x +
                this.homogeneousTransform[1] * y +
                this.homogeneousTransform[2];

            double y2 = this.homogeneousTransform[3] * x +
                this.homogeneousTransform[4] * y +
                this.homogeneousTransform[5];

            if (this.hasProjection)
            {
                double p = this.homogeneousTransform[6] * x +
                    this.homogeneousTransform[7] * y +
                    this.homogeneousTransform[6];

                if (p == 0)
                    return false;

                x2 /= p;
                y2 /= p;
            }

            if (this.hasVariations)
            {
                double x3 = 0;
                double y3 = 0;
                double w;
                int i;

                if (this.nrVariations > 0)
                {
                    for (i = 0; i < this.nrVariations; i++)
                    {
                        x = x2;
                        y = y2;

                        this.variations[i].Operate(ref x, ref y);
                        w = this.variationWeights[i];

                        x3 += w * x;
                        y3 += w * y;
                    }
                }

                if (this.nrVariations2 > 0)
                {
                    for (i = 0; i < this.nrVariations2; i++)
                    {
                        w = this.variation2Weights[i];

                        if (this.variation2IsReal[i])
                        {
							this.xv.Value = x2;
                            this.yv.Value = y2;

							IElement Result = this.variations2[i].Evaluate(this.doubleParameters, Variables);

							if (!(Result is IVector V) || V.Dimension != 2)
								throw new ScriptRuntimeException("Real-valued lambda expressions must return a vector containing 2 real-valued numbers.", this.node);

							x3 += w * Expression.ToDouble(V.GetElement(0).AssociatedObjectValue);
                            y3 += w * Expression.ToDouble(V.GetElement(1).AssociatedObjectValue);
                        }
                        else
                        {
                            this.zv.Value = new Complex(x2, y2);
                            Complex z = Expression.ToComplex(this.variations2[i].Evaluate(this.complexParameters, Variables).AssociatedObjectValue);

                            x3 += w * z.Real;
                            y3 += w * z.Imaginary;
                        }
                    }
                }

                point.x = x3;
                point.y = y3;
            }
            else
            {
                point.x = x2;
                point.y = y2;
            }

            if (!this.isTransparent)
            {
                switch (point.ColorMode)
                {
                    case ColorMode.Rgba:
                        point.red = (point.red + this.red) * 0.5;
                        point.green = (point.green + this.green) * 0.5;
                        point.blue = (point.blue + this.blue) * 0.5;
                        break;

                    case ColorMode.Hsl:
                        double d;

                        if (this.red < point.red)                           // H
                        {
                            d = point.red - this.red;
                            if (d < 0.5)
                                point.red -= d * 0.5;
                            else
                            {
                                d = 1 - d;
                                point.red += d * 0.5;
                                if (point.red > 1)
                                    point.red -= 1;
                            }
                        }
                        else
                        {
                            d = this.red - point.red;
                            if (d < 0.5)
                                point.red += d * 0.5;
                            else
                            {
                                d = 1 - d;
                                point.red -= d * 0.5;
                                if (point.red < 1)
                                    point.red += 1;
                            }
                        }

                        point.green = (point.green + this.green) * 0.5;     // S
                        point.blue = (point.blue + this.blue) * 0.5;        // L
                        break;
                }
            }

            return true;
        }

    }
}