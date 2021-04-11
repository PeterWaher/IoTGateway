using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.XmlDSig.Functions
{
	/// <summary>
	/// RsaPublicKey(Xml)
	/// </summary>
	public class RsaPublicKey : FunctionMultiVariate
	{
		/// <summary>
		/// RsaPublicKey(Xml)
		/// </summary>
		/// <param name="Xml">XML representation of public key.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public RsaPublicKey(ScriptNode Xml, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Xml }, argumentTypes1Normal, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "RsaPublicKey"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "Xml" }; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return new ObjectValue(ToKey(Arguments[0].AssociatedObjectValue, this));
		}

		internal static RSA ToKey(object Object, ScriptNode Node)
		{
			if (!(Object is string Xml))
			{
				if (Object is RSA RSA)
					return RSA;
				else if (Object is XmlDocument Doc)
					Xml = Doc.OuterXml;
				else if (Object is XmlElement E)
					Xml = E.OuterXml;
				else
					Xml = Object.ToString();
			}

			RSA PublicKey = RSACryptoServiceProvider.Create();

			try
			{
				PublicKey.FromXmlString(Xml);
			}
			catch (PlatformNotSupportedException)
			{
				RSAParameters P = new RSAParameters();
				XmlDocument Doc = new XmlDocument();
				Doc.LoadXml(Xml);

				if (Doc.DocumentElement is null || Doc.DocumentElement.LocalName != "RSAKeyValue")
					throw new ScriptRuntimeException("Not an RSA Public Key XML document.", Node);

				foreach (XmlNode N in Doc.DocumentElement.ChildNodes)
				{
					if (!(N is XmlElement E))
						continue;

					switch (E.LocalName)
					{
						case "Modulus":
							P.Modulus = Convert.FromBase64String(E.InnerText);
							break;

						case "Exponent":
							P.Exponent = Convert.FromBase64String(E.InnerText);
							break;
					}
				}
			}

			return PublicKey;
		}

	}
}
