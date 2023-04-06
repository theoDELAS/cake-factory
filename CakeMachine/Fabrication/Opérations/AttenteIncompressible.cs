namespace CakeMachine.Fabrication.Opérations;

internal static class AttenteIncompressible
{
    public static void Attendre(TimeSpan howLong) => Thread.Sleep(howLong);
    public static async Task AttendreAsync(TimeSpan howLong) 
        => await Task.Run(() => Thread.Sleep(howLong)).ConfigureAwait(false);
}