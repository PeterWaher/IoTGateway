using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;

namespace Waher.Script
{
	/// <summary>
	/// Delegate for preview events.
	/// </summary>
	/// <param name="Sender"></param>
	/// <param name="e"></param>
	public delegate void PreviewEventHandler(object Sender, PreviewEventArgs e);

	/// <summary>
	/// Event arguments for preview events.
	/// </summary>
	public class PreviewEventArgs : EventArgs
	{
		private readonly Expression expression;
		private readonly IElement preview;

		/// <summary>
		/// Event arguments for preview events.
		/// </summary>
		/// <param name="Expression">Expression being evaluated.</param>
		/// <param name="Preview">Preview of result.</param>
		public PreviewEventArgs(Expression Expression, IElement Preview)
		{
			this.expression = Expression;
			this.preview = Preview;
		}

		/// <summary>
		/// Expression being evaluated.
		/// </summary>
		public Expression Expression
		{
			get { return this.expression; }
		}

		/// <summary>
		/// Preview of result.
		/// </summary>
		public IElement Preview
		{
			get { return this.preview; }
		}
	}
}
