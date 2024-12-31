using System;
using System.Threading.Tasks;
using tusdotnet.Models.Configuration;

namespace tusdotnet.Helpers
{
    internal static class EventHelper
    {
        internal static Func<T, Task>? GetHandlerFromEvents<T>(Events? events) where T : EventContext<T>, new()
        {
            if (events is null)
            {
                return null;
            }

            var t = typeof(T);

            if (t == typeof(AuthorizeContext))
                return (Func<T, Task>)events.OnAuthorizeAsync;

            if (t == typeof(BeforeCreateContext))
                return (Func<T, Task>)events.OnBeforeCreateAsync;

            if (t == typeof(CreateCompleteContext))
                return (Func<T, Task>)events.OnCreateCompleteAsync;

            if (t == typeof(BeforeDeleteContext))
                return (Func<T, Task>)events.OnBeforeDeleteAsync;

            if (t == typeof(DeleteCompleteContext))
                return (Func<T, Task>)events.OnDeleteCompleteAsync;

            return null;
        }
    }
}