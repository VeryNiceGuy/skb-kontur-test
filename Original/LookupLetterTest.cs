using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace Original
{
	[TestFixture]
	class LookupLetterTest
	{
		private LookupLetter _lookup;
		private string[] _clientRequests;
		private Stopwatch _testTime;

		[SetUp]
		public void Initialize()
		{
			_lookup = new LookupLetter();
			_testTime = new Stopwatch();

			var lines = File.ReadAllLines("dictionary.txt", Encoding.ASCII);

			var line = 1;
			while (line < lines.Length)
			{
				var parts = lines[line].Split(' ');
				_lookup.AddDictionaryItem(new DictionaryItem { Word = parts[0], Frequency = Int32.Parse(parts[1]) }, 0);
				++line;
			}

			_clientRequests = File.ReadAllLines("clientRequests.txt", Encoding.ASCII);
		}

		[Test]
		public void Fetching()
		{
			_testTime.Start();

			var line = 1;
			while (line < _clientRequests.Length)
			{
				var items = new List<DictionaryItem>();
				_lookup.GetDictionaryItemsWithPrefix(_clientRequests[line].Trim(), 0, ref items);
				++line;
			}

			_testTime.Stop();

			Assert.Pass("Test time: " +
						_testTime.Elapsed.Hours + ":" +
						_testTime.Elapsed.Minutes + ":" +
						_testTime.Elapsed.Seconds + "." +
						_testTime.Elapsed.Milliseconds);
		}

		[TearDown]
		public void CleanUp(){}
	}
}
