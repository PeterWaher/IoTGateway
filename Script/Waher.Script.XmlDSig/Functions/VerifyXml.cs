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
	/// VerifyXml(KeyName,KeySize,XML)
	/// VerifyXml(PublicKeyXml,XML)
	/// VerifyXml(PublicKey,XML)
	/// </summary>
	public class VerifyXml : FunctionMultiVariate
	{
		/// <summary>
		/// VerifyXml(KeyName,KeySize,XML)
		/// </summary>
		/// <param name="KeyName">Key Name</param>
		/// <param name="KeySize">Key size, in bits.</param>
		/// <param name="Xml">XML to sign.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VerifyXml(ScriptNode KeyName, ScriptNode KeySize, ScriptNode Xml, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { KeyName, KeySize, Xml },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// VerifyXml(PublicKeyXml,XML)
		/// VerifyXml(PublicKey,XML)
		/// </summary>
		/// <param name="PublicKey">Public Key</param>
		/// <param name="Xml">XML to sign.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public VerifyXml(ScriptNode PublicKey, ScriptNode Xml, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { PublicKey, Xml },
				  new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "VerifyXml"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "PublicKey", "Xml" }; }
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			RSA PublicKey;
			int c = Arguments.Length;

			switch (c)
			{
				case 2:
					PublicKey = RsaPublicKey.ToKey(Arguments[0].AssociatedObjectValue, this);
					break;

				case 3:
					string KeyName = Arguments[0].AssociatedObjectValue.ToString();
					int KeySize = (int)Expression.ToDouble(Arguments[1].AssociatedObjectValue);

					if (KeySize <= 0)
						throw new ScriptRuntimeException("Invalid key size.", this);

					CspParameters CspParams = new CspParameters()
					{
						KeyContainerName = KeyName
					};

					PublicKey = new RSACryptoServiceProvider(KeySize, CspParams);
					break;

				default:
					throw new ScriptRuntimeException("Unexpected number of arguments.", this);
			}

			XmlDocument Xml = SignXml.ToXml(Arguments[c - 1].AssociatedObjectValue, this);

			Xml.LoadXml(Xml.OuterXml);  // Why is this necessary? Otherwise, the SignedXml.LoadXml(Signature); method call below will fail...

			SignedXml SignedXml = new SignedXml(Xml);
			XmlElement Signature = null;
			
			foreach (XmlNode N in Xml.DocumentElement.ChildNodes)
			{
				if (N is XmlElement E && E.LocalName == "Signature")
				{
					Signature = E;
					break;
				}
			}
			
			if (Signature is null)
				throw new Exception("XML not signed.");
			
			SignedXml.LoadXml(Signature);

			if (!SignedXml.CheckSignature(PublicKey))
				throw new Exception("XML signature invalid.");

			Xml.DocumentElement.RemoveChild(Signature);

			return new ObjectValue(Xml);
		}

	}
}
