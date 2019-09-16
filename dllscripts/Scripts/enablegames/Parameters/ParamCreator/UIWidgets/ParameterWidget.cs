using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.EventSystems;
using System;

public abstract class ParameterWidget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public delegate void ParameterUpdated(string parameterName);
    public static event ParameterUpdated OnParameterChanged;

    [SerializeField]
    private Text displayLabel;

    [SerializeField]
    [TextArea(3, 10)]
    private string toolTip;

    private GameParameter parameter;
    public GameParameter Parameter
    {
        get { return parameter; }
        set { parameter = value; }
    }

    public virtual void Setup(GameParameter aParameter)
    {
        parameter = aParameter;
        Initialize();
    }

    void Awake()
    {
        GameParameters.GameParameterUpdateCheck += HandleGameParameterUpdateCheck;
    }

    protected abstract void HandleGameParameterUpdateCheck(GameParameter parameter);

    protected virtual void OnDestroy()
    {
        GameParameters.GameParameterUpdateCheck -= HandleGameParameterUpdateCheck;
    }

    protected virtual void Initialize()
    {
        this.displayLabel.text = parameter.Alias;
        this.toolTip = parameter.Description;
    }

    public virtual void UpdateParameter(object o)
    {
        if (OnParameterChanged != null)
        {
            OnParameterChanged(displayLabel.text);
        }
    }


    void DisplayToolTip(string tip)
    {
        //Debug.Log("Tooltip: " + tip);
        if (ToolTipController.Instance != null)
        {
            ToolTipController.Instance.ShowTip(tip);
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(toolTip))
        {
            DisplayToolTip(toolTip);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ToolTipController.Instance != null)
        {
            ToolTipController.Instance.HideToolTip();
        }

    }
}
