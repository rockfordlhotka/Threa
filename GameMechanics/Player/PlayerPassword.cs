using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics.Player
{
    [Serializable]
    public class PlayerPassword : BusinessBase<PlayerPassword>
    {
        public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
        public int Id
        {
            get => GetProperty(IdProperty);
            private set => LoadProperty(IdProperty, value);
        }

        public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
        public string Name
        {
            get => GetProperty(NameProperty);
            private set => LoadProperty(NameProperty, value);
        }

        public static readonly PropertyInfo<string> EmailProperty = RegisterProperty<string>(nameof(Email));
        public string Email
        {
            get => GetProperty(EmailProperty);
            private set => LoadProperty(EmailProperty, value);
        }

        public static readonly PropertyInfo<string> OldPasswordProperty = RegisterProperty<string>(nameof(OldPassword));
        public string OldPassword
        {
            get => GetProperty(OldPasswordProperty);
            set => SetProperty(OldPasswordProperty, value);
        }

        public static readonly PropertyInfo<string> NewPasswordProperty = RegisterProperty<string>(nameof(NewPassword));
        [Required]
        public string NewPassword
        {
            get => GetProperty(NewPasswordProperty);
            set => SetProperty(NewPasswordProperty, value);
        }

        public static readonly PropertyInfo<string> ImageUrlProperty = RegisterProperty<string>(nameof(ImageUrl));
        public string ImageUrl
        {
            get => GetProperty(ImageUrlProperty);
            set => SetProperty(ImageUrlProperty, value);
        }

        [Fetch]
        private async Task Fetch(int id, [Inject] IPlayerDal dal)
        {
            var data = await dal.GetPlayerAsync(id);
            LoadProperties(data);
        }

        [Fetch]
        private async Task Fetch(string email, [Inject] IPlayerDal dal)
        {
            var data = await dal.GetPlayerByEmailAsync(email, NewPassword);
            LoadProperties(data);
        }

        private void LoadProperties(Threa.Dal.Dto.Player data)
        {
            using (BypassPropertyChecks)
            {
                Id = data.Id;
                Name = data.Name;
                Email = data.Email;
                ImageUrl = data.ImageUrl;
            }
            BusinessRules.CheckRules();
        }

        [Insert]
        [Update]
        private async Task SaveAsync([Inject] IPlayerDal dal)
        {
            Threa.Dal.Dto.Player? player = new()
            {
                Id = Id,
                Name = Name,
                Email = Email,
                ImageUrl = ImageUrl
            };
            if (!string.IsNullOrWhiteSpace(NewPassword))
                player.HashedPassword = NewPassword;
            var result = await dal.SavePlayerAsync(player);
            LoadProperty(IdProperty, result.Id);
            LoadProperty(NewPasswordProperty, string.Empty);
        }
    }
}
