using System;
using Waher.Runtime.Inventory;

namespace Waher.Content.Semantic.Ontologies
{
	/// <summary>
	/// XML Schema Datatypes
	/// </summary>
	public class XmlSchema : IOntology
	{
		/// <summary>
		/// XML Schema Datatypes
		/// </summary>
		public XmlSchema()
		{
		}

		/// <summary>
		/// Ontology namespace.
		/// </summary>
		public string OntologyNamespace => Namespace;

		/// <summary>
		/// Well-known ontology prefix.
		/// </summary>
		public string OntologyPrefix => "xsd";

		/// <summary>
		/// If the ontology should be shown by default in query interfaces.
		/// </summary>
		public bool ShowByDefault => true;

		/// <summary>
		/// If the interface understands objects such as <paramref name="Uri"/>.
		/// </summary>
		/// <param name="Uri">URI</param>
		/// <returns>How well objects of this type are supported.</returns>
		public Grade Supports(string Uri)
		{
			return Uri.StartsWith(Namespace) ? Grade.Ok : Grade.NotAtAll;
		}

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#
		/// </summary>
		public const string Namespace = "http://www.w3.org/2001/XMLSchema#";
	
		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#base64Binary
		/// </summary>
		public static readonly Uri base64Binary = new Uri(Namespace + "base64Binary");
	
		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#boolean
		/// </summary>
		public static readonly Uri boolean = new Uri(Namespace + "boolean");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#date
		/// </summary>
		public static readonly Uri date = new Uri(Namespace + "date");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#boolean
		/// </summary>
		public static readonly Uri dateTime = new Uri(Namespace + "dateTime");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#dayTimeDuration
		/// </summary>
		public static readonly Uri dayTimeDuration = new Uri(Namespace + "dayTimeDuration");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#decimal
		/// </summary>
		public static readonly Uri @decimal = new Uri(Namespace + "decimal");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#double
		/// </summary>
		public static readonly Uri @double = new Uri(Namespace + "double");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#duration
		/// </summary>
		public static readonly Uri duration = new Uri(Namespace + "duration");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#short
		/// </summary>
		public static readonly Uri @short = new Uri(Namespace + "short");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#int
		/// </summary>
		public static readonly Uri @int = new Uri(Namespace + "int");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#long
		/// </summary>
		public static readonly Uri @long = new Uri(Namespace + "long");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#byte
		/// </summary>
		public static readonly Uri @byte = new Uri(Namespace + "byte");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#byte
		/// </summary>
		public static readonly Uri integer = new Uri(Namespace + "integer");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#float
		/// </summary>
		public static readonly Uri @float = new Uri(Namespace + "float");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#string
		/// </summary>
		public static readonly Uri @string = new Uri(Namespace + "string");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#time
		/// </summary>
		public static readonly Uri time = new Uri(Namespace + "time");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#unsignedShort
		/// </summary>
		public static readonly Uri unsignedShort = new Uri(Namespace + "unsignedShort");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#unsignedInt
		/// </summary>
		public static readonly Uri unsignedInt = new Uri(Namespace + "unsignedInt");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#unsignedLong
		/// </summary>
		public static readonly Uri unsignedLong = new Uri(Namespace + "unsignedLong");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#unsignedByte
		/// </summary>
		public static readonly Uri unsignedByte = new Uri(Namespace + "unsignedByte");

		/// <summary>
		/// http://www.w3.org/2001/XMLSchema#anyURI
		/// </summary>
		public static readonly Uri anyURI = new Uri(Namespace + "anyURI");
	}
}
