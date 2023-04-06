namespace CakeMachine.Utils;

internal class ThreadSafeRandomNumberGenerator
{
    private static readonly Random Random = Random.Shared;

    public double NextDouble()
    {
        return Random.NextDouble();
    }

    public bool NextBoolean(double chancesOfTrue)
    {
        return Random.NextDouble() < chancesOfTrue;
    }
}