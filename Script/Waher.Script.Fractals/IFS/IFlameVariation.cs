using System;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS
{
    public interface IFlameVariation : ILambdaExpression, IElement
    {
        void Initialize(double[] HomogeneousTransform, double VariationWeight);
        void Operate(ref double x, ref double y);
    }
}
