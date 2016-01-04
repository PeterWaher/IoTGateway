using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Xsl;
using Waher.Events;

namespace Waher.Networking
{
	/// <summary>
	/// Helps with common XML-related tasks.
	/// </summary>
	public static class XML
	{
		#region Encoding/Decoding

		/// <summary>
		/// Encodes a string for use in XML.
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>XML-encoded string.</returns>
		public static string Encode(string s)
		{
			if (s.IndexOfAny(specialCharacters) < 0)
				return s;

			return s.
				Replace("&", "&amp;").
				Replace("<", "&lt;").
				Replace(">", "&gt;").
				Replace("\"", "&quot;").
				Replace("'", "&apos;");
		}

		private static readonly char[] specialCharacters = new char[] { '<', '>', '&', '"', '\'' };

		/// <summary>
		/// Encodes a <see cref="DateTime"/> for use in XML.
		/// </summary>
		/// <param name="DT">Value to encode.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(DateTime DT)
		{
			return Encode(DT, false);
		}

		/// <summary>
		/// Encodes a <see cref="DateTime"/> for use in XML.
		/// </summary>
		/// <param name="DT">Value to encode.</param>
		/// <param name="DateOnly">If only the date should be encoded (true), or both date and time (false).</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(DateTime DT, bool DateOnly)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(DT.Year.ToString("D4"));
			sb.Append('-');
			sb.Append(DT.Month.ToString("D2"));
			sb.Append('-');
			sb.Append(DT.Day.ToString("D2"));

			if (!DateOnly)
			{
				sb.Append('T');
				sb.Append(DT.Hour.ToString("D2"));
				sb.Append(':');
				sb.Append(DT.Minute.ToString("D2"));
				sb.Append(':');
				sb.Append(DT.Second.ToString("D2"));
				sb.Append('.');
				sb.Append(DT.Millisecond.ToString("D3"));
			}

			return sb.ToString();
		}

		/// <summary>
		/// Encodes a <see cref="DateTimeOffset"/> for use in XML.
		/// </summary>
		/// <param name="DTO">Value to encode.</param>
		/// <returns>XML-encoded value.</returns>
		public static string Encode(DateTimeOffset DTO)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(DTO.Year.ToString("D4"));
			sb.Append('-');
			sb.Append(DTO.Month.ToString("D2"));
			sb.Append('-');
			sb.Append(DTO.Day.ToString("D2"));
			sb.Append('T');
			sb.Append(DTO.Hour.ToString("D2"));
			sb.Append(':');
			sb.Append(DTO.Minute.ToString("D2"));
			sb.Append(':');
			sb.Append(DTO.Second.ToString("D2"));
			sb.Append('.');
			sb.Append(DTO.Millisecond.ToString("D3"));

			TimeSpan TS = DTO.Offset;
			if (TS < TimeSpan.Zero)
			{
				sb.Append('-');
				TS = -TS;
			}
			else
				sb.Append('+');

			sb.Append(TS.Hours.ToString("D2"));
			sb.Append(':');
			sb.Append(TS.Minutes.ToString("D2"));

