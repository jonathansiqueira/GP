using Amazon.Lambda.Core;
using H2HGermPlasmProcessor.Data.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace H2HGermPlasmProcessor.Data
{
    public interface INotifier
    {
        Task SendBandProcessStartAsync(BandNotificationMessage message);

        Task SendSelfProcessStartAsync(ILambdaContext context, GermPlasmSNSRequest request, string topicArn);
    }
}
