using System;
using System.Collections.Generic;
using System.Text;
using Waher.Content.Markdown.Model.Atoms;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Interface for elements containing editable text.
	/// </summary>
	public interface IEditableText
	{
		/// <summary>
		/// Return an enumeration of the editable text as atoms.
		/// </summary>
		/// <returns>Atoms.</returns>
		IEnumerable<Atom> Atomize();

		/// <summary>
		/// Assembles a markdown element from a sequence of atoms.
		/// </summary>
		/// <param name="Document">Document that will contain the new element.</param>
		/// <param name="Text">Assembled text.</param>
		/// <returns>Assembled markdown element.</returns>
		MarkdownElement Assemble(MarkdownDocument Document, string Text);
	}
}
