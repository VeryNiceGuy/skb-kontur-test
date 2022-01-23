using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Original
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

	class Program
	{
		static void Main(string[] args)
		{
			var lookup = new LookupLetter();

			while (true)
			{
				Console.WriteLine("Please enter a path to a dictionary");

				string pathToDictionary = Console.ReadLine();

				if (string.IsNullOrWhiteSpace(pathToDictionary))
				{
					Console.WriteLine("You haven't entered a path.");

					continue;
				}

				string[] lines;

				try
				{
					lines = File.ReadAllLines(pathToDictionary);
				}
				catch (Exception e)
				{
					Console.WriteLine("The following error has occured: " + e.Message);

					continue;
				}

				Console.Write('\n');
				Console.WriteLine(lines[0]);

				var line = 1;
				while (line < lines.Length)
				{
					Console.WriteLine(lines[line]);
					var parts = lines[line].Split(' ');
					lookup.AddDictionaryItem(new DictionaryItem { Word = parts[0], Frequency = Int32.Parse(parts[1]) }, 0);
					++line;
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

			Console.WriteLine("Please enter " + m + " prefixes.");

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

			var elapsedTime = new Stopwatch();
			elapsedTime.Start();

			foreach (var prefix in prefixList)
			{
				try
				{
					var items = new List<DictionaryItem>();
					lookup.GetDictionaryItemsWithPrefix(prefix.Trim(), 0, ref items);

					if (items != null)
					{
						foreach (var item in items)
						{
							Console.WriteLine(item.Word);
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

					return;
				}
			}

			Console.Write('\n');
			Console.WriteLine("Running time: " + elapsedTime.Elapsed.Hours + ":" + elapsedTime.Elapsed.Minutes + ":" +
			                  elapsedTime.Elapsed.Seconds + "." + elapsedTime.Elapsed.Milliseconds);
		}
	}
}
