using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SyntetiskTestdataGen.Build.Pushers;
using SyntetiskTestdataGen.Shared;
using SyntetiskTestdataGen.Shared.DataProviders;
using SyntetiskTestdataGen.Shared.DbEntities;
using SyntetiskTestdataGen.Shared.Models;
using SyntetiskTestdataGen.Shared.Resources;
using SyntetiskTestdataGen.Shared.Statistics;
using SyntetiskTestdataGen.Shared.Statistics.Properties;
using SyntetiskTestdataGen.Shared.Utils;

namespace SyntetiskTestdataGen.Build
{
    public class PregEngine
    {
        private CancellationTokenSource _cancellationTokenSource;
        private readonly string _logFilePath;
        private readonly SynteticModel _model;
        private IPushPregData _pusherPregData;
        public IdentifierDublicateControl IdControl { get; set; }

        public PregEngine(PregEngineConfiguration configuration)
        {
            _model = JsonConvert.DeserializeObject<SynteticModel>(File.ReadAllText(configuration.ModelFilePath));
            IdControl = new IdentifierDublicateControl();
            _logFilePath = configuration.LogfilePath;
        }

        public PregEngine(SynteticModel model)
        {
            _model = model;
            IdControl = new IdentifierDublicateControl();
        }

        public void SetPusher(IPushPregData pusherPregData)
        {
            _pusherPregData = pusherPregData;
        }

        private static void BuildDistributionModels(SynteticModel model)
        {
            BuildDistributionModel(model.Statistics, -1);

            foreach (var statforAgeQuant in model.Statistics.StatisticsByAgeQuants.Values)
            {
                BuildDistributionModel(statforAgeQuant, statforAgeQuant.AgeQuantLevel);
            }
        }

        private static void BuildDistributionModel(PersonStatistics statistics, int? ageQuant)
        {
            BuildDistributionModels(statistics, ageQuant);
            BuildDistributionModels(statistics.RelationshipStatistics, ageQuant);
            BuildDistributionModels(statistics.SivilstatusStatistics, ageQuant);
            BuildDistributionModels(statistics.RegstatusStatistics, ageQuant);
            BuildDistributionModels(statistics.NameStatistics, ageQuant);
            BuildDistributionModels(statistics.AdressStatistics, ageQuant);
        }

        private static void BuildDistributionModels<T>(T inst, int? ageQuant)
        {
            if (Equals(inst, default(T)))
                return;

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo p in properties)
            {
                if (p.PropertyType != typeof(Statistic))
                    continue;

                var stat = (Statistic) p.GetValue(inst);
                var report = stat.SetDistributionForBuilding(p.Name, ageQuant);

                if(!string.IsNullOrEmpty(report))
                    Console.WriteLine(report);
            }
        }

