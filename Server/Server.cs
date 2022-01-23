using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
	internal class DictionaryItem
	{
		public string Word { get; set; }
		public int Frequency { get; set; }
	}

	internal class LookupLetter
	{
		private readonly List<DictionaryItem> _correspondingItems = new List<DictionaryItem>();
		private readonly LookupLetter[] _downLevel = new LookupLetter[256];
		private readonly List<LookupLetter> _children = new List<LookupLetter>(); 

		public void AddDictionaryItem(DictionaryItem item, int numProcessedChars)
		{
			var raw = new byte[1];
			Encoding.ASCII.GetBytes(item.Word,
									numProcessedChars,
									1,
									raw,
									0);

			++numProcessedChars;

			if (numProcessedChars == 15 || numProcessedChars == item.Word.Length)
			{
				_correspondingItems.Add(item);
			}
			else
			{
				if (_downLevel[raw[0]] == null)
				{
					_downLevel[raw[0]] = new LookupLetter();
					_children.Add(_downLevel[raw[0]]);
				}

				_downLevel[raw[0]].AddDictionaryItem(item, numProcessedChars);
			}
		}

		public void GetAllDictionaryItems(ref List<DictionaryItem> items)
		{
			for (int item = 0; item < _correspondingItems.Count; ++item)
			{
				items.Add(_correspondingItems[item]); 
			}

			for (int child = 0; child < _children.Count; ++child)
			{
				_children[child].GetAllDictionaryItems(ref items);
			}
		}

		public void GetDictionaryItemsWithPrefix(string prefix,
												int numProcessedChars,
												ref List<DictionaryItem> items)
		{
			var raw = new byte[1];

			Encoding.ASCII.GetBytes(prefix,
									numProcessedChars,
									1,
									raw,
									0);

			++numProcessedChars;

			if (numProcessedChars == prefix.Length)
			{
				if (_downLevel[raw[0]] != null)
				{
					_downLevel[raw[0]].GetAllDictionaryItems(ref items);
				}

				return;
			}

			if (_downLevel[raw[0]] != null)
			{
				_downLevel[raw[0]].GetDictionaryItemsWithPrefix(prefix, numProcessedChars, ref items);
			}
		}
	}

	class Server
	{
		private TcpListener _tcpListener;
		private readonly SynchronizationContext _context = SynchronizationContext.Current;
		private readonly LookupLetter _lookup = new LookupLetter();

		private void ThreadProc(Object state)
		{
			var rbuffer = new byte[24];
			var clientSocket = state as Socket;

			if (clientSocket == null)
				return;

			try
			{
				while (true)
				{
					int byteCount = clientSocket.Receive(rbuffer);

					if (byteCount == 0)
					{
						clientSocket.Shutdown(SocketShutdown.Both);
						clientSocket.Close();

						return;
					}

					var command = Encoding.ASCII.GetString(rbuffer, 0, byteCount);

					if (command.Contains("get"))
					{
						var left = command.IndexOf('<');
						var right = command.IndexOf('>');

						var prefix = command.Substring(left + 1, right - (left + 1)).Trim();

						var relevantDictionaryItems = new List<DictionaryItem>();
						_lookup.GetDictionaryItemsWithPrefix(prefix, 0, ref relevantDictionaryItems);

						if (relevantDictionaryItems.Count == 0)
						{
							clientSocket.Send(BitConverter.GetBytes(0));

							continue;
						}

						relevantDictionaryItems.Sort(delegate(DictionaryItem a, DictionaryItem b)
						{
							var result = b.Frequency.CompareTo(a.Frequency);

							return result != 0 ? result : System.String.Compare(a.Word, b.Word, System.StringComparison.Ordinal);
						});

						clientSocket.Send(BitConverter.GetBytes(relevantDictionaryItems.Count));

						for (int item = 0; item < relevantDictionaryItems.Count; ++item)
						{
							clientSocket.Send(BitConverter.GetBytes(relevantDictionaryItems[item].Word.Length));
							clientSocket.Send(Encoding.ASCII.GetBytes(relevantDictionaryItems[item].Word));
						}
					}
				}
			}
			catch (Exception e)
			{
				_context.Post(OnThreadError, e.Message);
			}

			clientSocket.Close();
		}

		private void OnThreadError(object message)
		{
			Console.WriteLine("The following error occured while processing client request: " + (string)message);
		}

		public void InitiateListening(string dictionaryPath, int port, int backlog)
		{
			var localEndPoint = new IPEndPoint(IPAddress.Any, port);
			_tcpListener = new TcpListener(localEndPoint);

			var lines = File.ReadAllLines(dictionaryPath, Encoding.ASCII);

			var line = 1;
			while (line < lines.Length)
			{
				var parts = lines[line].Split(' ');
				_lookup.AddDictionaryItem(new DictionaryItem { Word = parts[0], Frequency = Int32.Parse(parts[1]) }, 0);
				++line;
			}

			_tcpListener.Start(backlog);
		}

		public void InitiateAccepting()
		{
			while (true)
			{
				Socket clientSocket = _tcpListener.AcceptSocket();
				ThreadPool.QueueUserWorkItem(ThreadProc, clientSocket);
			}
		}

		public void Stop()
		{
			_tcpListener.Stop();
		}
	}
}
