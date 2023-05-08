namespace Roseau.Decrement.UnitTests.AssertExtensions;

public static class AssertExtension
{
	public static void DoesNotThrow(this Assert _, Func<object?> action)
	{
		try
		{
			action();
		}
		catch (Exception)
		{

			throw new AssertFailedException();
		}
	}
}

