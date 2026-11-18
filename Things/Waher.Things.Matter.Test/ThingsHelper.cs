using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Waher.Networking.XMPP.Sensor;
using Waher.Runtime.Language;
using Waher.Things.Metering;
using Waher.Things.Queries;
using Waher.Things.SensorData;

namespace Waher.Things.Matter.Test
{
	/// <summary>
	/// Result of a readout.
	/// </summary>
	public class ReadoutResult
	{
		/// <summary>
		/// Fields reported.
		/// </summary>
		public List<Field> Fields { get; } = new List<Field>();

		/// <summary>
		/// Errors reported.
		/// </summary>
		public List<ThingError> Errors { get; } = new List<ThingError>();

		/// <summary>
		/// Finds a field.
		/// </summary>
		/// <param name="NodeId">Node ID of node reporting the field.</param>
		/// <param name="Name">Field name.</param>
		/// <returns>Field, or null if not found.</returns>
		public Field Find(string NodeId, string Name)
		{
			foreach (Field Field in this.Fields)
			{
				if (Field.Name == Name && Field.Thing?.NodeId == NodeId)
					return Field;
			}

			return null;
		}

		/// <summary>
		/// Gets a field, asserting it exists.
		/// </summary>
		/// <param name="Node">Node reporting the field.</param>
		/// <param name="Name">Field name.</param>
		/// <returns>Field.</returns>
		public Field Get(MeteringNode Node, string Name)
		{
			Field Result = this.Find(Node.NodeId, Name);
			Assert.IsNotNull(Result, "Field " + Name + " not reported by " + Node.NodeId + ".");
			return Result;
		}

		/// <summary>
		/// Gets a quantity field value.
		/// </summary>
		/// <param name="Node">Node reporting the field.</param>
		/// <param name="Name">Field name.</param>
		/// <returns>Value.</returns>
		public double Quantity(MeteringNode Node, string Name)
		{
			Field F = this.Get(Node, Name);
			Assert.IsInstanceOfType<QuantityField>(F, "Field " + Name + " is not a quantity field.");
			return ((QuantityField)F).Value;
		}

		/// <summary>
		/// Gets a boolean field value.
		/// </summary>
		/// <param name="Node">Node reporting the field.</param>
		/// <param name="Name">Field name.</param>
		/// <returns>Value.</returns>
		public bool Boolean(MeteringNode Node, string Name)
		{
			Field F = this.Get(Node, Name);
			Assert.IsInstanceOfType<BooleanField>(F, "Field " + Name + " is not a boolean field.");
			return ((BooleanField)F).Value;
		}

		/// <summary>
		/// Gets a string representation of a field value.
		/// </summary>
		/// <param name="Node">Node reporting the field.</param>
		/// <param name="Name">Field name.</param>
		/// <returns>Value.</returns>
		public string String(MeteringNode Node, string Name)
		{
			return this.Get(Node, Name).ValueString;
		}

		/// <summary>
		/// Prints the fields and errors to the console.
		/// </summary>
		public void Print()
		{
			foreach (Field Field in this.Fields)
				Console.Out.WriteLine(Field.Thing?.NodeId + ": " + Field.Name + " = " + Field.ValueString + " (" + Field.Type.ToString() + ")");

			foreach (ThingError Error in this.Errors)
				Console.Out.WriteLine("Error: " + Error.NodeId + ": " + Error.ErrorMessage);
		}
	}

	/// <summary>
	/// Result of a query.
	/// </summary>
	public class QueryResult
	{
		/// <summary>
		/// Records reported, per table.
		/// </summary>
		public Dictionary<string, List<Record>> Records { get; } = new Dictionary<string, List<Record>>();

		/// <summary>
		/// Messages reported.
		/// </summary>
		public List<string> Messages { get; } = new List<string>();

		/// <summary>
		/// Gets records of a table.
		/// </summary>
		/// <param name="TableId">Table ID.</param>
		/// <returns>Records.</returns>
		public List<Record> Get(string TableId)
		{
			return this.Records.TryGetValue(TableId, out List<Record> Result) ? Result : new List<Record>();
		}
	}

