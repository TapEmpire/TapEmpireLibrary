using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using TapEmpire.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RagDoll.UI
{
    public class RateUsUIView : UIView<RateUsUiViewModel>
    {
        [SerializeField] private Button _funGame;
        [SerializeField] private Button _boring;

        public override UniTask OpenAsync(CancellationToken cancellationToken)
        {
            _funGame.onClick.AddListener(FunGame);
            _boring.onClick.AddListener(Boring);
            return UniTask.CompletedTask;
        }

        private void FunGame()
        {
            DerivedModel.FunGameCommand.Execute(Unit.Default);
        }

        private void Boring()
        {
            DerivedModel.BoringCommand.Execute(Unit.Default);
        }

        public override UniTask CloseAsync(CancellationToken cancellationToken)
        {
            _funGame.onClick.RemoveAllListeners();
            _boring.onClick.RemoveAllListeners();
            return UniTask.CompletedTask;
        }
    }
}