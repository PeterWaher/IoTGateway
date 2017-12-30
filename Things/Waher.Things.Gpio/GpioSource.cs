using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Waher.Runtime.Language;

namespace Waher.Things.Gpio
{
    public class GpioSource : IDataSource
    {
		public const string ID = "GPIO";

		private Controller[] controllers;

		public GpioSource()
		{
			this.Init().Wait();
		}

		private async Task Init()
		{
			Language Language = await Translator.GetDefaultLanguageAsync();
			string Id = await Language.GetStringAsync(typeof(GpioSource), 4, "Default Controller");

			this.controllers = new Controller[] 
			{
				new Controller(Id, await GpioController.GetDefaultAsync())
			};
		}

		public static bool SupportsGpio
		{
			get
			{
				return GpioController.GetDefault() != null;
			}
		}

		public string SourceID => ID;
		public bool HasChildren => false;
		public DateTime LastChanged => DateTime.MinValue;
		public IEnumerable<IDataSource> ChildSources => null;

		public Task<bool> CanViewAsync(RequestOrigin Caller)
		{
			return Task.FromResult<bool>(true);	// TODO: Check privileges
		}

		public Task<string> GetNameAsync(Language Language)
		{
			return Language.GetStringAsync(typeof(GpioSource), 1, "General Purpose I/O");
		}

		public IEnumerable<INode> RootNodes => this.controllers;

		public Task<INode> GetNodeAsync(IThingReference NodeRef)
		{
			throw new NotImplementedException();
		}
	}
}
