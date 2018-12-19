using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Xsl;
using Waher.Script;

namespace Waher.Content.Xsl
{
	/// <summary>
	/// Static class managing loading of XSL resources stored as embedded resources or in content files.
	/// </summary>
	public static class XSL
	{
		/// <summary>
		/// Loads an XML schema from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <returns>XML Schema.</returns>
		/// <exception cref="XmlSchemaException">If a validation exception occurred.</exception>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static XmlSchema LoadSchema(string ResourceName)
		{
			return LoadSchema(ResourceName, Resources.GetAssembly(ResourceName));
		}

		/// <summary>
		/// Loads an XML schema from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Assembly">Assembly containing the resource.</param>
		/// <returns>XML Schema.</returns>
		/// <exception cref="XmlSchemaException">If a validation exception occurred.</exception>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static XmlSchema LoadSchema(string ResourceName, Assembly Assembly)
		{
			using (Stream f = Assembly.GetManifestResourceStream(ResourceName))
			{
				if (f is null)
					throw new ArgumentException("Resource not found: " + ResourceName, nameof(ResourceName));

				XmlValidator Validator = new XmlValidator(ResourceName);
				XmlSchema Result = XmlSchema.Read(f, Validator.ValidationCallback);

				Validator.AssertNoError();

				return Result;
			}
		}

		/// <summary>
		/// Loads an XSL transformation from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <returns>XSL tranformation.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static XslCompiledTransform LoadTransform(string ResourceName)
		{
			return LoadTransform(ResourceName, Resources.GetAssembly(ResourceName));
		}

		/// <summary>
		/// Loads an XSL transformation from an embedded resource.
		/// </summary>
		/// <param name="ResourceName">Resource Name.</param>
		/// <param name="Assembly">Assembly containing the resource.</param>
		/// <returns>XSL tranformation.</returns>
		/// <exception cref="IOException">If Resource name is not valid or resource not found.</exception>
		public static XslCompiledTransform LoadTransform(string ResourceName, Assembly Assembly)
		{
			using (Stream f = Assembly.GetManifestResourceStream(ResourceName))
			{
				if (f is null)
					throw new ArgumentException("Resource not found: " + ResourceName, nameof(ResourceName));

				using (XmlReader r = XmlReader.Create(f))
				{
					XslCompiledTransform Xslt = new XslCompiledTransform();
					Xslt.Load(r);

					return Xslt;
				}
			}
		}

		#region Validation

		/// <summary>
		/// Validates an XML document given a set of XML schemas.
		/// </summary>
		/// <param name="ObjectID">Object ID of XML document. Used in case validation warnings are found during validation.</param>
		/// <param name="Xml">XML document.</param>
		/// <param name="Schemas">XML schemas.</param>
		/// <exception cref="XmlSchemaException">If a validation exception occurred.</exception>
		public static void Validate(string ObjectID, XmlDocument Xml, params XmlSchema[] Schemas)
		{
			Validate(ObjectID, Xml, string.Empty, string.Empty, Schemas);
		}

		/// <summary>
		/// Validates an XML document given a set of XML schemas.
		/// </summary>
		/// <param name="ObjectID">Object ID of XML document. Used in case validation warnings are found during validation.</param>
		/// <param name="Xml">XML document.</param>
		/// <param name="ExpectedRootElement">Expected Root Element local name.</param>
		/// <param name="ExpectedRootElementNamespace">Expected Root Element namespace.</param>
		/// <param name="Schemas">XML schemas.</param>
		/// <exception cref="XmlSchemaException">If a validation exception occurred.</exception>
		public static void Validate(string ObjectID, XmlDocument Xml, string ExpectedRootElement, string ExpectedRootElementNamespace,
			params XmlSchema[] Schemas)
		{
			if (!string.IsNullOrEmpty(ExpectedRootElement) &&
				(Xml.DocumentElement is null ||
				Xml.DocumentElement.LocalName != ExpectedRootElement ||
				Xml.DocumentElement.NamespaceURI != ExpectedRootElementNamespace))
			{
				throw new XmlSchemaException("Expected root element is " + ExpectedRootElement + " (" + ExpectedRootElementNamespace + ")");
			}

			foreach (XmlSchema Schema in Schemas)
				Xml.Schemas.Add(Schema);

			XmlValidator Validator = new XmlValidator(ObjectID);
			Xml.Validate(Validator.ValidationCallback);

			Validator.AssertNoError();
		}

		#endregion

		#region Transformation

		/// <summary>
		/// Transforms an XML document using an XSL transform.
		/// </summary>
		/// <param name="XML">XML document.</param>
		/// <param name="Transform">Transform.</param>
		/// <returns>Transformed output.</returns>
		public static string Transform(string XML, XslCompiledTransform Transform)
		{
			StringBuilder Output = new StringBuilder();
			TextWriter OutputText = new StringWriter(Output);
			TextReader InputText = new StringReader(XML);
			XmlReader XmlReader = XmlReader.Create(InputText);
			XmlWriterSettings Settings = Content.Xml.XML.WriterSettings(false, true);
			Settings.ConformanceLevel = ConformanceLevel.Auto;

			XmlWriter XmlWriter = XmlWriter.Create(OutputText, Settings);

			try
			{
				Transform.Transform(XmlReader, XmlWriter);
			}
			finally
			{
				XmlWriter.Flush();
				OutputText.Flush();

				XmlReader.Close();
				InputText.Dispose();

				XmlWriter.Close();
				OutputText.Dispose();
			}

			return Output.ToString();
		}

		#endregion

	}
}
