using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EASwagger
{
    public class EASwaggerAddIn
    {
        private const string menuHeader = "-&Swagger";
        private const string menuItem1 = "&Generate";
        private readonly string[] genericDataTypes = {"integer", "string", "number", "boolean"};

        private List<Resource> resources = new List<Resource>();
        private List<Representation> representations = new List<Representation>();
        private EA.Element info = null;


        public String EA_Connect(EA.Repository repo)
        {
            return "connected.";
        }

        public object EA_GetMenuItems(EA.Repository repo, string location, string menuName)
        {
            switch (menuName)
            {
                case "":
                    return "-&Swagger";
                case "-&Swagger":
                    return "&Generate";
            }
            return "";
        }

        public void EA_MenuClick(EA.Repository repo, string menuName, string itemName)
        {
            if (itemName == "&Generate")
            {
                if (repo.GetTreeSelectedItemType() != EA.ObjectType.otPackage)
                {
                    MessageBox.Show("Please select a package to generate.");
                    return;
                }

                info = null;
                resources.Clear();
                representations.Clear();

                EA.Collection elements = repo.GetTreeSelectedPackage().Elements;
                foreach (EA.Element e in elements)
                {
                    if (e.HasStereotype("resource provider"))
                    {
                        if (info == null)
                        {
                            info = e;
                        }
                        else
                        {
                            MessageBox.Show("There must be only one object with stereotype <resource provider>");
                            return;
                        }
                    }

                    if (e.HasStereotype("resource"))
                    {
                        Resource r = new Resource();
                        r.name = e.Name;
                        r.operations = CreateOperationFromEAElement(e.Methods);
                        resources.Add(r);
                    }

                    if (e.HasStereotype("representation"))
                    {
                        Representation r = new Representation();
                        r.name = e.Name;
                        r.properties = CreatePropertiesFromEAElement(e.Attributes);
                        representations.Add(r);
                    }
                }

                // Write the results to the output file
                string outputFile = Path.GetDirectoryName(repo.ConnectionString) + "\\" +
                    repo.GetTreeSelectedPackage().Name + ".json";
                StreamWriter stream = new StreamWriter(File.Open(outputFile, FileMode.Create));
                stream.Write(FormatResultToJson());
                stream.Close();
            }
        }

        private List<Operation> CreateOperationFromEAElement(EA.Collection methods)
        {
            List<Operation> result = new List<Operation>();

            foreach (EA.Method m in methods)
            {
                Operation o = new Operation();

                o.name = m.Name;

                switch (m.Stereotype)
                {
                    default: o.type = null; break;
                    case "GET": o.type = OperationType.GET; break;
                    case "DELETE": o.type = OperationType.DELETE; break;
                    case "PATCH": o.type = OperationType.PATCH; break;
                    case "POST": o.type = OperationType.POST; break;
                    case "PUT": o.type = OperationType.PUT; break;
                }

                o.output = m.ReturnType;
                foreach (EA.Parameter p in m.Parameters)
                {
                    o.input.Add(p.Type);
                }
                result.Add(o);
            }

            return result;
        }

        private List<Property> CreatePropertiesFromEAElement(EA.Collection attributes)
        {
            List<Property> result = new List<Property>();

            foreach (EA.Attribute a in attributes)
            {
                Property p = new Property();
                p.name = a.Name;
                p.type = a.Type;
                p.format = "";
                result.Add(p);
            }

            return result;
        }

        private string FormatResultToJson()
        {
            String result = "swagger: '2.0'\n";

            // Info block
            result += "info:\n  description: 'auto-generated from Enterprise Architect'\n";
            result += "  title: '" + info.Name + "'\n";
            result += "  version: '1.0.0'\n";

            // Paths
            result += "paths:\n";

            foreach (Resource r in resources)
            {
                result += "  /" + r.name + ":\n";
                // TODO: create links to parameters and response schemas
                foreach (Operation o in r.operations)
                {
                    switch (o.type)
                    {
                        case OperationType.DELETE: result += "    delete:\n"; break;
                        case OperationType.GET: result += "    get:\n"; break;
                        case OperationType.PATCH: result += "    patch:\n"; break;
                        case OperationType.POST: result += "    post:\n"; break;
                        case OperationType.PUT: result += "    put:\n"; break;
                    }
                    result += "      operationId: " + o.name + "\n";
                    result += "      produces:\n      - application/json\n";
                    result += "      responses:\n";
                    result += "        200:\n";
                    result += "          description: OK\n";
                }
            }

            // Definitions
            result += "definitions:\n";

            foreach (Representation r in representations)
            {
                result += "  " + r.name + ":\n";
                result += "    type: object\n";
                result += "    properties:\n";
                foreach (Property p in r.properties)
                {
                    result += "      " + p.name + ":\n";
                    // TODO: validate the JSON types and map List<>, Array<>, Dictionary<> to JSON arrays
                    result += "        type: " + p.type + "\n";
                    if (p.format != "")
                        result += "        format: " + p.format + "\n";
                }
            }

            return result;
        }
    }
}
