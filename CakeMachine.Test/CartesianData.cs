using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CakeMachine.Test;

internal class CartesianData : IEnumerable<object[]>
{
    private readonly IEnumerable[] _parameters;

    public CartesianData(params IEnumerable[] parameters)
    {
        _parameters = parameters;
    }

    private object[][] InlineData => _parameters
        .Select(p => p.Cast<object>().ToArray())
        .ToArray();

    private IEnumerable<object[]> AddParameter(
        object[][] combinaisons,
        object[] paramètre)
    {
        foreach (var valeur in paramètre)
        foreach (var combinaison in combinaisons)
            yield return combinaison.Append(valeur).ToArray();
    }

    /// <inheritdoc />
    public IEnumerator<object[]> GetEnumerator()
    {
        var combinaisons = InlineData
            .First()
            .Select(value => new [] { value })
            .ToArray();

        foreach (var paramètreSupplémentaire in InlineData.Skip(1))
            combinaisons = AddParameter(combinaisons, paramètreSupplémentaire).ToArray();

        return combinaisons.Cast<object[]>().GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
