using System.Collections.Generic;
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
            _progressService = diContainer.Resolve<IProgressService>();

            _resourcesService.OnResourceAdded.Subscribe(OnResourceAdded).AddTo(_disposables);
            _resourcesService.OnResourceUsed.Subscribe(OnResourceUsed).AddTo(_disposables);
            
            _resourcesService.OnResourceAddedDetailed.Subscribe(OnResourceAddedDetailed).AddTo(_disposables);
            _resourcesService.OnResourceUsedDetailed.Subscribe(OnResourceUsedDetailed).AddTo(_disposables);
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
        
            SendGeneralEvents(data.resource, data.reason);
        }
        
        private void OnResourceAddedDetailed((ResourceType resource, int amountAdded, ResourceAcquireType type, string reason) data) 
            => SendAdjustAssetEvent("earn_asset", data.resource, data.amountAdded, data.type, data.reason);

        private void OnResourceUsedDetailed((ResourceType resource, int amountUsed, ResourceAcquireType type, string reason) data) 
            => SendAdjustAssetEvent("spend_asset", data.resource, data.amountUsed, data.type, data.reason);

        private void SendAdjustAssetEvent(string eventName, ResourceType resource, int amount, ResourceAcquireType type, string reason)
        {
            var level = _progressService.GetVisualProgress();

            _analyticsService.LogAdjustEvent(new Dictionary<string, object>
            {
                { "adjust_event_name", eventName },
                { "level", level },
                { "level_id", level },
                { "amount", amount },
                { "type", type.ToString() },
                { "asset", resource.ToString() },
                { "source", reason }
            });
        }

        private void SendGeneralEvents(ResourceType resource, string reason)
        {
            var level = _progressService.GetVisualProgress();

            _analyticsService.LogEvent(CoreAnalyticsStrings.TrashData, new Dictionary<string, object>{
                { $"Level_{level}", new JObject(new JProperty(ResourcesAnalyticsStrings.Tag,
                    new JObject(new JProperty(resource.ToString(), reason))))
                }
            });
        }
    }
}
