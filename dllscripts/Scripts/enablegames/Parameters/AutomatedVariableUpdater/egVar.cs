#region copyright
/*
* Copyright (C) 2017 EnAble Games LLC - All Rights Reserved
* Unauthorized copying of these files, via any medium is strictly prohibited
* Proprietary and confidential
*/
#endregion

using System;

namespace enableGame
{

    public class egInt : egVar
    {
        private int _Value;

        public static implicit operator egInt(int value)
        {
            return new egInt { _Value = value, updated = false };
        }

        public static implicit operator int(egInt value)
        {
            return value._Value;
        }

		public static implicit operator float(egInt value)
		{
			return value._Value;
		}

		public static implicit operator string(egInt value)
		{
			return value._Value.ToString();
		}

        public override void UpdateVal(egVar var)
        {
            if (var.GetType() == typeof(egInt))
            {
                updated = true;
                egInt i = (egInt)var;
                _Value = i;
            }
			else if (var.GetType() == typeof(egFloat))
			{
				updated = true;
				egFloat i = (egFloat)var;
				_Value = i;
			}
			else
				
            {
                Console.WriteLine("Type: " + var.GetType() + " not supported by egInt");
            }
        }
    }

    public class egBool : egVar
    {
        private bool _Value;

        public static implicit operator egBool(bool value)
        {
            return new egBool { _Value = value, updated = false };
        }

        public static implicit operator bool(egBool value)
        {
            return value._Value;
        }

		public static implicit operator string(egBool value)
		{
			return value._Value.ToString();
		}

        public override void UpdateVal(egVar var)
        {
            if (var.GetType() == typeof(egBool))
            {
                updated = true;
                egBool b = (egBool)var;
                _Value = b;
            }
            else
            {
                Console.WriteLine("Type: " + var.GetType() + " not supported by egBool");
            }
        }
    }

    public class egString : egVar
    {
        private string _Value;

        public static implicit operator egString(string value)
        {
            return new egString { _Value = value, updated = false };
        }

        public static implicit operator string(egString value)
        {
            return value._Value;
        }


        public override void UpdateVal(egVar var)
        {
            if (var.GetType() == typeof(egString))
            {
                updated = true;
                egString s = (egString)var;
                _Value = s;
            }
            else
            {
                Console.WriteLine("Type: " + var.GetType() + " not supported by egString");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class egFloat : egVar
    {

        private float _Value;

        public static implicit operator egFloat(float value)
        {
            return new egFloat { _Value = value, updated = false };
        }

        public static implicit operator float(egFloat value)
        {
            return value._Value;
        }

		public static implicit operator string(egFloat value)
		{
			return value._Value.ToString();
		}

		public static implicit operator int(egFloat value)
		{
			return (int) value._Value;
		}
		//Not sure about it, need to check how it works and the size of the int
        public static implicit operator egFloat(int value)
        {
            return new egFloat { _Value = Convert.ToInt32(value) };
        }

        public override void UpdateVal(egVar var)
        {
            if (var.GetType() == typeof(egFloat))
            {
                updated = true;
                egFloat s = (egFloat)var;
                _Value = s;
            }
            else
            {
                Console.WriteLine("Type: " + var.GetType() + " not supported by egString");
            }
        }
    }

    /// <summary>
    /// Abstract class that will be used to implement our personal egVariables
    /// It handle only the type of parameters implemented in our games.
    /// Further implementations will require an update of this class.
    /// </summary>
    public abstract class egVar
    {
        public abstract void UpdateVal(egVar var);
        // add a new UpdateVal method if a new type is implemented in the parameter scripts

        public bool updated;
    }

}