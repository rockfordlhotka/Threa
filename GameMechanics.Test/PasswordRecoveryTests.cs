using Microsoft.VisualStudio.TestTools.UnitTesting;
using Csla;
using Csla.Configuration;
using GameMechanics.Player;
using Microsoft.Extensions.DependencyInjection;
using Threa.Dal;

namespace GameMechanics.Test;

[TestClass]
[DoNotParallelize]
public class PasswordRecoveryTests : TestBase
{

    private async Task<int> CreateTestUser(IDataPortal<UserRegistration> registrationPortal, string username = "testuser", string secretAnswer = "test answer")
    {
        // Create a test user with known credentials
        var registration = await registrationPortal.CreateAsync();
        registration.Username = username;
        registration.Password = "password123";
        registration.SecretQuestion = "What is your test question?";
        registration.SecretAnswer = secretAnswer;
        registration = await registration.SaveAsync();
        return registration.Id;
    }

    [TestMethod]
    public async Task GetSecretQuestion_ExistingUser_ReturnsQuestion()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<PasswordRecovery>>();
        var registrationPortal = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        await ClearPlayersAsync();
        await CreateTestUser(registrationPortal, "testuser");

        // Act
        var cmd = await portal.CreateAsync();
        cmd.Username = "testuser";
        cmd = await portal.ExecuteAsync(cmd);

        // Assert
        Assert.AreEqual("What is your test question?", cmd.SecretQuestion);
        Assert.IsFalse(cmd.IsLockedOut);
        Assert.AreEqual(3, cmd.RemainingAttempts);
    }

    [TestMethod]
    public async Task GetSecretQuestion_UnknownUser_ReturnsEmptyQuestion()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<PasswordRecovery>>();

        await ClearPlayersAsync();

        // Act - no user created
        var cmd = await portal.CreateAsync();
        cmd.Username = "unknownuser";
        cmd = await portal.ExecuteAsync(cmd);

        // Assert - empty question but no error (prevent enumeration)
        Assert.AreEqual(string.Empty, cmd.SecretQuestion);
        Assert.IsFalse(cmd.IsLockedOut);
    }

    [TestMethod]
    public async Task ValidateAnswer_CorrectAnswer_ReturnsValid()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<PasswordRecovery>>();
        var registrationPortal = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        await ClearPlayersAsync();
        await CreateTestUser(registrationPortal, "testuser", "my secret");

        // Act
        var cmd = await portal.CreateAsync();
        cmd.Username = "testuser";
        cmd.SecretAnswer = "My Secret"; // Different case, extra spaces ok via trim
        cmd = await portal.ExecuteAsync(cmd);

        // Assert
        Assert.IsTrue(cmd.IsAnswerValid);
        Assert.IsFalse(cmd.IsLockedOut);
    }

    [TestMethod]
    public async Task ValidateAnswer_WrongAnswer_ReturnsInvalid()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<PasswordRecovery>>();
        var registrationPortal = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        await ClearPlayersAsync();
        await CreateTestUser(registrationPortal, "testuser", "correct answer");

        // Act
        var cmd = await portal.CreateAsync();
        cmd.Username = "testuser";
        cmd.SecretAnswer = "wrong answer";
        cmd = await portal.ExecuteAsync(cmd);

        // Assert
        Assert.IsFalse(cmd.IsAnswerValid);
        Assert.AreEqual(2, cmd.RemainingAttempts);
        Assert.IsFalse(cmd.IsLockedOut);
    }

    [TestMethod]
    public async Task ValidateAnswer_ThreeWrongAttempts_LocksOut()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<PasswordRecovery>>();
        var registrationPortal = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        await ClearPlayersAsync();
        await CreateTestUser(registrationPortal, "testuser", "correct answer");

        // Act - 3 wrong attempts
        for (int i = 0; i < 3; i++)
        {
            var cmd = await portal.CreateAsync();
            cmd.Username = "testuser";
            cmd.SecretAnswer = "wrong answer";
            cmd = await portal.ExecuteAsync(cmd);
        }

        // Assert - next attempt should show locked out
        var checkCmd = await portal.CreateAsync();
        checkCmd.Username = "testuser";
        checkCmd = await portal.ExecuteAsync(checkCmd);

        Assert.IsTrue(checkCmd.IsLockedOut);
        Assert.IsTrue(checkCmd.ErrorMessage.Contains("Too many failed attempts"));
    }

    [TestMethod]
    public async Task ResetPassword_ValidPassword_Succeeds()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<PasswordRecovery>>();
        var registrationPortal = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        await ClearPlayersAsync();
        await CreateTestUser(registrationPortal, "testuser", "secret");

        // Act
        var cmd = await portal.CreateAsync();
        cmd.Username = "testuser";
        cmd.NewPassword = "newpassword123";
        cmd = await portal.ExecuteAsync(cmd);

        // Assert
        Assert.IsTrue(cmd.PasswordResetSuccess);
        Assert.AreEqual(string.Empty, cmd.ErrorMessage);
    }

    [TestMethod]
    public async Task ResetPassword_ShortPassword_Fails()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<PasswordRecovery>>();
        var registrationPortal = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        await ClearPlayersAsync();
        await CreateTestUser(registrationPortal, "testuser", "secret");

        // Act
        var cmd = await portal.CreateAsync();
        cmd.Username = "testuser";
        cmd.NewPassword = "short"; // Less than 6 chars
        cmd = await portal.ExecuteAsync(cmd);

        // Assert
        Assert.IsFalse(cmd.PasswordResetSuccess);
        Assert.IsTrue(cmd.ErrorMessage.Contains("at least 6 characters"));
    }

    [TestMethod]
    public async Task ValidateAnswer_CaseInsensitive_Succeeds()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<PasswordRecovery>>();
        var registrationPortal = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        await ClearPlayersAsync();
        await CreateTestUser(registrationPortal, "testuser", "My Secret Answer");

        // Act - provide different case with extra spaces
        var cmd = await portal.CreateAsync();
        cmd.Username = "testuser";
        cmd.SecretAnswer = "  MY SECRET ANSWER  ";
        cmd = await portal.ExecuteAsync(cmd);

        // Assert
        Assert.IsTrue(cmd.IsAnswerValid);
    }
}
