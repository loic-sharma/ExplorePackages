﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Knapcode.ExplorePackages
{
    public class AutocompleteIdResults
    {
        [JsonProperty("totalHits")]
        public int TotalHits { get; set; }

        [JsonProperty("data")]
        public List<string> Data { get; set; }
    }
}
