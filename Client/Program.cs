using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Net;

namespace Client
{
	class Program
	{
		static void Main(string[] args)
		{
			var client = new Client();

			while (true)
			{
				Console.WriteLine("Please enter an address(IPv4) of a SKBTestServer:");

				var ipAddressOrHostnameOfSkbTestServerStr = Console.ReadLine();

				if (string.IsNullOrWhiteSpace(ipAddressOrHostnameOfSkbTestServerStr))
				{
					Console.WriteLine("You haven't entered an address or hostname.");

					continue;
				}

				IPAddress[] availableAddresses;

				try
				{
					availableAddresses = Dns.GetHostAddresses(ipAddressOrHostnameOfSkbTestServerStr);
				}
				catch (Exception e)
				{
					if(e is ArgumentException)
						Console.WriteLine("The address/hostname you have entered is not valid.");

					if (e is ArgumentOutOfRangeException)
						Console.WriteLine("What you have entered is definitely not an address or hostname.");

					if (e is SocketException)
					{
						Console.WriteLine("The following network error has occured:" + (e as SocketException).Message);

						return;
					}

					continue;
				}

				Console.WriteLine("Please enter a port used by a SKBTestServer");

				var portOfSkbTestServerStr = Console.ReadLine();
				int portOfSkbTestServer;

				if (!int.TryParse(portOfSkbTestServerStr, out portOfSkbTestServer))
				{
					Console.WriteLine("What you have entered is definitely not a port.");

					continue;
				}

				try
				{
					client.Connect(availableAddresses[0], portOfSkbTestServer);
				}
				catch (SocketException e)
				{
					Console.WriteLine("The following network error has occured: " + e.Message);

					client.Disconnect();

					return;
				}

				break;
			}

			int m;

			while (true)
			{
				Console.WriteLine("Please enter M");

				var mStr = Console.ReadLine();

				if (!int.TryParse(mStr, out m))
				{
					Console.WriteLine("What you have entered is definitely not a number.");

					continue;
				}

				if (m == 0)
				{
					Console.WriteLine("M must be more or equal to 1 and less or equal to 15000.");

					continue;
				}

				break;
			}

			Console.WriteLine("Please enter "+ m + " prefixes.");

			var prefixCount = 0;
			var prefixList = new List<string>();

			while (prefixCount < m)
			{
				var prefix = Console.ReadLine();

				if (string.IsNullOrWhiteSpace(prefix))
				{
					Console.WriteLine("What you have entered is not a prefix.");

					continue;
				}
				
				prefixList.Add(prefix);
				++prefixCount;
			}


			Console.Write('\n');

			foreach (var prefix in prefixList)
			{
				try
				{
					var words = client.RequestAutocomplete(prefix);

					if (words != null)
					{
						foreach (var word in words)
						{
							Console.WriteLine(word);
						}
					}
					else
					{
						Console.WriteLine("There is no word to autocomplete this prefix in the dictionary.");
					}

					Console.Write('\n');
				}
				catch (IOException e)
				{
					Console.WriteLine("The following io error has occured: " + e.Message);

					client.Disconnect();

					return;
				}
			}

			client.Disconnect();
		}
	}
}
