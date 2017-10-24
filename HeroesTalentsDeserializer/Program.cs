using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using HeroesTalentsDeserializer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeroesTalentsDeserializer
{
    class Program
    {
        static void Main(string[] args)
        {
            string jsonStringData;
            List<HeroData> _heroDataList = new List<HeroData>();

            foreach (var file in Directory.EnumerateFiles(@"D:\GitHub Repos\heroes-talents\hero"))
            {

                using (var fs = new FileStream(file, FileMode.Open))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        jsonStringData = sr.ReadToEnd();
                    }
                }

                dynamic jsonObj = JsonConvert.DeserializeObject(jsonStringData);

                var hero = JsonConvert.DeserializeObject<Hero>(Convert.ToString(jsonObj));
                var abilityList = new List<Ability>();
                var talentList = new List<Talent>();

                foreach (var abilityProfile in jsonObj["abilities"])
                {
                    foreach (var profile in abilityProfile)
                    {
                        foreach (var ability in profile)
                        {
                            abilityList.Add(JsonConvert.DeserializeObject<Ability>(Convert.ToString(ability)));
                        }
                    }
                }

                var talentTierNumber = 1;
                foreach (var talentTier in jsonObj["talents"])
                {
                    foreach (var talents in talentTier)
                    {
                        foreach (var talent in talents)
                        {
                            talent["talentTier"] = $"{talentTierNumber}";
                            talentList.Add(JsonConvert.DeserializeObject<Talent>(Convert.ToString(talent)));
                        }
                    }
                    ++talentTierNumber;
                }

                _heroDataList.Add(new HeroData(hero, abilityList, talentList));
            }
        }
    }
}
