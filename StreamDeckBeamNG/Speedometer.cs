using BarRaider.SdTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace StreamDeckBeamNG
{
    [PluginActionId("com.tomaszpieczykolan.streamdeck.beamng.action.speedometer")]
    public class Speedometer : PluginBase, IOutGaugeSubscriber
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

            OutGaugeReceiver.Instance.Subscribers.Add(this);
        }

        public override void Dispose()
        {
            OutGaugeReceiver.Instance.Subscribers.Remove(this);
        }

        public void OnOutGaugeDataReceived(OutGaugeData data) {
            string svg = makeSVG(data.speed);
            Connection.SetImageAsync(svg);
        }

        public override void KeyPressed(KeyPayload payload) { }

        public override void KeyReleased(KeyPayload payload) { }

        public override void OnTick() {
        }

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

        private string makeSVG(Single speed) {
            var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"144\" height=\"144\" viewBox=\"0 0 144 144\">";

            svg += $"<text text-anchor=\"middle\" alignment-baseline=\"middle\" x=\"72\" y=\"70\" font-size=\"65\" fill=\"#FFFFFF\" font-weight=\"bold\">{(speed * unitMultiplier):F0}</text>";
            svg += $"<text text-anchor=\"middle\" alignment-baseline=\"middle\" x=\"72\" y=\"130\" font-size=\"40\" fill=\"#EEEEEE\">{unit}</text>";

            svg += "</svg>";

            return $"data:image/svg+xml;base64,{encoding(svg)}";
        }

        private string encoding(string toEncode) {
            byte[] bytes = Encoding.GetEncoding(28591).GetBytes(toEncode);
            string toReturn = System.Convert.ToBase64String(bytes);
            return toReturn;
        }
    }
}