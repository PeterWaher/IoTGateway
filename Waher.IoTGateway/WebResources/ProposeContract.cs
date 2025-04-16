using System;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Waher.Content;
using Waher.Content.Markdown.Web.ScriptExtensions;
using Waher.Content.Xml;
using Waher.Networking.HTTP;
using Waher.Networking.XMPP.Contracts;
using Waher.Script;

namespace Waher.IoTGateway.WebResources
{
	/// <summary>
	/// Proposes a new smart contract
	/// </summary>
	public class ProposeContract : HttpSynchronousResource, IHttpPostMethod
	{
		/// <summary>
		/// Proposes a new smart contract
		/// </summary>
		public ProposeContract()
			: base("/ProposeContract")
		{
		}

		/// <summary>
		/// If the resource handles sub-paths.
		/// </summary>
		public override bool HandlesSubPaths => false;

		/// <summary>
		/// If the resource uses user sessions.
		/// </summary>
		public override bool UserSessions => true;

		/// <summary>
		/// If the POST method is allowed.
		/// </summary>
		public bool AllowsPOST => true;

		/// <summary>
		/// Executes the POST method on the resource.
		/// </summary>
		/// <param name="Request">HTTP Request</param>
		/// <param name="Response">HTTP Response</param>
		/// <exception cref="HttpException">If an error occurred when processing the method.</exception>
		public async Task POST(HttpRequest Request, HttpResponse Response)
		{
			string Xml = null;

			try
			{
				Gateway.AssertUserAuthenticated(Request, "Admin.Legal.ProposeContract");

				if (Gateway.ContractsClient is null)
				{
					await Response.SendResponse(new NotSupportedException("Proposing new contracts not permitted. Broker does not support smart contracts."));
					return;
				}

				if (!Request.HasData)
				{
					await Response.SendResponse(new BadRequestException("No data in post."));
					return;
				}

				Variables PageVariables = Page.GetPageVariables(Request.Session, "/ProposeContract.md");
				ContentResponse Posted = await Request.DecodeDataAsync();
				
				if (Posted.HasError)
				{
					await Response.SendResponse(Posted.Error);
					return;
				}

				if (Posted.Decoded is XmlDocument Doc)
				{
					ParsedContract ParsedContract = await Contract.Parse(Doc, Gateway.ContractsClient);
					if (ParsedContract?.Contract is null)
					{
						await Response.SendResponse(new BadRequestException("Unable to parse contract."));
						return;
					}

					if (ParsedContract.HasStatus)
					{
						await Response.SendResponse(new ForbiddenException(Request, "Contract must not have a status section."));
						return;
					}

					if (!ParsedContract.ParametersValid && ParsedContract.Contract.PartsMode != ContractParts.TemplateOnly)
					{
						await Response.SendResponse(new BadRequestException("Contract parameter values not valid."));
						return;
					}

					StringBuilder sb = new StringBuilder();

					Contract.NormalizeXml(ParsedContract.Contract.ForMachines, sb, ParsedContract.Contract.Namespace);

					Doc = new XmlDocument()
					{
						PreserveWhitespace = true
					};
					Doc.LoadXml(Xml = sb.ToString());

					ParsedContract.Contract.ForMachines = Doc.DocumentElement;

					PageVariables["Contract"] = ParsedContract.Contract;
				}
				else if (Posted.Decoded is bool Command)
				{
					if (!PageVariables.TryGetVariable("Contract", out Variable v) ||
						!(v.ValueObject is Contract Contract))
					{
						await Response.SendResponse(new BadRequestException("No smart contract uploaded."));
						return;
					}

					if (Command)
					{
						Contract = await Gateway.ContractsClient.CreateContractAsync(Contract.ForMachines, Contract.ForHumans,
							Contract.Roles, Contract.Parts, Contract.Parameters, Contract.Visibility, Contract.PartsMode, Contract.Duration,
							Contract.ArchiveRequired, Contract.ArchiveOptional, Contract.SignAfter, Contract.SignBefore, Contract.CanActAsTemplate);

						PageVariables["Contract"] = Contract;
					}
					else
						PageVariables.Remove("Contract");
				}
				else
				{
					await Response.SendResponse(new UnsupportedMediaTypeException("Invalid type of posted data."));
					return;
				}
			}
			catch (XmlException ex)
			{
				await Response.SendResponse(XML.AnnotateException(ex, Xml));
			}
			catch (Exception ex)
			{
				await Response.SendResponse(ex);
			}
		}

	}
}
