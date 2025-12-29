-- CharacterCurrency table: Currency held by characters
-- Uses the 4-denomination system: Copper, Silver, Gold, Platinum

ALTER TABLE Character ADD
    CopperCoins INT NOT NULL DEFAULT 0,
    SilverCoins INT NOT NULL DEFAULT 0,
    GoldCoins INT NOT NULL DEFAULT 0,
    PlatinumCoins INT NOT NULL DEFAULT 0;

-- Alternatively, if Character table hasn't been created yet:
/*
CREATE TABLE CharacterCurrency (
    CharacterId INT NOT NULL PRIMARY KEY,
    CopperCoins INT NOT NULL DEFAULT 0,
    SilverCoins INT NOT NULL DEFAULT 0,
    GoldCoins INT NOT NULL DEFAULT 0,
    PlatinumCoins INT NOT NULL DEFAULT 0,
    CONSTRAINT FK_CharacterCurrency_Character FOREIGN KEY (CharacterId) 
        REFERENCES Character(Id) ON DELETE CASCADE
);
*/

-- Currency conversion rates:
-- 1 sp (silver) = 20 cp (copper)
-- 1 gp (gold) = 20 sp = 400 cp
-- 1 pp (platinum) = 20 gp = 400 sp = 8,000 cp
-- 100 coins = 1 pound
