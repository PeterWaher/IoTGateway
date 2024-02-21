using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Content.Markdown.Rendering;

namespace Waher.Content.Markdown.Model
{
	/// <summary>
	/// Where baselign of horizontally organized elements are rendered.
	/// </summary>
	public enum BaselineAlignment
	{
		/// <summary>
		/// Center-aligned
		/// </summary>
		Center,

		/// <summary>
		/// Aligned along base-line
		/// </summary>
		Baseline
	}

	/// <summary>
	/// Abstract base class for all markdown elements.
	/// </summary>
	public abstract class MarkdownElement
	{
		private readonly MarkdownDocument document;

		/// <summary>
		/// Abstract base class for all markdown elements.
		/// </summary>
		/// <param name="Document">Markdown document.</param>
		public MarkdownElement(MarkdownDocument Document)
		{
			this.document = Document;
		}

		/// <summary>
		/// Markdown document.
		/// </summary>
		public MarkdownDocument Document => this.document;

		/// <summary>
		/// Any children of the element.
		/// </summary>
		public virtual IEnumerable<MarkdownElement> Children => new MarkdownElement[0];

		/// <summary>
		/// If the element is a block element.
		/// </summary>
		public virtual bool IsBlockElement => false;

		/// <summary>
		/// If the current object has same meta-data as <paramref name="E"/>
		/// (but not necessarily same content).
		/// </summary>
		/// <param name="E">Element to compare to.</param>
		/// <returns>If same meta-data as <paramref name="E"/>.</returns>
		public virtual bool SameMetaData(MarkdownElement E)
		{
			return this.GetType() == E.GetType();
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return this.GetType().Equals(obj.GetType());
		}

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return this.GetType().GetHashCode();
		}

		/// <summary>
		/// Renders the element.
		/// </summary>
		/// <param name="Output">Renderer</param>
		public abstract Task Render(IRenderer Output);

		/// <summary>
		/// If element, parsed as a span element, can stand outside of a paragraph if alone in it.
		/// </summary>
		public virtual bool OutsideParagraph => false;

		/// <summary>
		/// If the element is an inline span element.
		/// </summary>
		public abstract bool InlineSpanElement
		{
			get;
		}

		/// <summary>
		/// Baseline alignment
		/// </summary>
		public virtual BaselineAlignment BaselineAlignment => BaselineAlignment.Center;

		/// <summary>
		/// Loops through all child-elements for the element.
		/// </summary>
		/// <param name="Callback">Method called for each one of the elements.</param>
		/// <param name="State">State object passed on to the callback method.</param>
		/// <returns>If the operation was completed.</returns>
		public virtual bool ForEach(MarkdownElementHandler Callback, object State)
		{
			return Callback(this, State);
		}

		/// <summary>
		/// Checks if two typed arrays are equal
		/// </summary>
		/// <param name="Items1">First array</param>
		/// <param name="Items2">Second array</param>
		/// <returns>If arrays are equal</returns>
		protected static bool AreEqual(Array Items1, Array Items2)
		{
			int i, c = Items1.Length;
			if (Items2.Length != c)
				return false;

			for (i = 0; i < c; i++)
			{
				if (!Items1.GetValue(i)?.Equals(Items2.GetValue(i)) ?? Items2.GetValue(i) is null)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Calculates a hash value on an array.
		/// </summary>
		/// <param name="Items">Array</param>
		/// <returns>Hash Code</returns>
		protected static int GetHashCode(Array Items)
		{
			int h1 = 0;
			int h2;

			foreach (object Item in Items)
			{
				h2 = Item?.GetHashCode() ?? 0;
				h1 = ((h1 << 5) + h1) ^ h2;
			}

			return h1;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			using (MarkdownRenderer Renderer = new MarkdownRenderer())
			{
				this.Render(Renderer);
				return Renderer.ToString();
			}
		}

		/// <summary>
		/// Increments the property or properties in <paramref name="Statistics"/> corresponding to the element.
		/// </summary>
		/// <param name="Statistics">Contains statistics about the Markdown document.</param>
		public abstract void IncrementStatistics(MarkdownStatistics Statistics);

	}
}
