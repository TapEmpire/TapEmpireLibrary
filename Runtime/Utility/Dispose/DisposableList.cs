using System;
using System.Collections.Generic;

namespace TapEmpire.Utility
{
    public class DisposableList : List<IDisposable>, IDisposable
    {
        public void Dispose()
        {
            foreach (var subscription in this)
            {
                subscription.Dispose();
            }
            Clear();
        }
    }
}