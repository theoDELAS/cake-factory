using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Paramètres;
using CakeMachine.Utils;
using CakeMachine.Utils.CakeMachine.Utils;

namespace CakeMachine.Fabrication.Opérations;

internal class Cuisson : IMachine<GâteauCru[], GâteauCuit[]>
{
    private readonly ThreadSafeRandomNumberGenerator _rng;
    private readonly TimeSpan _tempsCuisson;
    private readonly ushort _nombrePlaces;
    private readonly double _defectRate;

    private readonly EngorgementProduction _lock = new(1);

    public Cuisson(ThreadSafeRandomNumberGenerator rng, ParamètresCuisson paramètres)
    {
        _rng = rng;

        var (nombrePlaces, defectRate, tempsCuisson) = paramètres;
        _tempsCuisson = tempsCuisson;
        _nombrePlaces = nombrePlaces;
        _defectRate = defectRate;
    }

    private void VérifierNombreGâteaux(IReadOnlyCollection<GâteauCru> gâteaux)
    {
        if (gâteaux.Count > _nombrePlaces)
            throw new InvalidOperationException(
                $"Le poste de Cuisson ne peut pas accepter plus de {_nombrePlaces} gâteaux en même temps.");
    }

    private GâteauCuit[] Factory(IEnumerable<GâteauCru> gâteaux)
        => gâteaux.Select(gâteau => new GâteauCuit(gâteau, _rng.NextBoolean(1 - _defectRate))).ToArray();

    public GâteauCuit[] Cuire(params GâteauCru[] gâteaux)
    {
        _lock.Wait();

        try 
        {
            VérifierNombreGâteaux(gâteaux);
            AttenteIncompressible.Attendre(_tempsCuisson);
            return Factory(gâteaux);
        } 
        finally
        {
            _lock.Release();
        }
    }

    public async Task<GâteauCuit[]> CuireAsync(params GâteauCru[] gâteaux)
    {
        await _lock.WaitAsync().ConfigureAwait(false);

        try
        {
            VérifierNombreGâteaux(gâteaux);
            await AttenteIncompressible.AttendreAsync(_tempsCuisson).ConfigureAwait(false);
            return Factory(gâteaux);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    async Task<GâteauCuit[]> IMachine<GâteauCru[], GâteauCuit[]>.ProduireAsync(GâteauCru[] input,
        CancellationToken token)
        => await CuireAsync(input).ConfigureAwait(false);

    /// <inheritdoc />
    GâteauCuit[] IMachine<GâteauCru[], GâteauCuit[]>.Produire(GâteauCru[] input)
        => Cuire(input);

    /// <inheritdoc />
    public int PlacesRestantes => _lock.PlacesRestantes;
}