using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics
{
  [Serializable]
  public class Player : BusinessBase<Player>
  {
    public static readonly PropertyInfo<string> IdProperty = RegisterProperty<string>(nameof(Id));
    public string Id
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
      set => SetProperty(EmailProperty, value);
    }

    protected override void AddBusinessRules()
    {
      base.AddBusinessRules();
      BusinessRules.AddRule(new Csla.Rules.CommonRules.RegExMatch(
        EmailProperty, 
        "^(?(\")(\".+?(?<!\\\\)\"@)|(([0-9a-z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-z][-\\w]*[0-9a-z]*\\.)+[a-z0-9][\\-a-z0-9]{0,22}[a-z0-9]))$"));
    }

    [Create]
    [RunLocal]
    private void Create()
    {
      Create(string.Empty);
    }

    [Create]
    [RunLocal]
    private void Create(string name)
    {
      Name = name;
      BusinessRules.CheckRules();
    }

    [Fetch]
    private async Task Fetch(string email, [Inject] IPlayerDal dal)
    {
      var data = await dal.GetPlayerByEmailAsync(email);
      if (data == null)
      {
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
        }
      }
      BusinessRules.CheckRules();
    }
  }
}
