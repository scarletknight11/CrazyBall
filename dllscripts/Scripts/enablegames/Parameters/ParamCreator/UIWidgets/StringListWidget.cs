using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using enableGame;
using OldMoatGames;


public class StringListWidget : ParameterWidget
{

    [SerializeField]
    private GameObject choice;
    [SerializeField]
    private GameObject fileChoice;
    [SerializeField]
    private ToggleGroup toggleGroup;
    [SerializeField]
    private Transform scrollRoot;
    private GameObject gif;

    private List<Toggle> toggleList = new List<Toggle>();

    private StringListParameter listParameter;

    private AnimatedGifPlayer AnimatedGifPlayer;

    /****
     * SUKI PARAMETER VARIABLES
     ****/
    private bool sukiParameter = false;
    private egString selectedFolder = new egString();
    private egString oldFolder = new egString();

    public RectTransform textRT;
    Text text;
    string sukiPath = "suki_icons/";

    void InitalizeFromStrings(List<string> strings)
    {
        RectTransform rootRect = (RectTransform)scrollRoot.transform;
        int i = 0;
        float xDist = 0f;
        for (i = 0; i < strings.Count; i++)
        {
            GameObject instantiatedChoice;
            if (sukiParameter)
            {
                instantiatedChoice = (GameObject)Instantiate(fileChoice,scrollRoot);
            }
            else
            {
                instantiatedChoice = (GameObject)Instantiate(choice,scrollRoot);
            }
            RectTransform instantiatedRect = instantiatedChoice.GetComponent<RectTransform>();
            Text t = instantiatedChoice.GetComponentInChildren<Text>();


            Toggle toggle = instantiatedChoice.GetComponentInChildren<Toggle>();
            if (!sukiParameter)
            {
                t.text = strings[i];
            }
            else
            {
                string[] s = strings[i].Split('\\');
                string l = s[s.Length - 1];//.ToLower();
                t.text = l;
                Debug.Log("StringListWidget:InitializeFromStrings:" + l);
            }
            xDist += instantiatedRect.rect.width;
//            instantiatedChoice.transform.SetParent(scrollRoot);

            // set all toggles to false initially so that onValueChanged fires for the defaults when we turn them on
            toggle.isOn = false;
            // must add to group after, since "allow switch off" disabled in toggle group will force first toggle to stay on
            toggle.group = toggleGroup;
            toggleList.Add(toggle);
            // set our default
            if (strings[i].Equals(listParameter.Value))
            {
                toggle.isOn = true;

            }

            toggle.onValueChanged.AddListener(ButtonToggled);
            string[] spriteFileParts = strings[i].Split('\\');
            string spriteFile = spriteFileParts[spriteFileParts.Length - 1].ToLower();
            spriteFile = spriteFile.Split('.')[0];


            //	SetIcon(t.transform.parent, strings[i], () => { t.color = Color.clear; });
            SetIconFromResources(t.transform.parent, spriteFile, () => { t.color = Color.red; t.alignment = TextAnchor.LowerLeft; });
            StartCoroutine(SetIconFromStreaming(t.transform.parent, spriteFile, () => { t.color = Color.red; t.alignment = TextAnchor.LowerLeft; }));//, icon));

        }

        rootRect.sizeDelta = new Vector2(xDist, rootRect.sizeDelta.y);
        gif = GameObject.Find("GIF");
        text = gif.GetComponentInChildren<Text>();
        playGif("default.suki");

    }

    /// <summary>
    /// Hard coded method to show only SUKI files in certain folder
    /// </summary>
    /// <param name="selection">select the folder to show</param>
    public void showOnlySelected(string selection)
    {
        bool b = true;
        for (int i = 0; i < listParameter.Strings.Count; i++)
        {
            //only show items under category (i.e. arms)
            if (string.Compare(listParameter.Strings[i].Split('\\')[0].ToLower(), selection.ToLower()) == 0)
            {
                //print("TURNING ON " + listParameter.Strings[i]);

                //active status MUST be changed first before isOn changed
                //because isOn will check active list
                toggleList[i].gameObject.SetActive(true);
                toggleList[i].isOn = b;
                b = false;
                //print("TURNED ON " + listParameter.Strings[i]);
            }
            else
            {
                //print("TURNING OFF " + listParameter.Strings[i]);
                //active status MUST be changed first before isOn changed
                //because isOn will check active list
                toggleList[i].gameObject.SetActive(false);
                toggleList[i].isOn = false;
                //print("TURNED OFF " + listParameter.Strings[i]);
            }
        }

    }

    private int count = 0;
    public void FixedUpdate()
    {
        if (listParameter.Name == egParameterStrings.SUKI_FILELIST)
        {
            /*  if (count++ > 200) //change to use time instead of counter
              {
                  count = 0;
                  gif.transform.localScale = new Vector3(1.0f, 1.0f, 1f);
              }*/
            if (selectedFolder.updated)
            {
                selectedFolder.updated = false;
                //print("========================NEW selected folder=" + selectedFolder);
                showOnlySelected(selectedFolder);
            }
        }
    }

    public void ButtonToggled(bool t)
    {
        //print("ButtonToggled entered{"+ t);
        if (t)
        {
            Toggle toggle = null;
            foreach (Toggle tg in toggleGroup.ActiveToggles())
            {
                toggle = tg;
                //print("ButtonToggled to true: " + toggle.GetComponentInChildren<Text>().text);

            }
            string butText = "";
            try
            {
                butText = toggle.GetComponentInChildren<Text>().text;
            }
            catch
            {

            }
            print("StringListWidget:ButTog=" + butText);
            UpdateParameter(butText);
            if (sukiParameter)
                playGif(butText);
        }
        //print("}ButtonToggled exit");
    }

