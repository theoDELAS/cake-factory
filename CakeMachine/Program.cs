using System.Runtime.CompilerServices;
using CakeMachine.Simulation;

[assembly:InternalsVisibleTo("CakeMachine.Test")]

const int nombreGâteaux = 100;

var runner = new MultipleAlgorithmsRunner();
await runner.ProduireNGâteaux(nombreGâteaux).ConfigureAwait(false);