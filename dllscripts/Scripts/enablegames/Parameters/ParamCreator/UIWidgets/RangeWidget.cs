#region copyright
/*
* Copyright (C) EnAble Games LLC - All Rights Reserved
* Unauthorized copying of these files, via any medium is strictly prohibited
* Proprietary and confidential
* fullserializer by jacobdufault is provided under the MIT license.
*/
#endregion

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RangeWidget : ParameterWidget
{
    [SerializeField]
    private Slider rangeSlider;

    [SerializeField]
    private float tick;

    [SerializeField]
    private Text valueField;

    private RangeParameter rangeParameter;

	protected override void Initialize()
	{
		base.Initialize();
		if (this.Parameter.GetType() != typeof(RangeParameter))
		{
			throw new System.ApplicationException("Mismatch Widget and Parameter Type");
		}
		rangeParameter = (RangeParameter)this.Parameter;
		//if (rangeParameter.Tick != 1f)
		if (rangeParameter.Tick != (int)rangeParameter.Tick) {
			rangeSlider.wholeNumbers = false;
		} else
			rangeSlider.wholeNumbers = true;
		float temp = rangeParameter.Value;  //The value gets overwritten by SliderUpdate if min/max changes, so save now to set later.
		rangeSlider.minValue = rangeParameter.Min;
		rangeSlider.maxValue = rangeParameter.Max;
		StartCoroutine(WaitAndUpdate(temp));
	}

    IEnumerator WaitAndUpdate(float newVal)
    { // Sketchy
        yield return null; // Wait a frame for new min and maxes before updating value
        rangeSlider.value = newVal;
        valueField.text = rangeParameter.Value.ToString();
    }

    public void SliderUpdate()
    {
        UpdateParameter((float)rangeSlider.value);
    }

    protected override void HandleGameParameterUpdateCheck(GameParameter parameter)
    {
        rangeSlider.value = rangeParameter.Value;
        valueField.text = rangeParameter.Value.ToString();
    }

    public override void UpdateParameter(object o)
    {
        if (o.GetType() != typeof(float))
        {
            throw new System.ApplicationException("Mismatch Widget and Parameter Type");
        }
        rangeParameter.Value = (float)o;
        valueField.text = rangeParameter.Value.ToString();
        base.UpdateParameter(o);
    }
}
