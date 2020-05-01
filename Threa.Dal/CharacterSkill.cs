using System;
using System.Collections.Generic;
using System.Text;

namespace Threa.Dal
{
  public class CharacterSkill : Skill
  {
    public int Level { get; set; }
    public double XPBanked { get; set; }
  }
}
