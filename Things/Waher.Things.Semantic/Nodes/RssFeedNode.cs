using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Html.Elements;
using Waher.Content.Rss;
using Waher.IoTGateway;
using Waher.Persistence.Attributes;
using Waher.Runtime.Language;
using Waher.Things.Attributes;
using Waher.Things.DisplayableParameters;
using Waher.Things.Ip;
using Waher.Things.Metering;
using Waher.Things.Metering.NodeTypes;
using Waher.Things.SensorData;
using Waher.Things.Virtual;

namespace Waher.Things.Semantic.Nodes
{
	public class RssFeedNode : ProvisionedMeteringNode, ISensor
	{
		private string url = string.Empty;

		/// <summary>
		/// If the node accepts a presumptive parent, i.e. can be added to that parent (if that parent accepts the node as a child).
		/// </summary>
		/// <param name="Parent">Presumptive parent node.</param>
		/// <returns>If the parent is acceptable.</returns>
		public override Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult(Parent is Root || Parent is IpHost || Parent is VirtualNode);
		}

		/// <summary>
		/// If the node accepts a presumptive child, i.e. can receive as a child (if that child accepts the node as a parent).
		/// </summary>
		/// <param name="Child">Presumptive child node.</param>
		/// <returns>If the child is acceptable.</returns>
		public override Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult(false);
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(RssFeedNode), 1, "RSS Feed");
		}

		/// <summary>
		/// Host name or IP address.
		/// </summary>
		[Page(2, "RSS")]
		[Header(3, "URL:", 50)]
		[ToolTip(4, "Link to RSS Feed.")]
		[DefaultValueStringEmpty]
		[Required]
		public string Url
		{
			get => this.url;
			set => this.url = value;
		}

		/// <summary>
		/// Gets displayable parameters.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <param name="Caller">Information about caller.</param>
		/// <returns>Set of displayable parameters.</returns>
		public async override Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			LinkedList<Parameter> Result = await base.GetDisplayableParametersAsync(Language, Caller) as LinkedList<Parameter>;

			Result.AddLast(new StringParameter("URL", await Language.GetStringAsync(typeof(RssFeedNode), 5, "URL"), this.url));

			return Result;
		}

		/// <summary>
		/// If the node can be read.
		/// </summary>
		public override bool IsReadable => true;

		/// <summary>
		/// Starts the readout of the sensor.
		/// </summary>
		/// <param name="Request">Request object. All fields and errors should be reported to this interface.</param>
		public async virtual Task StartReadout(ISensorReadout Request)
		{
			try
			{
				if (!Uri.TryCreate(this.url, UriKind.Absolute, out Uri Link))
					throw new Exception("Invalid URL.");

				object Obj = await InternetContent.GetAsync(Link, Gateway.Certificate, 10000,
					new KeyValuePair<string, string>("Accept", RssCodec.ContentType));

				if (!(Obj is RssDocument Doc))
				{
					if (Obj is XmlDocument Xml)
						Doc = new RssDocument(Xml, Link);
					else
						throw new Exception("URL did not return an RSS document.");
				}

				DateTime Now = DateTime.UtcNow;
				List<Field> Fields = new List<Field>();

				foreach (RssChannel Channel in Doc.Channels)
				{
					DateTime Publication = Now;

					if (Fields.Count > 0)
					{
						Request.ReportFields(false, Fields.ToArray());
						Fields.Clear();
					}

					if (!string.IsNullOrEmpty(Channel.Title))
						Fields.Add(new StringField(this, Now, "Channel, Title", Channel.Title, FieldType.Identity, FieldQoS.AutomaticReadout));

					if (!(Channel.Link is null))
						Fields.Add(new StringField(this, Now, "Channel, Link", Channel.Link.ToString(), FieldType.Identity, FieldQoS.AutomaticReadout));

					if (!string.IsNullOrEmpty(Channel.Description))
						Fields.Add(new StringField(this, Now, "Channel, Description", Channel.Description, FieldType.Identity, FieldQoS.AutomaticReadout));

					if (!string.IsNullOrEmpty(Channel.Language))
						Fields.Add(new StringField(this, Now, "Channel, Language", Channel.Language, FieldType.Identity, FieldQoS.AutomaticReadout));

					if (!string.IsNullOrEmpty(Channel.Copyright))
						Fields.Add(new StringField(this, Now, "Channel, Copyright", Channel.Copyright, FieldType.Identity, FieldQoS.AutomaticReadout));

					if (!string.IsNullOrEmpty(Channel.ManagingEditor))
						Fields.Add(new StringField(this, Now, "Channel, Managing Editor", Channel.ManagingEditor, FieldType.Identity, FieldQoS.AutomaticReadout));

					if (!string.IsNullOrEmpty(Channel.WebMaster))
						Fields.Add(new StringField(this, Now, "Channel, Web Master", Channel.WebMaster, FieldType.Identity, FieldQoS.AutomaticReadout));

					if (Channel.PublicationDate.HasValue)
					{
						Publication = Channel.PublicationDate.Value.UtcDateTime;
						Fields.Add(new DateTimeField(this, Now, "Channel, Publication", Publication, FieldType.Status, FieldQoS.AutomaticReadout));
					}

					if (!string.IsNullOrEmpty(Channel.Generator))
						Fields.Add(new StringField(this, Now, "Channel, Generator", Channel.Generator, FieldType.Identity, FieldQoS.AutomaticReadout));

					if (!(Channel.Documentation is null))
						Fields.Add(new StringField(this, Now, "Channel, Documentation", Channel.Documentation.ToString(), FieldType.Identity, FieldQoS.AutomaticReadout));

					Fields.Add(new TimeField(this, Now, "Channel, Time To Live", Channel.TimeToLive, FieldType.Status, FieldQoS.AutomaticReadout));

					if (!(Channel.Categories is null))
					{
						int i = 0;

						foreach (RssCategory Category in Channel.Categories)
						{
							string s = "Channel, Category " + (++i).ToString();

							Fields.Add(new StringField(this, Now, s + ", Name", Category.Name, FieldType.Identity, FieldQoS.AutomaticReadout));

							if (!(Category.Domain is null))
								Fields.Add(new StringField(this, Now, s + ", Domain", Category.Domain.ToString(), FieldType.Identity, FieldQoS.AutomaticReadout));
						}
					}

					if (!(Channel.Image is null))
					{
						if (!(Channel.Image.Url is null))
							Fields.Add(new StringField(this, Now, "Channel, Image, Url", Channel.Image.Url.ToString(), FieldType.Identity, FieldQoS.AutomaticReadout));

						if (!string.IsNullOrEmpty(Channel.Image.Title))
							Fields.Add(new StringField(this, Now, "Channel, Image, Title", Channel.Image.Title, FieldType.Identity, FieldQoS.AutomaticReadout));

						if (!(Channel.Image.Link is null))
							Fields.Add(new StringField(this, Now, "Channel, Image, Link", Channel.Image.Link.ToString(), FieldType.Identity, FieldQoS.AutomaticReadout));

						if (Channel.Image.Width > 0)
							Fields.Add(new StringField(this, Now, "Channel, Image, Width", Channel.Image.Width.ToString(), FieldType.Identity, FieldQoS.AutomaticReadout));

						if (Channel.Image.Height > 0)
							Fields.Add(new StringField(this, Now, "Channel, Image, Height", Channel.Image.Height.ToString(), FieldType.Identity, FieldQoS.AutomaticReadout));

						if (!string.IsNullOrEmpty(Channel.Image.Description))
							Fields.Add(new StringField(this, Now, "Channel, Image, Description", Channel.Image.Description, FieldType.Identity, FieldQoS.AutomaticReadout));
					}

					if (!(Channel.Items is null))
					{
						foreach (RssItem Item in Channel.Items)
						{
							Publication = Now;

							if (Item.PublicationDate.HasValue)
								Publication = Item.PublicationDate.Value.UtcDateTime;

							if (!string.IsNullOrEmpty(Item.Title))
								Fields.Add(new StringField(this, Publication, "Title", Item.Title, FieldType.Historical, FieldQoS.AutomaticReadout));

							if (!(Item.Link is null))
								Fields.Add(new StringField(this, Publication, "Link", Item.Link.ToString(), FieldType.Historical, FieldQoS.AutomaticReadout));

							if (!string.IsNullOrEmpty(Item.Description))
								Fields.Add(new StringField(this, Publication, "Description", Item.Description, FieldType.Historical, FieldQoS.AutomaticReadout));

							if (!string.IsNullOrEmpty(Item.Author))
								Fields.Add(new StringField(this, Publication, "Author", Item.Author, FieldType.Historical, FieldQoS.AutomaticReadout));

							if (!(Item.Comments is null))
								Fields.Add(new StringField(this, Publication, "Comments", Item.Comments.ToString(), FieldType.Historical, FieldQoS.AutomaticReadout));

							if (!(Item.Guid is null))
							{
								Fields.Add(new StringField(this, Publication, "Guid", Item.Guid.Id, FieldType.Historical, FieldQoS.AutomaticReadout));
								Fields.Add(new BooleanField(this, Publication, "Guid, IsPermaLink", Item.Guid.IsPermaLink, FieldType.Historical, FieldQoS.AutomaticReadout));
							}

							if (!(Item.Source is null))
							{
								if (!(Item.Source.Url is null))
									Fields.Add(new StringField(this, Publication, "Source, URL", Item.Source.Url.ToString(), FieldType.Historical, FieldQoS.AutomaticReadout));
							
								if (!string.IsNullOrEmpty(Item.Source.Title))
									Fields.Add(new StringField(this, Publication, "Source, Title", Item.Source.Title, FieldType.Historical, FieldQoS.AutomaticReadout));
							}

							if (!(Item.Enclosure is null))
							{
								if (!(Item.Enclosure.Url is null))
									Fields.Add(new StringField(this, Publication, "Enclosure, URL", Item.Enclosure.Url.ToString(), FieldType.Historical, FieldQoS.AutomaticReadout));

								if (!string.IsNullOrEmpty(Item.Enclosure.ContentType))
									Fields.Add(new StringField(this, Publication, "Enclosure, Content-Type", Item.Enclosure.ContentType, FieldType.Historical, FieldQoS.AutomaticReadout));

								if (Item.Enclosure.Length > 0)
									Fields.Add(new Int64Field(this, Publication, "Enclosure, Length", Item.Enclosure.Length, FieldType.Historical, FieldQoS.AutomaticReadout));
							}

							if (!(Item.Categories is null))
							{
								int i = 0;

								foreach (RssCategory Category in Item.Categories)
								{
									string s = "Category " + (++i).ToString();

									Fields.Add(new StringField(this, Publication, s + ", Name", Category.Name, FieldType.Historical, FieldQoS.AutomaticReadout));

									if (!(Category.Domain is null))
										Fields.Add(new StringField(this, Now, s + ", Domain", Category.Domain.ToString(), FieldType.Historical, FieldQoS.AutomaticReadout));
								}
							}
						}
					}
				}

				if (!(Doc.Warnings is null) && Doc.Warnings.Length > 0)
				{
					List<ThingError> Errors = new List<ThingError>();

					foreach (RssWarning Warning in Doc.Warnings)
						Errors.Add(new ThingError(this, Warning.Message));

					Request.ReportErrors(false, Errors.ToArray());
				}

				Request.ReportFields(true, Fields.ToArray());
			}
			catch (Exception ex)
			{
				Request.ReportErrors(true, new ThingError(this, ex.Message));
			}
		}

	}
}
