using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Runtime.Collections;
using Waher.Runtime.Language;
using Waher.Script;
using Waher.Things;
using Waher.Things.Attributes;
using Waher.Things.SensorData;

namespace Waher.Processors.Metering.NodeTypes.Fields.Actions
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
		[Text(TextPosition.AfterField, 5, "When executing the script, the variables Sensor and Field will contain references to the corresponding sensor and field objects being processed. Remember to return the field or array of fields you want to continue to process. Returning null or something that is not a field rejects the field from future processing.")]
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
			return Language.GetStringAsync(typeof(ScriptProcessing), 4, "Process field using script");
		}

		/// <summary>
		/// Processes a single sensor data field.
		/// </summary>
		/// <param name="Sensor">Sensor reporting the field.</param>
		/// <param name="Field">Field to process.</param>
		/// <returns>Processed set of fields. Can be null if field does not pass processing.</returns>
		public override async Task<Field[]> ProcessField(ISensor Sensor, Field Field)
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
				{ "Sensor", Sensor },
				{ "Field", Field }
			};

			object Result = await this.parsedScript.EvaluateAsync(Variables);

			if (Result is Field Field2)
				return new Field[] { Field2 };
			else if (Result is Field[] Fields)
				return Fields;
			else if (Result is Array A)
			{
				ChunkedList<Field> Fields2 = null;

				foreach (object Item in A)
				{
					if (Item is Field Field3)
					{
						Fields2 ??= new ChunkedList<Field>();
						Fields2.Add(Field3);
					}
				}

				return Fields2?.ToArray();
			}
			else
				return null;
		}
	}
}
