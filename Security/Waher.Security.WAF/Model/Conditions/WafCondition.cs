using System.Xml;
using Waher.Content.Xml;

namespace Waher.Security.WAF.Model.Conditions
{
	/// <summary>
	/// Condition comparison mode.
	/// </summary>
	public enum ComparisonMode
	{
		/// <summary>
		/// Comparison is done against a regular expression. The value must contain a match.
		/// </summary>
		ContainsRegex,

		/// <summary>
		/// Comparison is done against a regular expression. The start of the value must 
		/// match the regular expression.
		/// </summary>
		StartsWithRegex,

		/// <summary>
		/// Comparison is done against a regular expression. The end of the value must 
		/// match the regular expression.
		/// </summary>
		EndsWithRegex,

		/// <summary>
		/// Comparison is done against a regular expression. The entire value must match
		/// the regular expression.
		/// </summary>
		ExactRegex,

		/// <summary>
		/// Comparison is done against a string constant. The value must contain a match.
		/// </summary>
		ContainsConstant,

		/// <summary>
		/// Comparison is done against a string constant. The start of the value must 
		/// match the constant.
		/// </summary>
		StartsWithConstant,

		/// <summary>
		/// Comparison is done against a string constant. The end of the value must 
		/// match the constant.
		/// </summary>
		EndsWithConstant,

		/// <summary>
		/// Comparison is done against a string constant. The entire value must match
		/// the constant.
		/// </summary>
		ExactConstant,

		/// <summary>
		/// The value is compared against a script, that examines the value.
		/// </summary>
		Script,

		/// <summary>
		/// The value is compared against the contents of a list available in a file.
		/// </summary>
		FileList,

		/// <summary>
		/// The value is compared against the contents of a list available in the
		/// internal database.
		/// </summary>
		DatabaseList
	}

	/// <summary>
	/// Abstract base class for Web Application Firewall conditions.
	/// </summary>
	public abstract class WafCondition : WafActions
	{
		private readonly ComparisonMode mode;
		private readonly string value;

		/// <summary>
		/// Abstract base class for Web Application Firewall conditions.
		/// </summary>
		public WafCondition()
			: base()
		{
		}

		/// <summary>
		/// Abstract base class for Web Application Firewall conditions.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		public WafCondition(XmlElement Xml)
			: base(Xml)
		{
			this.value = XML.Attribute(Xml, "value");
			this.mode = XML.Attribute(Xml, "mode", ComparisonMode.ContainsRegex);
		}
	}
}
