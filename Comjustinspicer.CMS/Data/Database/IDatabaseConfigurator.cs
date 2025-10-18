using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Comjustinspicer.CMS.Data.Database;

public interface IDatabaseConfigurator
{
    string DBTypeSupported { get; }
    void Configure(IServiceCollection services, IConfiguration configuration);
}
