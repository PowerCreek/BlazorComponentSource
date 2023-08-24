using ComponentPreview.Construct;
using Microsoft.Extensions.DependencyInjection;

namespace ComponentPreview.Extensions
{
    public static class StartupExt
    {

        public static IServiceCollection With(this IServiceCollection services, Delegate register) => (IServiceCollection)register.DynamicInvoke(services)!;

    }
}
