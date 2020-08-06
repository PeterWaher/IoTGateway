using System;
using System.Collections.Generic;
using System.Xml;
using Waher.Layout.Layout2D.Exceptions;
using Waher.Layout.Layout2D.Model.Attributes;

namespace Waher.Layout.Layout2D.Model.Conditional
{
	/// <summary>
	/// Conditional layout based on multiple conditional statements.
	/// </summary>
	public class Switch : LayoutElement
	{
		private Case[] cases;
		private Otherwise otherwise;

		/// <summary>
		/// Conditional layout based on multiple conditional statements.
		/// </summary>
		/// <param name="Document">Layout document containing the element.</param>
		/// <param name="Parent">Parent element.</param>
		public Switch(Layout2DDocument Document, ILayoutElement Parent)
			: base(Document, Parent)
		{
		}

		/// <summary>
		/// Local name of type of element.
		/// </summary>
		public override string LocalName => "Switch";

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();

			if (!(this.cases is null))
			{
				foreach (Case Case in this.cases)
					Case.Dispose();
			}

			this.otherwise?.Dispose();
		}

		/// <summary>
		/// Populates the element (including children) with information from its XML definition.
		/// </summary>
		/// <param name="Input">XML definition.</param>
		public override void FromXml(XmlElement Input)
		{
			base.FromXml(Input);

			List<Case> Cases = new List<Case>();

			foreach (XmlNode Node in Input.ChildNodes)
			{
				if (Node is XmlElement E)
				{
					ILayoutElement Child = this.Document.CreateElement(E, this);
					if (Child is Case Case)
						Cases.Add(Case);
					else if (Child is Otherwise Otherwise)
					{
						if (this.otherwise is null)
							this.otherwise = Otherwise;
						else
							throw new LayoutSyntaxException("Switch statement already has an Otherwise statement.");
					}
					else
						throw new LayoutSyntaxException("Unrecognized child element in Switch statement: " + E.NamespaceURI + "#" + E.LocalName);
				}
			}

			this.cases = Cases.ToArray();
		}

		/// <summary>
		/// Exports child elements to XML.
		/// </summary>
		/// <param name="Output">XML output.</param>
		public override void ExportChildren(XmlWriter Output)
		{
			base.ExportChildren(Output);

			if (!(this.cases is null))
			{
				foreach (Case Case in this.cases)
					Case.ToXml(Output);
			}

			this.otherwise?.ToXml(Output);
		}

		/// <summary>
		/// Creates a new instance of the layout element.
		/// </summary>
		/// <param name="Document">Document containing the new element.</param>
		/// <param name="Parent">Parent element.</param>
		/// <returns>New instance.</returns>
		public override ILayoutElement Create(Layout2DDocument Document, ILayoutElement Parent)
		{
			return new Switch(Document, Parent);
		}

	}
}
