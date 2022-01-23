using System;

namespace Server
{
	class Program
	{
		static void Main(string[] args)
		{
			var server = new Server();

			string pathToDictionary;
			int port;

			while (true)
			{
				Console.WriteLine("Please enter a path to a dictionary");

				pathToDictionary = Console.ReadLine();

				if (string.IsNullOrWhiteSpace(pathToDictionary))
				{
					Console.WriteLine("You haven't entered a path.");

					continue;
				}

				Console.WriteLine("Please enter a port");
				
				string portStr = Console.ReadLine();

				if (!int.TryParse(portStr, out port))
				{
					Console.WriteLine("What you have entered is definitely not a port.");

					continue;
				}

				break;
			}

			try
			{
				server.InitiateListening(pathToDictionary, port, 1000);
			}
			catch (Exception e)
			{
				Console.WriteLine("The following error has occured while initiating listening: " + e.Message);

				return;
			}
			
			server.InitiateAccepting();
		}
	}
}
