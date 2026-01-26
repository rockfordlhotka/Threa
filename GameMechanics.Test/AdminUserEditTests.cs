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
/// Unit tests for AdminUserEdit business object.
/// Tests the last-admin protection rule.
/// </summary>
[TestClass]
[DoNotParallelize]
public class AdminUserEditTests
{
    private ServiceProvider InitServices()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddCsla();
        services.AddMockDb();
        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Wait for async validation rules to complete.
    /// CSLA runs async rules in background; we need to wait for them before checking IsSavable.
    /// </summary>
    private static async Task WaitForRulesAsync(AdminUserEdit user)
    {
        // Give async rules time to execute and complete
        // In CSLA 9, IsBusy indicates rules are still running
        var timeout = DateTime.UtcNow.AddSeconds(5);
        while (user.IsBusy && DateTime.UtcNow < timeout)
        {
            await Task.Delay(50);
        }
    }

    [TestMethod]
    public async Task DisableLastAdmin_ShouldFail()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<AdminUserEdit>>();

        MockDb.Players.Clear();
        MockDb.Players.Add(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "admin@test.com",
            Name = "Admin",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });

        // Act
        var user = await portal.FetchAsync(0);
        user.IsEnabled = false;

        // Wait for async rules to complete
        await WaitForRulesAsync(user);

        // Assert
        Assert.IsFalse(user.IsSavable, "Should not be savable when disabling last admin");
        Assert.IsTrue(user.BrokenRulesCollection.Any(r =>
            r.Description.Contains("at least one enabled administrator")),
            "Should have broken rule about needing at least one admin");
    }

    [TestMethod]
    public async Task DemoteLastAdmin_ShouldFail()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<AdminUserEdit>>();

        MockDb.Players.Clear();
        MockDb.Players.Add(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "admin@test.com",
            Name = "Admin",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });

        // Act
        var user = await portal.FetchAsync(0);
        user.IsAdministrator = false;

        // Wait for async rules to complete
        await WaitForRulesAsync(user);

        // Assert
        Assert.IsFalse(user.IsSavable, "Should not be savable when demoting last admin");
        Assert.IsTrue(user.BrokenRulesCollection.Any(r =>
            r.Description.Contains("at least one enabled administrator")),
            "Should have broken rule about needing at least one admin");
    }

    [TestMethod]
    public async Task DisableNonLastAdmin_ShouldSucceed()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<AdminUserEdit>>();

        MockDb.Players.Clear();
        MockDb.Players.Add(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "admin1@test.com",
            Name = "Admin 1",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });
        MockDb.Players.Add(new Threa.Dal.Dto.Player
        {
            Id = 1,
            Email = "admin2@test.com",
            Name = "Admin 2",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });

        // Act
        var user = await portal.FetchAsync(0);
        user.IsEnabled = false;

        // Wait for async rules to complete
        await WaitForRulesAsync(user);

        // Assert
        Assert.IsTrue(user.IsSavable, "Should be savable when other admins exist");
    }

    [TestMethod]
    public async Task DisableNonAdmin_ShouldAlwaysSucceed()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<AdminUserEdit>>();

        MockDb.Players.Clear();
        MockDb.Players.Add(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "admin@test.com",
            Name = "Admin",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });
        MockDb.Players.Add(new Threa.Dal.Dto.Player
        {
            Id = 1,
            Email = "user@test.com",
            Name = "User",
            Roles = "Player",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });

        // Act
        var user = await portal.FetchAsync(1); // Regular user
        user.IsEnabled = false;

        // Wait for async rules to complete
        await WaitForRulesAsync(user);

        // Assert
        Assert.IsTrue(user.IsSavable, "Should be savable when disabling non-admin");
    }

    [TestMethod]
    public async Task DemoteNonLastAdmin_ShouldSucceed()
    {
        // Arrange
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<AdminUserEdit>>();

        MockDb.Players.Clear();
        MockDb.Players.Add(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "admin1@test.com",
            Name = "Admin 1",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });
        MockDb.Players.Add(new Threa.Dal.Dto.Player
        {
            Id = 1,
            Email = "admin2@test.com",
            Name = "Admin 2",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });

        // Act
        var user = await portal.FetchAsync(0);
        user.IsAdministrator = false;

        // Wait for async rules to complete
        await WaitForRulesAsync(user);

        // Assert
        Assert.IsTrue(user.IsSavable, "Should be savable when other admins exist");
    }

    [TestMethod]
    public async Task DisableDisabledAdmin_ShouldNotAffectCount()
    {
        // Arrange: One enabled admin, one disabled admin
        var provider = InitServices();
        var portal = provider.GetRequiredService<IDataPortal<AdminUserEdit>>();

        MockDb.Players.Clear();
        MockDb.Players.Add(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "enabledadmin@test.com",
            Name = "Enabled Admin",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });
        MockDb.Players.Add(new Threa.Dal.Dto.Player
        {
            Id = 1,
            Email = "disabledadmin@test.com",
            Name = "Disabled Admin",
            Roles = "Administrator",
            IsEnabled = false, // Already disabled
            HashedPassword = "test",
            Salt = "salt"
        });

        // Act: Try to disable the only enabled admin
        var user = await portal.FetchAsync(0);
        user.IsEnabled = false;

        // Wait for async rules to complete
        await WaitForRulesAsync(user);

        // Assert: Should fail - only one ENABLED admin exists
        Assert.IsFalse(user.IsSavable, "Should not be savable - only one enabled admin");
    }
}
