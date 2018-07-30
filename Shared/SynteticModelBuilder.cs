using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SyntetiskTestdataGen.Shared.DataProviders;
using SyntetiskTestdataGen.Shared.Models;
using SyntetiskTestdataGen.Shared.Resources;
using SyntetiskTestdataGen.Shared.Statistics;

namespace SyntetiskTestdataGen.Shared
{
    public class SynteticModelBuilder
    {
        private PersonStatistics _statistics;
        private Dictionary<string, PregNode> _persons;
        private IPregDataProvider _dataProvider;
        private readonly int _ageQuants;
        private System.IO.StreamWriter _file; 
        public bool DebugMode { get; set; }
        private int _numberOfSamplesToTake = 10000;
        public string SessionId { get; set; }

        public SynteticModelBuilder(IPregDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
            _persons = new Dictionary<string, PregNode>();
            _ageQuants = 5;
        }

        public void SetNewDataProvder(IPregDataProvider provder)
        {
            _dataProvider = provder;
        }

        public async Task<bool> BuildCorrelationMatrix(SynteticModel model, string sessionId)
        {
            SessionId = sessionId;

            model.Statistics.Correlations = new CorrelationMatrix(model.Statistics.CorrelationFactory.CorrelationFactoryElements.Count, model.Statistics.CorrelationFactory.CorrelationFactoryElements.Values.Select(x => x.Key).ToArray());

            int c = 0;
            Console.WriteLine("Staring import");
            while (_dataProvider.HasMore())
            {
                var p = await _dataProvider.GetNextPerson();
                if (p == null)
                    continue;

                bool birthdayWasFoundFromNin;
                var ageQuant = PersonStatistics.CalculateAgeQuant(p, _ageQuants, out birthdayWasFoundFromNin);
                if (ageQuant == PersonStatistics.InvalidQuant)
                    continue;

                model.Statistics.Correlations.Update(p, ageQuant, model.Statistics.Statistics);

                if (c++ % 10000 == 0)
                    Console.WriteLine("Done calculating correlations for " + c);
            }

            model.Statistics.Correlations.Closure(model.Statistics.CorrelationFactory);

            return true;
        }

        public async Task<SynteticModel> BuildModel(string sessionId)
        {
            SessionId = sessionId;

            _statistics = PersonStatistics.CreateRoot(SessionId, _ageQuants, _numberOfSamplesToTake, 30);

            var totalPeople = await Import();

            Console.WriteLine($"Import done (numberOfPersons={totalPeople}). Starting AfterImport");
            _statistics.AfterImport(_persons, _persons);

            Console.WriteLine("AfterImport done. Settings distributions");
            _statistics.SetDistribution(true);

            var model = new SynteticModel()
            {
                AgeQuantLevel = _ageQuants,
                Statistics = _statistics
            };

            var confirmed = _persons.Values.Count(x => x.Confirmed);
            Console.WriteLine($"Found {confirmed} people");

            return model;
        }
        
        private int _nextId = 1;
        public static int Imported = 0;
        private async Task<int> Import()
        {
            Console.WriteLine("Staring import");
            while (_dataProvider.HasMore())
            {
                Imported++;

                var p = await _dataProvider.GetNextPerson();
                if(p == null)
                    continue;

                bool birthdayWasFoundFromNin;
                var ageQuant = PersonStatistics.CalculateAgeQuant(p, _ageQuants, out birthdayWasFoundFromNin);
                if(ageQuant == PersonStatistics.InvalidQuant)
                    continue;

                var pregNode = new PregNode
                {
                    Confirmed = true,
                    AgeQuants = ageQuant,
                    BirthDay = p.DateOfBirth,
                    Kjonn = CommonFunctions.GetKjonn(p.NIN).Value,
                    MomNin = GetParent(p.MothersNIN),
                    DadNin = GetParent(p.FathersNIN),
                    DebugId = DebugMode ? p.GivenName : null,
                    MarriedNin = p.SpouseNIN,
                    Nin = p.NIN,
                    Id = _nextId++
                };

                _statistics.FromImport(p, ageQuant);

                if (pregNode.MomNin != null)
                    _persons[pregNode.MomNin].ChildNins.Add(pregNode.Nin);
                if (pregNode.DadNin != null)
                    _persons[pregNode.DadNin].ChildNins.Add(pregNode.Nin);

                if (_persons.ContainsKey(p.NIN))
                {
                    _persons[p.NIN].MomNin = pregNode.MomNin;
                    _persons[p.NIN].Confirmed = pregNode.Confirmed;
                    _persons[p.NIN].DadNin = pregNode.DadNin;
                    _persons[p.NIN].DebugId = pregNode.DebugId;
                    _persons[p.NIN].FamilyId = pregNode.FamilyId;
                    _persons[p.NIN].Nin = pregNode.Nin;
                    _persons[p.NIN].Id = pregNode.Id;
                    _persons[p.NIN].AgeQuants = pregNode.AgeQuants;
                    _persons[p.NIN].Kjonn = pregNode.Kjonn;
                }
                else
                {
                    _persons.Add(p.NIN, pregNode);
                }

                if(_persons.Count % 10000 == 0)
                    Console.WriteLine("Done importing " + _persons.Count);
                
            }

            return Imported;
        }

        private string GetParent(string parentNin)
        {
            if (string.IsNullOrEmpty(parentNin) || !NinModel.IsValidNin(parentNin))
                return null;

            if (!_persons.ContainsKey(parentNin))
                _persons.Add(parentNin, new PregNode());

            return parentNin;
        }
    }
}