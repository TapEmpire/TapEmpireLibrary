using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TEL.Services;
using UnityEngine;
using TapEmpire.Services;
using TapEmpire.Utility;
using Zenject;

namespace TapEmpire.Services
{
    [System.Serializable]
    public class ABTestingService : Initializable, IABTestingService
    {
        private const string AB_TEST1_KEY = "AbTest1";
        
        // [SerializeField]
        // private SerializableDictionary<ABTestingGroup, GameExecutionScenarioSettings>
        //     _groupsExecutionSettings;

        [SerializeField]
        private string _defaultExecutingGroup;
        
        private string _executingGroup;
        
        public string Group => _executingGroup;

        public System.Action OnGroupAssigned { get; set; } = null;
        
        [Inject]
        private ISceneContextsService _contextsService;

        private SceneContext _coreContext;
        
        protected override UniTask OnInitializeAsync(CancellationToken cancellationToken)
        {
            GetAbGroup();
            _contextsService.OnSceneContextInstalled += SceneContextsService_OnSceneContextInstalled;
            return UniTask.CompletedTask;
        }

        protected override void OnRelease()
        {
            if (_coreContext != null)
            {
                _contextsService.OnSceneContextInstalled -= SceneContextsService_OnSceneContextInstalled;
            }
        }

        private void SceneContextsService_OnSceneContextInstalled(string contextId, SceneContext sceneContext)
        {
            if (contextId != "Core")
            {
                return;
            }
            _coreContext = sceneContext;
            // if (!_groupsExecutionSettings.TryGetValue(_executingGroup, out var settings))
            // {
            //     Debug.LogError($"No execution settings for group {_executingGroup}");
            //     return;
            // }
            // var gameExecutionCoreSystem = _coreContext.Container.Resolve<ScrewJam.CoreSystems.IGameExecutionCoreSystem>();
                //gameExecutionCoreSystem.ExecutionScenarioSettings = settings;
        }

        private string GetABGroupFromPrefs()
        {
            return PlayerPrefsUtility.GetString(AB_TEST1_KEY);
        }

        private void GetAbGroup()
        {
            var abGroup = GetABGroupFromPrefs();
            _executingGroup = abGroup ?? (Application.isEditor ? _defaultExecutingGroup : GetRandomAbGroup());
            PlayerPrefsUtility.SetStringFast(AB_TEST1_KEY, _executingGroup.ToString());
            SetupABTest();
        }

        private string GetRandomAbGroup()
        {
            // TODO
            return null;
        }

        private void SetupABTest()
        {
            DOVirtual.DelayedCall(0.0f, () => OnGroupAssigned?.Invoke());
            // OnGroupAssigned?.Invoke();
        }

        public void SetGroup(string group)
        {
            _executingGroup = group;
            PlayerPrefsUtility.SetStringFast(AB_TEST1_KEY, group.ToString());
            
            // apply
            if (_coreContext == null)
            {
                return;
            }
            // if (!_groupsExecutionSettings.TryGetValue(group, out var settings))
            // {
            //     Debug.LogError($"No execution settings for group {_executingGroup}");
            //     return;
            // }
            // var gameExecutionCoreSystem = _coreContext.Container.Resolve<IGameExecutionCoreSystem>();
            // gameExecutionCoreSystem.ExecutionScenarioSettings = settings;
            //
            // var levelsCoreSystem = _coreContext.Container.Resolve<ILevelsCoreSystem>();
            // levelsCoreSystem.StartLevelInOrder(1, 1);
        }
    }
}
