using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;
using Microsoft.Data.SqlClient;
using Dapper;

namespace Threa.Dal.SqlServer
{
  public class CharacterDal : ICharacterDal
  {
    private readonly SqlConnection db;
    private SqlTransaction tr;

    public CharacterDal(SqlConnection db)
    {
      this.db = db;
    }

    public ICharacter GetBlank()
    {
      return new Character();
    }

    public async Task DeleteCharacterAsync(int id)
    {
      var sql = "DELETE FROM Character WHERE ID = @id";
      var affectedRows = await db.ExecuteAsync(sql, new { id });
      if (affectedRows == 0)
        throw new NotFoundException($"Character {id}");
    }

    public async Task<ICharacter> GetCharacterAsync(int id)
    {
      var sql = "SELECT * FROM Character WHERE ID = @id";
      var data = (await db.QueryAsync<Character>(sql, new { id })).FirstOrDefault();
      if (data == null)
        throw new NotFoundException($"Character {id}");
      data.AttributeList = await GetCharacterAttributes(data.Id);
      return (ICharacter)data;
    }

    public async Task<List<ICharacter>> GetCharactersAsync(int playerId)
    {
      var sql = "SELECT * FROM Character WHERE PlayerId = @playerId";
      var data = (await db.QueryAsync<Character>(sql, new { playerId })).ToList();
      foreach (var item in data)
        item.AttributeList = await GetCharacterAttributes(item.Id);
      return new List<ICharacter>(data);
    }

    private async Task<List<ICharacterAttribute>> GetCharacterAttributes(int characterId)
    {
      var attributes = await GetAttributeList();
      var sql = @"SELECT CharacterAttribute.BaseValue,CharacterAttribute.Value,Attribute.Name,Attribute.ImageUrl FROM CharacterAttribute 
                  INNER JOIN Attribute ON Attribute.Id=CharacterAttribute.AttributeId
                  WHERE CharacterAttribute.CharacterId = @id";
      var list = await db.QueryAsync<CharacterAttribute>(sql, new { id = characterId }, tr);
      return list.ToList<ICharacterAttribute>();
    }

    public async Task<ICharacter> SaveCharacter(ICharacter character)
    {
      tr = db.BeginTransaction();
      using (tr)
      {
        var sql = "SELECT * FROM Character WHERE ID = @id";
        var data = (await db.QueryAsync<Character>(sql, new { id = character.Id }, tr)).FirstOrDefault();
        if (data == null)
        {
          sql = @"INSERT INTO Character (
            [PlayerId],[Name],[TrueName],[Aliases],[Species],[DamageClass],[Height],[Weight],[Notes],[SkinDescription],
            [HairDescription],[Description],[Birthdate],[XPTotal],[XPBanked],[IsPlayable],[ActionPointMax],[ActionPointRecovery],
            [ActionPointAvailable],[IsPassedOut],[VitValue],[VitBaseValue],[VitPendingHealing],[VitPendingDamage],
            [FatValue],[FatBaseValue],[FatPendingHealing],[FatPendingDamage],[ImageUrl]) 
          VALUES (
            @PlayerId,@Name,@TrueName,@Aliases,@Species,@DamageClass,@Height,@Weight,@Notes,@SkinDescription,
            @HairDescription,@Description,@Birthdate,@XPTotal,@XPBanked,@IsPlayable,@ActionPointMax,@ActionPointRecovery,
            @ActionPointAvailable,@IsPassedOut,@VitValue,@VitBaseValue,@VitPendingHealing,@VitPendingDamage,
            @FatValue,@FatBaseValue,@FatPendingHealing,@FatPendingDamage,@ImageUrl);
          SELECT CAST(SCOPE_IDENTITY() AS INT)";
          character.Id = (await db.QueryAsync<int>(sql, GetCharacterParams(character), tr)).Single();
          if (character.Id == 0)
            throw new OperationFailedException($"Insert Character {character.Id}");
        }
        else
        {
          sql = @"UPDATE Character SET 
                [Name] = @Name,
                [TrueName] = @TrueName,
                [Aliases] = @Aliases,
                [Species] = @Species,
                [DamageClass] = @DamageClass,
                [Height] = @Height,
                [Weight] = @Weight,
                [Notes] = @Notes,
                [SkinDescription] = @SkinDescription,
                [HairDescription] = @HairDescription,
                [Description] = @Description,
                [Birthdate] = @Birthdate,
                [XPTotal] = @XPTotal,
                [XPBanked] = @XPBanked,
                [IsPlayable] = @IsPlayable,
                [ActionPointMax] = @ActionPointMax,
                [ActionPointRecovery] = @ActionPointRecovery,
                [ActionPointAvailable] = @ActionPointAvailable,
                [IsPassedOut] = @IsPassedOut,
                [VitValue] = @VitValue,
                [VitBaseValue] = @VitBaseValue,
                [VitPendingHealing] = @VitPendingHealing,
                [VitPendingDamage] = @VitPendingDamage,
                [FatValue] = @FatValue,
                [FatBaseValue] = @FatBaseValue,
                [FatPendingHealing] = @FatPendingHealing,
                [FatPendingDamage] = @FatPendingDamage,
                [ImageUrl] = @ImageUrl
              WHERE Id = @id";
          var affectedRows = await db.ExecuteAsync(sql, GetCharacterParams(character), tr);
          if (affectedRows == 0)
            throw new OperationFailedException($"Update Character {character.Id}");
        }
        await UpdateAttributes(character);
        tr.Commit();
      }
      return character;
    }

