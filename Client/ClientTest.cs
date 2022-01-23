
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Client
{
	[TestFixture]
	class ClientTest
	{
		private Client _unit;
		private Thread _listenerThread;

		public void ListenerProc()
		{
			var rbuffer = new Byte[128];
			const int stringCount = 0;
			var listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8076);

			listener.Start(1);

			Socket clientSocket = listener.AcceptSocket();

			int byteCount = 0;
			string command = null;

			byteCount = clientSocket.Receive(rbuffer);
			command = Encoding.ASCII.GetString(rbuffer, 0, byteCount);

			if (command.Contains("get"))
			{
				clientSocket.Send(BitConverter.GetBytes(stringCount));
			}

			clientSocket.Receive(rbuffer);
			clientSocket.Close();
			listener.Stop();
		}

		[SetUp]
		public void Initialize()
		{
			_listenerThread = new Thread(ListenerProc);
			_unit = new Client();

			_listenerThread.Start();
		}

		[Test]
		public void Interaction()
		{
			Assert.DoesNotThrow(ConnectionExceptionCheck);
			Assert.DoesNotThrow(AutocompleteRequestExceptionCheck);
		}

		private void ConnectionExceptionCheck()
		{
			_unit.Connect(IPAddress.Parse("127.0.0.1"), 8076);
		}
		private void AutocompleteRequestExceptionCheck()
		{
			_unit.RequestAutocomplete("test");
		}

		[TearDown]
		public void CleanUp()
		{
			_unit.Disconnect();
			_unit = null;
		}
	}
}
