using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;
using System.Web.Services.Protocols;

namespace Waher.Networking.UPnP
{
	/// <summary>
	/// Contains information about an action.
	/// </summary>
	public class UPnPAction
	{
		private Dictionary<string, UPnPArgument> argumentByName = new Dictionary<string, UPnPArgument>();
		private ServiceDescriptionDocument parent;
		private XmlElement xml;
		private UPnPArgument[] arguments;
		private string name;

		internal UPnPAction(XmlElement Xml, ServiceDescriptionDocument Parent)
		{
			List<UPnPArgument> Arguments = new List<UPnPArgument>();

			this.parent = Parent;
			this.xml = Xml;

			foreach (XmlNode N in Xml.ChildNodes)
			{
				switch (N.LocalName)
				{
					case "name":
						this.name = N.InnerText;
						break;

					case "argumentList":
						foreach (XmlNode N2 in N.ChildNodes)
						{
							if (N2.LocalName == "argument")
							{
								UPnPArgument Argument = new UPnPArgument((XmlElement)N2);
								Arguments.Add(Argument);
								this.argumentByName[Argument.Name] = Argument;
							}
						}
						break;
				}
			}

			this.arguments = Arguments.ToArray();
		}

		/// <summary>
		/// Underlying XML definition.
		/// </summary>
		public XmlElement Xml
		{
			get { return this.xml; }
		}

		/// <summary>
		/// Action Name
		/// </summary>
		public string Name { get { return this.name; } }

		/// <summary>
		/// Service Arguments.
		/// </summary>
		public UPnPArgument[] Arguments
		{
			get { return this.arguments; }
		}

		/// <summary>
		/// Parent Service Description Document object.
		/// </summary>
		public ServiceDescriptionDocument ServiceDescriptionDocument
		{
			get { return this.parent; }
		}

		/// <summary>
		/// Invokes the action.
		/// </summary>
		/// <param name="OutputValues">Output values.</param>
		/// <param name="InputValues">Input values.</param>
		/// <returns>Return value, if any, null otherwise.</returns>
		public object Invoke(out Dictionary<string, object> OutputValues, params KeyValuePair<string, object>[] InputValues)
		{
			Dictionary<string, object> InputValues2 = new Dictionary<string, object>();

			foreach (KeyValuePair<string, object> P in InputValues)
				InputValues2[P.Key] = P.Value;

			return this.Invoke(InputValues2, out OutputValues);
		}

		/// <summary>
		/// Invokes the action.
		/// </summary>
		/// <param name="InputValues">Input values.</param>
		/// <param name="OutputValues">Output values.</param>
		/// <returns>Return value, if any, null otherwise.</returns>
		public object Invoke(Dictionary<string, object> InputValues, out Dictionary<string, object> OutputValues)
		{
			StringBuilder Soap = new StringBuilder();
			UPnPStateVariable Variable;
			object Value;
			object Result = null;
			object First = null;

