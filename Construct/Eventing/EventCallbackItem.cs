namespace ComponentPreview.Construct.Eventing
{
    public abstract class EventCallbackItem
    {
        public Func<bool>? PreventDefault;
        public Func<bool>? StopPropagation;
        public string EventName { get; set; } = null;
        protected Action<EventArgs>? CallbackAction { get; set; }
        public Action<EventArgs> SetCallback;

        public void Deconstruct(
            out string eventName,
            out Action<EventArgs>? callback,
            out Func<bool>? preventDefault,
            out Func<bool>? stopPropagation)
        {
            eventName = EventName;
            callback = SetCallback;
            preventDefault = PreventDefault;
            stopPropagation = StopPropagation;
        }

        public abstract EventCallbackItem Add(Action<EventArgs> action);
        public abstract EventCallbackItem Add<T>(Func<EventCallbackItem<T>, Action<T>> action) where T: EventArgs;
    }

    public class EventCallbackItem<T> : EventCallbackItem where T : EventArgs
    {

        public Action<T> Callback => CallbackAction!;


        public void AddVoid(Action<EventArgs> action)
        => this.SetCallbackAction(x => action((T)x));

        public EventCallbackItem<T> SetCallbackAction(Action<EventArgs> t)
        {
            SetCallback += t;
            return this;
        }

        public override EventCallbackItem Add(Action<EventArgs> action)
        {
            return this.SetCallbackAction(x => action((T)x));
        }

        public override EventCallbackItem Add<A>(Func<EventCallbackItem<A>, Action<A>> actionAction)
        {
            return this.SetCallbackAction(a=>actionAction((this as EventCallbackItem<A>)!)((A)a));
        }

        public EventCallbackItem<T> SetStopPropagation(Func<bool> func = null!)
        {
            StopPropagation = func?? (()=>true);
            return this;
        }

        public EventCallbackItem<T> SetPreventDefault(Func<bool> func = null!)
        {
            PreventDefault = func?? (()=>true);
            return this;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (obj == this) return true;
            return obj.GetType() == this.GetType() && EventName.Equals((obj as EventCallbackItem<T>)?.EventName);
        }

        public override int GetHashCode()
        {
            return EventName!.GetHashCode();
        }
    }
}
