using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Csla;
using Csla.Rules.CommonRules;
using Threa.Dal;

namespace GameMechanics.Player
{
    [Serializable]
    public class PlayerEdit : BusinessBase<PlayerEdit>
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

        public static readonly PropertyInfo<string> ContactEmailProperty = RegisterProperty<string>(nameof(ContactEmail));
        public string ContactEmail
        {
            get => GetProperty(ContactEmailProperty);
            set => SetProperty(ContactEmailProperty, value);
        }

        public static readonly PropertyInfo<bool> UseGravatarProperty = RegisterProperty<bool>(nameof(UseGravatar));
        public bool UseGravatar
        {
            get => GetProperty(UseGravatarProperty);
            set => SetProperty(UseGravatarProperty, value);
        }

        public static readonly PropertyInfo<string> SecretQuestionProperty = RegisterProperty<string>(nameof(SecretQuestion));
        public string SecretQuestion
        {
            get => GetProperty(SecretQuestionProperty);
            set => SetProperty(SecretQuestionProperty, value);
        }

        public static readonly PropertyInfo<string> SecretAnswerProperty = RegisterProperty<string>(nameof(SecretAnswer));
        public string SecretAnswer
        {
            get => GetProperty(SecretAnswerProperty);
            set => SetProperty(SecretAnswerProperty, value);
        }

        protected override void AddBusinessRules()
        {
            base.AddBusinessRules();

            // Display name validation
            BusinessRules.AddRule(new Required(NameProperty)
                { MessageText = "Display name is required" });
            BusinessRules.AddRule(new MinLength(NameProperty, 1)
                { MessageText = "Display name is required" });
            BusinessRules.AddRule(new MaxLength(NameProperty, 50)
                { MessageText = "Display name cannot exceed 50 characters" });
            BusinessRules.AddRule(new NoProfanityRule(NameProperty));

            // Secret question validation
            BusinessRules.AddRule(new MaxLength(SecretQuestionProperty, 200)
                { MessageText = "Secret question cannot exceed 200 characters" });
            BusinessRules.AddRule(new MaxLength(SecretAnswerProperty, 100)
                { MessageText = "Secret answer cannot exceed 100 characters" });
        }

        [Fetch]
        private async Task Fetch(int id, [Inject] IPlayerDal dal)
        {
            var data = await dal.GetPlayerAsync(id) 
                ?? throw new InvalidOperationException($"Player {id} not found");
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
                ContactEmail = data.ContactEmail;
                UseGravatar = data.UseGravatar;
                SecretQuestion = data.SecretQuestion;
                SecretAnswer = data.SecretAnswer;
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
                ImageUrl = ImageUrl,
                ContactEmail = ContactEmail,
                UseGravatar = UseGravatar,
                SecretQuestion = SecretQuestion,
                SecretAnswer = SecretAnswer
            };
            var result = await dal.SavePlayerAsync(player);
            LoadProperty(IdProperty, result.Id);
        }
    }
}
