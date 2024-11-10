using System;
using System.Text;
using System.Threading.Tasks;
using Waher.Events;
using Waher.Networking.Sniffers;

namespace Waher.Networking
{
	#region Delegates

	/// <summary>
	/// Asynchronous version of <see cref="EventArgs"/>.
	/// </summary>
	public delegate Task EventHandlerAsync(object Sender, EventArgs e);

	/// <summary>
	/// Asynchronous version of <see cref="EventArgs"/> with a typed event arguments.
	/// </summary>
	public delegate Task EventHandlerAsync<T>(object Sender, T e);

	/// <summary>
	/// Callback function with one argument.
	/// </summary>
	/// <typeparam name="ArgT">Argument type.</typeparam>
	/// <param name="Arg">Argument</param>
	public delegate void Callback<ArgT>(ArgT Arg);

	/// <summary>
	/// Asynchronous callback function with one argument.
	/// </summary>
	/// <typeparam name="ArgT">Argument type.</typeparam>
	/// <param name="Arg">Argument</param>
	public delegate Task CallbackAsync<ArgT>(ArgT Arg);

	#endregion

	/// <summary>
	/// Static class with method extensions simplifying raising events.
	/// </summary>
	public static class EventHandlerExtensions
	{
		#region Synchronous event handlers, non-sniffable

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static bool Raise(this EventHandler EventHandler, object Sender)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static bool Raise(this EventHandler EventHandler, object Sender, EventArgs e)
		{
			if (!(EventHandler is null))
			{
				try
				{
					EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static bool Raise<T>(this EventHandler<T> EventHandler, object Sender, T e)
		{
			if (!(EventHandler is null))
			{
				try
				{
					EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Synchronous event handlers, sniffable

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandler EventHandler, ISniffable Sender)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise(this EventHandler EventHandler, ISniffable Sender, EventArgs e)
		{
			if (EventHandler is null)
				await Sender.NoEventHandlerWarning(Sender, e);
			else
			{
				try
				{
					EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					await Sender.Exception(ex);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Shows a warning in sniffers that an event handler is not registered.
		/// </summary>
		/// <param name="Sniffable">Sniffable object.</param>
		/// <param name="Sender">Sender of event.</param>
		/// <param name="e">Event arguments.</param>
		private static Task NoEventHandlerWarning(this ISniffable Sniffable, object Sender, object e)
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("No event handler registered (");

			if (Sender is null)
				sb.Append("null sender");
			else
				sb.Append(Sender.GetType().FullName);

			sb.Append(", ");
			sb.Append(e.GetType().FullName);
			sb.Append(")");

			return Sniffable.Warning(sb.ToString());
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise<T>(this EventHandler<T> EventHandler, ISniffable Sender, T e)
		{
			if (EventHandler is null)
				await Sender.NoEventHandlerWarning(Sender, e);
			else
			{
				try
				{
					EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					await Sender.Exception(ex);
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Asynchronous event handlers, non-sniffable

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandlerAsync EventHandler, object Sender)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise(this EventHandlerAsync EventHandler, object Sender, EventArgs e)
		{
			if (!(EventHandler is null))
			{
				try
				{
					await EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise<T>(this EventHandlerAsync<T> EventHandler, object Sender, T e)
		{
			if (!(EventHandler is null))
			{
				try
				{
					await EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Asynchronous event handlers, sniffable

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static Task<bool> Raise(this EventHandlerAsync EventHandler, ISniffable Sender)
		{
			return Raise(EventHandler, Sender, EventArgs.Empty);
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise(this EventHandlerAsync EventHandler, ISniffable Sender, EventArgs e)
		{
			if (EventHandler is null)
				await Sender.NoEventHandlerWarning(Sender, e);
			else
			{
				try
				{
					await EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					await Sender.Exception(ex);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Raises an event, if handler is defined. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="EventHandler">Event handler, or null if not defined.</param>
		/// <param name="Sender">Sender of events.</param>
		/// <param name="e">Event arguments.</param>
		/// <returns>If event handler was processed or null (true), or if an exception was thrown and logged (false).</returns>
		public static async Task<bool> Raise<T>(this EventHandlerAsync<T> EventHandler, ISniffable Sender, T e)
		{
			if (EventHandler is null)
				await Sender.NoEventHandlerWarning(Sender, e);
			else
			{
				try
				{
					await EventHandler(Sender, e);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
					await Sender.Exception(ex);
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Synchronous callback functions

		/// <summary>
		/// Calls a callback function. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="Callback">Callback function, or null if not defined.</param>
		/// <param name="Argument">Argument</param>
		public static void Call<ArgT>(this Callback<ArgT> Callback, ArgT Argument)
		{
			if (!(Callback is null))
			{
				try
				{
					Callback(Argument);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		#endregion

		#region Asynchronous callback functions

		/// <summary>
		/// Calls a callback function. Any exceptions are trapped and logged.
		/// </summary>
		/// <param name="Callback">Callback function, or null if not defined.</param>
		/// <param name="Argument">Argument</param>
		public static async Task Call<ArgT>(this CallbackAsync<ArgT> Callback, ArgT Argument)
		{
			if (!(Callback is null))
			{
				try
				{
					await Callback(Argument);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
			}
		}

		#endregion

	}
}
