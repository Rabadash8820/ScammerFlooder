using System;
using System.IO;
using System.Linq;

namespace ScammerFlooder {

    internal class Program {

        public static double s_interval = 1d;

        private static int Main(string[] args) {
            Console.WriteLine(" -- Telecommunication Scammer Flooder -- v1.0");

            string accountSid = null;
            string authToken = null;
            string toNum = null;
            string fromNums = null;
            string message = null;
            string intervalStr = null;

            // Get Twilio credentials and the number to call from the config file, if one was provided
            bool configFileGiven = (args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]));
            if (configFileGiven) {
                string path = Path.GetFullPath(args[0]);
                try {
                    using (var reader = new StreamReader(path)) {
                        accountSid = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(accountSid))
                            Console.WriteLine($"No Twilio account SID provided in config file {path}");

                        authToken = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(authToken))
                            Console.WriteLine($"No Twilio auth token provided in config file {path}");

                        toNum = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(toNum))
                            Console.WriteLine($"No phone number to flood provided in config file {path}");

                        fromNums = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(fromNums))
                            Console.WriteLine($"No phone numbers to call from provided in config file {path}");

                        message = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(message))
                            Console.WriteLine($"No message was provided in config file {path}");

                        intervalStr = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(intervalStr))
                            Console.WriteLine($"No call interval provided in config file {path}");
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error reading from config file {path}:");
                    Console.WriteLine(ex);
                    Console.ReadLine();
                    return 1;
                }
            }

            // If any config values have not been set then ask for them on the command line
            Console.WriteLine();
            if (string.IsNullOrWhiteSpace(accountSid))
                if (!continueWithInput("Enter your Twilio account SID, or 'q' to exit: ", false, out accountSid))
                    return 1;
            if (string.IsNullOrWhiteSpace(authToken))
                if (!continueWithInput("Enter your Twilio Auth token, or 'q' to exit: ", false, out authToken))
                    return 1;
            if (string.IsNullOrWhiteSpace(toNum))
                if (!continueWithInput("Enter the number to flood (do not include '+1'), or 'q' to exit: ", false, out toNum))
                    return 1;
            if (string.IsNullOrWhiteSpace(fromNums))
                if (!continueWithInput("Enter the numbers to call from (separated by spaces, do not include '+1'), or 'q' to exit: ", false, out fromNums))
                    return 1;
            if (string.IsNullOrWhiteSpace(message))
                if (!continueWithInput("Enter the flood message, or 'q' to exit: ", false, out message))
                    return 1;
            if (string.IsNullOrWhiteSpace(intervalStr))
                if (!continueWithInput("Enter the interval (in seconds) at which to flood the number, or 'q' to exit: ", false, out intervalStr))
                    return 1;

            // Finalize configuration
            toNum = "+1" + toNum;
            string[] fromNumArr = fromNums.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(n => "+1" + n).ToArray();
            double.TryParse(intervalStr, out double interval);
            interval *= 1000;

            // Confirm config values for the user before starting
            Console.WriteLine();
            Console.WriteLine("Flooding will now continue with the following configuration:");
            Console.WriteLine();
            Console.WriteLine($"\tTwilio account SID: {accountSid}");
            Console.WriteLine($"\tTwilio auth token: {authToken}");
            Console.WriteLine($"\tPhone number to flood: {toNum}");
            Console.WriteLine($"\tPhone numbers to call from: {fromNumArr}");
            Console.WriteLine($"\tFlood message: {message}");
            Console.WriteLine($"\tInterval (seconds) at which to call: {interval / 1000d}");
            Console.WriteLine();
            Console.Write($"Press ENTER to start flooding.  You may press any key at any time to exit.");
            Console.ReadLine();

            // Set up Flooders
            ScammerFlooder[] flooders = fromNumArr.Select(num => {
                var flooder = new ScammerFlooder(accountSid, authToken, toNum, num, message, interval);
                flooder.StartingFlood += (sender, e) =>
                    Console.WriteLine($"Starting call {e.FloodCount} from {e.FromNumber} to {e.ToNumber}...");
                return flooder;
            }).ToArray();

            // Start flooding
            foreach (ScammerFlooder flooder in flooders)
                flooder.StartFlooding();

            // Stop all Flooders when user is ready to quit
            int input;
            do input = Console.Read();
            while (input == 0);
            Console.Write("Okay, exiting...");
            foreach (ScammerFlooder flooder in flooders)
                flooder.StopFlooding();

            return 0;
        }
        
        private static bool continueWithInput(string inputMsg, bool writeline, out string value, char exitKey = 'q') {
            string input;

            // Keep reading input until something is provided
            do {
                if (writeline)
                    Console.WriteLine(inputMsg);
                else
                    Console.Write(inputMsg);
                input = Console.ReadLine();
            } while (input == string.Empty);

            // Make sure the exit key wasn't enterred
            bool keepGoing = false;
            if (input == new string(exitKey, 1)) {
                Console.WriteLine("Okay, exiting...");
                value = string.Empty;
            }
            else {
                value = input;
                keepGoing = true;
            }
            return keepGoing;
        }

    }

}