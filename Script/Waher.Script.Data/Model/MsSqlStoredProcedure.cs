using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Operators;

namespace Waher.Script.Data.Model
{
	/// <summary>
	/// Represents a stored precedure in a MS SQL Database.
	/// </summary>
	public class MsSqlStoredProcedure : ILambdaExpression, IDisposable
	{
		private readonly SemaphoreSlim synchObj = new SemaphoreSlim(1);
		private readonly SqlCommand command;
		private readonly int nrParameters;
		private readonly string[] parameterNames;
		private readonly ArgumentType[] parameterTypes;

		internal MsSqlStoredProcedure(SqlCommand Command)
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
					SqlParameter Parameter = this.command.Parameters[i];
					object Value = Arguments[i].AssociatedObjectValue;

					switch (Parameter.SqlDbType)
					{
						case SqlDbType.Binary:
						case SqlDbType.Bit:
						case SqlDbType.Char:
						case SqlDbType.DateTime:
						case SqlDbType.Float:
						case SqlDbType.Image:
						case SqlDbType.NChar:
						case SqlDbType.NText:
						case SqlDbType.NVarChar:
						case SqlDbType.UniqueIdentifier:
						case SqlDbType.SmallDateTime:
						case SqlDbType.Text:
						case SqlDbType.Timestamp:
						case SqlDbType.VarBinary:
						case SqlDbType.VarChar:
						case SqlDbType.Variant:
						case SqlDbType.Xml:
						case SqlDbType.Udt:
						case SqlDbType.Structured:
						case SqlDbType.Date:
						case SqlDbType.Time:
						case SqlDbType.DateTime2:
						case SqlDbType.DateTimeOffset:
						default:
							Parameter.Value = Value;
							break;

						case SqlDbType.Decimal:
						case SqlDbType.SmallMoney:
						case SqlDbType.Money:
							if (Value is decimal Decimal)
								Parameter.Value = Decimal;
							else if (Value is double d)
								Parameter.Value = (decimal)d;
							else
								Parameter.Value = Convert.ToDecimal(Value);
							break;

						case SqlDbType.Real:
							if (Value is float Single)
								Parameter.Value = Single;
							else if (Value is double d)
								Parameter.Value = (float)d;
							else
								Parameter.Value = Convert.ToSingle(Value);
							break;

						case SqlDbType.SmallInt:
							if (Value is Int16 Int16)
								Parameter.Value = Int16;
							else if (Value is double d)
								Parameter.Value = (Int16)d;
							else
								Parameter.Value = Convert.ToInt16(Value);
							break;

						case SqlDbType.Int:
							if (Value is Int32 Int32)
								Parameter.Value = Int32;
							else if (Value is double d)
								Parameter.Value = (Int32)d;
							else
								Parameter.Value = Convert.ToInt32(Value);
							break;

						case SqlDbType.BigInt:
							if (Value is Int64 Int64)
								Parameter.Value = Int64;
							else if (Value is double d)
								Parameter.Value = (Int64)d;
							else
								Parameter.Value = Convert.ToInt64(Value);
							break;

						case SqlDbType.TinyInt:
							if (Value is byte UI8)
								Parameter.Value = UI8;
							else if (Value is double d)
								Parameter.Value = (byte)d;
							else
								Parameter.Value = Convert.ToByte(Value);
							break;
					}
				}

				SqlDataReader Reader = await this.command.ExecuteReaderAsync();

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