        public async Task<int> Do(int howMany, int numberOfThreads = 2, int bunchSize = 10000)
        {
            Outputter.LogFilePath = _logFilePath;
            int created = 0;
            _cancellationTokenSource = new CancellationTokenSource();

            var errorSignalTask = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    if (QueuePregPusher.UnexceptedQuit)
                        return;

                    await Task.Delay(50000);
                }
            }, _cancellationTokenSource.Token);

            try
            {
                var creatingTask = DoIt(howMany, numberOfThreads, bunchSize);
                await Task.WhenAny(creatingTask, errorSignalTask);

                _cancellationTokenSource.Cancel();

                if (creatingTask.IsFaulted)
                {
                    var result = creatingTask.Result;
                }

                await Task.WhenAll(creatingTask, errorSignalTask);

                created = creatingTask.Result;
            }
            catch (Exception e)
            {
                var inner = e.InnerException != null ? e.InnerException.Message + ". Stack: " + e.InnerException.StackTrace : "";
                Outputter.WriteLine("In Do: got fatal exception: " + e.Message + e.StackTrace + inner + ". Exiting");
            }
            finally
            {
                Outputter.Close();
            }

            return created;
        }

        private bool _holdOn = false;
        private async Task<int> DoIt(int howMany, int numberOfThreads = 2, int bunchSize = 10000)
        {
            var saveTask = _pusherPregData.Save();

            var tasks = StartExporterTasks(_cancellationTokenSource.Token, howMany, numberOfThreads, bunchSize);

            var surveillanceTask = Task.Run(async () =>
            {
                var start = DateTime.Now;

                int previousNumberOfCreated = _pusherPregData.NumberOfCreated();
                int preivousNumberOfHandled = _pusherPregData.NumberOfWritten();

                int handlingSum = 0;
                int creatingSum = 0;
                int count = 0;

                int secondsBetween = 10;

                while (!_pusherPregData.IsDone())
                {
                    if (QueuePregPusher.UnexceptedQuit)
                    {
                        Outputter.WriteLine("ERROR: QueuePregPusher.UnexceptedQuit=true, which means that the pusher is dead and the whole thing is in fatal state");
                        return;
                    }

                    count++;
                    int now = _pusherPregData.QueueLength();

                    int nowNumberOfCreated = _pusherPregData.NumberOfCreated();
                    int nowNumberOfHandled = _pusherPregData.NumberOfWritten();

                    handlingSum += (nowNumberOfHandled - preivousNumberOfHandled) / secondsBetween;
                    creatingSum += (nowNumberOfCreated - previousNumberOfCreated) / secondsBetween;

                    var handlingRate = handlingSum / count;
                    var creatingRate = creatingSum / count;

                    var diff = nowNumberOfCreated - nowNumberOfHandled;

                    Outputter.WriteLine($"[DB]: Current saveQueueLength is {now}. Number of created is {nowNumberOfCreated} ({creatingRate} p/s), number of handled is {nowNumberOfHandled} ({handlingRate} p/s)");

                    previousNumberOfCreated = nowNumberOfCreated;
                    preivousNumberOfHandled = nowNumberOfHandled;

                    if (now > 5 * bunchSize)
                    {
                        _holdOn = true;
                        Outputter.WriteLine($"[DB]: Stopping creating-tasks to clear queue");
                    }
                    else
                    {
                        if (_holdOn)
                        {
                            Outputter.WriteLine($"[DB]: Starting creating-tasks again");
                            _holdOn = false;
                        }
                    }

                    if (handlingRate > 0)
                    {
                        var timeSuffered = DateTime.Now.Subtract(start).TotalMinutes;
                        var remaining = howMany - nowNumberOfHandled;
                        var remainingTimeInSeconds = (1.0 / ((double)handlingRate)) * ((double)remaining);
                        var eta = DateTime.Now.AddSeconds(remainingTimeInSeconds);
                        Outputter.WriteLine("ETA: " + eta.ToLongTimeString());
                    }

                    if (_cancellationTokenSource.IsCancellationRequested)
                        return;

                    await Task.Delay(secondsBetween * 1000, _cancellationTokenSource.Token);
                }
            }, _cancellationTokenSource.Token);

            await Task.WhenAll(tasks);

            Outputter.WriteLine("Constructing threads done. Waiting for save task");

            _pusherPregData.DonePushing();
            await Task.WhenAll(saveTask, surveillanceTask);

            Outputter.WriteLine("Save-tasks done");

            return _pusherPregData.NumberOfCreated();
        }

        public SynteticModel GetSynteticModel()
        {
            return _model;
        }

        private List<Task> StartExporterTasks(CancellationToken cancelToken, int howMany, int numberOfThreads, int bunchSize)
        {
            var testWorkers = new List<Task>();

            if (howMany <= bunchSize)
            {
                numberOfThreads = 1;
                bunchSize = howMany;
            }

            int howManyIterationPerThread = Math.Max(((howMany / numberOfThreads) / bunchSize), 1);

            Outputter.WriteLine($"Using numberOfThreads {numberOfThreads} and bunchSize {bunchSize}");

            for (int i = 0; i < numberOfThreads; i++)
            {
                testWorkers.Add(CreateTask(i, cancelToken, howManyIterationPerThread, bunchSize));
            }

            return testWorkers;
        }

        private Task CreateTask(int number, CancellationToken cancelToken, int howManyIterationPerThread, int bunchSize)
        {
            return Task.Run(async () =>
            {
                var sessionId = number.ToString();
                var modelClone = _model.Clone();
                BuildDistributionModels(modelClone);

                var builder = new SynteticDataBuilderV1(modelClone, _pusherPregData, IdControl);

                Outputter.WriteLine($"[Thread: {sessionId}]: starting");

                for (int j = 0; j < howManyIterationPerThread; j++)
                {
                    builder.Do(sessionId, bunchSize);
                    Outputter.WriteLine($"[Thread: {sessionId}]: done with iteration {j} of {howManyIterationPerThread - 1}");

                    bool hasWritten = false;
                    bool hasHolded = false;
                    while (_holdOn)
                    {
                        hasHolded = true;
                        if (!hasWritten)
                            Outputter.WriteLine($"[Thread: {sessionId}]: holding .. ");

                        hasWritten = true;
                        await Task.Delay(1000, cancelToken);
                    }
                    if (hasHolded)
                        Outputter.WriteLine($"[Thread: {sessionId}]: starting again .. ");
                }
            }, cancelToken);
        }

        public async Task<int> ImportStaticData(HashSet<string> staticNinList, IPregDataProvider staticdataSource, IPushPregData targetPusher)
        {
            var person = await staticdataSource.GetNextPerson();
            while (staticdataSource.HasMore())
            {
                if (staticNinList.Contains(person.NIN))
                {
                    if (person.NewNIN == "")
                        person.NewNIN = null;

                    if (person.OldNIN == "")
                        person.OldNIN = null;

                    targetPusher.AddToSaveQueue(new List<Person> { person });
                    var wasAdded = IdControl.TakenAdd(person.NIN);
                    if(!wasAdded)
                        throw new Exception("TakenAdd feilet. dette skal ikke skje siden input liste er kun unike. Sjekk at database var tom initielt");

                    staticNinList.Remove(person.NIN);
                }

                person = await staticdataSource.GetNextPerson();
            }

            Outputter.WriteLine("Done importing static data. Number of object not found in source was " + staticNinList.Count + ". Items are put on save queue. Length:" + targetPusher.QueueLength());

            return staticNinList.Count;
        }
    }
}
