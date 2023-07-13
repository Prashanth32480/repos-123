using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Grassroots.IntegrationTest.Common.CustomAttributes;
using Grassroots.IntegrationTest.Common.Helpers;
using Grassroots.IntegrationTest.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Grassroots.Identity.API.IntegrationTest.Runner
{
    class Startup : IHostedService
    {
        #region Properties

        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IEnumerable<string> _configurationTestNamespaces;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly HashSet<string> _passedTestList = new HashSet<string>();
        private readonly HashSet<string> _failedTestList = new HashSet<string>();
        private static uint _totalTests;
        private readonly Stopwatch _integrationTestRunnerExecutionStopwatch;
        private readonly List<APIConfig> _apiConfigs;

        #endregion

        #region Constructor
        public Startup(ILogger<Startup> logger, IConfiguration configuration, IHostApplicationLifetime appLifetime)
        {
            _configurationTestNamespaces = configuration.GetSection("TestProjects")
                                               .Get<IEnumerable<string>>() ??
                                           throw new ArgumentNullException(nameof(_configurationTestNamespaces), "Object not found!");
            _apiConfigs = new List<APIConfig>(configuration.GetSection("APIConfig")
                                                                      .Get<List<APIConfig>>() ?? throw new ArgumentNullException(nameof(APIConfig),
                                                                      $"Object not found!"));
            _configuration = configuration;
            _logger = logger;
            _appLifetime = appLifetime;
            _integrationTestRunnerExecutionStopwatch = new Stopwatch();
        }

        #endregion

        #region IHostedService Methods

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            return Task.CompletedTask;
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        #endregion

        private async void OnStarted()
        {
            List<Assembly> testAssemblies;
            _integrationTestRunnerExecutionStopwatch.Start();
            _logger.LogInformation(LoggingEvents.MainMethod, "INTEGRATION TESTS STARTED");
            if ((testAssemblies = LoadTestAssemblies())?.Count != 0)
            {
                if (await FeedDataAsync(testAssemblies))
                {
                    if (await SetUpAndRunTestsAsync(testAssemblies))
                    {
                        ProvideResults();
                    }
                    else
                        _logger.LogInformation(LoggingEvents.MainMethod, "FAILED SETTING UP RUN METHODS");
                }
                else
                    _logger.LogInformation(LoggingEvents.MainMethod, "FAILED FEEDING DATA");
            }
            Exit();
        }


        private async Task<bool> FeedDataAsync(List<Assembly> testAssemblies)
        {
            _logger.LogInformation(LoggingEvents.FeedData, $"Feeding Generic Data to the Database...");
            var feedDataStopwatch = Stopwatch.StartNew();

            try
            {
                foreach (var assembly in testAssemblies)
                {
                    if (!await FeedValuesFromFileAsync(assembly))
                    {
                        SetReturnType(ExitCode.TestsDidNotRun);
                        return false;
                    }
                }
                _logger.LogInformation(LoggingEvents.FeedData, $"Feeding Generic Data completed in: {{0:hh\\:mm\\:ss\\.fff}}", feedDataStopwatch.Elapsed);
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(LoggingEvents.FeedData, exception, $"Error Feeding Data.");
                return false;
            }
        }

        private async Task<bool> SetUpAndRunTestsAsync(List<Assembly> testAssemblies)
        {
            _logger.LogInformation(LoggingEvents.RunTest, $"Running Integration Tests...");
            var setUpAndRunTestsStopwatch = Stopwatch.StartNew();

            //Get the list of all the classes that Annotates IntegrationTestFixtureAttribute.
            var testTypes = GetClasses(testAssemblies);

            foreach (var testClass in testTypes)
            {
                try
                {
                    _logger.LogDebug(LoggingEvents.RunTest, $"Running integration tests on class: {testClass.Name}...");
                    var runTestsOnClassStopwatch = Stopwatch.StartNew();

                    await RunTestOnClass(testClass);
                    _logger.LogDebug(LoggingEvents.RunTest, $"Running integration tests on class: {testClass.Name} completed in: {{0:hh\\:mm\\:ss\\.fff}}", runTestsOnClassStopwatch.Elapsed);
                }
                catch (Exception e)
                {
                    _logger.LogError(LoggingEvents.RunTest, $"{e.Message} {Environment.NewLine}Error running integration tests on class: {testClass.Name}.");
                    return false;
                }
            }
            _logger.LogInformation(LoggingEvents.RunTest, $"Running Integration Tests completed in: {{0:hh\\:mm\\:ss\\.fff}}", setUpAndRunTestsStopwatch.Elapsed);
            return true;
        }


        private async Task RunTestOnClass(Type testClass)
        {
            //Create instances of these classes.
            object instance;
            if (testClass.GetConstructor(new Type[] { }) != null)
                instance = Activator.CreateInstance(testClass);
            else
                throw new Exception($"The class {testClass.Name} has no default constructor or a parameterized constructor with 'IDatabaseOperation' as input parameter.");

            _logger.LogDebug(LoggingEvents.RunTest, $"Instance object created for Class: {testClass}");

            //Get the list of methods in each class using Reflection and custom attributes (that are marked as Test).
            var setUpMethodInfos = testClass.GetMethods().Where(m => m.GetCustomAttributes(typeof(SetUpAttribute)).Any()).ToList();
            var integrationTestMethodInfos = testClass.GetMethods().Where(m => m.GetCustomAttributes(typeof(IntegrationTestAttribute)).Any()).ToList();
            var tearDownMethodInfos = testClass.GetMethods().Where(m => m.GetCustomAttributes(typeof(TearDownAttribute)).Any()).ToList();

            _totalTests += (uint)integrationTestMethodInfos.Count;

            //Run one time setup method (if exists).
            if (await RunOneTimeSetupMethodsAsync(testClass, instance))
                _failedTestList.UnionWith(integrationTestMethodInfos.Select(t => t.Name));
            else
                await RunTestAsync(integrationTestMethodInfos, setUpMethodInfos, tearDownMethodInfos, instance);

            //Run one time Cleanup (if exists).
            if (await RunOnTimeTearDownMethodsAsync(testClass, instance))
            {
                _failedTestList.UnionWith(integrationTestMethodInfos.Select(t => t.Name));

                foreach (var integrationTestMethodInfo in integrationTestMethodInfos)
                    _passedTestList.Remove(integrationTestMethodInfo.Name);
            }
        }


        private async Task<bool> RunOneTimeSetupMethodsAsync(Type testClass, object instance)
        {
            var oneTimeSetUpMethodInfos =
                testClass.GetMethods().Where(m => m.GetCustomAttributes(typeof(OneTimeSetUpAttribute)).Any());
            bool failure = false;
            try
            {
                foreach (var oneTimeSetUpMethodInfo in oneTimeSetUpMethodInfos)
                {
                    _logger.LogDebug(LoggingEvents.RunTest, $"Setting up one time setup method: {oneTimeSetUpMethodInfo.Name}...");
                    failure = await InvokeMethod(instance, oneTimeSetUpMethodInfo);
                    _logger.LogDebug(LoggingEvents.RunTest, $"Setting up one time setup method: {oneTimeSetUpMethodInfo.Name} completed.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.RunTest, $"{e.InnerException?.Message ?? e.Message}{Environment.NewLine}Error Setting up one time setup method: {testClass.Name}.");
                SetReturnType(ExitCode.TestsFailed);
                failure = true;
            }
            return failure;
        }


        private async Task RunTestAsync(List<MethodInfo> integrationTestMethodInfos, List<MethodInfo> setUpMethodInfos, List<MethodInfo> tearDownMethodInfos, object instance)
        {
            foreach (var integrationTestMethodInfo in integrationTestMethodInfos)
            {
                // 5. Run setup method (if exists).
                // 6. Run the methods in a try catch and check if the run is successful.
                if (!RunSetupMethods(setUpMethodInfos, integrationTestMethodInfo, instance))
                    await RunTestMethodAsync(instance, integrationTestMethodInfo);

                // 7. Run Cleanup (if exists),
                RunTearDownMethods(tearDownMethodInfos, integrationTestMethodInfo, instance);
            }
        }

        private bool RunSetupMethods(List<MethodInfo> setUpMethodInfos, MemberInfo integrationTestMethodInfo, object instance)
        {
            var failure = false;
            try
            {
                setUpMethodInfos.ForEach(async i =>
                {
                    _logger.LogDebug(LoggingEvents.RunTest, $"Setting up setup method: {i.Name}...");
                    failure = await InvokeMethod(instance, i);
                    _logger.LogDebug(LoggingEvents.RunTest, $"Setting up setup method: {i.Name} completed.");
                });
            }
            catch (Exception e)
            {
                _failedTestList.Add(integrationTestMethodInfo.Name);
                _logger.LogError(LoggingEvents.RunTest,
                    $"{e.InnerException?.Message}{Environment.NewLine}Error Setting up setup method: {integrationTestMethodInfo.Name}");
                failure = true;
            }

            return failure;
        }


        private async Task RunTestMethodAsync(object instance, MethodInfo integrationTestMethodInfo)
        {
            try
            {
                _logger.LogDebug(LoggingEvents.RunTest, $"Running test method: {integrationTestMethodInfo.Name}...");
                await InvokeMethod(instance, integrationTestMethodInfo);
                _passedTestList.Add(integrationTestMethodInfo.Name);
                _logger.LogDebug(LoggingEvents.RunTest, $"Running test method: {integrationTestMethodInfo.Name} completed.");
            }
            catch (TargetInvocationException exception)
            {
                _failedTestList.Add(integrationTestMethodInfo.Name);
                _logger.LogError(LoggingEvents.TestFailed, exception.InnerException,
                    $"Integration Test Failed for: {integrationTestMethodInfo.Name}");
            }
            catch (Exception exception)
            {
                _logger.LogError(LoggingEvents.RunTest, $"Error Occured while testing: {exception}");
                SetReturnType(ExitCode.InternalError);
            }
        }


        private void RunTearDownMethods(List<MethodInfo> tearDownMethodInfos, MethodInfo integrationTestMethodInfo, object instance)
        {
            try
            {
                tearDownMethodInfos.ForEach(async i =>
                {
                    _logger.LogDebug(LoggingEvents.RunTest, $"Disposing tear down method: {i.Name}...");
                    await InvokeMethod(instance, i);
                    _logger.LogDebug(LoggingEvents.RunTest, $"Disposing tear down method: {i.Name} completed.");
                });
            }
            catch (Exception e)
            {
                _failedTestList.Add(integrationTestMethodInfo.Name);
                _logger.LogError(LoggingEvents.RunTest,
                    $"{e.InnerException?.Message}{Environment.NewLine}Error disposing: {integrationTestMethodInfo.Name}");
            }
        }


        private async Task<bool> RunOnTimeTearDownMethodsAsync(Type testClass, object instance)
        {
            var oneTimeTearDownMethodInfos =
                testClass.GetMethods().Where(m => m.GetCustomAttributes(typeof(OneTimeTearDownAttribute)).Any());
            bool failure = false;
            try
            {
                foreach (var oneTimeTearDownMethodInfo in oneTimeTearDownMethodInfos)
                {
                    _logger.LogDebug(LoggingEvents.RunTest, $"Disposing one time tear down method: {oneTimeTearDownMethodInfo.Name}...");
                    failure = await InvokeMethod(instance, oneTimeTearDownMethodInfo);
                    _logger.LogDebug(LoggingEvents.RunTest, $"Disposing one time tear down method: {oneTimeTearDownMethodInfo.Name} completed.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.RunTest, $"{e.InnerException?.Message}{Environment.NewLine}Error disposing up: {testClass.Name}");
                SetReturnType(ExitCode.TestsFailed);
                return true;
            }

            return failure;
        }


        /// <summary>
        /// Checks if the method to be invoke is awaitable.
        /// </summary>
        /// <param name="instance">instance object of the class.</param>
        /// <param name="methodInfo">methodInfo of method to be invoked.</param>
        /// <returns></returns>
        private static async Task<bool> InvokeMethod(object instance, MethodInfo methodInfo)
        {
            var isAwaitable = (AsyncStateMachineAttribute)methodInfo.GetCustomAttribute(typeof(AsyncStateMachineAttribute)) != null;
            if (isAwaitable)
            {
                if (methodInfo.ReturnType.FullName?.Equals("System.Void") == true)
                {
                    throw new Exception($"Integration Tests can't be run on 'async void' type methods.");
                }
                // ReSharper disable once PossibleNullReferenceException
                await (dynamic)methodInfo.Invoke(instance, null);
                return false;
            }
            methodInfo.Invoke(instance, null);
            return false;
        }


        private List<Type> GetClasses(IEnumerable<Assembly> testAssemblies)
        {
            _logger.LogDebug(LoggingEvents.RunTest, $"Loading Classes...");
            var logString = new StringBuilder();
            var typesForTesting = new List<Type>();

            foreach (var assembly in testAssemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.GetCustomAttributes(typeof(IntegrationTestFixtureAttribute), true).Length <= 0)
                        continue;

                    logString.Append(type.FullName + Environment.NewLine);
                    typesForTesting.Add(type);
                }
            }

            _logger.LogDebug(LoggingEvents.RunTest, $"{logString}All Classes loaded.");

            return typesForTesting;
        }


        private List<Assembly> LoadTestAssemblies()
        {
            _logger.LogInformation(LoggingEvents.LoadAssembly, $"Loading Test Assemblies...");
            var logString = new StringBuilder();
            var assembliesToTest = new List<Assembly>();
            try
            {
                var assemblies = new List<AssemblyName>();

                var files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "Grassroots.Identity.API.*.dll", SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {
                    assemblies.Add(AssemblyName.GetAssemblyName(file));
                }

                foreach (var list in assemblies)
                {
                    if (_configurationTestNamespaces.Contains(list.Name))
                    {
                        assembliesToTest.Add(Assembly.Load(list));
                        logString.Append(Environment.NewLine + list.FullName);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(LoggingEvents.InternalError, $"Error occured when loading Test Assemblies: {e}");
            }

            if (assembliesToTest.Count == 0)
            {
                _logger.LogError(LoggingEvents.LoadAssembly, $"No Test projects found.");
                SetReturnType(ExitCode.TestsDidNotRun);
            }
            else
                _logger.LogInformation(LoggingEvents.LoadAssembly, $"{logString}{Environment.NewLine} All Test assemblies loaded.");

            return assembliesToTest;
        }


        /// <summary>
        /// Provides result statistics once the tests are run.
        /// </summary>
        private void ProvideResults()
        {
            var result = new StringBuilder();
            result.Append($"Results:{Environment.NewLine}");
            result.Append($"Passed Tests:{Environment.NewLine}");

            foreach (var passedTest in _passedTestList)
            {
                result.Append(passedTest + Environment.NewLine);
            }

            result.Append($"Failed Tests:{Environment.NewLine}");

            foreach (var failedTest in _failedTestList)
            {
                result.Append(failedTest + Environment.NewLine);
            }

            result.Append($"Number of Feeds sent: {DataFeeder.NumberOfFeedsSent}{Environment.NewLine}");
            result.Append($"Number of Tests Passed: {_passedTestList.Count}{Environment.NewLine}");
            result.Append($"Number of Tests Failed: {_failedTestList.Count}{Environment.NewLine}");
            result.Append($"Number of Tests Ran: {_failedTestList.Count + _passedTestList.Count}{Environment.NewLine}");
            result.Append($"Total number of Tests: {_totalTests}");

            if (_failedTestList.Count != 0)
                SetReturnType(ExitCode.TestsFailed);

            _logger.LogInformation(LoggingEvents.ProvideResults, result.ToString());
        }


        /// <summary>
        /// Feeds embedded Xml and Json files as request to API
        /// </summary>
        /// <param name="assembly">Assembly that contains embedded files.</param>
        /// <returns></returns>
        private async Task<bool> FeedValuesFromFileAsync(Assembly assembly)
        {
            _logger.LogDebug(LoggingEvents.FeedData, $"Feeding Values before running Tests from {assembly.GetName().Name}...");
            var success = true;
            var dataFeeder = new DataFeeder(_configuration);

            foreach (var apiMConfig in _apiConfigs)
            {
                var resourceNames = assembly.GetManifestResourceNames().Where(name =>
                    name.Contains(apiMConfig.Name, StringComparison.OrdinalIgnoreCase) && name.Contains(apiMConfig.Extension) &&
                    name.Contains("BaseFeeds"));

                foreach (var resourceName in resourceNames)
                {
                    var bodyObject = dataFeeder.GetObject(assembly, resourceName);

                    if (!string.IsNullOrEmpty(bodyObject))
                    {
                        _logger.LogDebug(LoggingEvents.FeedData, $"Posting feed from {resourceName}.");

                        success = await dataFeeder.PostToApiAsync(bodyObject, apiMConfig.Url, apiMConfig.KeyName, apiMConfig.KeyValue);

                        if (success)
                            _logger.LogDebug(LoggingEvents.FeedData, $"Feed from {resourceName} Posted Successfully to: {apiMConfig.Url}");
                        else
                            _logger.LogError(LoggingEvents.FeedData, $"Posting Failed to: {apiMConfig.Url} for feed from {resourceName}.");

                        continue;
                    }

                    _logger.LogError(LoggingEvents.FeedData, $"No feed found. Error reading from {resourceName}.");
                }
            }

            if (DataFeeder.NumberOfFeedsSent == 0)
                _logger.LogWarning(LoggingEvents.FeedData, $"No data feed class Found for assembly: {assembly.FullName}.");
            else
                _logger.LogDebug(LoggingEvents.FeedData, $"Number of Feed sent: {DataFeeder.NumberOfFeedsSent}.");

            _logger.LogDebug(LoggingEvents.FeedData, $"Feeding completed for {assembly.GetName().Name}.");
            return success;
        }


        /// <summary>
        /// Changes the Exit code to indicate the overall status of the process.
        /// </summary>
        /// <param name="exitCodeType"></param>
        private static void SetReturnType(ExitCode exitCodeType)
        {
            Environment.ExitCode = (int)exitCodeType;
        }

        /// <summary>
        /// Requests termination of current process.
        /// </summary>
        private void Exit()
        {
            _logger.LogInformation(LoggingEvents.MainMethod,
                $"FINISHED RUNNING INTEGRATION TESTS IN: {{0:hh\\:mm\\:ss\\.fff}}", _integrationTestRunnerExecutionStopwatch.Elapsed);
            _appLifetime.StopApplication();
        }
    }
}