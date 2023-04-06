using CakeMachine.Fabrication.ContexteProduction;
using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Utils;

internal static class TrierRebutExtensions
{
    private static IEnumerable<T> TrierRebut<T>(this IEnumerable<T> elements, Usine usine)
        where T : IConforme
    {
        foreach (var conforme in elements)
        {
            if (conforme.EstConforme) yield return conforme;
            else usine.MettreAuRebut(conforme);
        }
    }

    public static T[] TrierRebut<T>(this T[] elements, Usine usine)
        where T : IConforme
        => ((IEnumerable<T>)elements).TrierRebut(usine).ToArray();
}