using System.Text.RegularExpressions;

namespace GameCore.Cards;

public static class CardNameExtensions
{
	// PascalCase enum names are split into words by inserting a space before every capital
	// letter except the first, e.g. CouncilRoom -> "Council Room", Copper -> "Copper"
	public static string ToDisplayName(this CardName name) =>
		Regex.Replace(name.ToString(), "(?<!^)([A-Z])", " $1");
}
