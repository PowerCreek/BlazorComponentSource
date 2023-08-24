
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ComponentPreview.Construct
{

    public abstract class ComponentSource<T> : ComponentBase
    {
        public ComponentSource()
        {
            Initialize?.Invoke();    
        }

        [Parameter]
        public ComponentData<T>? Data { get; set; }

        protected RenderFragmentGen? _self;

        public virtual Action? Initialize { get; }

        protected virtual RenderFragmentGen Self { get;  } = null!;
        protected RenderFragment RenderSelf => b => (_self ??= Self)?.GetRenderFragment(b);

        protected override void BuildRenderTree(RenderTreeBuilder builder) =>
            RenderSelf(builder);

        protected bool HasInitialized = false;

        public override Task SetParametersAsync(ParameterView parameters)
        {
            switch(parameters.TryGetValue(nameof(Data), out ComponentData<T>? a))
            {
                case true: Data = a; break;
            };

            if (!HasInitialized)
            {
                HasInitialized = true;
                
                return RunInitAndSetParametersAsync();
            }
            else
            {
                return CallOnParametersSetAsync();
            }
        }

        private async Task RunInitAndSetParametersAsync()
        {
            OnInitialized();
            var task = OnInitializedAsync();

            if (task.Status != TaskStatus.RanToCompletion && task.Status != TaskStatus.Canceled)
            {
                // Call state has changed here so that we render after the sync part of OnInitAsync has run
                // and wait for it to finish before we continue. If no async work has been done yet, we want
                // to defer calling StateHasChanged up until the first bit of async code happens or until
                // the end. Additionally, we want to avoid calling StateHasChanged if no
                // async work is to be performed.
                StateHasChanged();

                try
                {
                    await task;
                }
                catch // avoiding exception filters for AOT runtime support
                {
                    // Ignore exceptions from task cancellations.
                    // Awaiting a canceled task may produce either an OperationCanceledException (if produced as a consequence of
                    // CancellationToken.ThrowIfCancellationRequested()) or a TaskCanceledException (produced as a consequence of awaiting Task.FromCanceled).
                    // It's much easier to check the state of the Task (i.e. Task.IsCanceled) rather than catch two distinct exceptions.
                    if (!task.IsCanceled)
                    {
                        throw;
                    }
                }

                // Don't call StateHasChanged here. CallOnParametersSetAsync should handle that for us.
            }

            await CallOnParametersSetAsync();
        }

        private Task CallOnParametersSetAsync()
        {
            OnParametersSet();
            var task = OnParametersSetAsync();
            // If no async work is to be performed, i.e. the task has already ran to completion
            // or was canceled by the time we got to inspect it, avoid going async and re-invoking
            // StateHasChanged at the culmination of the async work.
            var shouldAwaitTask = task.Status != TaskStatus.RanToCompletion &&
                task.Status != TaskStatus.Canceled;

            // We always call StateHasChanged here as we want to trigger a rerender after OnParametersSet and
            // the synchronous part of OnParametersSetAsync has run.
            StateHasChanged();

            return shouldAwaitTask ?
                CallStateHasChangedOnAsyncCompletion(task) :
                Task.CompletedTask;
        }

        private async Task CallStateHasChangedOnAsyncCompletion(Task task)
        {
            try
            {
                await task;
            }
            catch // avoiding exception filters for AOT runtime support
            {
                // Ignore exceptions from task cancellations, but don't bother issuing a state change.
                if (task.IsCanceled)
                {
                    return;
                }

                throw;
            }

            StateHasChanged();
        }
    }
}
