using Csla;
using Csla.Configuration;
using GameMechanics.Player;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.MockDb;

namespace GameMechanics.Test;

/// <summary>
/// Unit tests for UserRegistration business object.
/// Tests username validation rules.
/// </summary>
[TestClass]
public class UserRegistrationTests
{
    private ServiceProvider InitServices()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddCsla();
        services.AddMockDb();
        return services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task UsernameWithSpaces_ShouldFail()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        // Act
        var registration = await portal.CreateAsync();
        registration.Username = "user name";
        registration.Password = "password123";
        registration.SecretQuestion = "Question?";
        registration.SecretAnswer = "Answer";

        // Assert
        Assert.IsFalse(registration.IsSavable, "Should not be savable when username contains spaces");
        Assert.IsTrue(registration.BrokenRulesCollection.Any(r =>
            r.Description.Contains("cannot contain spaces")),
            "Should have broken rule about spaces in username");
    }

    [TestMethod]
    public async Task UsernameWithoutSpaces_ShouldPass()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        MockDb.Players.Clear(); // Ensure no duplicate username

        // Act
        var registration = await portal.CreateAsync();
        registration.Username = "username123";
        registration.Password = "password123";
        registration.SecretQuestion = "Question?";
        registration.SecretAnswer = "Answer";

        // Assert
        Assert.IsTrue(registration.IsSavable, "Should be savable when username has no spaces");
        Assert.IsFalse(registration.BrokenRulesCollection.Any(r =>
            r.Description.Contains("cannot contain spaces")),
            "Should not have broken rule about spaces in username");
    }

    [TestMethod]
    public async Task UsernameWithEmailFormat_ShouldPass()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        MockDb.Players.Clear(); // Ensure no duplicate username

        // Act
        var registration = await portal.CreateAsync();
        registration.Username = "user@example.com";
        registration.Password = "password123";
        registration.SecretQuestion = "Question?";
        registration.SecretAnswer = "Answer";

        // Assert
        Assert.IsTrue(registration.IsSavable, "Should be savable when username is email format");
        Assert.IsFalse(registration.BrokenRulesCollection.Any(r =>
            r.Description.Contains("cannot contain spaces")),
            "Should not have broken rule about spaces in username");
    }

    [TestMethod]
    public async Task UsernameWithSymbols_ShouldPass()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        MockDb.Players.Clear(); // Ensure no duplicate username

        // Act
        var registration = await portal.CreateAsync();
        registration.Username = "user+name_123-test@example.com";
        registration.Password = "password123";
        registration.SecretQuestion = "Question?";
        registration.SecretAnswer = "Answer";

        // Assert
        Assert.IsTrue(registration.IsSavable, "Should be savable when username has valid symbols");
        Assert.IsFalse(registration.BrokenRulesCollection.Any(r =>
            r.Description.Contains("cannot contain spaces")),
            "Should not have broken rule about spaces in username");
    }

    [TestMethod]
    public async Task UsernameWithMultipleSpaces_ShouldFail()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<UserRegistration>>();

        // Act
        var registration = await portal.CreateAsync();
        registration.Username = "user  name  test";
        registration.Password = "password123";
        registration.SecretQuestion = "Question?";
        registration.SecretAnswer = "Answer";

        // Assert
        Assert.IsFalse(registration.IsSavable, "Should not be savable when username contains multiple spaces");
        Assert.IsTrue(registration.BrokenRulesCollection.Any(r =>
            r.Description.Contains("cannot contain spaces")),
            "Should have broken rule about spaces in username");
    }
}
