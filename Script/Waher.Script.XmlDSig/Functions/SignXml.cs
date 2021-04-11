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
	/// SignXml(KeyName,KeySize,XML,Reference)
	/// </summary>
	public class SignXml : FunctionMultiVariate
	{
		/// <summary>
		/// SignXml(KeyName,KeySize,XML,Reference)
		/// </summary>
		/// <param name="KeyName">Key Name</param>
		/// <param name="KeySize">Key size, in bits.</param>
		/// <param name="Xml">XML to sign.</param>
		/// <param name="Reference">Reference URI</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public SignXml(ScriptNode KeyName, ScriptNode KeySize, ScriptNode Xml, ScriptNode Reference, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { KeyName, KeySize, Xml, Reference },
				  new ArgumentType[] { ArgumentType.Scalar, ArgumentType.Scalar, ArgumentType.Normal, ArgumentType.Scalar },
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "SignXml"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames
		{
			get { return new string[] { "KeyName", "KeySize", "Xml", "Reference" }; }
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

			XmlDocument Xml = ToXml(Arguments[2].AssociatedObjectValue, this);
			string Ref = Arguments[3].AssociatedObjectValue.ToString();

			CspParameters CspParams = new CspParameters()
			{
				KeyContainerName = KeyName
			};

			RSACryptoServiceProvider RsaKey = new RSACryptoServiceProvider(KeySize, CspParams);

			SignedXml SignedXml = new SignedXml(Xml)
			{
				SigningKey = RsaKey
			};

			Reference Reference = new Reference()
			{
				Uri = Ref
			};

			XmlDsigEnvelopedSignatureTransform Env = new XmlDsigEnvelopedSignatureTransform();
			Reference.AddTransform(Env);

			SignedXml.AddReference(Reference);
			SignedXml.ComputeSignature();

			XmlElement XmlDigitalSignature = SignedXml.GetXml();

			Xml.DocumentElement.AppendChild(Xml.ImportNode(XmlDigitalSignature, true));

			return new ObjectValue(Xml);
		}

		internal static XmlDocument ToXml(object Object, ScriptNode Node)
		{
			if (!(Object is XmlDocument Xml))
			{
				if (Object is XmlElement E)
				{
					Xml = new XmlDocument()
					{
						PreserveWhitespace = true
					};
					Xml.AppendChild(Xml.ImportNode(E, true));
				}
				else if (Object is string s)
				{
					Xml = new XmlDocument()
					{
						PreserveWhitespace = true
					};
					Xml.Load(s);
				}
				else
					throw new ScriptRuntimeException("Third argument must be XML.", Node);
			}

			return Xml;
		}

	}
}
