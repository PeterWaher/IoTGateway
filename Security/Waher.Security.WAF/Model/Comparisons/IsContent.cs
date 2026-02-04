using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content.Xml;
using Waher.Content.Xml.Attributes;
using Waher.Events;
using Waher.Networking.HTTP.Interfaces;

namespace Waher.Security.WAF.Model.Comparisons
{
	/// <summary>
	/// Checks if the request is encrypted.
	/// </summary>
	public class IsContent : WafComparison
	{
		private readonly StringAttribute manifestFile;

		/// <summary>
		/// Checks if the request is encrypted.
		/// </summary>
		public IsContent()
			: base()
		{
		}

		/// <summary>
		/// Checks if the request is encrypted.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		public IsContent(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document)
			: base(Xml, Parent, Document)
		{
			this.manifestFile = new StringAttribute(Xml, "manifestFile");
		}

		/// <summary>
		/// XML Local Name for the XML element defining the action.
		/// </summary>
		public override string LocalName => nameof(IsContent);

		/// <summary>
		/// Creates a WAF action from its XML definition.
		/// </summary>
		/// <param name="Xml">XML definition.</param>
		/// <param name="Parent">Parent node.</param>
		/// <param name="Document">Document hosting the Web Application Firewall action.</param>
		/// <returns>Created action object.</returns>
		public override WafAction Create(XmlElement Xml, WafAction Parent, WebApplicationFirewall Document) => new IsContent(Xml, Parent, Document);

		/// <summary>
		/// Reviews the processing state, and returns a WAF result, if any.
		/// </summary>
		/// <param name="State">Current state.</param>
		/// <returns>Result to return, if any.</returns>
		public override async Task<WafResult?> Review(ProcessingState State)
		{
			if (!State.Request.TryGetLocalResourceFileName(State.Request.Header.ResourcePart,
				State.Request.Host, out string ResourceFileName))
			{
				return null;
			}

			string ManifestFileName = await this.manifestFile.EvaluateAsync(State.Variables, string.Empty);
			string Key = ResourceFileName + "|C|" + ManifestFileName;

			if (!State.TryGetCachedObject(Key, out bool IsMatch))
			{
				if (!File.Exists(ManifestFileName))
					ManifestFileName = Path.Combine(State.Firewall.AppDataFolder, ManifestFileName);

				if (ResourceFileName.StartsWith(State.Firewall.AppDataFolder, StringComparison.OrdinalIgnoreCase))
					ResourceFileName = ResourceFileName[State.Firewall.AppDataFolder.Length..];

				if (ResourceFileName.StartsWith('/'))
					ResourceFileName = ResourceFileName[1..];

				IsMatch = CheckMatch(ManifestFileName, ResourceFileName);

				State.AddToCache(Key, IsMatch, fiveMinutes);
			}

			return IsMatch ? await this.ReviewChildren(State) : null;
		}

		private static bool CheckMatch(string ManifestFileName, string ContentFileName)
		{
			try
			{
				if (!File.Exists(ManifestFileName))
					return false;

				XmlDocument Doc = XML.LoadFromFile(ManifestFileName);

				if (Doc.DocumentElement is null ||
					Doc.DocumentElement.LocalName != "Module" ||
					Doc.NamespaceURI != "http://waher.se/Schema/ModuleManifest.xsd")
				{
					return false;
				}

				XmlElement Loop = Doc.DocumentElement;
				string[] Parts = ContentFileName.Split('/');
				int i, c = Parts.Length;

				for (i = 0; i < c; i++)
				{
					XmlElement Found = null;

					foreach (XmlNode N in Loop.ChildNodes)
					{
						if (!(N is XmlElement E))
							continue;

						if (i == c - 1)
						{
							if (E.LocalName == "Content" &&
								E.HasAttribute("fileName") &&
								E.GetAttribute("fileName").Equals(Parts[i], StringComparison.CurrentCultureIgnoreCase))
							{
								Found = E;
								break;
							} 
						}
						else
						{
							if (E.LocalName == "Folder" &&
								E.HasAttribute("name") &&
								E.GetAttribute("name").Equals(Parts[i], StringComparison.CurrentCultureIgnoreCase))
							{
								Found = E;
								break;
							}
						}
					}

					if (Found is null)
						return false;
					else
						Loop = Found;
				}

				return true;
			}
			catch (Exception ex)
			{
				Log.Exception(ex, ManifestFileName);
				return false;
			}
		}
	}
}
