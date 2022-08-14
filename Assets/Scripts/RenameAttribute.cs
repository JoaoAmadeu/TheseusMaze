using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenameAttribute : PropertyAttribute
{
    public string newName;
    
    public RenameAttribute (string newName)
    {
        this.newName = newName;
    }
}
