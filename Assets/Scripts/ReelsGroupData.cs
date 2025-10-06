using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ReelGroupData
{
    [field: SerializeField] public int Count { get; private set; }
    [field: SerializeField] public int Spacing { get; private set; }
}
