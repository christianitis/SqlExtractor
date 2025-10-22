using System.Collections.ObjectModel;
using System.Text;
using SqlExtractor.Lib;

namespace SqlExtractor_Test;

[TestFixture]
public class SortedObservableCollection_Tests: AbstractBaseTestFixture
{
	private char[] _printableAscii = Enumerable.Range(0, 128).Select(it => (char)it).ToArray();

	/**
	 * Generate a list of names in alphabetical order.
	 * Starts with A, after we reach Z, add a character. So A, B, C...Z, AA, AB, AC...AZ, BA, etc.
	 */
	private IEnumerable<string> _generateNames(int count)
	{
		var names = new List<string>();
		var charIndex = 0;
		while (names.Count < count)
		{
			var numberOfChars = _printableAscii.Length % charIndex;
		}

		return [];
	}
	
}