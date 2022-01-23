using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
	class Client
	{
		private readonly TcpClient _tcpClient;
		private NetworkStream _stream;

		public Client()
		{
			_tcpClient = new TcpClient(AddressFamily.InterNetwork);
		}

		public void Connect(IPAddress address, int port)
		{
			_tcpClient.Connect(address, port);
			_stream = _tcpClient.GetStream();
		}

		public void Disconnect()
		{
			if(_stream!=null)
				_stream.Dispose();

			_tcpClient.Close();
		}

		public string RequestDictionaryInfo()
		{
			var command = Encoding.ASCII.GetBytes("info");
			_stream.Write(command, 0, command.Length);

			var stringLengthRaw = new byte[4];
			_stream.Read(stringLengthRaw, 0, 4);

			var stringLength = BitConverter.ToInt32(stringLengthRaw, 0);
			var stringRaw = new byte[stringLength];
			_stream.Read(stringRaw, 0, stringLength);

			return Encoding.ASCII.GetString(stringRaw);
		}

		public List<string> RequestAutocomplete(string prefix)
		{
			var command = Encoding.ASCII.GetBytes("get<" + prefix + ">");

			_stream.Write(command, 0, command.Length);

			var stringCountRaw = new byte[4];
			_stream.Read(stringCountRaw, 0, 4);
			var stringCount = BitConverter.ToInt32(stringCountRaw, 0);

			if (stringCount == 0)
				return null;

			var strings = new List<string>();

			int i = 0;

			while (i < stringCount)
			{
				var stringLengthRaw = new byte[4];
				_stream.Read(stringLengthRaw, 0, 4);

				var stringLength = BitConverter.ToInt32(stringLengthRaw, 0);
				var stringRaw = new byte[stringLength];
				_stream.Read(stringRaw, 0, stringLength);

				strings.Add(Encoding.ASCII.GetString(stringRaw));

				++i;
			}

			return strings;
		}
	}
}
