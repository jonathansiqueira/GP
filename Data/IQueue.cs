using Amazon.Lambda.Core;
using H2HGermPlasmProcessor.Data.EntryMeans;
using H2HGermPlasmProcessor.Data.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace H2HGermPlasmProcessor.Data
{
    public interface IQueue
    {
        void Initialize(GermPlasmSNSRequest request);

        QueueGermPlasmEvent GetNext(ILambdaContext context);

        Task CreateMessageAsync(ReducedBandKey key);

        Task CreateMessageAsync(QueueGermPlasmEvent gpEvent);

        Task DeleteGPQueueAsync();
    }
}
