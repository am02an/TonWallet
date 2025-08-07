using Newtonsoft.Json;

namespace UnitonConnect.Runtime.Data
{
    public sealed class NftMarketData
    {
        [JsonProperty("address")]
        public string address { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("is_scam")]
        public bool IsScam { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("is_wallet")]
        public bool IsWallet { get; set; }
    }
}