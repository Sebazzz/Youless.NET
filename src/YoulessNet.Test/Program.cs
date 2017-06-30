namespace YoulessNet.Test {
    using System;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    /// <summary>
    /// Notice: Create an 'Youless.user.config' with your configuration parameters (host, port, password)
    /// </summary>
    internal class Program {
        private static YoulessServiceClient Client;

        /// <summary>
        /// Entry point of the test application
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args) {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            
            // read config parameters    
            string host = args[0];
            int port = Int32.Parse(args[1]);
            string password = args.Length >= 3 ? args[2] : null;
            
            bool passwordProvided = !String.IsNullOrEmpty(password);
            ICredentials credentials = passwordProvided ? new NetworkCredential(null, password) : null;

            using (ConsoleColorChanger.Change(ConsoleColor.Blue)) {
                Console.WriteLine("Executing Youless tests using live instance @ {0}:{1}", host, port);
                Console.WriteLine(passwordProvided ? "  Using password: '{0}'" : "  Without authentication", password);
                Console.WriteLine();
            }

            // construct service
            using (Client = new YoulessServiceClient(host, port, credentials)) {

                // execute tests
                ExecuteTests();
            }

            // exit pause
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void ExecuteTests() {
            ExecuteTest("Get Youless Status", CurrentStatusTest);
            ExecuteTest("Get 24 hour log", Log24Test);
            ExecuteTest("Get 1 hour log", LogHourTest);
        }

        private static void LogHourTest() {
            YoulessUsageData data = Async(() => Client.GetLastHourMeasurement());

            Assert(() => data.Measurements.Count == 60, "Expected one hour of 60-seconds interval measuring points");

            Console.WriteLine(" Youless Status ");
            foreach (YoulessUsage usage in data.Measurements) {
                Console.WriteLine("{0:s}: {1, 5} {2}", usage.Timestamp, usage.Usage, data.Unit == UsageUnit.Watt ? "W" : "kWh");
            }
        }

        private static void Log24Test() {
            YoulessUsageData data = Async(() => Client.GetDailyMeasurements());

            Assert(() => data.Measurements.Count == 24*6, "Expected entries for every hour of the day in 10 minute intervals");

            Console.WriteLine(" Youless Status ");
            foreach (YoulessUsage usage in data.Measurements) {
                Console.WriteLine("{0:s}: {1, 5} {2}", usage.Timestamp, usage.Usage, data.Unit == UsageUnit.Watt ? "W" : "kWh");
            }
        }

        /// <summary>
        /// Tests the current status
        /// </summary>
        private static void CurrentStatusTest() {
            YoulessStatus status = Async(() => Client.GetStatusAsync());

            Assert(() => status.CurrentUsage > 0, "Expected current usage to be filled");

            Console.WriteLine(
@"Connection status: {0}
Current usage:       {1} watt
Total usage:         {2} kWh
Analog luminosity:   {3}
Reflection dev.:     {4}%
Next status update:  {5} sec",
                status.ConnectionStatus,
                status.CurrentUsage,
                status.TotalCounter,
                status.MovingAverageLevel,
                status.ReflectionDeviation,
                status.NextOnlineStatusUpdate);
        }

        /// <summary>
        /// Executes the test specified
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="functor"></param>
        private static void ExecuteTest(string testName, Action functor) {
            Console.WriteLine("Executing test '{0}'", testName);

            try {
                using (ConsoleColorChanger.Change(ConsoleColor.Gray)) {
                    functor.Invoke();
                    Console.WriteLine();
                }

                using (ConsoleColorChanger.Change(ConsoleColor.Green)) {
                    Console.WriteLine();
                }
            } catch (AssertionException ex) {
                using (ConsoleColorChanger.Change(ConsoleColor.Yellow)) {
                    Console.WriteLine("Assertion failed during test");
                    Console.WriteLine("  FN: {0}", ex.Function);
                    Console.WriteLine(" MSG: {0}", ex.Message);
                }
            } catch (Exception ex) {
                using (ConsoleColorChanger.Change(ConsoleColor.Red)) {
                    Console.WriteLine("Error occurred during test");
                    Console.WriteLine(ex);
                }
            } finally {
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Executes an asynchronous function synchronously on the current thread
        /// </summary>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="taskFunctor"></param>
        /// <returns></returns>
        private static TOutput Async<TOutput>(Func<Task<TOutput>> taskFunctor) {
            Task<TOutput> task = taskFunctor.Invoke();

            TaskAwaiter<TOutput> awaiter = task.GetAwaiter();

            return awaiter.GetResult();
        }

        /// <summary>
        /// Asserts the specified condition is true
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        private static void Assert(Expression<Func<bool>> condition, string message) {
            Func<bool> fn = condition.Compile();

            Debug.Assert(fn != null, "Unexpected assertion: condition compiled = null");
            if (!fn.Invoke()) {
                throw new AssertionException(condition, message);
            }
        }

        private class AssertionException : Exception {
            private readonly string _function;

            /// <summary>
            /// Gets the assertion executed
            /// </summary>
            public string Function {
                [DebuggerStepThrough] get { return this._function; }
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="T:System.Exception"/> class with a specified error message.
            /// </summary>
            /// <param name="function">Expression to visualize</param>
            /// <param name="message">The message that describes the error. </param>
            public AssertionException(Expression function, string message) : base(message) {
                this._function = function.ToString();
            }
        }
    }

    /// <summary>
    /// Helper class for changing console color
    /// </summary>
    public sealed class ConsoleColorChanger : IDisposable {
        private readonly ConsoleColor _previousColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        private ConsoleColorChanger(ConsoleColor previousColor) {
            this._previousColor = previousColor;
        }

        /// <summary>
        /// Changes the current console color to <paramref name="targetColor"/> until the returned <see cref="IDisposable"/> is disposed
        /// </summary>
        /// <param name="targetColor"></param>
        /// <returns></returns>
        public static IDisposable Change(ConsoleColor targetColor) {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = targetColor;

            return new ConsoleColorChanger(currentColor);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            Console.ForegroundColor = this._previousColor;
        }
    }
}