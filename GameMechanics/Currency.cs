using System;
using System.ComponentModel;

namespace GameMechanics
{
    /// <summary>
    /// Represents a currency purse containing coins of all denominations.
    /// Tracks copper, silver, gold, and platinum coins.
    /// </summary>
    public class Currency : INotifyPropertyChanged
    {
        /// <summary>
        /// Exchange rate: copper pieces per silver piece
        /// </summary>
        public const int CopperPerSilver = 20;

        /// <summary>
        /// Exchange rate: copper pieces per gold piece
        /// </summary>
        public const int CopperPerGold = 400;

        /// <summary>
        /// Exchange rate: copper pieces per platinum piece
        /// </summary>
        public const int CopperPerPlatinum = 8000;

        /// <summary>
        /// Exchange rate: silver pieces per gold piece
        /// </summary>
        public const int SilverPerGold = 20;

        /// <summary>
        /// Exchange rate: gold pieces per platinum piece
        /// </summary>
        public const int GoldPerPlatinum = 20;

        /// <summary>
        /// Number of coins that equal one pound of weight
        /// </summary>
        public const int CoinsPerPound = 100;

        private int _copper;
        private int _silver;
        private int _gold;
        private int _platinum;

        /// <summary>
        /// Creates a new empty currency purse.
        /// </summary>
        public Currency()
        {
        }

        /// <summary>
        /// Creates a new currency purse with the specified coin amounts.
        /// </summary>
        /// <param name="copper">Number of copper pieces</param>
        /// <param name="silver">Number of silver pieces</param>
        /// <param name="gold">Number of gold pieces</param>
        /// <param name="platinum">Number of platinum pieces</param>
        public Currency(int copper, int silver = 0, int gold = 0, int platinum = 0)
        {
            _copper = copper;
            _silver = silver;
            _gold = gold;
            _platinum = platinum;
        }

        /// <summary>
        /// Gets or sets the number of copper pieces.
        /// </summary>
        public int Copper
        {
            get => _copper;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Coin count cannot be negative.");
                _copper = value;
                OnPropertyChanged(nameof(Copper));
                OnPropertyChanged(nameof(TotalCopper));
                OnPropertyChanged(nameof(TotalCoins));
                OnPropertyChanged(nameof(WeightInPounds));
            }
        }

        /// <summary>
        /// Gets or sets the number of silver pieces.
        /// </summary>
        public int Silver
        {
            get => _silver;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Coin count cannot be negative.");
                _silver = value;
                OnPropertyChanged(nameof(Silver));
                OnPropertyChanged(nameof(TotalCopper));
                OnPropertyChanged(nameof(TotalCoins));
                OnPropertyChanged(nameof(WeightInPounds));
            }
        }

        /// <summary>
        /// Gets or sets the number of gold pieces.
        /// </summary>
        public int Gold
        {
            get => _gold;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Coin count cannot be negative.");
                _gold = value;
                OnPropertyChanged(nameof(Gold));
                OnPropertyChanged(nameof(TotalCopper));
                OnPropertyChanged(nameof(TotalCoins));
                OnPropertyChanged(nameof(WeightInPounds));
            }
        }

        /// <summary>
        /// Gets or sets the number of platinum pieces.
        /// </summary>
        public int Platinum
        {
            get => _platinum;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Coin count cannot be negative.");
                _platinum = value;
                OnPropertyChanged(nameof(Platinum));
                OnPropertyChanged(nameof(TotalCopper));
                OnPropertyChanged(nameof(TotalCoins));
                OnPropertyChanged(nameof(WeightInPounds));
            }
        }

        /// <summary>
        /// Gets the total value in copper pieces.
        /// </summary>
        public long TotalCopper =>
            (long)_copper +
            ((long)_silver * CopperPerSilver) +
            ((long)_gold * CopperPerGold) +
            ((long)_platinum * CopperPerPlatinum);

        /// <summary>
        /// Gets the total number of coins.
        /// </summary>
        public int TotalCoins => _copper + _silver + _gold + _platinum;

        /// <summary>
        /// Gets the total weight in pounds (100 coins = 1 pound).
        /// </summary>
        public decimal WeightInPounds => (decimal)TotalCoins / CoinsPerPound;

        /// <summary>
        /// Adds another currency purse to this one.
        /// </summary>
        /// <param name="other">The currency to add</param>
        public void Add(Currency other)
        {
            if (other == null) return;
            Copper += other.Copper;
            Silver += other.Silver;
            Gold += other.Gold;
            Platinum += other.Platinum;
        }

        /// <summary>
        /// Adds the specified amount of copper pieces.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        public void AddCopper(int amount) => Copper += amount;

        /// <summary>
        /// Adds the specified amount of silver pieces.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        public void AddSilver(int amount) => Silver += amount;

        /// <summary>
        /// Adds the specified amount of gold pieces.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        public void AddGold(int amount) => Gold += amount;

        /// <summary>
        /// Adds the specified amount of platinum pieces.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        public void AddPlatinum(int amount) => Platinum += amount;

        /// <summary>
        /// Checks if this purse contains at least the specified amounts.
        /// </summary>
        /// <param name="copper">Required copper</param>
        /// <param name="silver">Required silver</param>
        /// <param name="gold">Required gold</param>
        /// <param name="platinum">Required platinum</param>
        /// <returns>True if all required coins are available</returns>
        public bool HasExactCoins(int copper = 0, int silver = 0, int gold = 0, int platinum = 0)
        {
            return _copper >= copper &&
                   _silver >= silver &&
                   _gold >= gold &&
                   _platinum >= platinum;
        }

        /// <summary>
        /// Checks if this purse has enough total value (in copper equivalent) 
        /// to cover the specified amount.
        /// </summary>
        /// <param name="copperValue">The amount in copper pieces</param>
        /// <returns>True if the total value is sufficient</returns>
        public bool HasValue(long copperValue)
        {
            return TotalCopper >= copperValue;
        }

        /// <summary>
        /// Creates a copy of this currency purse.
        /// </summary>
        /// <returns>A new Currency object with the same values</returns>
        public Currency Clone()
        {
            return new Currency(_copper, _silver, _gold, _platinum);
        }

        /// <summary>
        /// Returns a formatted string representation of the currency.
        /// </summary>
        /// <returns>String like "5gp 2sp 10cp"</returns>
        public override string ToString()
        {
            var parts = new System.Collections.Generic.List<string>();
            if (_platinum > 0) parts.Add($"{_platinum}pp");
            if (_gold > 0) parts.Add($"{_gold}gp");
            if (_silver > 0) parts.Add($"{_silver}sp");
            if (_copper > 0 || parts.Count == 0) parts.Add($"{_copper}cp");
            return string.Join(" ", parts);
        }

        /// <summary>
        /// Returns a compact format showing only non-zero denominations.
        /// </summary>
        /// <returns>String like "5gp 2sp 10cp" or "0cp" if empty</returns>
        public string ToCompactString()
        {
            return ToString();
        }

        /// <summary>
        /// Returns a verbose format with total copper value.
        /// </summary>
        /// <returns>String with breakdown and total</returns>
        public string ToDetailedString()
        {
            return $"{ToString()} (Total: {TotalCopper}cp, Weight: {WeightInPounds:F2}lbs)";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
