using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GameMechanics.Messaging;
using GameMechanics.Time;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameMechanics.Messaging.RabbitMQ;

/// <summary>
/// Hosted service that listens for time events and processes them through TimeManager.
/// This bridges the messaging layer with the game mechanics.
/// </summary>
public class TimeEventHandlerService : IHostedService, ITimeEventHandler
{
    private readonly ITimeEventSubscriber _subscriber;
    private readonly ITimeEventPublisher _publisher;
    private readonly ILogger<TimeEventHandlerService> _logger;

    private TimeManager? _timeManager;
    private readonly List<CharacterEdit> _characters = new();
    private readonly object _lock = new();

    public TimeEventHandlerService(
        ITimeEventSubscriber subscriber,
        ITimeEventPublisher publisher,
        ILogger<TimeEventHandlerService> logger)
    {
        _subscriber = subscriber;
        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Registers a TimeManager to handle time events.
    /// </summary>
    public void RegisterTimeManager(TimeManager timeManager)
    {
        _timeManager = timeManager;
        _timeManager.RegisterHandler(this);
    }

    /// <summary>
    /// Registers a character to receive time events.
    /// </summary>
    public void RegisterCharacter(CharacterEdit character)
    {
        lock (_lock)
        {
            if (!_characters.Contains(character))
            {
                _characters.Add(character);
                _timeManager?.TrackCharacter(character);
            }
        }
    }

    /// <summary>
    /// Unregisters a character from time events.
    /// </summary>
    public void UnregisterCharacter(CharacterEdit character)
    {
        lock (_lock)
        {
            _characters.Remove(character);
            _timeManager?.UntrackCharacter(character.Id);
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting time event handler service...");

        try
        {
            // Connect publisher and subscriber
            await _publisher.ConnectAsync(cancellationToken);
            await _subscriber.ConnectAsync(cancellationToken);

            // Wire up event handlers
            _subscriber.TimeEventReceived += OnTimeEventReceived;
            _subscriber.TimeSkipReceived += OnTimeSkipReceived;
            _subscriber.CombatStateReceived += OnCombatStateReceived;

            // Start listening
            await _subscriber.SubscribeAsync(cancellationToken);

            _logger.LogInformation("Time event handler service started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start time event handler service");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping time event handler service...");

        try
        {
            // Unwire event handlers
            _subscriber.TimeEventReceived -= OnTimeEventReceived;
            _subscriber.TimeSkipReceived -= OnTimeSkipReceived;
            _subscriber.CombatStateReceived -= OnCombatStateReceived;

            // Stop listening
            await _subscriber.UnsubscribeAsync(cancellationToken);

            _logger.LogInformation("Time event handler service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping time event handler service");
        }
    }

    private async void OnTimeEventReceived(object? sender, TimeEventMessage message)
    {
        if (_timeManager == null)
        {
            _logger.LogWarning("Received time event but no TimeManager registered");
            return;
        }

        _logger.LogInformation("Processing time event: {EventType} x {Count} from {Source}",
            message.EventType, message.Count, message.SourceId);

        try
        {
            for (int i = 0; i < message.Count; i++)
            {
                var result = await _timeManager.AdvanceTimeAsync(message.EventType);

                // Publish result
                var resultMessage = CreateResultMessage(message, result, i + 1);
                await _publisher.PublishTimeEventAsync(new TimeEventMessage
                {
                    CampaignId = message.CampaignId,
                    EventType = message.EventType,
                    SourceId = "TimeEventHandlerService"
                });
            }

            _logger.LogInformation("Completed processing time event: {EventType} x {Count}",
                message.EventType, message.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing time event: {EventType}", message.EventType);
        }
    }

    private async void OnTimeSkipReceived(object? sender, TimeSkipMessage message)
    {
        if (_timeManager == null)
        {
            _logger.LogWarning("Received time skip but no TimeManager registered");
            return;
        }

        _logger.LogInformation("Processing time skip: {SkipUnit} x {Count} from {Source}",
            message.SkipUnit, message.Count, message.SourceId);

        try
        {
            var result = await _timeManager.SkipTimeAsync(message.SkipUnit, message.Count);

            _logger.LogInformation("Completed time skip: {SkipUnit} x {Count}, boundaries crossed: {Boundaries}",
                message.SkipUnit, message.Count, string.Join(", ", result.BoundariesCrossed));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing time skip: {SkipUnit}", message.SkipUnit);
        }
    }

    private void OnCombatStateReceived(object? sender, CombatStateMessage message)
    {
        if (_timeManager == null)
        {
            _logger.LogWarning("Received combat state change but no TimeManager registered");
            return;
        }

        _logger.LogInformation("Processing combat state change: EnteringCombat={Entering} from {Source}",
            message.EnteringCombat, message.SourceId);

        try
        {
            if (message.EnteringCombat)
            {
                _timeManager.EnterCombat();
            }
            else
            {
                _timeManager.ExitCombat();
            }

            _logger.LogInformation("Combat state changed: InCombat={InCombat}", _timeManager.InCombat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing combat state change");
        }
    }

    private TimeResultMessage CreateResultMessage(TimeEventMessage original, TimeAdvanceResult result, int iteration)
    {
        var characterResults = new List<CharacterTimeResult>();

        foreach (var charResult in result.CharacterResults)
        {
            characterResults.Add(new CharacterTimeResult
            {
                CharacterId = charResult.CharacterId,
                CharacterName = charResult.CharacterName ?? "",
                APRecovered = charResult.APRecovered,
                FATRecovered = charResult.FATRecovered,
                // Add more result mapping as needed
            });
        }

        return new TimeResultMessage
        {
            CampaignId = original.CampaignId,
            OriginalMessageId = original.MessageId,
            EventType = original.EventType,
            UnitsAdvanced = iteration,
            TotalRounds = _timeManager?.CurrentTime.TotalRounds ?? 0,
            CurrentTimeDisplay = _timeManager?.CurrentTime.DisplayTime ?? "",
            BoundariesCrossed = new List<TimeEventType>(result.BoundariesCrossed),
            CharacterResults = characterResults
        };
    }

    // ITimeEventHandler implementation - called by TimeManager when events occur
    public Task OnTimeAdvancedAsync(TimeAdvanceResult result)
    {
        _logger.LogDebug("TimeManager processed {EventType}: {UnitsAdvanced} units, {CharCount} characters affected",
            result.EventType, result.UnitsAdvanced, result.CharacterResults.Count);

        return Task.CompletedTask;
    }
}
