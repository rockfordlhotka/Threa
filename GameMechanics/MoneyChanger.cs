using System;

namespace GameMechanics
{
    /// <summary>
    /// Provides currency exchange and conversion services.
    /// Acts as a "money-changer" to convert between denominations
    /// and handle transactions.
    /// </summary>
    public static class MoneyChanger
    {
        /// <summary>
        /// Converts a copper value to the optimal coin distribution
        /// that minimizes total coin count.
        /// </summary>
        /// <param name="copperValue">Total value in copper pieces</param>
        /// <returns>Currency with optimized coin distribution</returns>
        public static Currency FromCopper(long copperValue)
        {
            if (copperValue < 0)
                throw new ArgumentOutOfRangeException(nameof(copperValue), "Value cannot be negative.");

            var remaining = copperValue;

            var platinum = (int)(remaining / Currency.CopperPerPlatinum);
            remaining %= Currency.CopperPerPlatinum;

            var gold = (int)(remaining / Currency.CopperPerGold);
            remaining %= Currency.CopperPerGold;

            var silver = (int)(remaining / Currency.CopperPerSilver);
            remaining %= Currency.CopperPerSilver;

            var copper = (int)remaining;

            return new Currency(copper, silver, gold, platinum);
        }

        /// <summary>
        /// Optimizes a currency purse to minimize total coin count
        /// by converting smaller denominations to larger ones where possible.
        /// </summary>
        /// <param name="currency">The currency to optimize</param>
        /// <returns>New Currency with optimized distribution</returns>
        public static Currency Optimize(Currency currency)
        {
            if (currency == null)
                throw new ArgumentNullException(nameof(currency));

            return FromCopper(currency.TotalCopper);
        }

        /// <summary>
        /// Attempts to make change for a transaction.
        /// Deducts the cost from the purse and returns any change owed.
        /// </summary>
        /// <param name="purse">The payer's currency purse</param>
        /// <param name="costInCopper">The cost in copper pieces</param>
        /// <param name="change">The change to return (if successful)</param>
        /// <returns>True if the transaction can be completed</returns>
        public static bool TryMakeChange(Currency purse, long costInCopper, out Currency change)
        {
            change = new Currency();

            if (purse == null || costInCopper < 0)
                return false;

            if (purse.TotalCopper < costInCopper)
                return false;

            var remaining = purse.TotalCopper - costInCopper;
            change = FromCopper(remaining);
            return true;
        }

        /// <summary>
        /// Attempts to pay an exact cost from a purse, modifying the purse in place.
        /// Uses the most efficient combination of coins.
        /// </summary>
        /// <param name="purse">The payer's currency purse (modified if successful)</param>
        /// <param name="costInCopper">The cost in copper pieces</param>
        /// <returns>True if payment was successful</returns>
        public static bool TryPay(Currency purse, long costInCopper)
        {
            if (purse == null || costInCopper < 0)
                return false;

            if (!purse.HasValue(costInCopper))
                return false;

            // Convert purse to total copper, subtract cost, redistribute
            var remaining = purse.TotalCopper - costInCopper;
            var newPurse = FromCopper(remaining);

            purse.Copper = newPurse.Copper;
            purse.Silver = newPurse.Silver;
            purse.Gold = newPurse.Gold;
            purse.Platinum = newPurse.Platinum;

            return true;
        }

        /// <summary>
        /// Attempts to pay using specific coins from a purse.
        /// Does not optimize; removes exact coins specified.
        /// </summary>
        /// <param name="purse">The payer's currency purse</param>
        /// <param name="payment">The exact coins to pay</param>
        /// <returns>True if the purse had the exact coins</returns>
        public static bool TryPayExact(Currency purse, Currency payment)
        {
            if (purse == null || payment == null)
                return false;

            if (!purse.HasExactCoins(payment.Copper, payment.Silver, payment.Gold, payment.Platinum))
                return false;

            purse.Copper -= payment.Copper;
            purse.Silver -= payment.Silver;
            purse.Gold -= payment.Gold;
            purse.Platinum -= payment.Platinum;

            return true;
        }

