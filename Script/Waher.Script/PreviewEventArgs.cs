using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script
{
	/// <summary>
	/// Delegate for preview events.
	/// </summary>
	/// <param name="Sender"></param>
	/// <param name="e">Event arguments</param>
	public delegate Task PreviewEventHandler(object Sender, PreviewEventArgs e);

	/// <summary>
	/// Event arguments for preview events.
	/// </summary>
	public class PreviewEventArgs : EventArgs
	{
		private readonly Expression expression;
		private readonly Variables variables;
		private readonly IElement preview;

		/// <summary>
		/// Event arguments for preview events.
		/// </summary>
		/// <param name="Expression">Expression being evaluated.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <param name="Preview">Preview of result.</param>
		public PreviewEventArgs(Expression Expression, Variables Variables, IElement Preview)
		{
			this.expression = Expression;
			this.variables = Variables;
			this.preview = Preview;
		}

		/// <summary>
		/// Expression being evaluated.
		/// </summary>
		public Expression Expression => this.expression;

		/// <summary>
		/// Current Variables Collection.
		/// </summary>
		public Variables Variables => this.variables;

		/// <summary>
		/// Preview of result.
		/// </summary>
		public IElement Preview => this.preview;
	}
}
