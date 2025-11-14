using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things;
using Waher.Things.Attributes;

namespace Waher.Processors.Metering.NodeTypes.Errors.Actions
{
	/// <summary>
	/// Processes a field using script.
	/// </summary>
	public class ScriptProcessing : DecisionTreeLeafStatement
	{
		private string[] script;
		private Expression parsedScript;

		/// <summary>
		/// Processes a field using script.
		/// </summary>
		public ScriptProcessing()
			: base()
		{
		}

		/// <summary>
		/// Script to execute.
		/// </summary>
		[Header(2, "Script:", 0)]
		[Page(1, "Script", 10)]
		[ToolTip(3, "Script used to process fields.")]
		[Text(TextPosition.AfterField, 5, "When executing the script, the variables Device and Error will contain references to the corresponding device and error objects being processed. Remember to return the error or array of errors you want to continue to process. Returning null or something that is not an error mutes the error from future processing.")]
		[ContentType("application/x-webscript")]
		public string[] Script
		{
			get => this.script;
			set
			{
				this.script = value;
				this.parsedScript = null;
			}
		}

		/// <summary>
		/// Gets the type name of the node.
		/// </summary>
		/// <param name="Language">Language to use.</param>
		/// <returns>Localized type node.</returns>
		public override Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(ScriptProcessing), 4, "Process error using script");
		}

		/// <summary>
		/// Processes a single thing error.
		/// </summary>
		/// <param name="Device">Thing reporting the errors.</param>
		/// <param name="Error">Error to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		public override async Task<ThingError[]> ProcessError(INode Device, ThingError Error)
		{
			if (this.parsedScript is null)
			{
				StringBuilder sb = new StringBuilder();

				foreach (string s in this.Script)
					sb.AppendLine(s);

				this.parsedScript = new Expression(sb.ToString());
			}

			Variables Variables = new Variables()
			{
				{ "Device", Device },
				{ "Error", Error }
			};

			object Result = await this.parsedScript.EvaluateAsync(Variables);

			if (Result is ThingError Error2)
				return new ThingError[] { Error2 };
			else if (Result is ThingError[] Errors)
				return Errors;
			else if (Result is Array A)
			{
				ChunkedList<ThingError> Errors2 = null;

				foreach (object Item in A)
				{
					if (Item is ThingError Error3)
					{
						Errors2 ??= new ChunkedList<ThingError>();
						Errors2.Add(Error3);
					}
				}

				return Errors2?.ToArray();
			}
			else
				return null;
		}
	}
}
