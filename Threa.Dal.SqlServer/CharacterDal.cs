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
      if (data != null)
        throw new NotFoundException($"Character {id}");
      return (ICharacter)data;
    }

    public async Task<List<ICharacter>> GetCharactersAsync(int playerId)
    {
      var sql = "SELECT * FROM Character WHERE PlayerId = @playerId";
      var data = (await db.QueryAsync<Character>(sql, new { playerId })).ToList();
      return new List<ICharacter>(data);
    }

    public async Task<ICharacter> SaveCharacter(ICharacter character)
    {
      var sql = "SELECT * FROM Character WHERE ID = @id";
      var data = (await db.QueryAsync<Character>(sql, new { id = character.Id })).FirstOrDefault();
      if (data != null)
      {
        sql = @"INSERT INTO Character (
            [Name],[TrueName],[Aliases],[Species],[DamageClass],[Height],[Weight],[Notes],[SkinDescription],
            [HairDescription],[Description],[Birthdate],[XPTotal],[XPBanked],[IsPlayable],[ActionPointMax],[ActionPointRecovery],
            [ActionPointAvailable],[IsPassedOut],[VitValue],[VitBaseValue],[VitPendingHealing],[VitPendingDamage],
            [FatValue],[FatBaseValue],[FatPendingHealing],[FatPendingDamage],[ImageUrl]) 
          VALUES (
            @Name,@TrueName,@Aliases,@Species,@DamageClass,@Height,@Weight,@Notes,@SkinDescription,
            @HairDescription,@Description,@Birthdate,@XPTotal,@XPBanked,@IsPlayable,@ActionPointMax,@ActionPointRecovery,
            @ActionPointAvailable,@IsPassedOut,@VitValue,@VitBaseValue,@VitPendingHealing,@VitPendingDamage,
            @FatValue,@FatBaseValue,@FatPendingHealing,@FatPendingDamage,@ImageUrl);
          SELECT CAST(SCOPE_IDENTITY() AS INT)";
        character.Id = (await db.QueryAsync<int>(sql, GetParams(character))).Single();
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
        var affectedRows = await db.ExecuteAsync(sql, GetParams(character));
        if (affectedRows == 0)
          throw new OperationFailedException($"Update Character {character.Id}");
      }
      return character;
    }

    private object GetParams(ICharacter character)
    {
      return new
      {
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
