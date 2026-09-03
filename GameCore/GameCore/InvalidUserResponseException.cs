namespace GameCore;

/// <summary>
/// Thrown by <see cref="UserProxy"/> when an <see cref="IUser"/> implementation's answer to a
/// choice violates that choice's contract (e.g. wrong number of picks, a card that wasn't among
/// the candidates offered, a duplicate pick). Signals a bug in that specific IUser implementation,
/// not in the game engine itself.
/// </summary>
public class InvalidUserResponseException : Exception
{
	public InvalidUserResponseException()
	{
	}

	public InvalidUserResponseException(string message) : base(message)
	{
	}

	public InvalidUserResponseException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
