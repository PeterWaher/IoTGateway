using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Waher.Persistence;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Script.Persistence.SQL.Processors
{
	/// <summary>
	/// Processor that reorders a sequence of items.
	/// </summary>
	public class CustomOrderProcessor : IProcessor<object>
	{
		private readonly IProcessor<object> processor;
		private readonly bool isAsynchronous;
		private readonly Dictionary<Type, ObjectProperties> propertiesX = new Dictionary<Type, ObjectProperties>();
		private readonly Dictionary<Type, ObjectProperties> propertiesY = new Dictionary<Type, ObjectProperties>();
		private readonly KeyValuePair<ScriptNode, bool>[] order;
		private readonly List<object> items = new List<object>();
		private readonly Variables variables;

		/// <summary>
		/// Processor that reorders a sequence of items.
		/// </summary>
		/// <param name="Processor">Inner processor.</param>
		/// <param name="Variables">Current set of variables.</param>
		/// <param name="Order">Custom order.</param>
		public CustomOrderProcessor(IProcessor<object> Processor,
			Variables Variables, KeyValuePair<ScriptNode, bool>[] Order)
		{
			this.processor = Processor;
			this.order = Order;
			this.variables = Variables;
			this.isAsynchronous = Processor.IsAsynchronous;

			foreach (KeyValuePair<ScriptNode, bool> P in Order)
			{
				if (P.Key.IsAsynchronous)
				{
					this.isAsynchronous = true;
					break;
				}
			}
		}

		/// <summary>
		/// If the processor operates asynchronously.
		/// </summary>
		public bool IsAsynchronous => this.isAsynchronous;

		/// <summary>
		/// Processes an object synchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public bool Process(object Object)
		{
			this.items.Add(Object);
			return true;
		}

		/// <summary>
		/// Processes an object asynchronously.
		/// </summary>
		/// <param name="Object">Object to process.</param>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public Task<bool> ProcessAsync(object Object)
		{
			this.items.Add(Object);
			return Task.FromResult(true);
		}

		/// <summary>
		/// Called at the end of processing, to allow for flushing of buffers, etc.
		/// </summary>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public bool Flush()
		{
			this.Sort();

			foreach (object Item in this.items)
			{
				if (!this.processor.Process(Item))
					return false;
			}

			return this.processor.Flush();
		}

		private void Sort()
		{
			this.items.Sort((x, y) =>
			{
				if (x is null)
				{
					if (y is null)
						return 0;
					else
						return -1;
				}
				else if (y is null)
					return 1;

				Type Tx = x.GetType();
				Type Ty = y.GetType();

				if (this.propertiesX.TryGetValue(Tx, out ObjectProperties Vx))
					Vx.Object = x;
				else
				{
					Vx = new ObjectProperties(x, this.variables);
					this.propertiesX[Tx] = Vx;
				}

				if (this.propertiesY.TryGetValue(Ty, out ObjectProperties Vy))
					Vy.Object = y;
				else
				{
					Vy = new ObjectProperties(y, this.variables);
					this.propertiesY[Ty] = Vy;
				}

				int i, j, c = this.order.Length;
				IElement Ex, Ey;
				ScriptNode Node;

				for (i = 0; i < c; i++)
				{
					Node = this.order[i].Key;
					Ex = Node.Evaluate(Vx);     // TODO: Async
					Ey = Node.Evaluate(Vy);     // TODO: Async

					if (!(Ex.AssociatedSet is IOrderedSet S))
						throw new ScriptRuntimeException("Result not member of an ordered set.", Node);

					j = S.Compare(Ex, Ey);
					if (j != 0)
					{
						if (this.order[i].Value)
							return j;
						else
							return -j;
					}
				}

				return 0;
			});
		}

		/// <summary>
		/// Called at the end of processing, to allow for flushing of buffers, etc.
		/// </summary>
		/// <returns>If processing should continue (true), or be cancelled (false).</returns>
		public async Task<bool> FlushAsync()
		{
			this.Sort();

			foreach (object Item in this.items)
			{
				if (!await this.processor.ProcessAsync(Item))
					return false;
			}

			return await this.processor.FlushAsync();
		}
	}
}
