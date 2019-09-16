using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GameParameterCreator : MonoBehaviour {
    [SerializeField]
    private string parameterKey;
    public string ParameterKey {
        get {
            return parameterKey;
        }
    }

    [SerializeField]
    private string displayName;

    [SerializeField]
    private Text title;
    [SerializeField]
    private GameObject BoolWidgetPrefab;
    [SerializeField]
    private GameObject RangeWidgetPrefab;
    [SerializeField]
    private GameObject StringListPrefab;
    [SerializeField]
    private GameObject CategoryPrefab;
    [SerializeField]
    private GameObject scrollViewRoot;

    private CanvasGroup canvasGroup;

    public delegate  void GameParameterHandler( bool state);
    public static event GameParameterHandler SetUpComplete;

    public void Activate () {
        if(canvasGroup == null) {
            canvasGroup = this.GetComponent<CanvasGroup>();
        }
       // Game.ActiveParameters = ParameterHandler.Instance.GetParameters(parameterKey);
        //ThemeController.SetThemeForPhase( );
        canvasGroup.DOFade(1f, .1f);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Deactivate () {
        if (canvasGroup == null)
        {
            canvasGroup = this.GetComponent<CanvasGroup>();
        }
        canvasGroup.DOFade(0f, .1f);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    private List<ParameterWidget> Widgets;
    private List<GameObject> CategoryObjects;

    private bool setUpComplete = false;
    private GameParameters paramInst;
    public GameParameters Parameters {
        get {
            return paramInst;
        }
        set {
            paramInst = value;
        }
    }

    void RaiseOnSetupComplete(bool state )
    {
        if( SetUpComplete  != null )
        {
            SetUpComplete( state );
        }
    }

    public void Reset() {
        if (!setUpComplete) {
            return;
        }
        setUpComplete = false;
        RaiseOnSetupComplete( false );

        paramInst = null;
        foreach (ParameterWidget widg in Widgets) {
            DestroyImmediate(widg.gameObject);
        }
        foreach (GameObject go in CategoryObjects) {
            DestroyImmediate(go);
        }
    }

    public void LoadIntoParamHandler(GameParameters parameter) {
        ParameterHandler.Instance.AddParameters(parameterKey/*, parameter*/);
    }

    /// <summary>
    /// Populate list of widgets/parameters in the UI
    /// </summary>
    public void SetUp() {
        if (title != null) {
            title.text = displayName;
        }
        if (setUpComplete == true) {
            return;
        }

        if(paramInst == null) {
            ParameterHandler.Instance.AddParameters(parameterKey/*, true*/);
            paramInst = ParameterHandler.Instance.GetParameters(parameterKey);
        }
        
        //Game.ActiveParameters = paramInst;
        Widgets = new List<ParameterWidget>();
        CategoryObjects = new List<GameObject>();
        foreach (string category in paramInst.Categories.Keys) {
            AddCategory(category);
            foreach (GameParameter p in paramInst.Categories[category]) {
                SetupWidget(p);
            }
        }
        setUpComplete = true;
        RaiseOnSetupComplete( true );
    }

    void ActivateTheme() {
        GameParameters establishedParams = ParameterHandler.Instance.GetParameters(parameterKey);
        List<GameParameter> gameParams = establishedParams.Categories["Graphics"];
        string themeName = ((StringListParameter)gameParams[0]).Value;
       // Theme activeTheme = (Theme)System.Enum.Parse(typeof(Theme), themeName);
      //  ThemeController.ActivateTheme(activeTheme);
    }

    void AddCategory(string name) {
        GameObject categoryGO = (GameObject)Instantiate(CategoryPrefab, scrollViewRoot.transform);
		categoryGO.name = name;
        Text text = categoryGO.GetComponentInChildren<Text>();
        text.text = name;
        categoryGO.transform.SetParent(scrollViewRoot.transform);
        categoryGO.transform.localScale = Vector3.one;
        CategoryObjects.Add(categoryGO);
    }

    /// <summary>
    /// Instantiate widget, attach parameter
    /// </summary>
    /// <param name="parameter"></param>
    void SetupWidget(GameParameter parameter) {
        ParameterWidget widget = null;
        GameObject toAdd = RangeWidgetPrefab;
        if (parameter.GetType() == typeof(BoolParameter)) {
            toAdd = BoolWidgetPrefab;
        }
        if (parameter.GetType() == typeof(StringListParameter)) {
            toAdd = StringListPrefab;
        }
        widget = ((GameObject)Instantiate(toAdd, scrollViewRoot.transform)).GetComponent<ParameterWidget>();
		widget.name = parameter.Name + "-Widget";
        Widgets.Add(widget);
        widget.Setup(parameter);
        //PositionWidget(widget);
    }

    void PositionWidget(ParameterWidget widget) {
        if (widget == null) {
            return;
        }
        widget.transform.SetParent(scrollViewRoot.transform);
        widget.transform.localScale = Vector3.one;
    }

    public void LoadGame() {
        paramInst.Print();
        //Application.LoadLevel("game");//this is obsolete
		UnityEngine.SceneManagement.SceneManager.LoadScene("game");
    }
}
