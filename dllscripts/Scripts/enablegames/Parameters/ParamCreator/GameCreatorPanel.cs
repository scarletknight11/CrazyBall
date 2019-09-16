using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameCreatorPanel : MonoBehaviour {
    [SerializeField]
    private GameParameterCreator[] gameCreators;
    public GameParameterCreator[] GameParameterCreators {
        get {
            return gameCreators;
        }
    }

    private int i = 0;

    private static GameCreatorPanel instance;
    public static GameCreatorPanel Instance {
        get {
            return instance;
        }
    }

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        this.gameObject.SetActive(false);

        SetCreatorNames( gameCreators );

        SetParentName( gameCreators, "Image" );
    }

    void SetCreatorNames( GameParameterCreator[] creators )
    {
        for ( int i = 0; i < creators.Length; i++ )
        {
            if(!creators[i].gameObject.name.Contains(" Panel" ) )
            {
                creators[i].gameObject.name += " Panel";
            }
        }
    }

    void SetParentName( GameParameterCreator[] creators , string name )
    {
        for ( int i = 0; i < creators.Length; i++ )
        {
            if(creators[i].gameObject.transform.GetChild( 0 ).transform.GetChild( 0 ).name != name)
            {
                creators[i].gameObject.transform.GetChild( 0 ).transform.GetChild( 0 ).name = name;
            }
        }
    }

    void OnEnable() {
        StartCoroutine(WaitAndActivate());
    }

    IEnumerator WaitAndActivate() {
        yield return new WaitForSeconds(GamePanel.TWEEN_TIME);
        ActivateIndex();
    }

    void ActivateIndex() {
        foreach (GameParameterCreator gc in gameCreators) {
            if (gc == gameCreators[i]) {
                gc.Activate();
            } else {
                gc.Deactivate();
            }
        }
        gameCreators[i].SetUp();
    }

    public void Next() {
        i++;
        if (i >= gameCreators.Length) {
            i = 0;
        }
        ActivateIndex();
    }

    public void Previous() {
        i--;
        if (i < 0) {
            i = gameCreators.Length - 1;
        }
        ActivateIndex();
    }
}
