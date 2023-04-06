using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Paramètres;
using CakeMachine.Utils;

namespace CakeMachine.Fabrication.Opérations;

internal class Préparation : IMachine<Plat, GâteauCru>
{
    private readonly (TimeSpan Min, TimeSpan Max) _tempsPréparation;
    private readonly ThreadSafeRandomNumberGenerator _rng;
    private readonly double _defectRate;

    private readonly EngorgementProduction _lock;
    private TimeSpan TempsPréparation => _rng.NextDouble() * _tempsPréparation.Min + (_tempsPréparation.Max - _tempsPréparation.Min);

    public Préparation(ThreadSafeRandomNumberGenerator rng, ParamètresPréparation paramètres)
    {
        _tempsPréparation = (paramètres.TempsMin, paramètres.TempsMax);
        _rng = rng;
        _defectRate = paramètres.DefectRate;
        _lock = new EngorgementProduction(paramètres.NombrePlaces);
    }

    /// <inheritdoc />
    async Task<GâteauCru> IMachine<Plat, GâteauCru>.ProduireAsync(Plat input, CancellationToken token)
        => await PréparerAsync(input).ConfigureAwait(false);

    /// <inheritdoc />
    GâteauCru IMachine<Plat, GâteauCru>.Produire(Plat input)
        => Préparer(input);

    public int PlacesRestantes => _lock.PlacesRestantes;

    public GâteauCru Préparer(Plat plat)
    {
        _lock.Wait();

        try
        {
            AttenteIncompressible.Attendre(TempsPréparation);
            return new GâteauCru(plat, _rng.NextBoolean(1 - _defectRate));
        } 
        finally
        {
            _lock.Release();
        }
    }

    public async Task<GâteauCru> PréparerAsync(Plat plat)
    {
        await _lock.WaitAsync().ConfigureAwait(false);

        try
        {
            await AttenteIncompressible.AttendreAsync(TempsPréparation).ConfigureAwait(false);
            return new GâteauCru(plat, _rng.NextBoolean(1 - _defectRate));
        }
        finally
        {
            _lock.Release();
        }
    }
}