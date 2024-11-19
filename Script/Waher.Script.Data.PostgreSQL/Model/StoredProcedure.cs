using System;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Waher.Runtime.Threading;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Data.Model;
using Waher.Script.Model;
using Waher.Script.Operators;

namespace Waher.Script.Data.PostgreSQL.Model
{
	/// <summary>
	/// Represents a stored precedure in a MySQL Database.
	/// </summary>
	public class StoredProcedure : ILambdaExpression, IDisposable
	{
		private readonly MultiReadSingleWriteObject synchObj;
		private readonly NpgsqlCommand command;
		private readonly int nrParameters;
		private readonly string[] parameterNames;
		private readonly ArgumentType[] parameterTypes;

		internal StoredProcedure(NpgsqlCommand Command)
		{
			this.synchObj = new MultiReadSingleWriteObject(this);
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

			await this.synchObj.BeginWrite();
			try
			{
				for (i = 0; i < this.nrParameters; i++)
				{
					NpgsqlParameter Parameter = this.command.Parameters[i];
					object Value = Arguments[i].AssociatedObjectValue;

					switch (Parameter.NpgsqlDbType)
					{
						case NpgsqlDbType.Array:
						case NpgsqlDbType.Boolean:
						case NpgsqlDbType.Box:
						case NpgsqlDbType.Bytea:
						case NpgsqlDbType.Circle:
						case NpgsqlDbType.Char:
						case NpgsqlDbType.Date:
						case NpgsqlDbType.Double:
						case NpgsqlDbType.Line:
						case NpgsqlDbType.LSeg:
						case NpgsqlDbType.Path:
						case NpgsqlDbType.Point:
						case NpgsqlDbType.Polygon:
						case NpgsqlDbType.Text:
						case NpgsqlDbType.Time:
						case NpgsqlDbType.Timestamp:
						case NpgsqlDbType.Varchar:
						case NpgsqlDbType.Refcursor:
						case NpgsqlDbType.Inet:
						case NpgsqlDbType.Bit:
						case NpgsqlDbType.TimestampTz:
						case NpgsqlDbType.Uuid:
						case NpgsqlDbType.Xml:
						case NpgsqlDbType.Oidvector:
						case NpgsqlDbType.Interval:
						case NpgsqlDbType.TimeTz:
						case NpgsqlDbType.Name:
						case NpgsqlDbType.MacAddr:
						case NpgsqlDbType.Json:
						case NpgsqlDbType.Jsonb:
						case NpgsqlDbType.Hstore:
						case NpgsqlDbType.InternalChar:
						case NpgsqlDbType.Varbit:
						case NpgsqlDbType.Unknown:
						case NpgsqlDbType.Oid:
						case NpgsqlDbType.Xid:
						case NpgsqlDbType.Cid:
						case NpgsqlDbType.Cidr:
						case NpgsqlDbType.TsVector:
						case NpgsqlDbType.TsQuery:
						case NpgsqlDbType.Regtype:
						case NpgsqlDbType.Geometry:
						case NpgsqlDbType.Citext:
						case NpgsqlDbType.Int2Vector:
						case NpgsqlDbType.Tid:
						case NpgsqlDbType.MacAddr8:
						case NpgsqlDbType.Geography:
						case NpgsqlDbType.Regconfig:
						case NpgsqlDbType.JsonPath:
						case NpgsqlDbType.PgLsn:
						case NpgsqlDbType.LTree:
						case NpgsqlDbType.LQuery:
						case NpgsqlDbType.LTxtQuery:
						case NpgsqlDbType.Xid8:
						case NpgsqlDbType.Multirange:
						case NpgsqlDbType.BigIntMultirange:
						case NpgsqlDbType.DateMultirange:
						case NpgsqlDbType.IntegerMultirange:
						case NpgsqlDbType.NumericMultirange:
						case NpgsqlDbType.TimestampMultirange:
						case NpgsqlDbType.TimestampTzMultirange:
						case NpgsqlDbType.Range:
						case NpgsqlDbType.BigIntRange:
						case NpgsqlDbType.DateRange:
						case NpgsqlDbType.IntegerRange:
						case NpgsqlDbType.NumericRange:
						case NpgsqlDbType.TimestampRange:
						case NpgsqlDbType.TimestampTzRange:
						default:
							Parameter.Value = Value;
							break;

						case NpgsqlDbType.Money:
						case NpgsqlDbType.Numeric:
							if (Value is decimal Decimal)
								Parameter.Value = Decimal;
							else if (Value is double d)
								Parameter.Value = (decimal)d;
							else
								Parameter.Value = Convert.ToDecimal(Value);
							break;

						case NpgsqlDbType.Real:
							if (Value is float Single)
								Parameter.Value = Single;
							else if (Value is double d)
								Parameter.Value = (float)d;
							else
								Parameter.Value = Convert.ToSingle(Value);
							break;

						case NpgsqlDbType.Smallint:
							if (Value is Int16 Int16)
								Parameter.Value = Int16;
							else if (Value is double d)
								Parameter.Value = (Int16)d;
							else
								Parameter.Value = Convert.ToInt16(Value);
							break;

						case NpgsqlDbType.Integer:
							if (Value is Int32 Int32)
								Parameter.Value = Int32;
							else if (Value is double d)
								Parameter.Value = (Int32)d;
							else
								Parameter.Value = Convert.ToInt32(Value);
							break;

						case NpgsqlDbType.Bigint:
							if (Value is Int64 Int64)
								Parameter.Value = Int64;
							else if (Value is double d)
								Parameter.Value = (Int64)d;
							else
								Parameter.Value = Convert.ToInt64(Value);
							break;
					}
				}

				NpgsqlDataReader Reader = await this.command.ExecuteReaderAsync();

				return await Reader.ParseAndClose();
			}
			finally
			{
				await this.synchObj.EndWrite();
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return LambdaDefinition.ToString(this);
		}
	}
}
