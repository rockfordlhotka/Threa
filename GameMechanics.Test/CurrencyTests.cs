using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test
{
    [TestClass]
    public class CurrencyTests
    {
        [TestMethod]
        public void Currency_DefaultConstructor_AllZero()
        {
            var currency = new Currency();
            
            Assert.AreEqual(0, currency.Copper);
            Assert.AreEqual(0, currency.Silver);
            Assert.AreEqual(0, currency.Gold);
            Assert.AreEqual(0, currency.Platinum);
            Assert.AreEqual(0, currency.TotalCopper);
        }

        [TestMethod]
        public void Currency_ParameterizedConstructor_SetsValues()
        {
            var currency = new Currency(10, 5, 2, 1);
            
            Assert.AreEqual(10, currency.Copper);
            Assert.AreEqual(5, currency.Silver);
            Assert.AreEqual(2, currency.Gold);
            Assert.AreEqual(1, currency.Platinum);
        }

        [TestMethod]
        public void TotalCopper_CalculatesCorrectly()
        {
            // 10cp + 5sp(100cp) + 2gp(800cp) + 1pp(8000cp) = 8910cp
            var currency = new Currency(10, 5, 2, 1);
            
            Assert.AreEqual(8910, currency.TotalCopper);
        }

        [TestMethod]
        public void TotalCoins_CalculatesCorrectly()
        {
            var currency = new Currency(10, 5, 2, 1);
            
            Assert.AreEqual(18, currency.TotalCoins);
        }

        [TestMethod]
        public void WeightInPounds_CalculatesCorrectly()
        {
            // 100 coins = 1 pound
            var currency = new Currency(50, 25, 15, 10);
            
            Assert.AreEqual(1.0m, currency.WeightInPounds);
        }

        [TestMethod]
        public void Add_CombinesCurrencies()
        {
            var a = new Currency(10, 5, 2, 1);
            var b = new Currency(5, 3, 1, 0);
            
            a.Add(b);
            
            Assert.AreEqual(15, a.Copper);
            Assert.AreEqual(8, a.Silver);
            Assert.AreEqual(3, a.Gold);
            Assert.AreEqual(1, a.Platinum);
        }

        [TestMethod]
        public void HasExactCoins_ReturnsTrueWhenSufficient()
        {
            var currency = new Currency(10, 5, 2, 1);
            
            Assert.IsTrue(currency.HasExactCoins(5, 3, 1, 1));
            Assert.IsTrue(currency.HasExactCoins(10, 5, 2, 1));
        }

        [TestMethod]
        public void HasExactCoins_ReturnsFalseWhenInsufficient()
        {
            var currency = new Currency(10, 5, 2, 1);
            
            Assert.IsFalse(currency.HasExactCoins(11, 0, 0, 0));
            Assert.IsFalse(currency.HasExactCoins(0, 0, 0, 2));
        }

        [TestMethod]
        public void HasValue_ReturnsTrueWhenSufficient()
        {
            var currency = new Currency(10, 5, 2, 1);
            
            Assert.IsTrue(currency.HasValue(8910));
            Assert.IsTrue(currency.HasValue(5000));
        }

        [TestMethod]
        public void HasValue_ReturnsFalseWhenInsufficient()
        {
            var currency = new Currency(10, 5, 2, 1);
            
            Assert.IsFalse(currency.HasValue(9000));
        }

        [TestMethod]
        public void Clone_CreatesCopy()
        {
            var original = new Currency(10, 5, 2, 1);
            var clone = original.Clone();
            
            Assert.AreEqual(original.TotalCopper, clone.TotalCopper);
            
            // Modify original, clone should be unchanged
            original.Copper = 100;
            Assert.AreEqual(10, clone.Copper);
        }

        [TestMethod]
        public void ToString_FormatsCorrectly()
        {
            var currency = new Currency(10, 5, 2, 1);
            
            Assert.AreEqual("1pp 2gp 5sp 10cp", currency.ToString());
        }

        [TestMethod]
        public void ToString_OmitsZeroDenominations()
        {
            var currency = new Currency(10, 0, 2, 0);
            
            Assert.AreEqual("2gp 10cp", currency.ToString());
        }

        [TestMethod]
        public void ToString_ShowsZeroCopperWhenEmpty()
        {
            var currency = new Currency();
            
            Assert.AreEqual("0cp", currency.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void Copper_ThrowsOnNegative()
        {
            var currency = new Currency();
            currency.Copper = -1;
        }

        [TestMethod]
        public void PropertyChanged_RaisedOnChange()
        {
            var currency = new Currency();
            var changedProperties = new System.Collections.Generic.List<string>();
            currency.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);
            
            currency.Copper = 10;
            
            Assert.IsTrue(changedProperties.Contains("Copper"));
            Assert.IsTrue(changedProperties.Contains("TotalCopper"));
        }
    }

    [TestClass]
    public class MoneyChangerTests
    {
        [TestMethod]
        public void FromCopper_OptimizesCorrectly()
        {
            // 850cp = 2gp + 2sp + 10cp
            var currency = MoneyChanger.FromCopper(850);
            
            Assert.AreEqual(10, currency.Copper);
            Assert.AreEqual(2, currency.Silver);
            Assert.AreEqual(2, currency.Gold);
            Assert.AreEqual(0, currency.Platinum);
        }

        [TestMethod]
        public void FromCopper_HandlesLargeValues()
        {
            // 16850cp = 2pp + 2sp + 10cp
            var currency = MoneyChanger.FromCopper(16850);
            
            Assert.AreEqual(10, currency.Copper);
            Assert.AreEqual(2, currency.Silver);
            Assert.AreEqual(2, currency.Gold);
            Assert.AreEqual(2, currency.Platinum);
        }

        [TestMethod]
        public void Optimize_ReducesCoinCount()
        {
            // 400 copper should become 1 gold
            var currency = new Currency(400, 0, 0, 0);
            var optimized = MoneyChanger.Optimize(currency);
            
            Assert.AreEqual(0, optimized.Copper);
            Assert.AreEqual(0, optimized.Silver);
            Assert.AreEqual(1, optimized.Gold);
            Assert.AreEqual(0, optimized.Platinum);
        }

        [TestMethod]
        public void TryMakeChange_Success()
        {
            var purse = new Currency(0, 0, 5, 0); // 2000cp
            
            var result = MoneyChanger.TryMakeChange(purse, 850, out var change);
            
            Assert.IsTrue(result);
            Assert.AreEqual(1150, change.TotalCopper);
        }

        [TestMethod]
        public void TryMakeChange_InsufficientFunds()
        {
            var purse = new Currency(100, 0, 0, 0);
            
            var result = MoneyChanger.TryMakeChange(purse, 200, out var change);
            
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryPay_DeductsAndOptimizes()
        {
            var purse = new Currency(0, 0, 3, 0); // 1200cp
            
            var result = MoneyChanger.TryPay(purse, 850);
            
            Assert.IsTrue(result);
            Assert.AreEqual(350, purse.TotalCopper);
            // Should be 17sp 10cp
            Assert.AreEqual(10, purse.Copper);
            Assert.AreEqual(17, purse.Silver);
        }

        [TestMethod]
        public void TryPayExact_RequiresExactCoins()
        {
            var purse = new Currency(10, 5, 2, 0);
            var payment = new Currency(5, 3, 1, 0);
            
            var result = MoneyChanger.TryPayExact(purse, payment);
            
            Assert.IsTrue(result);
            Assert.AreEqual(5, purse.Copper);
            Assert.AreEqual(2, purse.Silver);
            Assert.AreEqual(1, purse.Gold);
        }

        [TestMethod]
        public void TryPayExact_FailsWhenMissingCoins()
        {
            var purse = new Currency(10, 5, 0, 0);
            var payment = new Currency(5, 3, 1, 0);
            
            var result = MoneyChanger.TryPayExact(purse, payment);
            
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryConvert_SilverToCopper()
        {
            var purse = new Currency(0, 5, 0, 0);
            
            var result = MoneyChanger.TryConvert(purse, CoinType.Silver, CoinType.Copper, 2);
            
            Assert.IsTrue(result);
            Assert.AreEqual(40, purse.Copper);
            Assert.AreEqual(3, purse.Silver);
        }

        [TestMethod]
        public void TryConvert_CopperToSilver()
        {
            var purse = new Currency(40, 0, 0, 0);
            
            var result = MoneyChanger.TryConvert(purse, CoinType.Copper, CoinType.Silver, 40);
            
            Assert.IsTrue(result);
            Assert.AreEqual(0, purse.Copper);
            Assert.AreEqual(2, purse.Silver);
        }

        [TestMethod]
        public void TryConvert_FailsOnInexactAmount()
        {
            var purse = new Currency(35, 0, 0, 0);
            
            // 35cp is not evenly divisible by 20 for silver
            var result = MoneyChanger.TryConvert(purse, CoinType.Copper, CoinType.Silver, 35);
            
            Assert.IsFalse(result);
            Assert.AreEqual(35, purse.Copper); // Unchanged
        }

        [TestMethod]
        public void TryBreakCoin_GoldToSilver()
        {
            var purse = new Currency(0, 5, 2, 0);
            
            var result = MoneyChanger.TryBreakCoin(purse, CoinType.Gold);
            
            Assert.IsTrue(result);
            Assert.AreEqual(1, purse.Gold);
            Assert.AreEqual(25, purse.Silver);
        }

        [TestMethod]
        public void TryBreakCoin_CannotBreakCopper()
        {
            var purse = new Currency(10, 0, 0, 0);
            
            var result = MoneyChanger.TryBreakCoin(purse, CoinType.Copper);
            
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryTransfer_MovesExactCoins()
        {
            var from = new Currency(10, 5, 2, 1);
            var to = new Currency(0, 0, 0, 0);
            var amount = new Currency(5, 2, 1, 0);
            
            var result = MoneyChanger.TryTransfer(from, to, amount);
            
            Assert.IsTrue(result);
            Assert.AreEqual(5, from.Copper);
            Assert.AreEqual(3, from.Silver);
            Assert.AreEqual(1, from.Gold);
            Assert.AreEqual(5, to.Copper);
            Assert.AreEqual(2, to.Silver);
            Assert.AreEqual(1, to.Gold);
        }

        [TestMethod]
        public void TryTransferValue_MovesOptimizedCoins()
        {
            var from = new Currency(0, 0, 5, 0); // 2000cp
            var to = new Currency(0, 0, 0, 0);
            
            var result = MoneyChanger.TryTransferValue(from, to, 850);
            
            Assert.IsTrue(result);
            Assert.AreEqual(1150, from.TotalCopper);
            Assert.AreEqual(850, to.TotalCopper);
        }

        [TestMethod]
        public void FormatCopper_ReturnsReadableString()
        {
            // 8910cp = 1pp (8000) + 2gp (800) + 5sp (100) + 10cp
            var result = MoneyChanger.FormatCopper(8910);
            
            Assert.AreEqual("1pp 2gp 5sp 10cp", result);
        }

        [TestMethod]
        public void TryParse_ParsesComplexString()
        {
            var result = MoneyChanger.TryParse("2gp 5sp 10cp", out long value);
            
            Assert.IsTrue(result);
            Assert.AreEqual(910, value);
        }

        [TestMethod]
        public void TryParse_ParsesSingleDenomination()
        {
            var result = MoneyChanger.TryParse("5gp", out long value);
            
            Assert.IsTrue(result);
            Assert.AreEqual(2000, value);
        }

        [TestMethod]
        public void TryParse_HandlesAlternateFormats()
        {
            Assert.IsTrue(MoneyChanger.TryParse("5gold", out long value1));
            Assert.AreEqual(2000, value1);
            
            Assert.IsTrue(MoneyChanger.TryParse("100c", out long value2));
            Assert.AreEqual(100, value2);
        }

        [TestMethod]
        public void Compare_ReturnsCorrectOrdering()
        {
            // a = 100cp, b = 5sp = 100cp, so they are equal
            // Let's use different values: a = 50cp, b = 5sp = 100cp
            var a = new Currency(50, 0, 0, 0);
            var b = new Currency(0, 5, 0, 0);
            
            Assert.IsTrue(MoneyChanger.Compare(a, b) < 0);
            Assert.IsTrue(MoneyChanger.Compare(b, a) > 0);
            Assert.AreEqual(0, MoneyChanger.Compare(a, new Currency(50, 0, 0, 0)));
        }
    }
}
