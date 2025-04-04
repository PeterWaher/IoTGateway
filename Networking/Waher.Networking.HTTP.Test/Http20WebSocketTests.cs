﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;

namespace Waher.Networking.HTTP.Test
{
	[TestClass]
	public class Http20WebSocketTests : WebSocketTests
	{
		[ClassInitialize]
		public new static void ClassInitialize(TestContext _)
		{
			WebSocketTests.ClassInitialize(_);
		}

		public override Version ProtocolVersion => HttpVersion.Version20;
	}
}
