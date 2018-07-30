using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SyntetiskTestdataGen.Shared.Resources
{
    public class NameRandomzier
    {
        private readonly List<Tuple<int, string>> _maleIndexes = new List<Tuple<int, string>>();
        private readonly List<Tuple<int, string>> _femaleIndexes = new List<Tuple<int, string>>();
        private readonly List<Tuple<int, string>> _sirIndexes = new List<Tuple<int, string>>();
        private readonly int _male;
        private readonly int _female;
        private readonly int _sirname;

        public NameRandomzier(string[] path, bool sirnameHasThreeColumns)
        {
            _female = GenerateList(path[0], _femaleIndexes);
            _male = GenerateList(path[1], _maleIndexes);
            _sirname = sirnameHasThreeColumns ? GenerateList(path[2], _sirIndexes, 1) : GenerateList(path[2], _sirIndexes);
        }

        private int GenerateList(string path, List<Tuple<int, string>> list, int nameIndex=0)
        {
            var i = 0;

            using (StreamReader sr = GetEmbeddedResource(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var array = line.Split(';').ToList();
                    if(!array.Any())
                        continue;
                    i += int.Parse(array.FindLast(l => !string.IsNullOrEmpty(l) && l.All(char.IsDigit)) ?? "200");
                    list.Add(new Tuple<int, string>(i, array[nameIndex]));
                }
            }
            return i;
        }

        private static StreamReader GetEmbeddedResource(string filename)
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            string path = "SyntetiskTestdataGen.Shared";

            string encodingName = $"ISO-8859-1";
            var encoding = Encoding.GetEncoding(encodingName);

            var s = thisAssembly.GetManifestResourceStream(path + ".Resources." + filename);
            return new StreamReader(s, encoding);
        }

        public string NextFemaleFirstname(Randomizer randyRandomizer)
        {
            return GetName(randyRandomizer, _femaleIndexes, _female);
        }

        public string NextFemaleMiddlename(string firstname, Randomizer randyRandomizer)
        {
            return GetName(randyRandomizer, _sirIndexes, _sirname);
        }

        public string NextMaleMiddlename(string firstname, Randomizer randyRandomizer)
        {
            return GetName(randyRandomizer, _sirIndexes, _sirname);
        }

        public string NextMaleFirstname(Randomizer randyRandomizer)
        {
            return GetName(randyRandomizer, _maleIndexes, _male);
        }

        public string NextSirname(Randomizer randyRandomizer)
        {
            return GetName(randyRandomizer, _sirIndexes, _sirname);
        }

        public string NextSirname(bool doublename, Randomizer randyRandomizer)
        {
            var name = GetName(randyRandomizer, _sirIndexes, _sirname);
            if (!doublename)
                return name;

            var withMinus = randyRandomizer.Hit(30);
            var nextname = GetName(randyRandomizer, _sirIndexes, _sirname);

            return withMinus ? name + "-" + nextname : name + " " + nextname;
        }

        private string GetName(Randomizer randyRandomizer, List<Tuple<int, string>> index, int maxValue, string notEqual = "")
        {
            if (!string.IsNullOrEmpty(notEqual))
            {
                while (true)
                {
                    var name = index.First(x => x.Item1 >= randyRandomizer.Next(maxValue)).Item2;
                    if (name != notEqual && !name.Contains(notEqual) && !notEqual.Contains(name))
                        return name;
                }
            }

            var i = randyRandomizer.Next(maxValue);

            return index.First(x => x.Item1 >= i).Item2;
        }

    }
}
