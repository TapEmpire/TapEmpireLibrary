using System;
using R3;

namespace TapEmpire.UI
{
    public class SceneLoadingUIViewModel : IUIViewModel
    {
        public Action<float, float> SetProgressCallback;

        public ReactiveProperty<bool> IsLoadingVisible = new(true);
    }
}