using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS
{
	/// <summary>
	/// TODO
	/// </summary>
	public interface IFlameVariation : ILambdaExpression, IElement
    {
		/// <summary>
		/// TODO
		/// </summary>
		void Initialize(double[] HomogeneousTransform, double VariationWeight);
	
		/// <summary>
		/// TODO
		/// </summary>
		void Operate(ref double x, ref double y);
    }
}
