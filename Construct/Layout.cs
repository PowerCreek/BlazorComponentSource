using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ComponentPreview.Construct
{
    public abstract record Layout
    {
        public Layout(params (string Key, RenderFragmentGen? Val)[] fragments)
        {
            SetLayout(fragments);
        }

        public Layout SetLayout(params (string Key, RenderFragmentGen? Val)[] fragments)
        {
            foreach ((string key, var val) in fragments)
                LayoutMap[key] = val;

            return this;
        }

        protected Dictionary<string, RenderFragmentGen> LayoutMap { get; init; } = new();

        public RenderFragmentGen this[string val]
        {
            get => LayoutMap[val];
            set => LayoutMap[val] = value;
        }

        public virtual Layout GetLayout() => this;


    }
}
