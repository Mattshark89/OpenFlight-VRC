# UdonDictionary

Dictionary implementation for UdonSharp.

## Example

```cs
using UdonSharp;
using UnityEngine;
using Koyashiro.UdonDictionary;

public class UdonDictionarySample : UdonSharpBehaviour
{
    public void Start()
    {
        var dic = UdonDictionary<string, int>.New(); // {}

        dic.SetValue("first", 1);  // { first: 1 }
        dic.SetValue("second", 2); // { first: 1, second: 2 }
        dic.SetValue("third", 3);  // { first: 1, second: 2, third: 3 }

        Debug.Log(dic.GetValue("first"));  // 1
        Debug.Log(dic.GetValue("second")); // 2
        Debug.Log(dic.GetValue("third"));  // 3
    }
}
```
