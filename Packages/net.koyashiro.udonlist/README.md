# UdonList

List implementation for UdonSharp.

## Example

`UdonList` example for a `string`.

In addition, `UdonList<int>`, `UdonList<float>`, `UdonList<bool>`,
`UdonList<UdonSharpBehaviour>`, `UdonList<object>`, and so on can be used.

```cs
using UnityEngine;
using UdonSharp;
using Koyashiro.UdonList;

public class UdonListSample : UdonSharpBehaviour
{
    private void Start()
    {
        var list = UdonList<string>.New(); // Same as C# `new List<string>();`
        list.Add("first");     // ["first"]
        list.Add("second");    // ["first", "second"]
        list.Add("third");     // ["first", "second", "third"]
        list.Remove("second"); // ["first", "third"]

        // List to array
        var convertedArray = list.ToArray();

        // Array to list
        var convertedList = UdonList<string>.New(convertedArray);

        // Alternative foreach
        for (var i = 0; i < convertedList.Count(); i++)
        {
            Debug.Log(list.GetValue(i)); // "first", "third"
        }
    }
}
```
