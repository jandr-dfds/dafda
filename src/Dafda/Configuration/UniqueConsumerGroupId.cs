using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Dafda.Configuration
{
    internal static class UniqueConsumerGroupId
    {
        public static void Ensure(IServiceCollection services, string groupId)
        {
            var consumerGroupIdRepository = services.GetOrAddSingleton(() => new ConsumerGroupIdRepository());

            consumerGroupIdRepository.Add(groupId);
        }

        private class ConsumerGroupIdRepository
        {
            private readonly ISet<string> _ids = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            public void Add(string groupId)
            {
                if (_ids.Contains(groupId))
                {
                    throw new InvalidConfigurationException($"Multiple consumers CANNOT be configured with same consumer group id \"{groupId}\".");
                }

                _ids.Add(groupId);
            }
        }
    }
}