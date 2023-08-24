using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentPreview.Construct
{
    public abstract class PageBase<T> : ComponentSource<T> where T : PageBase<T>
    {
        public virtual Layout? Layout { get; } = new DefaultLayout();

        public virtual IPageImplementation<T> Implementation { get; set; }

        protected override RenderFragmentGen Self { get => Implementation?.RenderFragmentGen ?? IntrinsicElement.Div.WithContent("test"); }
    }

    public record DefaultLayout : Layout
    {

    }
}
