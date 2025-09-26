using System.Collections.Generic;

namespace EASwagger
{
  class Property
  {
    public string name = "";
    public string type = "";
    public string format = "";
  }

  class Representation
  {
    public string name;
    public List<Property> properties;
  }
}
