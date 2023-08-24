using Microsoft.Extensions.DependencyInjection;
using System;

namespace ComponentPreview.Construct
{
    public interface IPageImplementation<T>
    {
        public Layout PageLayout { get; }

        public RenderFragmentGen RenderFragmentGen { get; }
    }

    public interface ILink
    {
        public string Text { get; }
        public string Url { get; }
    }

    public record Links<T>(ILink[] ILinks);
    public record struct Link<T>(string Text, string Url) : ILink;

    public record PageImplementation<T> : IPageImplementation<T>
    {
        public virtual Layout PageLayout { get; } = new DefaultLayout();

        public virtual RenderFragmentGen RenderFragmentGen { get; } = null!;
    }

    public delegate IServiceCollection Register(IServiceCollection services);
}
