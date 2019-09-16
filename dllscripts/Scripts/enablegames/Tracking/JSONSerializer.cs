using System;
using System.IO;
using FullSerializer;

public static class JSONSerializer {
    private static readonly fsSerializer _serializer = new fsSerializer();

    public static string Serialize(Type type, object value) {
        // serialize the data
        fsData data = Data(type, value);

        // emit the data via JSON
        return fsJsonPrinter.PrettyJson(data);
    }

    public static void Serialize(Type type, object value, string filePath) {
        StreamWriter writer = new StreamWriter(filePath);
        // serialize the data
        fsData data = Data(type, value);

        // emit the data via JSON
        fsJsonPrinter.PrettyJson(data, writer);
        writer.Close();
    }

    public static fsData Data(Type type, object value) {
        fsData data;
        _serializer.TrySerialize(type, value, out data).AssertSuccessWithoutWarnings();

        return data;
    }

    public static object Deserialize(Type type, string serializedState) {
        // step 1: parse the JSON data
        fsData data = fsJsonParser.Parse(serializedState);

        // step 2: deserialize the data
        object deserialized = null;
        _serializer.TryDeserialize(data, type, ref deserialized).AssertSuccessWithoutWarnings();

        return deserialized;
    }
}