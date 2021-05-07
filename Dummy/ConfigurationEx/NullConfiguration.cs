using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace Dummy.ConfigurationEx
{
    public class NullConfiguration : IConfiguration
    {
        public static NullConfiguration Instance { get; } = new();

        private NullConfiguration()
        {
        }

        public IConfigurationSection GetSection(string key)
        {
            return default!;
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return Enumerable.Empty<IConfigurationSection>();
        }

        public IChangeToken GetReloadToken()
        {
            return NullChangeToken.Singleton;
        }

        public string this[string key]
        {
            get => default!;
            set {}
        }
    }
}