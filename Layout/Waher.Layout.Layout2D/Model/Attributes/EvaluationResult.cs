namespace Waher.Layout.Layout2D.Model.Attributes
{
	/// <summary>
	/// Result of asynchronous evaluation.
	/// </summary>
	/// <typeparam name="T">Type of result.</typeparam>
	public class EvaluationResult<T>
	{
		/// <summary>
		/// Positive result of asynchronous evaluation.
		/// </summary>
		public EvaluationResult(T Result)
		{
			this.Ok = true;
			this.Result = Result;
		}

		/// <summary>
		/// Positive result of asynchronous evaluation.
		/// </summary>
		private EvaluationResult(bool Ok, T Result)
		{
			this.Ok = Ok;
			this.Result = Result;
		}

		/// <summary>
		/// If evaluation was successful.
		/// </summary>
		public bool Ok;

		/// <summary>
		/// Evaluated result, if successful.
		/// </summary>
		public T Result;

		/// <summary>
		/// No result
		/// </summary>
		public static readonly EvaluationResult<T> Empty = new EvaluationResult<T>(false, default);
	}
}
