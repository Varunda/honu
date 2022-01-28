using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using watchtower.Models.Queues;

namespace watchtower.Services.Queues {

    public class JaegerSignInOutQueue {

        private readonly ILogger<JaegerSignInOutQueue> _Logger;

        private ConcurrentQueue<JaegerSigninoutEntry> _Signin = new ConcurrentQueue<JaegerSigninoutEntry>();
        private ConcurrentQueue<JaegerSigninoutEntry> _Signout = new ConcurrentQueue<JaegerSigninoutEntry>();

        public JaegerSignInOutQueue(ILogger<JaegerSignInOutQueue> logger) {
            _Logger = logger;
        }

        public void QueueSignIn(JaegerSigninoutEntry entry) {
            _Signin.Enqueue(entry);
        }

        public void QueueSignOut(JaegerSigninoutEntry entry) {
            _Signout.Enqueue(entry);
        }

        public List<JaegerSigninoutEntry> GetSignIn() {
            JaegerSigninoutEntry[] arr = new JaegerSigninoutEntry[_Signin.Count];
            _Signin.CopyTo(arr, 0);
            return new List<JaegerSigninoutEntry>(arr);
        }

        public List<JaegerSigninoutEntry> GetSignOut() {
            JaegerSigninoutEntry[] arr = new JaegerSigninoutEntry[_Signout.Count];
            _Signout.CopyTo(arr, 0);
            return new List<JaegerSigninoutEntry>(arr);
        }

        public void Clear() {
            _Signin.Clear();
            _Signout.Clear();
        }

    }
}
