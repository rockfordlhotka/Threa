using System.ComponentModel;

namespace GameMechanics
{
  /// <summary>
  /// Calculates basic game mechanic values
  /// </summary>
  public class Calculator : INotifyPropertyChanged
  {
    private int _tv = 5;
    /// <summary>
    /// Gets or sets the Target Value (TV)
    /// </summary>
    public int TvValue
    {
      get { return _tv; }
      set 
      { 
        _tv = value; 
        OnPropertyChanged(nameof(TvValue));
        CalcRv();
      }
    }

    private int _as = 5;
    /// <summary>
    /// Gets or sets the Ability Score (AS)
    /// </summary>
    public int AsValue
    {
      get { return _as; }
      set 
      { 
        _as = value; 
        OnPropertyChanged(nameof(AsValue));
        CalcRv();
      }
    }

    private int _rv;
    /// <summary>
    /// Gets or sets the Result Value (RV)
    /// </summary>
    public int RvValue
    {
      get { return _rv; }
      set 
      { 
        _rv = value; 
        OnPropertyChanged(nameof(RvValue));
        CalcSv();
      }
    }

    private int _roll;
    /// <summary>
    /// Gets or sets the dice roll value
    /// </summary>
    public int RollValue
    {
      get { return _roll; }
      set { _roll = value; OnPropertyChanged(nameof(RollValue)); }
    }

    /// <summary>
    /// Generates a new Result Value (RV)
    /// property value for a 4d3 roll
    /// </summary>
    public void CalcRv()
    {
      RollValue = Dice.Roll(4, 3);
      RvValue = (AsValue + RollValue) - TvValue;
    }

    private int _plusSv;
    /// <summary>
    /// Gets or sets the Plus SV value for
    /// the current attack/weapon
    /// </summary>
    public int PlusSvValue
    {
      get { return _plusSv; }
      set 
      { 
        _plusSv = value; 
        OnPropertyChanged(nameof(PlusSvValue));
        CalcSv();
      }
    }

    private int _svValue;
    /// <summary>
    /// Gets or sets the SV property value;
    /// setting the value recalculates damage
    /// </summary>
    public int SvValue
    {
      get { return _svValue; }
      set 
      { 
        _svValue = value;
        if (_svValue < 0)
          _svValue = 0;
        OnPropertyChanged(nameof(SvValue));
        CalcDamage();
      }
    }

    /// <summary>
    /// Calculates the current SV property
    /// value based on the attack/defend SV
    /// properties and RVs value
    /// </summary>
    public void CalcSv()
    {
      SvValue = RVs(RvValue) + PlusSvValue + ArmorValue;
    }

    private int _armor;
    /// <summary>
    /// Gets or sets the minus SV value
    /// for the current target/armor
    /// </summary>
    public int ArmorValue
    {
      get { return _armor; }
      set 
      { 
        _armor = value; 
        OnPropertyChanged(nameof(ArmorValue));
        CalcSv();
      }
    }

    private int _damage;
    /// <summary>
    /// Gets or sets the damage value
    /// </summary>
    public int DamageValue
    {
      get { return _damage; }
      set { _damage = value; OnPropertyChanged(nameof(DamageValue)); }
    }

    private int _fat;
    /// <summary>
    /// Gets or sets the FAT damage value
    /// </summary>
    public int FatValue
    {
      get { return _fat; }
      set { _fat = value; OnPropertyChanged(nameof(FatValue)); }
    }

    private int _vit;
    /// <summary>
    /// Gets or sets the VIT damage value
    /// </summary>
    public int VitValue
    {
      get { return _vit; }
      set { _vit = value; OnPropertyChanged(nameof(VitValue)); }
    }

    private int _wnd;
    /// <summary>
    /// Gets or sets the WND damage value
    /// </summary>
    public int WndValue
    {
      get { return _wnd; }
      set { _wnd = value; OnPropertyChanged(nameof(WndValue)); }
    }

    /// <summary>
    /// Calculates damage based on the RVs value
    /// </summary>
    public void CalcDamage()
    {
      DamageValue = RollSvResult();
      CalcActualDamage();
    }

