using UnityEngine;
using System.Collections.Generic;

namespace Suki
{

    class SukiData
    {

        private Dictionary<string, bool> triggers;
        private Dictionary<string, bool> signals;
        private Dictionary<string, float> ranges;
        private Dictionary<string, Vector2> location2ds;
        private Dictionary<string, Vector3> location3ds;

        private static SukiData instance;
        internal static SukiData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SukiData();
                    instance.Init();
                }
                return instance;
            }
        }

        internal void Reset()
        {
            Init();
        }

        private void Init()
        {
            triggers = new Dictionary<string, bool>();
            signals = new Dictionary<string, bool>();
            ranges = new Dictionary<string, float>();
            location2ds = new Dictionary<string, Vector2>();
            location3ds = new Dictionary<string, Vector3>();
        }

        internal bool GetTrigger(string name)
        {
            if (!triggers.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Trigger can be found for " + name);
            }
            bool ret = triggers[name];
            // triggers reset on read
            triggers[name] = false;
            return ret;
        }

        internal bool GetSignal(string name)
        {
            if (!signals.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Signal can be found for " + name);
            }
            bool ret = signals[name];
            return ret;
        }

        internal float GetRange(string name)
        {
            if (!ranges.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Range can be found for " + name);
            }
            float ret = ranges[name];
            return ret;
        }

        internal Vector2 GetLocation2D(string name)
        {
            if (!location2ds.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Location2D can be found for " + name);
            }
            Vector2 ret = location2ds[name];
            return ret;
        }

        internal Vector3 GetLocation3D(string name)
        {
            if (!location3ds.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Location3D can be found for " + name);
            }
            Vector3 ret = location3ds[name];
            return ret;
        }

        internal void SetTrigger(string name)
        {
            if (!triggers.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Trigger can be found for " + name);
            }
            triggers[name] = true;
        }

        internal void SetSignal(string name, bool isOn)
        {
            if (!signals.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Signal can be found for " + name);
            }
            signals[name] = isOn;
        }

        internal void SetRange(string name, float value)
        {
            //Debug.Log("SetRange:" + name + "=" + value);
            if (!ranges.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Range can be found for " + name);
            }
            if (value < 0f || value > 1f)
            {
                throw new System.Exception("Range value must be between 0.0 - 1.0 (inclusive)");
            }
            ranges[name] = value;
        }

        internal void SetLocation2D(string name, Vector2 value)
        {
            if (!location2ds.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Location2D can be found for " + name);
            }
            if (value.x < 0f || value.x > 1f || value.y < 0f || value.y > 1f)
            {
                throw new System.Exception("Location x,y values must be between 0.0 - 1.0 (inclusive)");
            }
            location2ds[name] = value;
        }

        internal void SetLocation3D(string name, Vector3 value)
        {
            if (!location3ds.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Location3D can be found for " + name);
            }
            if (value.x < 0f || value.x > 1f || value.y < 0f || value.y > 1f || value.z < 0f || value.z > 1f)
            {
                throw new System.Exception("Location x,y,z values must be between 0.0 - 1.0 (inclusive)");
            }
            location3ds[name] = value;
        }

        internal void CreateTrigger(string name, bool defaultValue)
        {
            if (triggers.ContainsKey(name))
            {
                throw new System.Exception("Trigger already exists for " + name);
            }
            triggers.Add(name, defaultValue);
        }

        internal void CreateSignal(string name, bool defaultValue)
        {
            if (signals.ContainsKey(name))
            {
                throw new System.Exception("Signal already exists for " + name);
            }
            signals.Add(name, defaultValue);
        }

        internal void CreateRange(string name, float defaultValue)
        {
            if (ranges.ContainsKey(name))
            {
                throw new System.Exception("Range already exists for " + name);
            }
            if (defaultValue < 0f || defaultValue > 1f)
            {
                throw new System.Exception("Range default value must be between 0.0 - 1.0 (inclusive)");
            }
            ranges.Add(name, defaultValue);
        }

        internal void CreateLocation2D(string name, Vector2 defaultValue)
        {
            if (location2ds.ContainsKey(name))
            {
                throw new System.Exception("Location2D already exists for " + name);
            }
            if (defaultValue.x < 0f || defaultValue.x > 1f || defaultValue.y < 0f || defaultValue.y > 1f)
            {
                throw new System.Exception("Location x,y default values must be between 0.0 - 1.0 (inclusive)");
            }
            location2ds.Add(name, defaultValue);
        }

        internal void CreateLocation3D(string name, Vector3 defaultValue)
        {
            if (location3ds.ContainsKey(name))
            {
                throw new System.Exception("Location3D already exists for " + name);
            }
            if (defaultValue.x < 0f || defaultValue.x > 1f || defaultValue.y < 0f || defaultValue.y > 1f || defaultValue.z < 0f || defaultValue.z > 1f)
            {
                throw new System.Exception("Location x,y,z default values must be between 0.0 - 1.0 (inclusive)");
            }
            location3ds.Add(name, defaultValue);
        }

        internal void DeleteTrigger(string name)
        {
            if (!triggers.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Trigger can be found for " + name);
            }
            triggers.Remove(name);
        }

        internal void DeleteSignal(string name)
        {
            if (!signals.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Signal can be found for " + name);
            }
            signals.Remove(name);
        }

        internal void DeleteRange(string name)
        {
            if (!ranges.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Range can be found for " + name);
            }
            ranges.Remove(name);
        }

        internal void DeleteLocation2D(string name)
        {
            if (!location2ds.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Location2D can be found for " + name);
            }
            location2ds.Remove(name);
        }

        internal void DeleteLocation3D(string name)
        {
            if (!location3ds.ContainsKey(name))
            {
                throw new KeyNotFoundException("No Location3D can be found for " + name);
            }
            location3ds.Remove(name);
        }

        internal bool TriggerExists(string name)
        {
            return triggers.ContainsKey(name);
        }

        internal bool SignalExists(string name)
        {
            return signals.ContainsKey(name);
        }

        internal bool RangeExists(string name)
        {
            return ranges.ContainsKey(name);
        }

        internal bool Location2DExists(string name)
        {
            return location2ds.ContainsKey(name);
        }

        internal bool Location3DExists(string name)
        {
            return location3ds.ContainsKey(name);
        }
    }
}
