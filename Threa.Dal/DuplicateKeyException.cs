using System;

namespace Threa.Dal
{
  public class DuplicateKeyException : Exception
  {
    public DuplicateKeyException(string message)
      : base(message)
    { }

    public DuplicateKeyException(string message, Exception innerException)
      : base(message, innerException)
    { }
  }
}
