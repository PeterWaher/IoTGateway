using System;
using System.Text;
using System.Xml;

namespace Waher.Content.Semantic.Model.Literals
{
	/// <summary>
	/// Represents an XML literal.
	/// </summary>
	public class XmlLiteral : SemanticLiteral
    {
		/// <summary>
		/// Represents an XML literal.
		/// </summary>
		public XmlLiteral()
            : base()
        {
        }

		/// <summary>
		/// Represents an XML literal.
		/// </summary>
		/// <param name="Value">Parsed value</param>
		public XmlLiteral(XmlNodeList Value)
            : base(Value, ToString(Value))
        {
        }

        private static string ToString(XmlNodeList List)
        {
            StringBuilder sb = new StringBuilder();

            foreach (XmlNode N in List)
                sb.Append(N.OuterXml);

            return sb.ToString();
        }

        /// <summary>
        /// Represents a Uri literal.
        /// </summary>
        /// <param name="Value">Parsed value</param>
        /// <param name="StringValue">String value</param>
        public XmlLiteral(XmlNodeList Value, string StringValue)
            : base(Value, StringValue)
        {
        }

        /// <summary>
        /// Type name
        /// </summary>
        public override string StringType => "http://www.w3.org/2001/XMLSchema#any";

        /// <summary>
        /// Tries to parse a string value of the type supported by the class..
        /// </summary>
        /// <param name="Value">String value.</param>
        /// <param name="DataType">Data type.</param>
        /// <returns>Parsed literal.</returns>
        public override ISemanticLiteral Parse(string Value, string DataType)
        {
            try
            {
                XmlDocument Doc = new XmlDocument();
                Doc.LoadXml(Value);
                return new XmlLiteral(Doc.ChildNodes);
            }
            catch (Exception)
            {
				return new CustomLiteral(Value, DataType);
			}
		}
    }
}