    protected override void HandleGameParameterUpdateCheck(GameParameter parameter)
    {
        if (parameter == listParameter)
        {
            throw new System.NotImplementedException();
        }
    }

    protected override void Initialize()
    {
        base.Initialize();
        this.listParameter = (StringListParameter)this.Parameter;
        // hard coded part for suki -- we will delete this part when we have a different way to select a suki schema 
        if (listParameter.Name == egParameterStrings.SUKI_FILELIST || listParameter.Name == egParameterStrings.SUKI_TYPE)
        {

            sukiParameter = true;
            AnimatedGifPlayer = GetComponent<AnimatedGifPlayer>();

            if (listParameter.Name == egParameterStrings.SUKI_FILELIST)
            {
                VariableHandler.Instance.Register(egParameterStrings.SUKI_TYPE, selectedFolder);
                oldFolder = selectedFolder;
            }
        }

        if (this.Parameter.GetType() != typeof(StringListParameter))
        {
            throw new System.ApplicationException("Mismatch Widget and Parameter Type");
        }

        this.InitalizeFromStrings(this.listParameter.Strings);
    }

    void playGif(string o)
    {
        string spritePath = (string)o;
        string[] spriteFileParts = spritePath.Split('\\');
        string spriteFile = spriteFileParts[spriteFileParts.Length - 1].ToLower();
        spriteFile = spriteFile.Split('.')[0];
        if (text != null)
            text.text = "Body Profile: " + spriteFile;
        string gifFile = sukiPath + spriteFile + ".gif";
        //print("gifFile=" + gifFile);
        gif = GameObject.Find("GIF");
        //gif.transform.localScale = new Vector3(6.0f, 6.0f, 1f);
        AnimatedGifPlayer = gif.GetComponentInChildren<AnimatedGifPlayer>();

        AnimatedGifPlayer.Pause();
        AnimatedGifPlayer.FileName = gifFile;// "AnimatedGIFPlayerExampe 1.gif";
        AnimatedGifPlayer.AutoPlay = true;
        AnimatedGifPlayer.Init();
    }

    public override void UpdateParameter(object o)
    {
        if (o.GetType() != typeof(string))
        {
            throw new System.ApplicationException("Mismatch Widget and Parameter Type");
        }
        listParameter.Value = (string)o;
        print("listParam name=" + listParameter.Name);
        print("listParam=" + listParameter.Value);
        base.UpdateParameter(o);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    //public IEnumerator 
    void SetIconFromResources(Transform parentTransform, string spriteFile, Action onSuccess)
    {
        spriteFile = "Suki/icon_" + spriteFile;
        if (!string.IsNullOrEmpty(spriteFile))
        {
            Sprite sprite = Resources.Load<Sprite>(spriteFile);

            if (sprite != null)// && sprite.Length > 0)
            {
                print("spritePath found icon for =" + spriteFile);

                GameObject icon = new GameObject("Icon");
                icon.transform.parent = parentTransform;
                icon.AddComponent<Image>();
                icon.GetComponent<Image>().sprite = sprite;//[0];
                RectTransform rect = icon.GetComponent<RectTransform>();
                rect.localScale = Vector3.one;
                rect.anchorMin = new Vector2(.15f, .35f);
                rect.anchorMax = new Vector2(.85f, .9f);
                rect.offsetMin = new Vector2(0, 0);
                rect.offsetMax = new Vector2(1, 1);

                if (onSuccess != null)
                {
                    onSuccess();
                }
            }
        }
    }

    //sets icon from StreamingAssets folder (asynchronously, since the file might take a while to load)
    public IEnumerator SetIconFromStreaming(Transform parentTransform, string spriteFile, Action onSuccess)
    //string absoluteImagePath,/*GameObject icon,*/ Action onSuccess)
    {
        string finalPath;
        WWW localFile;
        Texture texture;
        Sprite sprite;
        spriteFile = sukiPath + spriteFile;
        string absoluteImagePath = spriteFile + ".png";
        //print("loadSprite" + absoluteImagePath);
        finalPath = "file://" + System.IO.Path.Combine(Application.streamingAssetsPath, absoluteImagePath);
        //print("here0:" + absoluteImagePath);  
        localFile = new WWW(finalPath);
        //print("here1:" + absoluteImagePath);  //BUG!!! will not work without this print statement!!!! WHY???
        //MORE INFO ON BUG:  Works if call to SetIconFromResources is commented out.
        //WORKS if yield return below is removed.
        while (!localFile.isDone)
        {
            //            print("here3:" + absoluteImagePath);  
            //yield return 
            //             new WaitForSeconds(1.0f);
            //           print("here4:" + absoluteImagePath);  
        }
        //yield return localFile;

        if (!string.IsNullOrEmpty(localFile.error))
        {
            //            Debug.Log(localFile.error);
            yield break;
        }
        else
        {
            //            print("foundSprite" + absoluteImagePath);
            texture = localFile.texture;
            sprite = Sprite.Create(texture as Texture2D, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
            GameObject icon = new GameObject("Icon");
            icon.transform.parent = parentTransform;
            icon.AddComponent<Image>();
            icon.GetComponent<Image>().sprite = sprite;//[0];
            RectTransform rect = icon.GetComponent<RectTransform>();
            // rect.localScale = Vector3.one;
            // rect.anchorMin = new Vector2(.15f, .35f);
            //  rect.anchorMax = new Vector2(.85f, .9f);
            // rect.offsetMin = new Vector2(0, 0);
            //  rect.offsetMax = new Vector2(1, 1);
            rect.localScale = new Vector3(0.9f, 0.5f, 1f);
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(0, 0);
            rect.offsetMax = new Vector2(1, 1);
            if (onSuccess != null)
            {
                onSuccess();
            }
        }
        yield return 0;
    }

}
