#region copyright
/*
* Copyright (C) 2017 EnAble Games LLC - All Rights Reserved
* Unauthorized copying of these files, via any medium is strictly prohibited
* Proprietary and confidential
*/
#endregion
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace enableGame
{
    public abstract class Subject
    {
        public void Notify(Subject s)
        {
            Tracker.Instance.Interrupt((int)(egEvent.Type.CustomEvent), "ParameterChange");
            VariableHandler.Instance.UpdateRoutine(s);
        }
    }
}
