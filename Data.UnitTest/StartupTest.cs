using Enyim.Caching;
using Enyim.Caching.Configuration;
using H2HGermPlasmProcessor.Data.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace H2HGermPlasmProcessor.Data.UnitTest
{
    public class StartupTest
    {
        [Fact]
        public void VerifyStartup()
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            Startup startup = new Startup();
            startup.ConfigureServices(serviceCollection);
            Assert.NotEqual(0, serviceCollection.Count);
        }
    }
}
