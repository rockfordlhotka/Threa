namespace GameMechanics
{
    /// <summary>
    /// Represents the different coin denominations in the game.
    /// </summary>
    public enum CoinType
    {
        /// <summary>
        /// Copper piece - base unit of currency
        /// </summary>
        Copper,

        /// <summary>
        /// Silver piece - worth 20 copper
        /// </summary>
        Silver,

        /// <summary>
        /// Gold piece - worth 20 silver (400 copper)
        /// </summary>
        Gold,

        /// <summary>
        /// Platinum piece - worth 20 gold (8,000 copper)
        /// </summary>
        Platinum
    }
}
