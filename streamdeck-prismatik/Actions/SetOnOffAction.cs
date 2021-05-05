using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using sibalzer.streamdeck.prismatik.Common;

namespace sibalzer.streamdeck.prismatik
{
    [PluginActionId("prismatik_streamdeck.setonoffsetting")]
    public class SetOnOffAction : PluginBase
    {
        private GlobalSettings global;
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                return new PluginSettings();
            }
        }

        #region Private Members

        private PluginSettings settings;

        #endregion
        public SetOnOffAction(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                this.settings = PluginSettings.CreateDefaultSettings();
                SaveSettings();
            }
            else
            {
                this.settings = payload.Settings.ToObject<PluginSettings>();
            }
        }

        public override void Dispose()
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, $"Destructor called");
        }

        public override async void KeyPressed(KeyPayload payload)
        {
            Logger.Instance.LogMessage(TracingLevel.INFO, "Key Pressed");

            try
            {
                if (PrismatikApiClient.PRISMATIC_CLIENT == null || !PrismatikApiClient.PRISMATIC_CLIENT.IsConnected)
                {
                    PrismatikApiClient.SetupTelnetClient(this.global.ApiKey);
                    if (PrismatikApiClient.PRISMATIC_CLIENT == null) return;

                    if (payload.IsInMultiAction)
                    {
                        if (payload.UserDesiredState == 0)
                            PrismatikApiClient.SetStatus(true);
                        if (payload.UserDesiredState == 1)
                            PrismatikApiClient.SetStatus(false);
                    }
                    else
                    {
                        var currStatus = PrismatikApiClient.GetStatus();
                        if (currStatus != "on")
                            PrismatikApiClient.SetStatus(true);
                        else
                            PrismatikApiClient.SetStatus(false);
                    }

                }
            }
            catch(Exception)
            {

            }
                
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload)
        { 
            if (payload?.Settings != null && payload.Settings.Count > 0)
            {
                this.global = payload.Settings.ToObject<GlobalSettings>();

            }
            else
            {
                Logger.Instance.LogMessage(TracingLevel.WARN, $"No this.global settings found, creating new object");
                this.global = new GlobalSettings();
                SetGlobalSettings();
            }
        }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        // Saves the this.global object back the this.global settings
        private void SetGlobalSettings()
        {
            Connection.SetGlobalSettingsAsync(JObject.FromObject(this.global));
        }
        #endregion

    }
}