	/// <summary>
	/// Helper methods for testing the Things layer.
	/// </summary>
	public static class ThingsHelper
	{
		/// <summary>
		/// Performs a readout of a sensor node.
		/// </summary>
		/// <param name="Sensor">Sensor node.</param>
		/// <param name="Types">Field types to read.</param>
		/// <param name="FieldNames">Optional field names to read.</param>
		/// <returns>Readout result.</returns>
		public static async Task<ReadoutResult> ReadoutAsync(ISensor Sensor, FieldType Types = FieldType.All, string[] FieldNames = null)
		{
			ReadoutResult Result = new ReadoutResult();
			TaskCompletionSource<bool> Done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			InternalReadoutRequest Request = new InternalReadoutRequest(string.Empty, new IThingReference[] { Sensor }, Types, FieldNames,
				DateTime.MinValue, DateTime.MaxValue,
				(Sender, e) =>
				{
					lock (Result)
					{
						Result.Fields.AddRange(e.Fields);
					}

					if (e.Done)
						Done.TrySetResult(true);

					return Task.CompletedTask;
				},
				(Sender, e) =>
				{
					lock (Result)
					{
						Result.Errors.AddRange(e.Errors);
					}

					if (e.Done)
						Done.TrySetResult(true);

					return Task.CompletedTask;
				}, null);

			await Sensor.StartReadout(Request);

			Assert.AreEqual(Done.Task, await Task.WhenAny(Done.Task, Task.Delay(30000)), "Readout not completed.");

			Result.Print();

			return Result;
		}

		/// <summary>
		/// Executes a query command.
		/// </summary>
		/// <param name="Command">Command.</param>
		/// <param name="Node">Node on which the command is executed.</param>
		/// <returns>Query result.</returns>
		public static async Task<QueryResult> ExecuteQueryAsync(ICommand Command, INode Node)
		{
			Language Language = await Translator.GetDefaultLanguageAsync();
			Query Query = new Query(Command.CommandID, Guid.NewGuid().ToString(), null, Language, Node, RequestOrigin.Empty);
			QueryResult Result = new QueryResult();
			TaskCompletionSource<bool> Done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

			Query.OnNewRecords += (Sender, e) =>
			{
				lock (Result)
				{
					if (!Result.Records.TryGetValue(e.TableId, out List<Record> List))
					{
						List = new List<Record>();
						Result.Records[e.TableId] = List;
					}

					List.AddRange(e.Records);
				}

				return Task.CompletedTask;
			};

			Query.OnMessage += (Sender, e) =>
			{
				lock (Result)
				{
					Result.Messages.Add(e.Type.ToString() + ": " + e.Body);
				}

				return Task.CompletedTask;
			};

			Query.OnDone += (Sender, e) =>
			{
				Done.TrySetResult(true);
				return Task.CompletedTask;
			};

			Query.OnAborted += (Sender, e) =>
			{
				Done.TrySetResult(false);
				return Task.CompletedTask;
			};

			await Command.StartQueryExecutionAsync(Query, Language);

			Assert.AreEqual(Done.Task, await Task.WhenAny(Done.Task, Task.Delay(60000)), "Query not completed.");

			foreach (string Message in Result.Messages)
				Console.Out.WriteLine(Message);

			foreach (KeyValuePair<string, List<Record>> P in Result.Records)
			{
				Console.Out.WriteLine("Table " + P.Key + ": " + P.Value.Count.ToString() + " records.");

				int i = 0;
				foreach (Record Record in P.Value)
				{
					if (++i > 25)
						break;

					Console.Out.WriteLine(string.Join(" | ", Record.Elements));
				}
			}

			return Result;
		}

		/// <summary>
		/// Finds a command of a given type, on a node.
		/// </summary>
		/// <typeparam name="T">Command type.</typeparam>
		/// <param name="Node">Node.</param>
		/// <returns>Command.</returns>
		public static async Task<T> GetCommandAsync<T>(INode Node)
			where T : ICommand
		{
			foreach (ICommand Command in await Node.Commands)
			{
				if (Command is T Result)
					return Result;
			}

			Assert.Fail("Command " + typeof(T).Name + " not found on node " + Node.NodeId);
			return default;
		}

		/// <summary>
		/// Finds a command, given its name.
		/// </summary>
		/// <param name="Node">Node.</param>
		/// <param name="Name">Name of command.</param>
		/// <returns>Command.</returns>
		public static async Task<ICommand> GetCommandAsync(INode Node, string Name)
		{
			Language Language = await Translator.GetDefaultLanguageAsync();

			foreach (ICommand Command in await Node.Commands)
			{
				if (await Command.GetNameAsync(Language) == Name)
					return Command;
			}

			Assert.Fail("Command " + Name + " not found on node " + Node.NodeId);
			return null;
		}
	}
}
