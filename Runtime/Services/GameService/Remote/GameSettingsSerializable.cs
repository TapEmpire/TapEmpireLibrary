using System;
using System.Collections.Generic;
using System.Linq;
using TapEmpire.Settings;

namespace TapEmpire.Services
{
    public class GameSettingsRemoteModel : IRemoteModel<GameSettings>
    {
        public bool? AdsOnQuit;
        public bool? AdsOnRestart;
        public WinFlow? WinFlow;
        public List<int> WinFlowExceptionLevels;

        public virtual void FromSettings(GameSettings settings)
        {
            AdsOnQuit = settings.AdsOnQuit;
            AdsOnRestart = settings.AdsOnRestart;
            WinFlow = settings.WinFlow;
            WinFlowExceptionLevels = settings.WinFlowExceptionLevels;
        }

        public virtual void ToSettings(GameSettings settings)
        {
            settings.AdsOnQuit = AdsOnQuit ?? settings.AdsOnQuit;
            settings.AdsOnRestart = AdsOnRestart ?? settings.AdsOnRestart;
            settings.WinFlow = WinFlow ?? settings.WinFlow;

            if (WinFlowExceptionLevels != null)
            {
                settings.WinFlowExceptionLevels = WinFlowExceptionLevels.ToList();
            }
        }
    }

    [Serializable]
    public class GameSettingsSerializable : RemoteSerializableBase<GameSettings, GameSettingsRemoteModel>
    {
        public override string TokenName => "GameSettings";
    }
}
