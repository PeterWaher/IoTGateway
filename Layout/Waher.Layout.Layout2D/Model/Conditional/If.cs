using System;
using System.Xml;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Conditional
{
	/// <summary>
	/// Conditional layout based on one conditional statement.
	/// </summary>
	public class If : LayoutElement
	{
		private ExpressionAttribute condition;
		private LayoutContainer ifTrue;
		private LayoutContainer ifFalse;

		/// <summary>
		/// Conditional layout based on one conditional statement.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public If(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "If";

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			this.ifTrue?.Dispose();
			this.ifFalse?.Dispose();
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			this.condition = new ExpressionAttribute(Input, "condition");

			foreach (XmlNode Node in Input.ChildNodes)
			{
				if (Node is XmlElement E)
				{
					ILayoutElement Child = this.Document.CreateElement(E, this);
					if (Child is True True)
					{
						if (this.ifTrue is null)
							this.ifTrue = True;
						else
							throw new LayoutSyntaxException("If statement already has a True statement.");
					}
					else if (Child is False False)
					{
						if (this.ifFalse is null)
							this.ifFalse = False;
						else
							throw new LayoutSyntaxException("If statement already has a False statement.");
					}
					else
						throw new LayoutSyntaxException("Unrecognized child element in If statement: " + E.NamespaceURI + "#" + E.LocalName);
				}
			}
		}

		/// <summary>
		/// Exports attributes to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportAttributes(XmlWriter Output)
		{
			base.ExportAttributes(Output);

			this.condition.Export(Output);
		}

		/// <summary>
		/// Exports child elements to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportChildren(XmlWriter Output)
		{
			base.ExportChildren(Output);

			this.ifTrue?.ToXml(Output);
			this.ifFalse?.ToXml(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new If(Document, Parent);
		}

	}
}
