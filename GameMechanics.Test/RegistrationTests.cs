using Csla;
using Csla.Configuration;
using GameMechanics.Player;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

/// <summary>
/// Unit tests for the UserRegistration business object.
/// Tests validation rules and first-user-admin logic.
/// </summary>
[TestClass]
public class RegistrationTests : TestBase
{

    #region Validation Tests

    [TestMethod]
    public void Create_HasBrokenRules_WhenFieldsEmpty()
    {
        var provider = InitServices(seedData: false);
        var dp = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        var registration = dp.Create();

        Assert.IsFalse(registration.IsValid, "Registration should be invalid when all fields are empty");
        Assert.IsFalse(registration.IsSavable, "Registration should not be savable when invalid");

        var brokenRules = registration.BrokenRulesCollection;
        Assert.IsTrue(brokenRules.Any(r => r.Property == "Username"), "Should have broken rule for Username");
        Assert.IsTrue(brokenRules.Any(r => r.Property == "Password"), "Should have broken rule for Password");
        Assert.IsTrue(brokenRules.Any(r => r.Property == "SecretQuestion"), "Should have broken rule for SecretQuestion");
        Assert.IsTrue(brokenRules.Any(r => r.Property == "SecretAnswer"), "Should have broken rule for SecretAnswer");
    }

    [TestMethod]
    public void Create_HasBrokenRules_WhenPasswordTooShort()
    {
        var provider = InitServices(seedData: false);
        var dp = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        var registration = dp.Create();
        registration.Username = "testuser";
        registration.Password = "12345"; // Only 5 chars, need 6
        registration.SecretQuestion = "What is your pet's name?";
        registration.SecretAnswer = "Fluffy";

        Assert.IsFalse(registration.IsValid, "Registration should be invalid with short password");

        var passwordRules = registration.BrokenRulesCollection.Where(r => r.Property == "Password").ToList();
        Assert.IsTrue(passwordRules.Any(), "Should have broken rule for Password");
        Assert.IsTrue(passwordRules.Any(r => r.Description.Contains("6")), "Error message should mention minimum 6 characters");
    }

    [TestMethod]
    public void Create_HasBrokenRules_WhenUsernameTooShort()
    {
        var provider = InitServices(seedData: false);
        var dp = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        var registration = dp.Create();
        registration.Username = "ab"; // Only 2 chars, need 3
        registration.Password = "password123";
        registration.SecretQuestion = "What is your pet's name?";
        registration.SecretAnswer = "Fluffy";

        Assert.IsFalse(registration.IsValid, "Registration should be invalid with short username");

        var usernameRules = registration.BrokenRulesCollection.Where(r => r.Property == "Username").ToList();
        Assert.IsTrue(usernameRules.Any(), "Should have broken rule for Username");
        Assert.IsTrue(usernameRules.Any(r => r.Description.Contains("3")), "Error message should mention minimum 3 characters");
    }

    [TestMethod]
    public void Create_IsSavable_WhenAllFieldsValid()
    {
        var provider = InitServices(seedData: false);
        var dp = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        var registration = dp.Create();
        registration.Username = "validuser";
        registration.Password = "password123";
        registration.SecretQuestion = "What is your pet's name?";
        registration.SecretAnswer = "Fluffy";

        Assert.IsTrue(registration.IsValid, "Registration should be valid with all fields filled");
        Assert.IsTrue(registration.IsSavable, "Registration should be savable when valid");
        Assert.AreEqual(0, registration.BrokenRulesCollection.Count, "Should have no broken rules");
    }

    [TestMethod]
    public void Create_HasBrokenRules_WhenUsernameTooLong()
    {
        var provider = InitServices(seedData: false);
        var dp = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        var registration = dp.Create();
        registration.Username = new string('a', 51); // 51 chars, max is 50
        registration.Password = "password123";
        registration.SecretQuestion = "What is your pet's name?";
        registration.SecretAnswer = "Fluffy";

        Assert.IsFalse(registration.IsValid, "Registration should be invalid with too long username");

        var usernameRules = registration.BrokenRulesCollection.Where(r => r.Property == "Username").ToList();
        Assert.IsTrue(usernameRules.Any(), "Should have broken rule for Username");
    }

    [TestMethod]
    public void Create_IsValid_WithBoundaryLengths()
    {
        var provider = InitServices(seedData: false);
        var dp = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        var registration = dp.Create();
        registration.Username = "abc"; // Exactly 3 chars (minimum)
        registration.Password = "123456"; // Exactly 6 chars (minimum)
        registration.SecretQuestion = "Q?";
        registration.SecretAnswer = "A";

        Assert.IsTrue(registration.IsValid, "Registration should be valid at boundary lengths");
    }

