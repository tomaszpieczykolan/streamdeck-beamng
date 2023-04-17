using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDeckBeamNG
{
    [PluginActionId("com.tomaszpieczykolan.streamdeck.beamng.action.speedometer")]
    public class Speedometer : PluginBase
    {
        private class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                PluginSettings instance = new PluginSettings();
                instance.SpeedUnitMs = false;
                instance.SpeedUnitKmh = true;
                instance.SpeedUnitMph = false;
                return instance;
            }

            [JsonProperty(PropertyName = "speedUnitMs")]
            public bool SpeedUnitMs { get; set; }

            [JsonProperty(PropertyName = "speedUnitKmh")]
            public bool SpeedUnitKmh { get; set; }

            [JsonProperty(PropertyName = "speedUnitMph")]
            public bool SpeedUnitMph { get; set; }
        }

        #region Private Members

        private PluginSettings settings;
        private string unit = "";
        private Single unitMultiplier = 1.0f;

        #endregion
        public Speedometer(SDConnection connection, InitialPayload payload) : base(connection, payload)
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

            UpdateFromSettings();
        }

        public override void Dispose()
        {
        }

        public override void KeyPressed(KeyPayload payload) { }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() { }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            Tools.AutoPopulateSettings(settings, payload.Settings);
            SaveSettings();

            UpdateFromSettings();
        }

        public override void ReceivedGlobalSettings(ReceivedGlobalSettingsPayload payload) { }

        #region Private Methods

        private Task SaveSettings()
        {
            return Connection.SetSettingsAsync(JObject.FromObject(settings));
        }

        #endregion

        private void UpdateFromSettings() {
            if (settings.SpeedUnitMs)
            {
                unit = "m/s";
                unitMultiplier = 1.0f;
            }
            if (settings.SpeedUnitKmh)
            {
                unit = "km/h";
                unitMultiplier = 3.6f;
            }
            if (settings.SpeedUnitMph)
            {
                unit = "mph";
                unitMultiplier = 2.23693629f;
            }
        }
    }
}