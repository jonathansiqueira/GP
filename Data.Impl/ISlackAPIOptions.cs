using System;
using System.Collections.Generic;
using System.Text;

namespace H2HGermPlasmProcessor.Data.Impl
{
    public interface ISlackAPIOptions
    {
        string SlackWebhookUrl { get; set; }
        string SlackAppName { get; set; }
    }
}
