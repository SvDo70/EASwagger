using System;
using System.Collections.Generic;

namespace EASwagger
{
  public enum OperationType
  {
    GET,
    POST,
    PUT,
    PATCH,
    DELETE
  }

  public class Operation
  {
    public OperationType? type = null;
    public String name = "";
    public String output = "";
    public List<String> input = new List<string>();
  }

  public class Resource
  {
    public String name = "";
    public List<Operation> operations = new List<Operation>();
  }
}
