using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Impl
{
    public class SlackAPIOptions : ISlackAPIOptions
    {
        public string SlackWebhookUrl { get; set; }
        public string SlackAppName { get; set; }
    }
}
