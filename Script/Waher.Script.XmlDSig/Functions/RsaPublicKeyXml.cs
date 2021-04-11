using System;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.XmlDSig.Functions
{
	/// <summary>
	/// RsaPublicKeyXml(KeyName,KeySize)
	/// </summary>
	public class RsaPublicKeyXml : FunctionMultiVariate
	{
		/// <summary>
		/// RsaPublicKeyXml(KeyName,KeySize)
		/// </summary>
		/// <param name="KeyName">Key Name</param>
		/// <param name="KeySize">Key size, in bits.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public RsaPublicKeyXml(ScriptNode KeyName, ScriptNode KeySize, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { KeyName, KeySize }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "RsaPublicKeyXml"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "KeyName", "KeySize" }; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			string KeyName = Arguments[0].AssociatedObjectValue.ToString();
			int KeySize = (int)Expression.ToDouble(Arguments[1].AssociatedObjectValue);

			if (KeySize <= 0)
				throw new ScriptRuntimeException("Invalid key size.", this);

			CspParameters CspParams = new CspParameters()
			{
				KeyContainerName = KeyName
			};

			RSACryptoServiceProvider RsaKey = new RSACryptoServiceProvider(KeySize, CspParams);
			string s;

			try
			{
				s = RsaKey.ToXmlString(false);
			}
			catch (PlatformNotSupportedException)
			{
				StringBuilder sb = new StringBuilder();
				RSAParameters P = RsaKey.ExportParameters(false);

				sb.Append("<RSAKeyValue><Modulus>");
				sb.Append(Convert.ToBase64String(P.Modulus));
				sb.Append("</Modulus><Exponent>");
				sb.Append(Convert.ToBase64String(P.Exponent));
				sb.Append("</Exponent></RSAKeyValue>");

				s = sb.ToString();
			}

			XmlDocument Doc = new XmlDocument();
			Doc.LoadXml(s);

			return new ObjectValue(Doc);
		}

	}
}
