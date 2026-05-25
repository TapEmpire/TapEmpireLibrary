using System;
using R3;

namespace TapEmpire.Services
{
    public interface IAd : IDisposable
    {
        Subject<AdImpressionData> OnImpression { get; }
    }
}
