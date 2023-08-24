using ComponentPreview.Construct.Eventing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using static ComponentPreview.Construct.RenderFragmentGen;

namespace ComponentPreview.Construct
{
    public class AtrinsicElements
    {
        public static RenderFragmentGen<T> CreateComponent<T>(ComponentData<T> data) where T : ComponentSource<T> => RenderFragmentGen.Create<T>().SetData(data);
        public static RenderFragmentGen<T> CreateComponent<T>() where T : ComponentSource<T> => RenderFragmentGen.Create<T>();
        public static RenderFragmentGen AuthorizingComponent<T>(IAuthorizeComponent t) where T: IAuthorizeComponent => Create<IntrinsicElement>(typeof(T))
            .AddAttributes(
                (nameof(t.Authorizing), t.Authorizing),
                (nameof(t.Authorized), t.Authorized),
                (nameof(t.NotAuthorized), t.NotAuthorized));
    }

    public interface IAuthorizeComponent
    {
        public RenderFragmentGen Authorized { get; }
        public RenderFragmentGen NotAuthorized { get; }
        public RenderFragmentGen Authorizing { get; }
    }

    public class IntrinsicElement : RenderFragmentGen
    {
        public static IntrinsicElement Content => Create<IntrinsicElement>();
        public static IntrinsicElement Main => Create<IntrinsicElement>("main");
        public static IntrinsicElement Div => Create<IntrinsicElement>("div");
        public static IntrinsicElement Fa_I => (IntrinsicElement) Create<IntrinsicElement>("i").AddClasses("fa");
        public static IntrinsicElement Span => Create<IntrinsicElement>("span");
        public static IntrinsicElement Input => Create<IntrinsicElement>("input");
        public static IntrinsicElement Section => Create<IntrinsicElement>("section");
        public static HrefElement Href => HrefElement.Create();
        public static IntrinsicElement Button => Create<IntrinsicElement>("button");
        public static IntrinsicElement Img => Create<IntrinsicElement>("img");
        public static IntrinsicElement Footer => Create<IntrinsicElement>("footer");

        public static SvgElement Svg => SvgElement.Create();
    }

    public class HrefElement : IntrinsicElement
    {
        public static HrefElement Create() => Create<HrefElement>("a");

        public HrefElement SetHref(string link) => (HrefElement) AddAttributes((Key: "href", Entry: link));
    }

    public class SvgElement : IntrinsicElement
    {
        public static SvgElement Create() => Create<SvgElement>("svg");

        public record ViewboxValue(int X, int Y, double W, double H)
        {
            public override string ToString() => $"{X} {Y} {W} {H}";
        }

        public SvgElement WithViewbox(ViewboxValue value)
        {
            this.AddAttributes((Key: "viewBox", Entry: value));
            return this;
        }

        public SvgElement WithFill(string fill = "none")
        {
            this.AddAttributes((Key: "fill", Entry: fill));
            return this;
        }

        public SvgElement WithStroke(string color = "currentColor")
        {
            this.AddAttributes((Key: "stroke", Entry: color));
            return this;
        }

        public SvgElement WithStrokeWidth(double width = 1)
        {
            this.AddAttributes((Key: "stroke-width", Entry: width));
            return this;
        }

        public SvgElement WithStrokeLineJoin(string join = "round")
        {
            this.AddAttributes((Key: "stroke-linejoin", Entry: join));
            return this;
        }

        public SvgElement WithStrokeLineCap(string cap = "round")
        {
            this.AddAttributes((Key: "stroke-linecap", Entry: cap));
            return this;
        }

        public record PathValue(string? d = null, string? e = null);

        private void AddPathDValue(string dValue) => this.WithContent(Create<IntrinsicElement>("path")
            .AddAttributes((Key: "d", Entry: dValue)));

        public SvgElement WithPath(PathValue value)
        {
            (var d, var e) = value;
            
            if(d is { Length: > 0 } dValue)
               AddPathDValue(dValue);
            
            return this;
        }
    }

    public record RefItem() { public ElementReference ElementReference { get; set; } }

    public class RenderFragmentGen<T> : RenderFragmentGen where T: ComponentSource<T>
    {
        public ComponentData<T> Data { get; set; }
        public RenderFragmentGen<T> SetData<E>(E data) where E: ComponentData<T>
        {
            Data = data;
            return this;
        }

        public override IEnumerable<Action<int, RenderTreeBuilder>> GetAttributes()
        {
            foreach(var attr in base.GetAttributes()){
                yield return attr;
            }

            if(Data is { } data)
                yield return (seq, b) => { b.AddAttribute(seq, nameof(ComponentSource<T>.Data), data); };
        }
    }

    public class RenderFragmentGen
    {
        public delegate RenderItem ProcRenderItem();

