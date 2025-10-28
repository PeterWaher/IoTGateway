using System;
using System.Threading.Tasks;
using Waher.Content.Semantic;
using Waher.Content.Semantic.Model;
using Waher.Content.Semantic.Model.Literals;
using Waher.Content.Semantic.Ontologies;
using Waher.IoTGateway;
using Waher.IoTGateway.Setup;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Things.Semantic.Ontologies;

namespace Waher.Things.Semantic.Sources.DynamicGraphs
{
	/// <summary>
	/// Dynamic graph of the Neuron.
	/// </summary>
	public class MachineGraph : IDynamicGraph
	{
		/// <summary>
		/// Dynamic graph of the Neuron.
		/// </summary>
		public MachineGraph()
		{
		}

		/// <summary>
		/// Generates the semantic graph.
		/// </summary>
		/// <param name="Result">Result set will be output here.</param>
		/// <param name="Language">Preferred language.</param>
		/// <param name="Caller">Origin of request.</param>
		public Task GenerateGraph(InMemorySemanticCube Result, Language Language, RequestOrigin Caller)
		{
			string BrokerPath = "/";
			UriNode BrokerGraphUriNode = new UriNode(new Uri(Gateway.GetUrl(BrokerPath)));
			int i, c;

			Result.Add(BrokerGraphUriNode, Rdf.type, IoTConcentrator.Broker);

			ChunkedList<ISemanticElement> Names = new ChunkedList<ISemanticElement>();

			if (!string.IsNullOrEmpty(DomainConfiguration.Instance.HumanReadableName))
			{
				Names.Add(new StringLiteral(DomainConfiguration.Instance.HumanReadableName,
					DomainConfiguration.Instance.HumanReadableNameLanguage));
			}

			if ((c = (DomainConfiguration.Instance.LocalizedNames?.Length) ?? 0) > 0)
			{
				for (i = 0; i < c; i++)
				{
					Names.Add(new StringLiteral(
						DomainConfiguration.Instance.LocalizedNames[i].Value,
						DomainConfiguration.Instance.LocalizedNames[i].Key));
				}
			}

			Result.AddContainer(BrokerGraphUriNode, Rdf.Alt, RdfSchema.label, Names);
			Names.Clear();

			if (!string.IsNullOrEmpty(DomainConfiguration.Instance.HumanReadableDescription))
			{
				Names.Add(new StringLiteral(
					DomainConfiguration.Instance.HumanReadableDescription,
					DomainConfiguration.Instance.HumanReadableDescriptionLanguage));
			}

			if ((c = (DomainConfiguration.Instance.LocalizedDescriptions?.Length) ?? 0) > 0)
			{
				for (i = 0; i < c; i++)
				{
					Names.Add(new StringLiteral(
						DomainConfiguration.Instance.LocalizedDescriptions[i].Value,
						DomainConfiguration.Instance.LocalizedDescriptions[i].Key));
				}
			}

			Result.AddContainer(BrokerGraphUriNode, Rdf.Alt, RdfSchema.comment, Names);
			Names.Clear();

			if (DomainConfiguration.Instance.UseDomainName)
			{
				ChunkedList<string> Domains = new ChunkedList<string>()
				{
					DomainConfiguration.Instance.Domain
				};

				if ((DomainConfiguration.Instance.AlternativeDomains?.Length ?? 0) > 0)
					Domains.AddRange(DomainConfiguration.Instance.AlternativeDomains);

				Result.AddContainer(BrokerGraphUriNode, Rdf.Alt, IoTConcentrator.domain,
					Domains.ToArray());
			}

			ChunkedList<ISemanticElement> Items = new ChunkedList<ISemanticElement>();

			foreach (IDataSource RootSource in Gateway.ConcentratorServer.RootDataSources)
				Items.Add(new UriNode(DataSourceGraph.GetSourceUri(RootSource.SourceID)));

			Result.AddContainer(BrokerGraphUriNode, Rdf.Bag, IoTConcentrator.rootSources, Items);

			return Task.CompletedTask;
		}
	}
}
