using System.Runtime.CompilerServices;
using CakeMachine.Fabrication.ContexteProduction;
using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Simulation.Algorithmes;

internal class PremiersPas : Algorithme
{
    /// <inheritdoc />
    public override bool SupportsSync => true;

    /// <inheritdoc />
    public override bool SupportsAsync => true;

    /// <inheritdoc />
    public override IEnumerable<GâteauEmballé> Produire(Usine usine, CancellationToken token)
    {
        var postePréparation = usine.Préparateurs.Single();
        var posteCuisson = usine.Fours.Single();
        var posteEmballage = usine.Emballeuses.Single();

        while (!token.IsCancellationRequested)
        {
            var plats = usine.StockInfiniPlats.Take(2);

            var gâteauxCrus = plats.Select(postePréparation.Préparer);

            //var plat1 = usine.StockInfiniPlats.First();
            //var plat2 = usine.StockInfiniPlats.First();

            //var gâteauCru1 = postePréparation.Préparer(plat1);
            //var gâteauCru2= postePréparation.Préparer(plat2);

            var gâteauCuit = posteCuisson.Cuire(gâteauxCrus.ToArray());

            //var gâteauEmballé1 = posteEmballage.Emballer(gâteauCuit.First());
            //var gâteauEmballé2 = posteEmballage.Emballer(gâteauCuit.Last());
            var gâteauxEmballés = gâteauCuit.Select(posteEmballage.Emballer).AsParallel();
          
            //yield return gâteauEmballé1;
            //yield return gâteauEmballé2;

            foreach (var gâteauEmballé in gâteauxEmballés)
            {
                yield return gâteauEmballé;
            }
        }
    }
}