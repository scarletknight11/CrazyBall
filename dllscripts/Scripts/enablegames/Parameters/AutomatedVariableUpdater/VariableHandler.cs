#region copyright
/*
* Copyright (C) 2017 EnAble Games LLC - All Rights Reserved
* Unauthorized copying of these files, via any medium is strictly prohibited
* Proprietary and confidential
*/
#endregion
using System;
using System.Collections.Generic;
using UnityEngine;

namespace enableGame
{
    /// <summary>
    /// Singleton that bind variables to the game parameters from GameParameters.cs
    /// It will update the state of the variables each time the game parameter bind to them changes.
    /// </summary>
    public class VariableHandler : MonoBehaviour
    {
        private static VariableHandler instance;
        public static VariableHandler Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("_VariabelHandler");
                    // instance = new VariableHandler();
                    go.AddComponent<VariableHandler>();
                    instance = go.GetComponent<VariableHandler>();
                }
                return instance;
            }
        }

        private Dictionary<String, List<egVar>> varDictionary;

        public VariableHandler()
        {
            varDictionary = new Dictionary<string, List<egVar>>();
        }

        /// <summary>
        /// This method will update all the variables binded to the Game Parameter "subject"
        /// </summary>
        /// <param name="subject">Game Parameters inherit Subject class</param>
        public void UpdateRoutine(Subject subject)
        {
            // creating a generic variable egVar and instantiating the correct type 
            // the eg types are the same implemented as parameters (string, int, bool, double) 
            egVar var;
            GameParameter gp = (GameParameter)subject;
            if (subject.GetType() == typeof(IntParameter))
            {
                IntParameter iP = (IntParameter)subject;
                egInt i = iP.Value;
                var = i;
            }
            else if (subject.GetType() == typeof(BoolParameter))
            {
                BoolParameter bP = (BoolParameter)subject;
                egBool b = bP.Value;
                var = b;
            }
            else if (subject.GetType() == typeof(StringListParameter))
            {
                StringListParameter sP = (StringListParameter)subject;
                egString s = sP.Value;
                var = s;
            }
			else if (subject.GetType() == typeof(StringParameter))
			{
				StringParameter sP = (StringParameter)subject;
				egString s = sP.Value;
                Debug.Log((string)s + " variable Handler test");
                var = s;
            }
            else if (subject.GetType() == typeof(RangeParameter))
            {
                RangeParameter fP = (RangeParameter)subject;
                egFloat s = fP.Value;
                var = s;
            }
            else
            {
                // if the type is not registered something went wrong
                Debug.Log("ERROR, type: " + subject.GetType() + " not defined as a parameter type. Check parameter types.");
                return;
            }
            //then we will update the variables
            UpdateVariables(var, gp.Name);
        }

        /// <summary>
        /// This method will update all the variables bind to the Alias string with the current newValue
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="Alias"></param>
        private void UpdateVariables(egVar newValue, string Alias)
        {

            // errorVariableList will collect all the wrong binds and delete them
            List<egVar> errorVariableList = new List<egVar>();

            // foreach element in the dicitonary with the Alias key we will update the variables
            if (varDictionary.ContainsKey(Alias))
            {
                List<egVar> egList = varDictionary[Alias];
                foreach (egVar var in egList)
                {
                    // if the type is correct we will update the variable 
                    // there is another control on the type inside the UpdateVal method in the egVar sons
					if (true)//newValue.GetType() == var.GetType())
                    {
                        var.UpdateVal(newValue);
                    }
                    else
                    {
                        //if the type is not correct we will add the variables to the errorVariableList and send a message to the console
                        Debug.Log("Attempting to update a " + var.GetType() + " with a " + newValue.GetType() + " variable. Check the variable type. This registration will be deleted.");
                        errorVariableList.Add(var);
                    }
                }
            }
            // at the end of the routine we will delete all the variables on the errorVariableList will be unbinded
            FixErrorList(errorVariableList, Alias);
        }

        /// <summary>
        /// This method unbind variables that rescountred problems during the updating procedure to decrease the memory allocation and have a better performance
        /// </summary>
        /// <param name="errorVariableList"></param>
        /// <param name="Alias"></param>
        private void FixErrorList(List<egVar> errorVariableList, string Alias)
        {
            if (varDictionary.ContainsKey(Alias))
            {
                foreach (egVar var in errorVariableList)
                {
                    varDictionary[Alias].Remove(var);
                }
            }
        }

        /// <summary>
        /// Register a variable to the variableHandler and keep it updated as the parameters changes
        /// </summary>
        /// <param name="Key">the key represent the aslias of the parameters. Use the static class egParameterStrings to have access to the avaiable keys</param>
        /// <param name="var"></param>
        public void Register(string Key, egVar var)
        {
            List<egVar> list;
            if (!varDictionary.TryGetValue(Key, out list))
            {
                list = new List<egVar>();
                varDictionary.Add(Key, list);
            }
            list.Add(var);
			if (ParameterHandler.Instance != null && ParameterHandler.Instance.AllParameters!=null)
              UpdateRoutine(ParameterHandler.Instance.AllParameters[0].GetParameter(Key));
        }
    }
}
