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
