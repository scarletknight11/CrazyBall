using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// Enum parameters are set essentially like strings but allow greater flexibility elsewhere in games. This allows devs to define their own game modes. Makes the package portable.
// This is a template class to facilitate that any enum could be used
// Quality of life things like getting the enum type and converting everything in that enum in to a list of string will make this easy to use in guis. The list can generate drop down lists and the like for GUIs.
// As of 12/4/2014 this is the coolest code in the project.
public class EnumParameter<T> : GenericParameter<T> {
    public void SetVal(string valString) {
        value_ = (T)Enum.Parse(typeof(T), valString);
    }

    public Type EnumType {
        get {
            return typeof(T);
        }
    }

    public List<string> EnumStrings () {
        return new List<string>(Enum.GetNames(typeof(T)));
    }

    public EnumParameter(string name, T val)
        : base(name, val) {
        if (typeof(T).IsEnum == false) {
            throw new System.ApplicationException("Initializing Enum Parameter with non enumerated type.");
        }
    }
}
