using System;

namespace Waher.Script.Xml.Model
{
	/// <summary>
	/// Abstract base class for XML Script attribute nodes.
	/// </summary>
	public abstract class XmlScriptAttribute : XmlScriptNode
	{
		private readonly string name;

		/// <summary>
		/// Abstract base class for XML Script attribute nodes.
		/// </summary>
		/// <param name="Name">Element name.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public XmlScriptAttribute(string Name, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.name = Name;
		}

		/// <summary>
		/// Attribute name.
		/// </summary>
		public string Name => this.name;

		/// <summary>
		/// Gets the attribute value.
		/// </summary>
		/// <param name="Variables">Current set of variables.</param>
		internal abstract string GetValue(Variables Variables);
	}
}
