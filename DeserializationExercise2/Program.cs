using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Xml.Serialization;

public class Program
{
    public class Person
    {
        public string UserName { get; set; }
        public int UserAge { get; set; }

    }

    static void Main()
    {

        /************************
        * Time for Binary Deserialization
        *************************/


        Stopwatch stopwatch = Stopwatch.StartNew();
        // ---------------------

        // Open filestream
        // var fs = new FileStream("person.dat", FileMode.Open);
        // Read binary from the filestream
        using (var fs = new FileStream("person.dat", FileMode.Open))
        using (var reader = new BinaryReader(fs))
        {//=

            // Deserialized result (into object)
            var deserializedPersonBinary = new Person
            {
                // Read the string property
                UserName = reader.ReadString(),
                // Read the integer property
                UserAge = reader.ReadInt32()
            };


            stopwatch.Stop();
            //------------------

            Console.WriteLine(
                $"Binary Deserialization - UserName: {deserializedPersonBinary.UserName}, UserAge: {deserializedPersonBinary.UserAge}"
            );
            Console.WriteLine($"Binary Deserialization took {stopwatch.ElapsedMilliseconds}ms");

        }

        var xmlData = File.ReadAllText("person.xml");
        var serializer = new XmlSerializer(typeof(Person));


        /************************
        * Time for XML Deserialization
        *************************/
        stopwatch.Start();
        // ---------------------


        using (var reader = new StringReader(xmlData))
        {

            // Deserialize JSON data
            var deserializedPersonXml = (Person)serializer.Deserialize(reader);
            stopwatch.Stop();
            // ---------------------

            Console.WriteLine(
                $"XML Deserialization - UserName: {deserializedPersonXml.UserName}, UserAge: {deserializedPersonXml.UserAge}"
            );
            Console.WriteLine($"XML Deserialization took {stopwatch.ElapsedMilliseconds} ms");

        }



        /************************
        * Time for JSON Deserialization
        *************************/
        stopwatch.Start();
        // ---------------------

        var jsonData = File.ReadAllText("person.json");

        // Deserialize JSON data
        var deserializedPersonJson = JsonSerializer.Deserialize<Person>(jsonData);

        stopwatch.Stop();

        Console.WriteLine(
            $"XML Deserialization - UserName: {deserializedPersonJson.UserName}, UserAge: {deserializedPersonJson.UserAge}"
        );
        Console.WriteLine($"XML Deserialization took {stopwatch.ElapsedMilliseconds} ms");


    }
}