    /// <summary>
    /// Calculates the FAT/VIT/WND damage
    /// values based on the DamageValue
    /// property
    /// </summary>
    public void CalcActualDamage()
    {
      if (DamageValue < 20)
      {
        FatValue = DamageValue;
        switch (DamageValue)
        {
          case 5:
            VitValue = 1;
            WndValue = 0;
            break;
          case 6:
            VitValue = 2;
            WndValue = 0;
            break;
          case 7:
            VitValue = 4;
            WndValue = 1;
            break;
          case 8:
            VitValue = 6;
            WndValue = 1;
            break;
          case 9:
            VitValue = 8;
            WndValue = 1;
            break;
          case 10:
            VitValue = 10;
            WndValue = 2;
            break;
          case 11:
            VitValue = 11;
            WndValue = 2;
            break;
          case 12:
            VitValue = 12;
            WndValue = 2;
            break;
          case 13:
            VitValue = 13;
            WndValue = 2;
            break;
          case 14:
            VitValue = 14;
            WndValue = 2;
            break;
          case 15:
            VitValue = 15;
            WndValue = 3;
            break;
          case 16:
            VitValue = 16;
            WndValue = 3;
            break;
          case 17:
            VitValue = 17;
            WndValue = 3;
            break;
          case 18:
            VitValue = 18;
            WndValue = 3;
            break;
          case 19:
            VitValue = 19;
            WndValue = 3;
            break;
          default:
            VitValue = 0;
            WndValue = 0;
            break;
        }
      }
      else
      {
        var old = DamageValue;
        var count = DamageValue / 10;
        DamageValue = 10;
        CalcActualDamage();
        DamageValue = old;
        FatValue *= count;
        VitValue *= count;
        WndValue *= count;
      }
    }

    /// <summary>
    /// Randomly generates the damage value
    /// based on the current SvValue property
    /// </summary>
    /// <returns></returns>
    public int RollSvResult()
    {
      var roll = 0;
      if (RvValue >= 0)
      {
        switch (SvValue)
        {
          case 0:
            roll = Dice.Roll(1, 2);
            break;
          case 1:
            roll = (Dice.Roll(1, 6) + 1) / 2;
            if (roll > 3) roll = 3;
            break;
          case 2:
            roll = Dice.Roll(1, 6);
            break;
          case 3:
            roll = Dice.Roll(1, 8);
            break;
          case 4:
            roll = Dice.Roll(1, 10);
            break;
          case 5:
            roll = Dice.Roll(1, 12);
            break;
          case 6:
            roll = Dice.Roll(1, 6) + Dice.Roll(1, 8);
            break;
          case 7:
            roll = Dice.Roll(2, 8);
            break;
          case 8:
            roll = Dice.Roll(2, 10);
            break;
          case 9:
            roll = Dice.Roll(2, 12);
            break;
          case 10:
            roll = Dice.Roll(3, 10);
            break;
          case 11:
            roll = Dice.Roll(3, 12);
            break;
          case 12:
          case 13:
          case 14:
            roll = Dice.Roll(4, 10);
            break;
          case 15:
          case 16:
            roll = Dice.Roll(1, 6) * 10;
            break;
          case 17:
          case 18:
            roll = Dice.Roll(1, 8) * 10;
            break;
          default:
            roll = Dice.Roll(1, 10) * 10;
            break;
        }
      }
      return roll;
    }

    /// <summary>
    /// Gets the RVs value for the 
    /// given result value (RV)
    /// </summary>
    /// <param name="rv">Result value</param>
    /// <returns></returns>
    public static int RVs(int rv)
    {
      int result;
      if (rv < -8)
        result = -3;
      else if (rv < -6)
        result = -2;
      else if (rv < -4)
        result = -2;
      else if (rv < -2)
        result = -1;
      else if (rv < 2)
        result = 0;
      else if (rv < 4)
        result = 1;
      else if (rv < 8)
        result = 2;
      else if (rv < 12)
        result = 3;
      else
        result = 4;
      return result;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
  }
}
