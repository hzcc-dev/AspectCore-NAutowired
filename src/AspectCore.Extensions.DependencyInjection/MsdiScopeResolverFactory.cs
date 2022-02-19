using System;
using AspectCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.NAutowired
{
    internal class MsdiScopeResolverFactory : IScopeResolverFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public MsdiScopeResolverFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IServiceResolver CreateScope()
        {
            return new MsdiServiceResolver(_serviceProvider.CreateScope().ServiceProvider);
        }
    }
}
