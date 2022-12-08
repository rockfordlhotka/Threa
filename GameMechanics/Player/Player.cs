using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics.Player
{
  [Serializable]
  public class Player : BusinessBase<Player>
  {
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id
    {
      get => GetProperty(IdProperty);
      private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    [Required]
    public string Name
    {
      get => GetProperty(NameProperty);
      set => SetProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<string> EmailProperty = RegisterProperty<string>(nameof(Email));
    [Required]
    public string Email
    {
      get => GetProperty(EmailProperty);
      private set => LoadProperty(EmailProperty, value);
    }

    public static readonly PropertyInfo<string> ImageUrlProperty = RegisterProperty<string>(nameof(ImageUrl));
    public string ImageUrl
    {
      get => GetProperty(ImageUrlProperty);
      set => SetProperty(ImageUrlProperty, value);
    }

    protected override void AddBusinessRules()
    {
      base.AddBusinessRules();
    }

    [Fetch]
    private async Task Fetch(string email, [Inject] IPlayerDal dal)
    {
      var data = await dal.GetPlayerByEmailAsync(email);
      if (data == null)
      {
        Name = email;
        Email = email;
        MarkNew();
      }
      else
      {
        using (BypassPropertyChecks)
        {
          Id = data.Id;
          Name = data.Name;
          Email = data.Email;
          ImageUrl = data.ImageUrl;
        }
      }
      BusinessRules.CheckRules();
      if (IsNew)
        await this.SaveAndMergeAsync();
    }

    [Insert]
    [Update]
    private async Task SaveAsync([Inject] IPlayerDal dal)
    {
      var player = new Threa.Dal.Dto.Player
      {
        Name = Name,
        Email = Email,
        ImageUrl = ImageUrl
      };
      var result = await dal.SavePlayerAsync(player);
      using (BypassPropertyChecks)
      {
        Id = result.Id;
      }
    }
  }
}
