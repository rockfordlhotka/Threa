using System;
using System.Collections.Generic;
using System.Text;

namespace Threa.Dal
{
  public interface IAttribute
  {
    int Id { get; set; }
    string Name { get; set; }
    string ImageUrl { get; set; }
  }
}
