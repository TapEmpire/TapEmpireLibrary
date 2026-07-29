using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TapEmpire.Level;
using TapEmpire.Settings;
using TapEmpire.Utility;
using UnityEngine;
using Zenject;

namespace TapEmpire.Services
{
    [Serializable]
    public class GameService : Initializable, IGameService
    {
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField] private LevelsTable _levelsTable;
        [SerializeField] private LevelSortTable _sortTable;

        public GameSettings GameSettings => _gameSettings;
        public LevelsTable LevelsTable => _levelsTable;

        protected IProgressService _progressService;

        [Inject]
        private void Construct(IProgressService progressService)
        {
            _progressService = progressService;
        }

        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            SortLevels();

            return base.OnInitializeAsync(cancellationToken);
        }

        public LevelSettings GetCurrentLevel()
        {
            return _levelsTable.Levels[_progressService.GetLevelProgress()];
        }

        protected void UpdateCycles(string lastVersion, string version, int fromLevel)
        {
            if (string.IsNullOrEmpty(lastVersion) ||
                string.Compare(lastVersion, version, StringComparison.InvariantCulture) > 0)
            {
                return;
            }

            if (_progressService.GetCyclesProgress() > 0)
            {
                _progressService.ClearCyclesProgress();
                _progressService.SetLevelProgress(fromLevel);
            }
        }

        private void SortLevels()
        {
            if (_sortTable == null)
            {
                return;
            }

            SortLevelsByOrder();
            SortLevelsByNames();
        }

        private void SortLevelsByOrder()
        {
            if (_sortTable.Order.Length == 0)
            {
                return;
            }

            var levels = _levelsTable.Levels.ToList();
            _levelsTable.Levels.Resize(_sortTable.Order.Length);

            for (var i = 0; i < _sortTable.Order.Length; i++)
            {
                var order = _sortTable.Order[i] - 1;
                _levelsTable.Levels[i] = levels[order];
            }
        }

        private void SortLevelsByNames()
        {
            if (_sortTable.Names.Length == 0)
            {
                return;
            }

            var dictionary = new Dictionary<string, LevelSettings>();
            _levelsTable.Levels.ForEach(x => dictionary.Add(x.GetLevelShortName(), x));
            _levelsTable.Levels.Resize(_sortTable.Names.Length);

            for (var i = 0; i < _sortTable.Names.Length; i++)
            {
                var name = _sortTable.Names[i];
                _levelsTable.Levels[i] = dictionary[name];
            }
        }
    }
}
