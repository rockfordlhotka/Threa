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
/// Unit tests for AdminUserEdit business object.
/// Tests the last-admin protection rule.
/// </summary>
[TestClass]
public class AdminUserEditTests : TestBase
{
    /// <summary>
    /// Wait for async validation rules to complete.
    /// CSLA runs async rules in background; we need to wait for them before checking IsSavable.
    /// </summary>
    private static async Task WaitForRulesAsync(AdminUserEdit user)
    {
        // Wait for the object to not be busy (async rules complete)
        // Use a TaskCompletionSource to wait for BusyChanged event
        if (!user.IsBusy)
        {
            // Trigger a small delay to allow any pending rules to start
            await Task.Delay(100);
        }

        var tcs = new TaskCompletionSource<bool>();
        var timeout = Task.Delay(5000);

        void BusyChangedHandler(object? sender, Csla.Core.BusyChangedEventArgs e)
        {
            if (!e.Busy)
            {
                tcs.TrySetResult(true);
            }
        }

        user.BusyChanged += BusyChangedHandler;
        try
        {
            if (!user.IsBusy)
            {
                // Already not busy
                return;
            }

            // Wait for either not busy or timeout
            var completed = await Task.WhenAny(tcs.Task, timeout);
            if (completed == timeout)
            {
                throw new TimeoutException("Timed out waiting for async rules to complete");
            }
        }
        finally
        {
            user.BusyChanged -= BusyChangedHandler;
        }
    }

    [TestMethod]
    public async Task DisableLastAdmin_ShouldFail()
    {
        // Arrange
        var provider = InitServices(seedData: false);
        var portal = provider.GetRequiredService<IDataPortal<AdminUserEdit>>();

        var savedPlayer = await AddPlayerAsync(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "admin@test.com",
            Name = "Admin",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });

        // Verify the DAL count is correct
        var dal = provider.GetRequiredService<IPlayerDal>();
        var adminCount = await dal.CountEnabledAdminsAsync();
        Assert.AreEqual(1, adminCount, "Should have exactly 1 enabled admin before test");

        // Act
        var user = await portal.FetchAsync(savedPlayer.Id);

        // Check if the async rule will be triggered
        bool wasBusyEver = false;
        user.BusyChanged += (s, e) => { if (e.Busy) wasBusyEver = true; };

        user.IsEnabled = false;
        bool isBusyAfterChange = user.IsBusy;

        // Wait for async rules to complete
        await WaitForRulesAsync(user);

        // Debug: Was the object ever busy?
        System.Diagnostics.Debug.WriteLine($"IsBusy after change: {isBusyAfterChange}, WasBusyEver: {wasBusyEver}");

        // Debug output - check broken rules
        var brokenRules = string.Join(", ", user.BrokenRulesCollection.Select(r => $"{r.Property}: {r.Description}"));

        // Assert
        Assert.IsFalse(user.IsSavable, $"Should not be savable when disabling last admin. IsDirty={user.IsDirty}, IsValid={user.IsValid}, BrokenRules=[{brokenRules}]");
        Assert.IsTrue(user.BrokenRulesCollection.Any(r =>
            r.Description.Contains("at least one enabled administrator")),
            $"Should have broken rule about needing at least one admin. BrokenRules=[{brokenRules}]");
    }

    [TestMethod]
    public async Task DemoteLastAdmin_ShouldFail()
    {
        // Arrange
        var provider = InitServices(seedData: false);
        var portal = provider.GetRequiredService<IDataPortal<AdminUserEdit>>();

        var savedPlayer = await AddPlayerAsync(new Threa.Dal.Dto.Player
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
        var user = await portal.FetchAsync(savedPlayer.Id);
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
        var provider = InitServices(seedData: false);
        var portal = provider.GetRequiredService<IDataPortal<AdminUserEdit>>();

        var savedAdmin1 = await AddPlayerAsync(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "admin1@test.com",
            Name = "Admin 1",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });
        var savedAdmin2 = await AddPlayerAsync(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "admin2@test.com",
            Name = "Admin 2",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });

        // Act
        var user = await portal.FetchAsync(savedAdmin1.Id);
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
        var provider = InitServices(seedData: false);
        var portal = provider.GetRequiredService<IDataPortal<AdminUserEdit>>();

        var savedAdmin = await AddPlayerAsync(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "admin@test.com",
            Name = "Admin",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });
        var savedUser = await AddPlayerAsync(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "user@test.com",
            Name = "User",
            Roles = "Player",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });

        // Act
        var user = await portal.FetchAsync(savedUser.Id); // Regular user
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
        var provider = InitServices(seedData: false);
        var portal = provider.GetRequiredService<IDataPortal<AdminUserEdit>>();

        var savedAdmin1 = await AddPlayerAsync(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "admin1@test.com",
            Name = "Admin 1",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });
        var savedAdmin2 = await AddPlayerAsync(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "admin2@test.com",
            Name = "Admin 2",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });

        // Act
        var user = await portal.FetchAsync(savedAdmin1.Id);
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
        var provider = InitServices(seedData: false);
        var portal = provider.GetRequiredService<IDataPortal<AdminUserEdit>>();

        var savedEnabledAdmin = await AddPlayerAsync(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "enabledadmin@test.com",
            Name = "Enabled Admin",
            Roles = "Administrator",
            IsEnabled = true,
            HashedPassword = "test",
            Salt = "salt"
        });
        var savedDisabledAdmin = await AddPlayerAsync(new Threa.Dal.Dto.Player
        {
            Id = 0,
            Email = "disabledadmin@test.com",
            Name = "Disabled Admin",
            Roles = "Administrator",
            IsEnabled = false, // Already disabled
            HashedPassword = "test",
            Salt = "salt"
        });

        // Act: Try to disable the only enabled admin
        var user = await portal.FetchAsync(savedEnabledAdmin.Id);
        user.IsEnabled = false;

        // Wait for async rules to complete
        await WaitForRulesAsync(user);

        // Assert: Should fail - only one ENABLED admin exists
        Assert.IsFalse(user.IsSavable, "Should not be savable - only one enabled admin");
    }
}