        public (string? Tag, Type? Component)? Tag { get; set; } = null!;
        public string Id { get; set; } = null!;
        public string Key { get; set; } = null!;

        public string CssKey { get; set; }
        public record EventValue (string EventName, Action<EventArgs> Callback, Func<bool>? PreventDefault, Func<bool>? StopPropagation)
        {
            public static EventValue GetEventValueFromEventCallbackItem(EventCallbackItem evt)
            {
                (var name, var callback, var prevent, var stop) = evt;
                return new(name, callback!, prevent, stop);
            }
        }
        public List<EventValue> Listeners = new ();
        public List<(string Key, object Value)> Attributes = new ();
        public List<(string Key, EventCallback<object> Callback)> AttributeCallbacks = new ();

        public RefItem ReferenceItem { get; set; } = null!;

        public (Action<int, object,RenderTreeBuilder> Open, Action<RenderTreeBuilder> Close) GetOpenCloseActions => Tag switch
        {
            { Tag: { Length: > 0 }  t} => (Open: (n, t, r) => r.OpenElement(n, (string)t), Close: (r) => r.CloseElement()),
            { Component: { } t } => (Open: (n, t, r) => r.OpenComponent(n, (Type)t), Close: (r) => r.CloseComponent()),
            { Tag:null, Component: null} => (Open: (n, s, r) => r.OpenRegion(n), Close: (r) => r.CloseRegion()),
            _ => throw new KeyNotFoundException()
        };

        public List<RenderItem> Contents { get; set; } = null!;

        //public RenderFragmentGen AddEventListener<T>(EventCallbackItem<T> evtCallbackItem) where T: EventArgs
        //{
        //    Listeners.Add(EventValue.GetEventValueFromEventCallbackItem(evtCallbackItem));
        //    return this;
        //}

        public RenderFragmentGen AddEventListener(params EventCallbackItem[] evtCallbackItems)
        {
            Listeners.AddRange(evtCallbackItems.Select(e => EventValue.GetEventValueFromEventCallbackItem(e)));
            return this;
        }

        public RenderFragmentGen AddBoundAttribute(string name, EventCallback<object> callback)
        {
            AttributeCallbacks.Add((name, callback));
            return this;
        }

        public RenderFragmentGen AddClasses(params string[] classNames)
        {
            const string KEY = "class";

            if (classNames?.Length == 0) return this;

            var hold = ((string Key, string Value)?) Attributes.FirstOrDefault(a => a.Key is KEY);
            if(hold is { Key: {Length: > 0 } } entry)
                Attributes.Remove(entry);

            AddAttributes((KEY, string.Join(' ', classNames!.Append($"{hold?.Value}").ToArray())));

            return this;
        }

        public RenderFragmentGen AddAttributes(params (string Key, object Entry)[] attributes)
        {
            Attributes.AddRange(attributes);
            return this;
        }

        public static RenderFragmentGen FromComponent<T>()
        {
            var fragGen = new RenderFragmentGen
            {
                Tag = (Tag: null, Component: typeof(T))
            };

            return fragGen;
        }

        public static RenderFragmentGen<T> Create<T>() where T: ComponentSource<T>
        {
            var fragGen = new RenderFragmentGen<T>
            {
                Tag = (Tag: null, Component: typeof(T))
            };

            return fragGen;
        }


        public static T Create<T>(string tag = null!) where T : RenderFragmentGen, new()
        {
            var fragGen = new T
            {
                Tag = (Tag: tag, Component: null),
            };

            return fragGen;
        }
        public static T Create<T>(Type tag) where T : RenderFragmentGen, new()
        {
            var fragGen = new T
            {
                Tag = (Tag: null, Component: tag),
            };

            return fragGen;
        }

        public delegate object ValueCondition();
        public delegate IEnumerable<RenderFragmentGen> ElementCondition();

        public virtual IEnumerable<Action<int, RenderTreeBuilder>> GetAttributes()
        {
            foreach ((var EventName, var Callback, var PreventDefault, var StopPropagation) in Listeners)
            {
                yield return (seq, b) => b.AddAttribute(seq, EventName, Callback);

                if (PreventDefault?.Invoke() is true)
                    yield return (seq, b) => b.AddEventPreventDefaultAttribute(seq, EventName, true);

                if (StopPropagation?.Invoke() is true)
                    yield return (seq,b) => b.AddEventStopPropagationAttribute(seq, EventName, true);
            }

            foreach((var key, var entry) in Attributes)
            {
                yield return key switch
                {
                    _ => (seq, b) => b.AddAttribute(seq, key,entry)                   
                };
            }
        }

        public string? GetCssKey(RenderTreeFrame[] frames)
        {
            RenderTreeFrame? result = frames.Reverse().SkipWhile(e => e.FrameType is not RenderTreeFrameType.Attribute).FirstOrDefault(e =>
               e!.AttributeName is string s and ['b','-',..]
            );
            return result?.AttributeName as string;
        }