			Soap.AppendLine("<?xml version=\"1.0\"?>");
			Soap.AppendLine("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">");
			Soap.AppendLine("<s:Body>");
			Soap.Append("<u:");
			Soap.Append(this.name);
			Soap.Append(" xmlns:u=\"");
			Soap.Append(XmlAttributeEncode(this.parent.Service.ServiceType));
			Soap.AppendLine("\">");

			foreach (UPnPArgument Argument in this.arguments)
			{
				if (Argument.Direction == ArgumentDirection.In)
				{
					Soap.Append("<");
					Soap.Append(Argument.Name);
					Soap.Append(">");

					if (InputValues.TryGetValue(Argument.Name, out Value) &&
						(Variable = this.parent.GetVariable(Argument.RelatedStateVariable)) != null)
					{
						Soap.Append(XmlAttributeEncode(Variable.ValueToXmlString(Value)));
					}

					Soap.Append("</");
					Soap.Append(Argument.Name);
					Soap.Append(">");
				}
			}

			Soap.Append("</u:");
			Soap.Append(this.name);
			Soap.AppendLine(">");
			Soap.AppendLine("</s:Body>");
			Soap.AppendLine("</s:Envelope>");

			string Body = Soap.ToString();
			byte[] BodyBin = Encoding.UTF8.GetBytes(Body);

			using (WebClient WebClient = new WebClient())
			{
				WebClient.Headers["CONTENT-TYPE"] = "text/xml; charset=\"utf-8\"";
				WebClient.Headers["SOAPACTION"] = this.parent.Service.ServiceType + "#" + this.name;

				byte[] Response;
				XmlDocument ResponseXml;

				try
				{
					Response = WebClient.UploadData(this.parent.Service.ControlURI, "POST", BodyBin);
					ResponseXml = new XmlDocument();
					ResponseXml.Load(new MemoryStream(Response));
				}
				catch (WebException ex)
				{
					ResponseXml = new XmlDocument();
					try
					{
						ResponseXml.Load(ex.Response.GetResponseStream());
					}
					catch
					{
						ResponseXml = null;
					}

					if (ResponseXml == null)
						throw;
				}

				if (ResponseXml.DocumentElement == null ||
					ResponseXml.DocumentElement.LocalName != "Envelope" ||
					ResponseXml.DocumentElement.NamespaceURI != "http://schemas.xmlsoap.org/soap/envelope/")
				{
					throw new Exception("Unexpected response returned.");
				}

				XmlElement ResponseBody = GetChildElement(ResponseXml.DocumentElement, "Body", "http://schemas.xmlsoap.org/soap/envelope/");
				if (ResponseBody == null)
					throw new Exception("Response body not found.");

				XmlElement ActionResponse = GetChildElement(ResponseBody, this.name + "Response", this.parent.Service.ServiceType);
				if (ActionResponse == null)
				{
					XmlElement ResponseFault = GetChildElement(ResponseBody, "Fault", "http://schemas.xmlsoap.org/soap/envelope/");
					if (ResponseFault == null)
						throw new Exception("Unable to parse response.");

					string FaultCode = string.Empty;
					string FaultString = string.Empty;
					string UPnPErrorCode = string.Empty;
					string UPnPErrorDescription = string.Empty;

					foreach (XmlNode N in ResponseFault.ChildNodes)
					{
						switch (N.LocalName)
						{
							case "faultcode":
								FaultCode = N.InnerText;
								break;

							case "faultstring":
								FaultString = N.InnerText;
								break;

							case "detail":
								foreach (XmlNode N2 in N.ChildNodes)
								{
									switch (N2.LocalName)
									{
										case "UPnPError":
											foreach (XmlNode N3 in N2.ChildNodes)
											{
												switch (N3.LocalName)
												{
													case "errorCode":
														UPnPErrorCode = N3.InnerText;
														break;

													case "errorDescription":
														UPnPErrorDescription = N3.InnerText;
														break;
												}
											}
											break;
									}
								}
								break;
						}
					}

					throw new UPnPException(FaultCode, FaultString, UPnPErrorCode, UPnPErrorDescription);
				}

				OutputValues = new Dictionary<string, object>();

				UPnPArgument Argument2;
				XmlElement E;

				foreach (XmlNode N in ActionResponse.ChildNodes)
				{
					E = N as XmlElement;
					if (E == null)
						continue;

					if (this.argumentByName.TryGetValue(E.LocalName, out Argument2))
					{
						if ((Variable = this.parent.GetVariable(Argument2.RelatedStateVariable)) != null)
						{
							object Value2 = Variable.XmlStringToValue(E.InnerText);
							OutputValues[E.LocalName] = Value2;

							if (First == null)
								First = Value2;

							if (Argument2.ReturnValue && Result == null)
								Result = Value2;
						}
						else
						{
							if (First == null)
								First = E.InnerXml;

							OutputValues[E.LocalName] = E.InnerXml;
						}
					}
					else
					{
						if (First == null)
							First = E.InnerXml;

						OutputValues[E.LocalName] = E.InnerXml;
					}
				}
			}

			if (Result == null)
				Result = First;

			return Result;
		}

		private static XmlElement GetChildElement(XmlElement E, string LocalName, string Namespace)
		{
			XmlElement E2;

			foreach (XmlNode N in E.ChildNodes)
			{
				E2 = N as XmlElement;
				if (E2 == null)
					continue;

				if (E2.LocalName == LocalName && E2.NamespaceURI == Namespace)
					return E2;
			}

			return null;
		}

		public static string XmlAttributeEncode(string AttributeValue)
		{
			if (AttributeValue.IndexOfAny(reservedCharacters) < 0)
				return AttributeValue;

			return AttributeValue.
				Replace("&", "&amp;").
				Replace("<", "&lt;").
				Replace(">", "&gt;").
				Replace("\"", "&quot;").
				Replace("'", "&apos;");
		}

		private static readonly char[] reservedCharacters = new char[] { '&', '<', '>', '"', '\'' };

	}
}
