using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using Threa.Dal.Dto;

namespace Threa.Dal.MongoDb
{
  public class Character : ICharacter
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string PlayerEmail { get; set; }
    public string Name { get; set; }
    public string TrueName { get; set; }
    public string Aliases { get; set; }
    public string Species { get; set; }
    public int DamageClass { get; set; }
    public string Height { get; set; }
    public string Weight { get; set; }
    public string Notes { get; set; }
    public string SkinDescription { get; set; }
    public string HairDescription { get; set; }
    public string Description { get; set; }
    public long Birthdate { get; set; }
    public bool IsPassedOut { get; set; }
    public double XPTotal { get; set; }
    public double XPBanked { get; set; }
    public List<IAttribute> AttributeList { get; set; } = new List<IAttribute>();
    public List<IDamage> DamageList { get; set; } = new List<IDamage>();
    public List<IWound> Wounds { get; set; } = new WoundList();
    public List<ICharacterSkill> Skills { get; set; } = new CharacterSkillList();
    public IActionPoints ActionPoints { get; set; } = new ActionPoints();
  }
}
