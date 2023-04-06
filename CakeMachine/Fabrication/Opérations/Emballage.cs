using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Paramètres;
using CakeMachine.Utils;

namespace CakeMachine.Fabrication.Opérations;

internal class Emballage : IMachine<GâteauCuit, GâteauEmballé>
{
    private readonly EngorgementProduction _lock;
    private readonly TimeSpan _tempsEmballage;
    private readonly ThreadSafeRandomNumberGenerator _rng;
    private readonly double _defectRate;

    public Emballage(ThreadSafeRandomNumberGenerator rng, ParamètresEmballage paramètres)
    {
        var (nombrePlaces, defectRate, tempsEmballage) = paramètres;
        _lock = new EngorgementProduction(nombrePlaces);
        _tempsEmballage = tempsEmballage;
        _defectRate = defectRate;

        _rng = rng;
    }

    public int PlacesRestantes => _lock.PlacesRestantes;

    public GâteauEmballé Emballer(GâteauCuit gâteau)
    {
        _lock.Wait();

        try
        {
            AttenteIncompressible.Attendre(_tempsEmballage);
            return new GâteauEmballé(gâteau, _rng.NextBoolean(1 - _defectRate));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<GâteauEmballé> EmballerAsync(GâteauCuit gâteau)
    {
        await _lock.WaitAsync().ConfigureAwait(false);

        try
        {
            await AttenteIncompressible.AttendreAsync(_tempsEmballage).ConfigureAwait(false);
            return new GâteauEmballé(gâteau, _rng.NextBoolean(1 - _defectRate));
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    async Task<GâteauEmballé> IMachine<GâteauCuit, GâteauEmballé>.ProduireAsync(GâteauCuit input,
        CancellationToken token)
        => await EmballerAsync(input).ConfigureAwait(false);

    /// <inheritdoc />
    GâteauEmballé IMachine<GâteauCuit, GâteauEmballé>.Produire(GâteauCuit input)
        => Emballer(input);
}