    #endregion

    #region Insert Tests (Integration with SQLite)

    [TestMethod]
    public async Task Insert_FirstUser_BecomesAdministrator()
    {
        var provider = InitServices(seedData: false);
        var dp = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        var registration = dp.Create();
        var uniqueUsername = $"firstadmin_{Guid.NewGuid():N}";
        registration.Username = uniqueUsername;
        registration.Password = "password123";
        registration.SecretQuestion = "What city were you born in?";
        registration.SecretAnswer = "New York";

        var saved = await registration.SaveAsync();

        Assert.IsTrue(saved.Id >= 0, "Should have assigned an ID after save");

        // Verify in database
        var player = await GetPlayerByEmailAsync(uniqueUsername);
        Assert.IsNotNull(player, "Player should exist in database");
        Assert.AreEqual(Roles.Administrator, player.Roles, "First user should be Administrator");
    }

    [TestMethod]
    public async Task Insert_SubsequentUser_BecomesPlayer()
    {
        var provider = InitServices(seedData: false);
        var dp = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        // Add a first user (admin)
        var savedAdmin = await AddPlayerAsync(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "existingadmin",
            Name = "Existing Admin",
            Roles = Roles.Administrator,
            HashedPassword = "dummy",
            Salt = "dummy"
        });

        var registration = dp.Create();
        registration.Username = "newplayer";
        registration.Password = "password123";
        registration.SecretQuestion = "What is your favorite color?";
        registration.SecretAnswer = "Blue";

        var saved = await registration.SaveAsync();

        // Verify in database
        var player = await GetPlayerByEmailAsync("newplayer");
        Assert.IsNotNull(player, "Player should exist in database");
        Assert.AreEqual(Roles.Player, player.Roles, "Subsequent user should be Player");
    }

    [TestMethod]
    public async Task Insert_DuplicateUsername_ThrowsException()
    {
        var provider = InitServices(seedData: false);
        var dp = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        // Add an existing user
        var savedUser = await AddPlayerAsync(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "existinguser",
            Name = "Existing User",
            Roles = Roles.Player,
            HashedPassword = "dummy",
            Salt = "dummy"
        });

        var registration = dp.Create();
        registration.Username = "existinguser"; // Same as existing
        registration.Password = "password123";
        registration.SecretQuestion = "What is your pet's name?";
        registration.SecretAnswer = "Rex";

        DataPortalException? ex = null;
        try
        {
            _ = await registration.SaveAsync();
            Assert.Fail("Expected DataPortalException for duplicate username");
        }
        catch (DataPortalException dpex)
        {
            ex = dpex;
        }

        Assert.IsNotNull(ex, "Should have thrown DataPortalException");
        Assert.IsInstanceOfType(ex.BusinessException, typeof(DuplicateKeyException),
            "Should throw DuplicateKeyException for duplicate username");
        Assert.IsTrue(ex.BusinessException.Message.Contains("existinguser"),
            "Exception message should contain the duplicate username");
    }

    [TestMethod]
    public async Task Insert_PasswordIsHashed()
    {
        var provider = InitServices(seedData: false);
        var dp = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        var registration = dp.Create();
        registration.Username = "hashtest";
        registration.Password = "myplainpassword";
        registration.SecretQuestion = "Secret?";
        registration.SecretAnswer = "Answer";

        _ = await registration.SaveAsync();

        var player = await GetPlayerByEmailAsync("hashtest");
        Assert.IsNotNull(player, "Player should exist in database");
        Assert.AreNotEqual("myplainpassword", player.HashedPassword, "Password should be hashed, not stored plain");
        Assert.IsTrue(player.HashedPassword.StartsWith("$2"), "Hashed password should be BCrypt format");
        Assert.IsFalse(string.IsNullOrEmpty(player.Salt), "Salt should be stored");
    }

    [TestMethod]
    public async Task Insert_SecretAnswerIsNormalized()
    {
        var provider = InitServices(seedData: false);
        var dp = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        var registration = dp.Create();
        registration.Username = "normalizetest";
        registration.Password = "password123";
        registration.SecretQuestion = "What is your pet's name?";
        registration.SecretAnswer = "  FLUFFY  "; // Uppercase with whitespace

        _ = await registration.SaveAsync();

        var player = await GetPlayerByEmailAsync("normalizetest");
        Assert.IsNotNull(player, "Player should exist in database");
        Assert.AreEqual("fluffy", player.SecretAnswer, "Secret answer should be trimmed and lowercase");
    }

    #endregion
}
