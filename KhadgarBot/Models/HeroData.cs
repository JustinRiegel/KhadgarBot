//using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace KhadgarBot.Models
//{
//    public class HeroData : DbContext
//    {
//        public HeroData(Hero hero, List<Ability> abilities, List<Talent> talents)
//        {
//            Hero = hero;
//            Abilities = abilities;
//            Talents = talents;
//        }
//        public Hero Hero { get; }
//        public List<Ability> Abilities { get; }
//        public List<Talent> Talents { get; }

//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            optionsBuilder.UseSqlite("Data Source=KhadgarBot.db");
//        }
//    }

//    public class Hero
//    {
//        [JsonProperty(PropertyName = "id")]
//        public int HeroId { get; set; }

//        [JsonProperty(PropertyName = "shortName")]
//        public string ShortName { get; set; }

//        [JsonProperty(PropertyName = "name")]
//        public string Name { get; set; }

//        [JsonProperty(PropertyName = "attributeId")]
//        public string AttributeId { get; set; }

//        [JsonProperty(PropertyName = "role")]
//        public string Role { get; set; }

//        [JsonProperty(PropertyName = "type")]
//        public string Type { get; set; }

//        [JsonProperty(PropertyName = "releaseDate")]
//        public DateTime ReleaseDate { get; set; }
//    }

//    public class Ability
//    {
//        [JsonProperty(PropertyName = "name")]
//        public string Name { get; set; }

//        [JsonProperty(PropertyName = "description")]
//        public string Description { get; set; }

//        [JsonProperty(PropertyName = "hotkey")]
//        public string Hotkey { get; set; }

//        [JsonProperty(PropertyName = "abilityId")]
//        public string AbilityId { get; set; }

//        [JsonProperty(PropertyName = "cooldown")]
//        public string Cooldown { get; set; }

//        [JsonProperty(PropertyName = "manaCost")]
//        public string ManaCost { get; set; }

//        [JsonProperty(PropertyName = "trait")]
//        public bool Trait { get; set; }
//    }

//    public class Talent
//    {
//        [JsonProperty(PropertyName = "tooltipId")]
//        public string TooltipId { get; set; }

//        [JsonProperty(PropertyName = "talentTreeId")]
//        public string TalentTreeId { get; set; }

//        [JsonProperty(PropertyName = "name")]
//        public string Name { get; set; }

//        [JsonProperty(PropertyName = "description")]
//        public string Description { get; set; }

//        [JsonProperty(PropertyName = "icon")]
//        public string Icon { get; set; }

//        [JsonProperty(PropertyName = "sort")]
//        public int Sort { get; set; }

//        [JsonProperty(PropertyName = "abilityId")]
//        public string AbilityId { get; set; }

//        [JsonProperty(PropertyName = "talentTier")]
//        public string TalentTier { get; set; }
//    }
//}
