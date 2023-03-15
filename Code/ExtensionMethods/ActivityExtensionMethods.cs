using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace watchtower.Code.ExtensionMethods {

    public static class ActivityExtensionMethods {

        /// <summary>
        ///     Add an event to a span indicating an exception occured. This handles creating the event and tagging it correctly
        /// </summary>
        /// <param name="span">Extension instance</param>
        /// <param name="ex">Exception to tag the <see cref="Activity"/> with</param>
        public static void AddExceptionEvent(this Activity span, Exception ex) {
            Dictionary<string, object?> tags = new() {
                { "exception.message", ex.Message },
                { "exception.type", ex.GetType().Name },
                { "exception.stacktrace", ex.StackTrace }
            };

            span.AddEvent(new ActivityEvent("exception", tags: new(tags)));
        }

    }
}
