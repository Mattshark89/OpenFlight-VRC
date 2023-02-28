# UdonJson

JSON serializer/deserializer implementation for UdonSharp.

## Serialization

```cs
using UnityEngine;
using UdonSharp;
using Koyashiro.UdonJson;

public class UdonJsonSerializerSample : UdonSharpBehaviour
{
    public void Start()
    {
        var json = UdonJsonValue.NewObject();
        json.SetValue("keyA", "valueA");
        json.SetValue("keyB", 123);
        var valueC = UdonJsonValue.NewObject();
        valueC.SetValue("keyC1", "valueC1");
        valueC.SetValue("keyC2", "valueC2");
        valueC.SetValue("keyC3", "valueC3");
        json.SetValue("keyC", valueC);
        var valueD = UdonJsonValue.NewArray();
        valueD.AddValue(0);
        valueD.AddValue(1);
        valueD.AddValue(2);
        json.SetValue("keyD", valueD);
        json.SetValue("keyE", true);
        json.SetValue("keyF", false);
        json.SetNullValue("keyG");

        Debug.Log(UdonJsonSerializer.Serialize(json)); // {"keyA":"valueA","keyB":123,"keyC":{"keyC1":"valueC1","keyC2":"valueC1","keyC3":"valueC2"},"keyD":[0,1,2],"keyE":true,"keyF":false,"keyG":null}}
    }
}
```

## Deserialization

```cs
using UnityEngine;
using UdonSharp;
using Koyashiro.UdonJson;

public class UdonJsonDeserializerSample : UdonSharpBehaviour
{
    public void Start()
    {
        var jsonStr = @"{
            ""keyA"": ""valueA"",
            ""keyB"": 123,
            ""keyC"": true,
            ""keyD"": false,
            ""keyE"": null,
            ""keyF"": {
                ""keyFA"": ""valueFA"",
                ""keyFB"": ""valueFB"",
                ""keyFC"": ""valuFC""
            },
            ""keyG"": [0, 1, 2, 3]
        }";

        var result = UdonJsonDeserializer.TryDeserialize(jsonStr, out var json);

        // Deserialize result
        Debug.Log(result); // True

        // JSON value kind
        Debug.Log(json.GetKind() == UdonJsonValueKind.Object); // True

        // String
        var valueA = json.GetValue("keyA");
        Debug.Log(valueA.GetKind() == UdonJsonValueKind.String); // True
        Debug.Log(valueA.AsString()); // valueA

        // Number
        var valueB = json.GetValue("keyB");
        Debug.Log(valueB.GetKind() == UdonJsonValueKind.Number); // True
        Debug.Log(valueB.AsNumber()); // 123

        // Object
        var valueF = json.GetValue("keyF");
        Debug.Log(valueF.GetKind() == UdonJsonValueKind.Object); // True
        var valueFA = valueF.GetValue("keyFA");
        Debug.Log(valueFA.GetKind() == UdonJsonValueKind.String); // True
        Debug.Log(valueFA.AsString()); // valueFA

        // Array
        var valueG = json.GetValue("keyG");
        Debug.Log(valueG.GetKind() == UdonJsonValueKind.Array); // True
        var valueG0 = valueG.GetValue(0);
        Debug.Log(valueG0.GetKind() == UdonJsonValueKind.Number); // True
        Debug.Log(valueG0.AsNumber()); // 0

        // True
        var valueC = json.GetValue("keyC");
        Debug.Log(valueC.GetKind() == UdonJsonValueKind.True); // True
        Debug.Log(valueC.AsBool()); // True

        // False
        var valueD = json.GetValue("keyD");
        Debug.Log(valueD.GetKind() == UdonJsonValueKind.False); // True
        Debug.Log(valueD.AsBool()); // false

        // Null
        var valueE = json.GetValue("keyE");
        Debug.Log(valueE.GetKind() == UdonJsonValueKind.Null); // True
        Debug.Log(valueE.AsNull()); // Null
    }
}
```