        /// <summary>
        /// Converts coins from one denomination to another.
        /// Used by money-changers to break larger coins into smaller ones
        /// or combine smaller coins into larger ones.
        /// </summary>
        /// <param name="purse">The currency purse to modify</param>
        /// <param name="from">Source denomination</param>
        /// <param name="to">Target denomination</param>
        /// <param name="count">Number of source denomination coins to convert</param>
        /// <returns>True if conversion was successful</returns>
        public static bool TryConvert(Currency purse, CoinType from, CoinType to, int count)
        {
            if (purse == null || count <= 0)
                return false;

            // Check if purse has enough of the source denomination
            int available = from switch
            {
                CoinType.Copper => purse.Copper,
                CoinType.Silver => purse.Silver,
                CoinType.Gold => purse.Gold,
                CoinType.Platinum => purse.Platinum,
                _ => 0
            };

            if (available < count)
                return false;

            // Calculate value in copper
            long valueInCopper = from switch
            {
                CoinType.Copper => count,
                CoinType.Silver => (long)count * Currency.CopperPerSilver,
                CoinType.Gold => (long)count * Currency.CopperPerGold,
                CoinType.Platinum => (long)count * Currency.CopperPerPlatinum,
                _ => 0
            };

            // Calculate how many of target denomination we get
            int targetCopperValue = to switch
            {
                CoinType.Copper => 1,
                CoinType.Silver => Currency.CopperPerSilver,
                CoinType.Gold => Currency.CopperPerGold,
                CoinType.Platinum => Currency.CopperPerPlatinum,
                _ => 1
            };

            // For converting up (e.g., copper to silver), must have exact amount
            if (targetCopperValue > 1 && valueInCopper % targetCopperValue != 0)
            {
                // Can't make exact change - would need to handle remainder
                // For simplicity, we require exact conversion amounts
                return false;
            }

            int targetCount = (int)(valueInCopper / targetCopperValue);

            // Perform the conversion
            switch (from)
            {
                case CoinType.Copper: purse.Copper -= count; break;
                case CoinType.Silver: purse.Silver -= count; break;
                case CoinType.Gold: purse.Gold -= count; break;
                case CoinType.Platinum: purse.Platinum -= count; break;
            }

            switch (to)
            {
                case CoinType.Copper: purse.Copper += targetCount; break;
                case CoinType.Silver: purse.Silver += targetCount; break;
                case CoinType.Gold: purse.Gold += targetCount; break;
                case CoinType.Platinum: purse.Platinum += targetCount; break;
            }

            return true;
        }

