using UnityEngine;
using System.Collections;
using FullSerializer;
public class RangeParameter : GameParameter {
    [fsProperty]
    private float min_;
    [fsIgnore]
    public float Min {
        get { return min_; }
        set { min_ = value; }
    }
    [fsProperty]
    private float max_;
    [fsIgnore]
    public float Max {
        get { return max_; }
        set { max_ = value; }
    }
    [fsProperty]
    private float value_;
    [fsIgnore]
    public float Value {
        get { return value_; }
        set {
            Notify(this);
//			value_ = GetServerOverride<float>(Mathf.Round(value * RoundFactor) / RoundFactor);
			value_ = GetServerOverride<float>(Mathf.Round(value / tick_) * tick_);
		}
    }

    [fsProperty]
    private float tick_; // degree to round to
    [fsIgnore]
	public float Tick {
		get { return tick_; }
		set { tick_ = value; }
	}
    [fsIgnore]
	public float RoundFactor {
		get { return 1f/ tick_; }
	}

	public void LoadDefaultValue () {
		this.Value  = (this.max_ - this.min_) / 2 + this.min_;
	}

    public RangeParameter() {
        min_ = 0f;
        max_ = 10f;
        value_ = 5f;
        tick_ = 1f;
    }

	public RangeParameter(string name, float min, float max, float startValue) : this(name, min, max, startValue, 1f) { Print(); }
	
	public RangeParameter(string name, float min, float max, float startValue, float tick) : base(name) {
		min_ = min;
		max_ = max;
		tick_ = tick;
		Value = startValue;
        Print();
	}

	public override bool AllowNetworkSync () {
		return true;
	}

	public override float NetworkValue () {
		return Value;
	}

	public override void AssignValue (float newVal) {
		Value = newVal;
	}

    public override void Print() {
        Debug.Log(this.Name + " set to: " + value_.ToString());
    }
}
