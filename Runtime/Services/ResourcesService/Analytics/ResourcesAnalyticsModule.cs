using System.Collections.Generic;
using Game.Services;
using Newtonsoft.Json.Linq;
using R3;
using Zenject;

namespace TapEmpire.Services.Analytics
{
    public class ResourcesAnalyticsModule<ResourceType>
    {
        private IAnalyticsService _analyticsService = null;
        private IResourcesService<ResourceType> _resourcesService = null;
        private IProgressService _progressService = null;

        private CompositeDisposable _disposables = new();

        public ResourcesAnalyticsModule(DiContainer diContainer)
        {
            _analyticsService = diContainer.Resolve<IAnalyticsService>();
            _resourcesService = diContainer.Resolve<IResourcesService<ResourceType>>();

            _resourcesService.OnResourceAdded.Subscribe(OnResourceAdded).AddTo(_disposables);
            _resourcesService.OnResourceUsed.Subscribe(OnResourceUsed).AddTo(_disposables);
        }

        public void OnRelease()
        {
            _disposables.Dispose();
        }

        private void OnResourceAdded((ResourceType resource, int amount, string reason) data)
        {
            _analyticsService.LogEvent(CoreAnalyticsStrings.CommonData, new Dictionary<string, object>{
                { ResourcesAnalyticsStrings.Tag, new JObject(new JProperty(data.resource.ToString(),
                    new JObject(new JProperty(ResourcesAnalyticsStrings.Add,
                        new JObject(
                            new JProperty(ResourcesAnalyticsStrings.Amount, data.amount),
                            new JProperty(ResourcesAnalyticsStrings.Reason, data.reason)
                        )
                    ))
                ))}
            });

            SendGeneralEvents(data.resource, data.reason);
        }

        private void OnResourceUsed((ResourceType resource, int amount, string reason) data)
        {
            _analyticsService.LogEvent(CoreAnalyticsStrings.CommonData, new Dictionary<string, object>{
                { ResourcesAnalyticsStrings.Tag, new JObject(new JProperty(data.resource.ToString(),
                    new JObject(new JProperty(ResourcesAnalyticsStrings.Sub,
                        new JObject(
                            new JProperty(ResourcesAnalyticsStrings.Amount, data.amount),
                            new JProperty(ResourcesAnalyticsStrings.Reason, data.reason)
                        )
                    ))
                ))}
            });
        }

        private void SendGeneralEvents(ResourceType resource, string reason)
        {
            var level = _progressService.GetLevelProgress() + 1;

            _analyticsService.LogEvent(CoreAnalyticsStrings.GameData, new Dictionary<string, object>{
                    { $"Level_{level}", new JObject(new JProperty(ResourcesAnalyticsStrings.Tag, reason)) }
                });
        }
    }
}