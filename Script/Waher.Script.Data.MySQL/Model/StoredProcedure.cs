using MySqlConnector;
using System;
using System.Threading;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Data.Model;
using Waher.Script.Model;
using Waher.Script.Operators;

namespace Waher.Script.Data.MySQL.Model
{
	/// <summary>
	/// Represents a stored precedure in a MySQL Database.
	/// </summary>
	public class StoredProcedure : ILambdaExpression, IDisposable
	{
		private readonly SemaphoreSlim synchObj = new SemaphoreSlim(1);
		private readonly MySqlCommand command;
		private readonly int nrParameters;
		private readonly string[] parameterNames;
		private readonly ArgumentType[] parameterTypes;

		internal StoredProcedure(MySqlCommand Command)
		{
			this.command = Command;
			this.nrParameters = this.command.Parameters.Count;
			this.parameterNames = new string[this.nrParameters];
			this.parameterTypes = new ArgumentType[this.nrParameters];

			for (int i = 0; i < this.nrParameters; i++)
			{
				this.parameterNames[i] = this.command.Parameters[i].ParameterName;
				this.parameterTypes[i] = ArgumentType.Normal;
			}
		}

		/// <summary>
		/// Number of arguments.
		/// </summary>
		public int NrArguments => this.nrParameters;

		/// <summary>
		/// Argument Names.
		/// </summary>
		public string[] ArgumentNames => this.parameterNames;

		/// <summary>
		/// Argument types.
		/// </summary>
		public ArgumentType[] ArgumentTypes => this.parameterTypes;

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public bool IsAsynchronous => true;

		/// <summary>
		/// <see cref="IDisposable.Dispose"/>
		/// </summary>
		public void Dispose()
		{
			this.command?.Dispose();
		}

		/// <summary>
		/// Evaluates the lambda expression.
		/// </summary>
		/// <param name="Arguments">Arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			return this.EvaluateAsync(Arguments, Variables).Result;
		}

		/// <summary>
		/// Evaluates the lambda expression.
		/// </summary>
		/// <param name="Arguments">Arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public async Task<IElement> EvaluateAsync(IElement[] Arguments, Variables Variables)
		{
			int i;

			await this.synchObj.WaitAsync();
			try
			{
				for (i = 0; i < this.nrParameters; i++)
				{
					MySqlParameter Parameter = this.command.Parameters[i];
					object Value = Arguments[i].AssociatedObjectValue;

					switch (Parameter.MySqlDbType)
					{
						case MySqlDbType.Binary:
						case MySqlDbType.Bit:
						case MySqlDbType.Blob:
						case MySqlDbType.Bool:
						case MySqlDbType.Date:
						case MySqlDbType.Double:
						case MySqlDbType.DateTime:
						case MySqlDbType.Enum:
						case MySqlDbType.Geometry:
						case MySqlDbType.Guid:
						case MySqlDbType.JSON:
						case MySqlDbType.LongBlob:
						case MySqlDbType.LongText:
						case MySqlDbType.MediumBlob:
						case MySqlDbType.MediumText:
						case MySqlDbType.Newdate:
						case MySqlDbType.NewDecimal:
						case MySqlDbType.Null:
						case MySqlDbType.Set:
						case MySqlDbType.String:
						case MySqlDbType.Text:
						case MySqlDbType.Time:
						case MySqlDbType.Timestamp:
						case MySqlDbType.TinyBlob:
						case MySqlDbType.TinyText:
						case MySqlDbType.VarBinary:
						case MySqlDbType.VarChar:
						case MySqlDbType.VarString:
						case MySqlDbType.Year:
						default:
							Parameter.Value = Value;
							break;

						case MySqlDbType.Byte:
							if (Value is sbyte I8)
								Parameter.Value = I8;
							else if (Value is double d)
								Parameter.Value = (sbyte)d;
							else
								Parameter.Value = Convert.ToSByte(Value);
							break;

						case MySqlDbType.Decimal:
							if (Value is decimal Decimal)
								Parameter.Value = Decimal;
							else if (Value is double d)
								Parameter.Value = (decimal)d;
							else
								Parameter.Value = Convert.ToDecimal(Value);
							break;

						case MySqlDbType.Float:
							if (Value is float Single)
								Parameter.Value = Single;
							else if (Value is double d)
								Parameter.Value = (float)d;
							else
								Parameter.Value = Convert.ToSingle(Value);
							break;

						case MySqlDbType.Int16:
							if (Value is Int16 Int16)
								Parameter.Value = Int16;
							else if (Value is double d)
								Parameter.Value = (Int16)d;
							else
								Parameter.Value = Convert.ToInt16(Value);
							break;

						case MySqlDbType.Int24:
						case MySqlDbType.Int32:
							if (Value is Int32 Int32)
								Parameter.Value = Int32;
							else if (Value is double d)
								Parameter.Value = (Int32)d;
							else
								Parameter.Value = Convert.ToInt32(Value);
							break;

						case MySqlDbType.Int64:
							if (Value is Int64 Int64)
								Parameter.Value = Int64;
							else if (Value is double d)
								Parameter.Value = (Int64)d;
							else
								Parameter.Value = Convert.ToInt64(Value);
							break;

						case MySqlDbType.UInt16:
							if (Value is UInt16 UInt16)
								Parameter.Value = UInt16;
							else if (Value is double d)
								Parameter.Value = (UInt16)d;
							else
								Parameter.Value = Convert.ToUInt16(Value);
							break;

						case MySqlDbType.UInt24:
						case MySqlDbType.UInt32:
							if (Value is UInt32 UInt32)
								Parameter.Value = UInt32;
							else if (Value is double d)
								Parameter.Value = (UInt32)d;
							else
								Parameter.Value = Convert.ToUInt32(Value);
							break;

						case MySqlDbType.UInt64:
							if (Value is UInt64 UInt64)
								Parameter.Value = UInt64;
							else if (Value is double d)
								Parameter.Value = (UInt64)d;
							else
								Parameter.Value = Convert.ToUInt64(Value);
							break;

						case MySqlDbType.UByte:
							if (Value is byte UI8)
								Parameter.Value = UI8;
							else if (Value is double d)
								Parameter.Value = (byte)d;
							else
								Parameter.Value = Convert.ToByte(Value);
							break;
					}
				}

				MySqlDataReader Reader = await this.command.ExecuteReaderAsync();

				return await Reader.ParseAndClose();
			}
			finally
			{
				this.synchObj.Release();
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return LambdaDefinition.ToString(this);
		}
	}
}