        public RenderFragment GetRenderFragment 
        {
            get
            {
                var contents = Contents?.ToArray() ?? Array.Empty<RenderItem>();
                (var open, var close) = GetOpenCloseActions;
                return b =>
                {
                        int seq = 0;
                    switch (Tag)
                    {
                        case { Tag: { Length: > 0 } t }:
                            var cssKey = GetCssKey(b.GetFrames().Array);
                            CssKey = cssKey ?? CssKey;
                            open(seq++, t, b);
                            
                            break;
                        case { Component: { } t }:
                            open(seq++, t, b);
                            break;
                        case { Tag: null, Component: null}:
                            open(seq++, null!, b);
                            break;
                    }
                    
                    if(Key is not null)
                        b.SetKey(Key);
                    
                    if (Id is not null)
                        b.AddAttribute(seq++, "id", Id);

                    foreach (var v in AttributeCallbacks)
                        b.AddAttribute(seq++, v.Key, v.Callback);
                    
                    foreach(var v in GetAttributes())
                        v.Invoke(seq++, b);

                    if(Tag is { Tag: { Length: > 0} } && CssKey is { Length: > 0 })
                            b.AddAttribute(seq++, CssKey, true);

                    if(ReferenceItem is not null)
                        b.AddElementReferenceCapture(seq++, v=>ReferenceItem.ElementReference = v); 
                    

                    foreach (var v in contents)
                    {

                        var result = v.Value switch
                        {
                            ElementCondition e => e.Invoke(),
                            ValueCondition e => e.Invoke(),
                            Func<object> e => e.Invoke() switch
                            {
                                RenderFragmentGen r => r.GetRenderFragment,
                                var r => r
                            },
                            RenderFragment e => e,
                            RenderFragmentGen e => e.GetRenderFragment,
                            object e => e
                        };

                        switch (result)
                        {
                            case MarkupString m:
                                b.AddMarkupContent(seq++, $"{m}");
                                break;
                            case IEnumerable<RenderFragmentGen> a:
                                foreach (var f in a)
                                {
                                    b.AddContent(seq++, f.GetRenderFragment);
                                }
                                break;
                            case RenderFragment a:
                                b.AddContent(seq++, a);
                                break;
                            case object a:
                                b.AddContent(seq++, a);
                                break;
                            case null:
                                break;
                        }
                    }

                    close(b);
                };
            }
        }
    }

    public class RenderItem
    {
        public object Value { get; set; } = null!;
    }

    public static class RenderFragmentGenExt
    {

        public static T SetId<T>(this T self, string id) where T: RenderFragmentGen
        {
            self.Id = id;
            return self;
        }

        public static T SetKey<T>(this T self, string key) where T : RenderFragmentGen
        {
            self.Key = key;
            return self;
        }

        public static T SetRef<T>(this T self, ElementReference reference) where T : RenderFragmentGen
        {
            self.ReferenceItem = new RefItem() { ElementReference = reference };
            return self;
        }

        public static T WithContent<T>(this T self, params object[] contents) where T : RenderFragmentGen
        {
            if (contents is null || contents.Length == 0)
                return self;

            (self.Contents ??= new List<RenderItem>())
                .AddRange(contents.Select(c => {
                    var result = c switch
                    {
                        ProcRenderItem g => g.Invoke(),
                        Func<RenderFragmentGen> g => new RenderItem() { Value = g },
                        ValueCondition e => new RenderItem() { Value = e },
                        ElementCondition e => new RenderItem() { Value = e },
                        var g when g is RenderFragmentGen or Func<object> or { } => new RenderItem() { Value = g },
                        _ => throw new NotSupportedException()
                    };

                    return result;
                }).Where(e => e is not { Value: null }));
            return self;
        }

        public static RenderFragmentGen WithComponentContent<T>(this RenderFragmentGen self, ComponentData<T> data = null!) where T : ComponentSource<T>
            => data switch
            {
                null => self.WithContent(AtrinsicElements.CreateComponent<T>()),
                _ => self.WithContent(AtrinsicElements.CreateComponent(data))
            };
    }

    public static class ElementConditionExt
    {
        public static IEnumerable<RenderFragmentGen> GetFrags(params RenderFragmentGen[] s) => s.AsEnumerable();
        public static ElementCondition FromElementCondition(this RenderFragmentGen source) => (ElementCondition)(()=> GetFrags(source).ToArray());
        public static ElementCondition FromElementCondition(this IEnumerable<RenderFragmentGen> source) => () => source;
        public static ElementCondition FromElementCondition(this RenderFragmentGen[] source) => () => GetFrags(source);

        public static RenderFragmentGen[] ToArray(this RenderFragmentGen s) => new[] { s }; 
    }
}