        /// <summary>
        /// Breaks a single coin into smaller denominations.
        /// Useful for making change when exact coins aren't available.
        /// </summary>
        /// <param name="purse">The currency purse to modify</param>
        /// <param name="coinType">The type of coin to break</param>
        /// <returns>True if the coin was successfully broken</returns>
        public static bool TryBreakCoin(Currency purse, CoinType coinType)
        {
            if (purse == null)
                return false;

            switch (coinType)
            {
                case CoinType.Platinum:
                    if (purse.Platinum < 1) return false;
                    purse.Platinum -= 1;
                    purse.Gold += Currency.GoldPerPlatinum;
                    return true;

                case CoinType.Gold:
                    if (purse.Gold < 1) return false;
                    purse.Gold -= 1;
                    purse.Silver += Currency.SilverPerGold;
                    return true;

                case CoinType.Silver:
                    if (purse.Silver < 1) return false;
                    purse.Silver -= 1;
                    purse.Copper += Currency.CopperPerSilver;
                    return true;

                case CoinType.Copper:
                    // Cannot break copper further
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Transfers currency from one purse to another.
        /// </summary>
        /// <param name="from">Source purse</param>
        /// <param name="to">Destination purse</param>
        /// <param name="amount">Amount to transfer</param>
        /// <returns>True if transfer was successful</returns>
        public static bool TryTransfer(Currency from, Currency to, Currency amount)
        {
            if (from == null || to == null || amount == null)
                return false;

            if (!from.HasExactCoins(amount.Copper, amount.Silver, amount.Gold, amount.Platinum))
                return false;

            from.Copper -= amount.Copper;
            from.Silver -= amount.Silver;
            from.Gold -= amount.Gold;
            from.Platinum -= amount.Platinum;

            to.Copper += amount.Copper;
            to.Silver += amount.Silver;
            to.Gold += amount.Gold;
            to.Platinum += amount.Platinum;

            return true;
        }

        /// <summary>
        /// Transfers a copper value from one purse to another,
        /// optimizing the coins transferred.
        /// </summary>
        /// <param name="from">Source purse</param>
        /// <param name="to">Destination purse</param>
        /// <param name="copperValue">Value to transfer</param>
        /// <returns>True if transfer was successful</returns>
        public static bool TryTransferValue(Currency from, Currency to, long copperValue)
        {
            if (from == null || to == null || copperValue < 0)
                return false;

            if (!from.HasValue(copperValue))
                return false;

            // Deduct from source, converting to optimal coins first
            var fromRemaining = from.TotalCopper - copperValue;
            var fromNew = FromCopper(fromRemaining);

            from.Copper = fromNew.Copper;
            from.Silver = fromNew.Silver;
            from.Gold = fromNew.Gold;
            from.Platinum = fromNew.Platinum;

            // Add optimized coins to destination
            var toAdd = FromCopper(copperValue);
            to.Copper += toAdd.Copper;
            to.Silver += toAdd.Silver;
            to.Gold += toAdd.Gold;
            to.Platinum += toAdd.Platinum;

            return true;
        }

        /// <summary>
        /// Formats a copper value as a readable currency string.
        /// </summary>
        /// <param name="copperValue">Value in copper pieces</param>
        /// <returns>Formatted string like "5gp 2sp 10cp"</returns>
        public static string FormatCopper(long copperValue)
        {
            return FromCopper(copperValue).ToString();
        }

        /// <summary>
        /// Parses a currency string into a copper value.
        /// Supports formats like "5gp", "2sp 10cp", "100cp".
        /// </summary>
        /// <param name="currencyString">The currency string to parse</param>
        /// <param name="copperValue">The parsed value in copper</param>
        /// <returns>True if parsing was successful</returns>
        public static bool TryParse(string currencyString, out long copperValue)
        {
            copperValue = 0;

            if (string.IsNullOrWhiteSpace(currencyString))
                return false;

            var parts = currencyString.ToLower().Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (!TryParseCoinPart(part.Trim(), out long partValue))
                    return false;
                copperValue += partValue;
            }

            return true;
        }

        private static bool TryParseCoinPart(string part, out long copperValue)
        {
            copperValue = 0;

            if (string.IsNullOrEmpty(part))
                return false;

            // Find where the number ends and denomination begins
            int i = 0;
            while (i < part.Length && (char.IsDigit(part[i]) || part[i] == '-'))
                i++;

            if (i == 0)
                return false;

            if (!int.TryParse(part.Substring(0, i), out int count))
                return false;

            var denomination = part.Substring(i).Trim();

            copperValue = denomination switch
            {
                "cp" or "c" or "copper" => count,
                "sp" or "s" or "silver" => (long)count * Currency.CopperPerSilver,
                "gp" or "g" or "gold" => (long)count * Currency.CopperPerGold,
                "pp" or "p" or "platinum" => (long)count * Currency.CopperPerPlatinum,
                _ => 0
            };

            return denomination is "cp" or "c" or "copper" or "sp" or "s" or "silver" 
                   or "gp" or "g" or "gold" or "pp" or "p" or "platinum";
        }

        /// <summary>
        /// Compares two currency values and returns their relative ordering.
        /// </summary>
        /// <param name="a">First currency</param>
        /// <param name="b">Second currency</param>
        /// <returns>Negative if a less than b, zero if equal, positive if a greater than b</returns>
        public static int Compare(Currency a, Currency b)
        {
            var aValue = a?.TotalCopper ?? 0;
            var bValue = b?.TotalCopper ?? 0;
            return aValue.CompareTo(bValue);
        }
    }
}
