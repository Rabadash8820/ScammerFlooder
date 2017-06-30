using System;
using System.Timers;
using System.Web;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ScammerFlooder {

    public class FloodEventArgs : EventArgs {
        public FloodEventArgs(string to, string from, long floodCount) {
            ToNumber = to;
            FromNumber = from;
            FloodCount = floodCount;
        }

        public string ToNumber { get; }
        public string FromNumber { get; }
        public long FloodCount { get; }
    }

    public class ScammerFlooder {

        private Timer _timer;
        private string _toNum;
        private string _fromNum;
        private Uri _twimlUri;
        private long _count = 0;

        public ScammerFlooder(string accountSid, string authToken, string toNumber, string fromNumber, Uri twimlURI, double interval) {
            TwilioClient.Init(accountSid, authToken);
            initialize(toNumber, fromNumber, twimlURI, interval);
        }
        public ScammerFlooder(string username, string password, string accountSid, string toNumber, string fromNumber, Uri twimlURI, double interval) {
            TwilioClient.Init(username, password, accountSid);
            initialize(toNumber, fromNumber, twimlURI, interval);
        }

        public event EventHandler<FloodEventArgs> StartingFlood;

        public void StartFlooding() => _timer.Start();
        public void StopFlooding() => _timer.Stop();

        private void initialize(string toNumber, string fromNumber, Uri twimlURI, double interval) {
            _toNum = toNumber;
            _fromNum = fromNumber;
            _twimlUri = twimlURI;

            _timer = new Timer(interval);
            _timer.Elapsed += timer_Elapsed;
        }
        private void timer_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                var args = new FloodEventArgs(_toNum, _fromNum, ++_count);
                StartingFlood?.Invoke(this, args);
                var Call = CallResource.Create(
                    to: new PhoneNumber(_toNum),
                    from: new PhoneNumber(_fromNum),
                    record: true,
                    url: _twimlUri
                );
            }
            catch (Exception ex) {
                Console.WriteLine($"Error using number {_fromNum}:");
                Console.WriteLine(ex);
            }
        }

    }

}
