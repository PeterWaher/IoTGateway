using System.Threading.Tasks;
using Waher.Script;

namespace Waher.Content.Xml.Attributes
{
	/// <summary>
	/// Attribute extensions, simplifying evaluation of optional attributes.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Tries to evaluate the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <returns>Evaluation result.</returns>
		public static Task<EvaluationResult<string>> TryEvaluate(this StringAttribute Attribute, Variables Session)
		{
			return Attribute<string>.TryEvaluate(Attribute, Session);
		}

		/// <summary>
		/// Tries to evaluate the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <returns>Evaluation result.</returns>
		public static Task<EvaluationResult<bool>> TryEvaluate(this BooleanAttribute Attribute, Variables Session)
		{
			return Attribute<bool>.TryEvaluate(Attribute, Session);
		}

		/// <summary>
		/// Tries to evaluate the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <returns>Evaluation result.</returns>
		public static Task<EvaluationResult<Expression>> TryEvaluate(this ExpressionAttribute Attribute, Variables Session)
		{
			return Attribute<Expression>.TryEvaluate(Attribute, Session);
		}

		/// <summary>
		/// Tries to evaluate the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <returns>Evaluation result.</returns>
		public static Task<EvaluationResult<float>> TryEvaluate(this FloatAttribute Attribute, Variables Session)
		{
			return Attribute<float>.TryEvaluate(Attribute, Session);
		}

		/// <summary>
		/// Tries to evaluate the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <returns>Evaluation result.</returns>
		public static Task<EvaluationResult<int>> TryEvaluate(this PositiveIntegerAttribute Attribute, Variables Session)
		{
			return Attribute<int>.TryEvaluate(Attribute, Session);
		}

		/// <summary>
		/// Tries to evaluate the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <returns>Evaluation result.</returns>
		public static Task<EvaluationResult<T>> TryEvaluate<T>(this EnumAttribute<T> Attribute, Variables Session)
			where T : struct
		{
			return Attribute<T>.TryEvaluate(Attribute, Session);
		}

		/// <summary>
		/// Tries to evaluate the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <param name="DefaultValue">Default value if value or expression is missing or invalid.</param>
		/// <returns>Evaluation result.</returns>
		public static Task<string> Evaluate(this StringAttribute Attribute, Variables Session, string DefaultValue)
		{
			if (Attribute is null)
				return Task.FromResult(DefaultValue);
			else
				return Attribute.EvaluateAsync(Session, DefaultValue);
		}

		/// <summary>
		/// Evaluates the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <param name="DefaultValue">Default value if value or expression is missing or invalid.</param>
		/// <returns>Evaluation result.</returns>
		public static Task<bool> Evaluate(this BooleanAttribute Attribute, Variables Session, bool DefaultValue)
		{
			if (Attribute is null)
				return Task.FromResult(DefaultValue);
			else
				return Attribute.EvaluateAsync(Session, DefaultValue);
		}

		/// <summary>
		/// Evaluates the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <param name="DefaultValue">Default value if value or expression is missing or invalid.</param>
		/// <returns>Evaluation result.</returns>
		public static Task<Expression> Evaluate(this ExpressionAttribute Attribute, Variables Session, Expression DefaultValue)
		{
			if (Attribute is null)
				return Task.FromResult(DefaultValue);
			else
				return Attribute.EvaluateAsync(Session, DefaultValue);
		}

		/// <summary>
		/// Evaluates the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <param name="DefaultValue">Default value if value or expression is missing or invalid.</param>
		/// <returns>Evaluation result.</returns>
		public static Task<float> Evaluate(this FloatAttribute Attribute, Variables Session, float DefaultValue)
		{
			if (Attribute is null)
				return Task.FromResult(DefaultValue);
			else
				return Attribute.EvaluateAsync(Session, DefaultValue);
		}

		/// <summary>
		/// Evaluates the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <param name="DefaultValue">Default value if value or expression is missing or invalid.</param>
		/// <returns>Evaluation result.</returns>
		public static Task<int> Evaluate(this PositiveIntegerAttribute Attribute, Variables Session, int DefaultValue)
		{
			if (Attribute is null)
				return Task.FromResult(DefaultValue);
			else
				return Attribute.EvaluateAsync(Session, DefaultValue);
		}

		/// <summary>
		/// Evaluates the attribute value.
		/// </summary>
		/// <param name="Attribute">Attribute to evaluate.</param>
		/// <param name="Session">Current session.</param>
		/// <param name="DefaultValue">Default value if value or expression is missing or invalid.</param>
		/// <returns>Evaluation result.</returns>
		public static Task<T> Evaluate<T>(this EnumAttribute<T> Attribute, Variables Session, T DefaultValue)
			where T : struct
		{
			if (Attribute is null)
				return Task.FromResult(DefaultValue);
			else
				return Attribute.EvaluateAsync(Session, DefaultValue);
		}
	}
}
