-- Migration: Add epoch-based timing to CharacterEffect and Character tables
-- Date: 2026-01-27
-- Purpose: Improve performance of effect expiration checks from O(n×rounds) to O(n)

-- Add epoch time columns to CharacterEffect table
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.CharacterEffect')
    AND name = 'CreatedAtEpochSeconds'
)
BEGIN
    ALTER TABLE dbo.CharacterEffect
    ADD CreatedAtEpochSeconds BIGINT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.CharacterEffect')
    AND name = 'ExpiresAtEpochSeconds'
)
BEGIN
    ALTER TABLE dbo.CharacterEffect
    ADD ExpiresAtEpochSeconds BIGINT NULL;
END;

-- Add current game time to Character table
IF NOT EXISTS (
    SELECT * FROM sys.columns
    WHERE object_id = OBJECT_ID('dbo.Character')
    AND name = 'CurrentGameTimeSeconds'
)
BEGIN
    ALTER TABLE dbo.Character
    ADD CurrentGameTimeSeconds BIGINT NOT NULL DEFAULT 0;
END;

-- Add index for efficient expiration queries
IF NOT EXISTS (
    SELECT * FROM sys.indexes
    WHERE object_id = OBJECT_ID('dbo.CharacterEffect')
    AND name = 'IX_CharacterEffect_ExpiresAtEpochSeconds'
)
BEGIN
    CREATE INDEX IX_CharacterEffect_ExpiresAtEpochSeconds
    ON dbo.CharacterEffect(ExpiresAtEpochSeconds)
    WHERE ExpiresAtEpochSeconds IS NOT NULL;
END;

-- Update existing effects to use epoch-based expiration (if possible)
-- This migration assumes characters are at table's current time
-- Effects without duration (wounds) will remain null (permanent until healed)
UPDATE ce
SET
    ce.CreatedAtEpochSeconds = 0,  -- Unknown creation time for existing effects
    ce.ExpiresAtEpochSeconds = CASE
        WHEN ce.RoundsRemaining IS NOT NULL
        THEN ce.RoundsRemaining * 6  -- Convert rounds to seconds (6 sec per round)
        ELSE NULL  -- Permanent effect
    END
FROM dbo.CharacterEffect ce
WHERE ce.CreatedAtEpochSeconds IS NULL;

PRINT 'Migration 001: Epoch-based timing added successfully';
