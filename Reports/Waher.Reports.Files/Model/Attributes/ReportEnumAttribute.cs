using System;
using System.Text;
using System.Xml;

namespace Waher.Reports.Model.Attributes
{
	/// <summary>
	/// Report enumerated attribute.
	/// </summary>
	public class ReportEnumAttribute<T> : ReportAttribute<T>
		where T : Enum
	{
		/// <summary>
		/// Report enumerated attribute.
		/// </summary>
		/// <param name="Xml">XML Element hosting the attribute.</param>
		/// <param name="AttributeName">Name of attribute.</param>
		public ReportEnumAttribute(XmlElement Xml, string AttributeName)
			: base(Xml, AttributeName)
		{
		}

		/// <summary>
		/// Parses a string representation of a value.
		/// </summary>
		/// <param name="s">Enum representation</param>
		/// <returns>Parsed value.</returns>
		public override T ParseValue(string s)
		{
			if (Enum.TryParse(typeof(T), s, true, out object Result) && Result is T TypedResult)
				return TypedResult;
			else
			{
				StringBuilder sb = new StringBuilder();

				sb.Append("Expected a value of type ");
				sb.Append(typeof(T).FullName);
				sb.Append(". Invalid enumerated value: ");
				sb.Append(s);

				throw new ArgumentException(sb.ToString(), nameof(s));
			}
		}
	}
}
