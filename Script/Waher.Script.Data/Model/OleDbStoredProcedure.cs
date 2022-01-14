using System;
using System.Data.Common;
using System.Data.OleDb;
using System.Threading;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Operators;

namespace Waher.Script.Data.Model
{
	/// <summary>
	/// Represents a stored precedure in an OLE DB Database.
	/// </summary>
	public class OleDbStoredProcedure : ILambdaExpression, IDisposable
	{
		private readonly SemaphoreSlim synchObj = new SemaphoreSlim(1);
		private readonly OleDbCommand command;
		private readonly int nrParameters;
		private readonly string[] parameterNames;
		private readonly ArgumentType[] parameterTypes;

		internal OleDbStoredProcedure(OleDbCommand Command)
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
					OleDbParameter Parameter = this.command.Parameters[i];
					object Value = Arguments[i].AssociatedObjectValue;

					switch (Parameter.OleDbType)
					{
						case OleDbType.Empty:
						case OleDbType.Double:
						case OleDbType.Date:
						case OleDbType.BSTR:
						case OleDbType.IDispatch:
						case OleDbType.Error:
						case OleDbType.Boolean:
						case OleDbType.Variant:
						case OleDbType.IUnknown:
						case OleDbType.Filetime:
						case OleDbType.Guid:
						case OleDbType.Binary:
						case OleDbType.Char:
						case OleDbType.WChar:
						case OleDbType.DBDate:
						case OleDbType.DBTime:
						case OleDbType.DBTimeStamp:
						case OleDbType.PropVariant:
						case OleDbType.VarChar:
						case OleDbType.LongVarChar:
						case OleDbType.VarWChar:
						case OleDbType.LongVarWChar:
						case OleDbType.VarBinary:
						case OleDbType.LongVarBinary:
						default:
							Parameter.Value = Value;
							break;

						case OleDbType.Decimal:
						case OleDbType.Currency:
						case OleDbType.Numeric:
						case OleDbType.VarNumeric:
							if (Value is decimal Decimal)
								Parameter.Value = Decimal;
							else if (Value is double d)
								Parameter.Value = (decimal)d;
							else
								Parameter.Value = Convert.ToDecimal(Value);
							break;

						case OleDbType.Single:
							if (Value is float Single)
								Parameter.Value = Single;
							else if (Value is double d)
								Parameter.Value = (float)d;
							else
								Parameter.Value = Convert.ToSingle(Value);
							break;

						case OleDbType.SmallInt:
							if (Value is Int16 Int16)
								Parameter.Value = Int16;
							else if (Value is double d)
								Parameter.Value = (Int16)d;
							else
								Parameter.Value = Convert.ToInt16(Value);
							break;

						case OleDbType.Integer:
							if (Value is Int32 Int32)
								Parameter.Value = Int32;
							else if (Value is double d)
								Parameter.Value = (Int32)d;
							else
								Parameter.Value = Convert.ToInt32(Value);
							break;

						case OleDbType.BigInt:
							if (Value is Int64 Int64)
								Parameter.Value = Int64;
							else if (Value is double d)
								Parameter.Value = (Int64)d;
							else
								Parameter.Value = Convert.ToInt64(Value);
							break;

						case OleDbType.TinyInt:
							if (Value is sbyte I8)
								Parameter.Value = I8;
							else if (Value is double d)
								Parameter.Value = (sbyte)d;
							else
								Parameter.Value = Convert.ToSByte(Value);
							break;

						case OleDbType.UnsignedTinyInt:
							if (Value is byte UI8)
								Parameter.Value = UI8;
							else if (Value is double d)
								Parameter.Value = (byte)d;
							else
								Parameter.Value = Convert.ToByte(Value);
							break;

						case OleDbType.UnsignedSmallInt:
							if (Value is UInt16 UI16)
								Parameter.Value = UI16;
							else if (Value is double d)
								Parameter.Value = (UInt16)d;
							else
								Parameter.Value = Convert.ToUInt16(Value);
							break;

						case OleDbType.UnsignedInt:
							if (Value is UInt32 UI32)
								Parameter.Value = UI32;
							else if (Value is double d)
								Parameter.Value = (UInt32)d;
							else
								Parameter.Value = Convert.ToUInt32(Value);
							break;

						case OleDbType.UnsignedBigInt:
							if (Value is UInt64 UI64)
								Parameter.Value = UI64;
							else if (Value is double d)
								Parameter.Value = (UInt64)d;
							else
								Parameter.Value = Convert.ToUInt64(Value);
							break;
					}
				}

				DbDataReader Reader = await this.command.ExecuteReaderAsync();

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
