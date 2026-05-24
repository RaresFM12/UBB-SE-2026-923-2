using System;
using System.Collections.Generic;
using System.Text.Json;

var dict = new Dictionary<int, Tuple<string, bool>>();
dict[1] = new Tuple<string, bool>("TEST", true);
var json = JsonSerializer.Serialize(dict);
Console.WriteLine("Serialized: " + json);

try {
    var deserialized = JsonSerializer.Deserialize<Dictionary<int, Tuple<string, bool>>>(json);
    Console.WriteLine("Deserialized dict count: " + deserialized.Count);
    Console.WriteLine("Item1: " + deserialized[1].Item1);
} catch (Exception ex) {
    Console.WriteLine("Error: " + ex.Message);
}
