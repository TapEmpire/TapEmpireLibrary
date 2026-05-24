using System;
using R3;

namespace TapEmpire.Experimental
{
    public interface IAd : IDisposable
    {
        Subject<AdImpressionData> OnImpression { get; }
    }
}