			return sb.ToString();
		}

		/// <summary>
		/// Decodes a string used in XML.
		/// </summary>
		/// <param name="s">String</param>
		/// <returns>XML-decoded string.</returns>
		public static string DecodeString(string s)
		{
			if (s.IndexOf('&') < 0)
				return s;

			return s.
				Replace("&apos;", "'").
				Replace("&qout;", "\"").
				Replace("&lt;", "<").
				Replace("&gt;", ">").
				Replace("&amp;", "&");
		}

		#endregion

		#region XML Attributes

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <returns>Value of attribute, if found, or the empty string, if not found.</returns>
		public static string Attribute(XmlElement E, string Name)
		{
			if (E.HasAttribute(Name))
				return E.GetAttribute(Name);
			else
				return string.Empty;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static string Attribute(XmlElement E, string Name, string DefaultValue)
		{
			if (E.HasAttribute(Name))
				return E.GetAttribute(Name);
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static bool Attribute(XmlElement E, string Name, bool DefaultValue)
		{
			bool Result;

			if (E.HasAttribute(Name))
			{
				if (CommonTypes.TryParse(E.GetAttribute(Name), out Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static int Attribute(XmlElement E, string Name, int DefaultValue)
		{
			int Result;

			if (E.HasAttribute(Name))
			{
				if (int.TryParse(E.GetAttribute(Name), out Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static double Attribute(XmlElement E, string Name, double DefaultValue)
		{
			double Result;

			if (E.HasAttribute(Name))
			{
				if (CommonTypes.TryParse(E.GetAttribute(Name), out Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static decimal Attribute(XmlElement E, string Name, decimal DefaultValue)
		{
			decimal Result;

			if (E.HasAttribute(Name))
			{
				if (CommonTypes.TryParse(E.GetAttribute(Name), out Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static DateTime Attribute(XmlElement E, string Name, DateTime DefaultValue)
		{
			DateTime Result;

			if (E.HasAttribute(Name))
			{
				if (CommonTypes.TryParse(E.GetAttribute(Name), out Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static Enum Attribute(XmlElement E, string Name, Enum DefaultValue)
		{
			if (E.HasAttribute(Name))
			{
				try
				{
					return (Enum)Enum.Parse(DefaultValue.GetType(), E.GetAttribute(Name));
				}
				catch (Exception)
				{
					return DefaultValue;
				}
			}
			else
				return DefaultValue;
		}

		/// <summary>
		/// Gets the value of an XML attribute.
		/// </summary>
		/// <param name="E">XML Element</param>
		/// <param name="Name">Name of attribute</param>
		/// <param name="DefaultValue">Default value.</param>
		/// <returns>Value of attribute, if found, or the default value, if not found.</returns>
		public static Duration Attribute(XmlElement E, string Name, Duration DefaultValue)
		{
			Duration Result;

			if (E.HasAttribute(Name))
			{
				if (Duration.TryParse(E.GetAttribute(Name), out Result))
					return Result;
				else
					return DefaultValue;
			}
			else
				return DefaultValue;
		}

		#endregion

		#region Validation

		/// <summary>
		/// Loads an XML schema from a file.
		/// </summary>
		/// <param name="FileName">File Name.</param>
		/// <returns>XML Schema.</returns>
		/// <exception cref="XmlSchemaException">If a validation exception occurred.</exception>
		/// <exception cref="IOException">If File name is not valid or file not found.</exception>
		public static XmlSchema LoadSchema(string FileName)
		{
			using (FileStream f = File.OpenRead(FileName))
			{
				XmlValidator Validator = new XmlValidator(FileName);
				XmlSchema Result = XmlSchema.Read(f, Validator.ValidationCallback);

				Validator.AssertNoError();

				return Result;
			}
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
				XmlValidator Validator = new XmlValidator(ResourceName);
				XmlSchema Result = XmlSchema.Read(f, Validator.ValidationCallback);

				Validator.AssertNoError();

				return Result;
			}
		}

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
				(Xml.DocumentElement == null ||
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
		/// Loads an XSL transformation from a file.
		/// </summary>
		/// <param name="FileName">File Name.</param>
		/// <returns>XSL tranformation.</returns>
		/// <exception cref="IOException">If File name is not valid or file not found.</exception>
		public static XslCompiledTransform LoadTransform(string FileName)
		{
			using (FileStream f = File.OpenRead(FileName))
			{
				using (XmlReader r = XmlReader.Create(f))
				{
					XslCompiledTransform Xslt = new XslCompiledTransform();
					Xslt.Load(r);

					return Xslt;
				}
			}
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
				using (XmlReader r = XmlReader.Create(f))
				{
					XslCompiledTransform Xslt = new XslCompiledTransform();
					Xslt.Load(r);

					return Xslt;
				}
			}
		}

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
			XmlWriter XmlWriter = XmlWriter.Create(OutputText, WriterSettings(false, true));

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

		#region Settings

		/// <summary>
		/// Gets an XML writer settings object.
		/// </summary>
		/// <param name="Indent">If output should be indented.</param>
		/// <param name="OmitXmlDeclaration">If the XML declaration should be omitted.</param>
		/// <returns>Settings object.</returns>
		public static XmlWriterSettings WriterSettings(bool Indent, bool OmitXmlDeclaration)
		{
			XmlWriterSettings Settings = new XmlWriterSettings();
			Settings.CloseOutput = false;
			Settings.ConformanceLevel = ConformanceLevel.Document;
			Settings.Encoding = System.Text.Encoding.UTF8;

			if (Indent)
			{
				Settings.Indent = true;
				Settings.IndentChars = "\t";
				Settings.NewLineChars = "\r\n";
				Settings.NewLineHandling = NewLineHandling.Entitize;
			}
			else
				Settings.Indent = false;

			Settings.NewLineOnAttributes = false;
			Settings.OmitXmlDeclaration = OmitXmlDeclaration;

			return Settings;
		}

		#endregion

	}
}
