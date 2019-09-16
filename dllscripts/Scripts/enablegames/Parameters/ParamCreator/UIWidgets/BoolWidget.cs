using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BoolWidget : ParameterWidget {
	[SerializeField]
	private Toggle on;
	[SerializeField]
	private Toggle off;

	private BoolParameter boolParameter;

	[SerializeField]
	private List<string> gameObjectsToActivateName;
	public List<GameObject> gameObjectsToActivate;

	public void OnChange () {
		if (on.isOn) {
			UpdateParameter(true);
		}
	}
	
	public void OffChange () {
		if(off.isOn) {
			UpdateParameter(false);
		}
	}

	protected override void HandleGameParameterUpdateCheck (GameParameter parameter) {
        Set();
	}

    void Set() {
		gameObjectsToActivateName = boolParameter.gameObjectsToActivateName;
		Set(boolParameter.Value);
	}

	void Set(bool val) {
        if (val) {
            on.isOn = true;
            off.isOn = false;
        }
        else {
            on.isOn = false;
            off.isOn = true;
        }

        SetStateForGameObjects( val );

    }

    string GetObjectName( int i)
    {
        // return Game.ActiveParameters.Name + " Panel/ScrollView/Image/" + gameObjectsToActivateName[i];
        return base.name;
    }

    public void SetStateForGameObjects( bool state)
    {
        if ( gameObjectsToActivateName != null )
        {
            for ( int i = 0; i < gameObjectsToActivateName.Count; i++ )
            {
                GameObject obj = GameObject.Find( GetObjectName (i) );
                if ( obj != null )
                {
                    gameObjectsToActivate.Add( obj );
                    gameObjectsToActivate[i].SetActive( state );
                }
            }
        }
    }
	protected override void Initialize () {
		base.Initialize ();
		if(this.Parameter.GetType() != typeof(BoolParameter)) {
			throw new System.ApplicationException("Mismatch Widget and Parameter Type");
		}
		boolParameter = (BoolParameter) this.Parameter;

        Set();
    }

	public override void UpdateParameter (object o) {
		if(o.GetType() != typeof(bool)) {
			throw new System.ApplicationException("Mismatch Widget and Parameter Type");
		}
		boolParameter.Value = (bool)o;

        if ( gameObjectsToActivate != null )
        {
            if ( gameObjectsToActivate.Count > 0 )
            {
                for ( int i = 0; i < gameObjectsToActivate.Count; i++ )
                {
                    gameObjectsToActivate[i].SetActive( boolParameter.Value );
                }  
            }
        }
        base.UpdateParameter(o);
	}
}
