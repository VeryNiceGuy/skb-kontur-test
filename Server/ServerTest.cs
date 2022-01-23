using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Server
{
	[TestFixture]
	class ServerTest
	{
		private Server _unit;
		private Thread _clientThread;
		private Stopwatch _testTime;

		void ThreadProc()
		{
			Thread.Sleep(1000);

			var client = new TcpClient(AddressFamily.InterNetwork);
			client.Connect("127.0.0.1", 8076);
			var stream = client.GetStream();

			byte[] command = null; 

			_testTime.Start();

			command = Encoding.ASCII.GetBytes("get<a>");
			stream.Write(command, 0, command.Length);

			var stringCountRaw = new byte[4];
			stream.Read(stringCountRaw, 0, 4);
			var stringCount = BitConverter.ToInt32(stringCountRaw, 0);

			int str = 0;

			while (str < stringCount)
			{
				var stringLengthRaw = new byte[4];
				stream.Read(stringLengthRaw, 0, 4);
				var stringLength = BitConverter.ToInt32(stringLengthRaw, 0);

				var stringRaw = new byte[stringLength];
				stream.Read(stringRaw, 0, stringLength);

				++str;
			}

			_testTime.Stop();

			stream.Close();
			client.Close();

			_unit.Stop();
		}

		[SetUp]
		public void Initialize()
		{
			_unit = new Server();
			_clientThread = new Thread(ThreadProc);
			_testTime = new Stopwatch();
		}

		[Test]
		public void Interaction()
		{
			Assert.DoesNotThrow(InteractionExceptionCheck);
			Assert.Pass("Test time: " +
						_testTime.Elapsed.Hours + ":" +
						_testTime.Elapsed.Minutes + ":" +
						_testTime.Elapsed.Seconds + "." +
						_testTime.Elapsed.Milliseconds);
		}

		public void InteractionExceptionCheck()
		{
			_unit.InitiateListening("dictionary.txt", 8076, 1);
			_clientThread.Start();

			try
			{
				// We are going to cancel a blocking operation when done
				// which will cause throwing an exeption we expect
				_unit.InitiateAccepting();
			}
			catch (SocketException e){}
			
			_clientThread.Join();
		}

		[TearDown]
		public void CleanUp()
		{
			_unit.Stop();
		}
	}
}