    private IEnumerable<Attribute> attributeList;
    private async Task<IEnumerable<Attribute>> GetAttributeList()
    {
      if (attributeList == null)
      {
        var sql = "SELECT Id,Name FROM Attribute";
        attributeList = await db.QueryAsync<Attribute>(sql, null, tr);
      }
      return attributeList;
    }

    private async Task UpdateAttributes(ICharacter character)
    {
      var attributes = await GetAttributeList();

      var sql = "SELECT * FROM CharacterAttribute WHERE CharacterId = @id";
      var charAttributes = await GetCharacterAttributes(character.Id);
      foreach (var item in character.AttributeList)
      {
        var data = charAttributes.Where(r => r.Name == item.Name).FirstOrDefault();
        if (data == null)
        {
          sql = @"INSERT INTO CharacterAttribute (
            [CharacterId],[AttributeId],[BaseValue],[Value]) 
          VALUES (
            @CharacterId,@AttributeId,@BaseValue,@Value);";
        }
        else
        {
          sql = @"UPDATE CharacterAttribute SET 
                [BaseValue] = @BaseValue,
                [Value] = @Value
              WHERE CharacterId = @CharacterId AND AttributeId = @AttributeId";
        }
        var attribute = attributes.Where(r => r.Name == item.Name).First();
        var affectedRows = await db.ExecuteAsync(sql,
          new { CharacterId = character.Id, AttributeId = attribute.Id, BaseValue = item.BaseValue, Value = item.Value }, tr);
        if (affectedRows == 0)
          throw new OperationFailedException($"Update CharacterAttribute {character.Id}-{attribute.Name}");
      }
    }

    private object GetCharacterParams(ICharacter character)
    {
      return new
      {
        character.PlayerId,
        character.Name,
        character.TrueName,
        character.Aliases,
        character.Species,
        character.DamageClass,
        character.Height,
        character.Weight,
        character.Notes,
        character.SkinDescription,
        character.HairDescription,
        character.Description,
        character.Birthdate,
        character.XPTotal,
        character.XPBanked,
        character.IsPlayable,
        character.ActionPointMax,
        character.ActionPointRecovery,
        character.ActionPointAvailable,
        character.IsPassedOut,
        character.VitValue,
        character.VitBaseValue,
        character.VitPendingHealing,
        character.VitPendingDamage,
        character.FatValue,
        character.FatBaseValue,
        character.FatPendingHealing,
        character.FatPendingDamage,
        character.ImageUrl,
        character.Id
      };
    }
  }
}
