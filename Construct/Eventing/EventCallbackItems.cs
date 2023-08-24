using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Runtime.CompilerServices;

namespace ComponentPreview.Construct.Eventing
{
    public static class EventCallbackItems
    {
        public static EventCallbackItem<Args> CreateDefaultItem<Args>(
            [CallerMemberName] string propertyName = "") where Args : EventArgs =>
            new()
            {
                EventName = propertyName.ToLowerInvariant()
            };

        public static EventCallbackItem<MouseEventArgs> OnMouseDown => CreateDefaultItem<MouseEventArgs>();
        public static EventCallbackItem<MouseEventArgs> OnMouseUp => CreateDefaultItem<MouseEventArgs>();
        public static EventCallbackItem<MouseEventArgs> OnClick => CreateDefaultItem<MouseEventArgs>();
        public static EventCallbackItem<KeyboardEventArgs> OnKeyDown => CreateDefaultItem<KeyboardEventArgs>();
        public static EventCallbackItem<KeyboardEventArgs> OnKeyUp => CreateDefaultItem<KeyboardEventArgs>();
        public static EventCallbackItem<FocusEventArgs> OnBlur => CreateDefaultItem<FocusEventArgs>();
        public static EventCallbackItem<FocusEventArgs> OnFocus => CreateDefaultItem<FocusEventArgs>();
        public static EventCallbackItem<FocusEventArgs> OnFocusIn => CreateDefaultItem<FocusEventArgs>();
        public static EventCallbackItem<FocusEventArgs> OnFocusOut => CreateDefaultItem<FocusEventArgs>();
        public static EventCallbackItem<ChangeEventArgs> OnChange => CreateDefaultItem<ChangeEventArgs>();
        public static EventCallbackItem<ChangeEventArgs> OnInput => CreateDefaultItem<ChangeEventArgs>();

    }

    public static class EventItemsExt
    {

        public static HashSet<EventCallbackItem<T>>? RemoveEventListener<T>(this HashSet<EventCallbackItem<T>>? source,
            EventCallbackItem<T> item, Action<EventArgs>? action) where T : EventArgs
        {
            if (source is null) return null;

            if (source!.TryGetValue(item, out var eventCallbackItem) && eventCallbackItem is { SetCallback: not null })
                eventCallbackItem!.SetCallback -= action;

            return source;
        }
    }
}
