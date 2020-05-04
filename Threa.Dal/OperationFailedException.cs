using System;

namespace Threa.Dal
{
  public class OperationFailedException : Exception
  {
    public OperationFailedException(string message)
      : base(message)
    { }

    public OperationFailedException(string message, Exception innerException)
      : base(message, innerException)
    { }
  }
}
