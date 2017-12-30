using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Waher.Runtime.Language;
using Waher.Things.DisplayableParameters;

namespace Waher.Things.Gpio
{
	/// <summary>
	/// Node representing a GPIO conroller.
	/// </summary>
	public class Controller : INode
	{
		private GpioController controller;
		private string id;

		/// <summary>
		/// Node representing a GPIO conroller.
		/// </summary>
		///	<param name="Id">Node ID.</param>
		/// <param name="Controller">GPIO Controller</param>
		public Controller(string Id, GpioController Controller)
		{
			this.id = Id;
			this.controller = Controller;
		}

		public string LocalId => this.id;
		public string LogId => this.id;
		public string NodeId => this.id;
		public string SourceId => GpioSource.ID;
		public string Partition => "Controller";
		public bool HasChildren => true;
		public bool ChildrenOrdered => false;
		public bool IsReadable => false;
		public bool IsControllable => false;
		public bool HasCommands => true;
		public IThingReference Parent => null;
		public DateTime LastChanged => DateTime.MinValue;
		public NodeState State => NodeState.None;

		public Task<bool> AcceptsChildAsync(INode Child)
		{
			return Task.FromResult<bool>(false);
		}

		public Task<bool> AcceptsParentAsync(INode Parent)
		{
			return Task.FromResult<bool>(false);
		}

		public Task AddAsync(INode Child)
		{
			throw new NotSupportedException();
		}

		public Task<bool> CanAddAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(false);
		}

		public Task<bool> CanDestroyAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(false);
		}

		public Task<bool> CanEditAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(false);
		}

		public Task<bool> CanViewAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(true);
		}

		public Task DestroyAsync()
		{
			throw new NotSupportedException();
		}

		public async Task<IEnumerable<Parameter>> GetDisplayableParametersAsync(Language Language, RequestOrigin Caller)
		{
			return new Parameter[]
			{
				new Int32Parameter("NrPins", await Language.GetStringAsync(typeof(GpioSource), 3, "#Pins"), this.controller.PinCount)
			};
		}

		public Task<IEnumerable<Message>> GetMessagesAsync(RequestOrigin Caller)
		{
			return Task.FromResult<IEnumerable<Message>>(null);
		}

		public Task<string> GetTypeNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(GpioSource), 2, "Controller");
		}

		public Task<bool> MoveDownAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(true);
		}

		public Task<bool> MoveUpAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(true);
		}

		public Task<bool> RemoveAsync(INode Child)
		{
			return Task.FromResult<bool>(false);
		}

		public Task<IEnumerable<INode>> ChildNodes => throw new NotImplementedException();
		public Task<IEnumerable<ICommand>> Commands => throw new NotImplementedException();
	}
}
