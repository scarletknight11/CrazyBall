using UnityEngine;
using System.Collections;
using enableGame;
using System.IO;
//using System.Collections.Generic;
public class gameCreatorToMainMenu : MonoBehaviour {

	private GameObject _mainMenu;
	private GameObject _gameCreator;
	private GameObject _options;

	

	void Start(){
		_mainMenu = GameObject.Find ("MainMenu");
		_gameCreator = GameObject.Find ("GameCreatorPanel");
		_options = GameObject.Find ("options");

	

	}

	public void backToMainMenu(){
        SaveSukiSelection();
		print (_mainMenu);
		
		_mainMenu.SetActive (true);
		_gameCreator.SetActive (false);
	}


    public void SaveSukiSelection() {
        /*egString sukiFile = new egString();
        VariableHandler.Instance.Register(egParameterStrings.SUKI_FILELIST, sukiFile);
        egString sukiType = new egString();
        VariableHandler.Instance.Register(egParameterStrings.SUKI_TYPE, sukiType);*/
        print("SaveSukiSelection:" + egParameterStrings.SUKI_FILELIST);
        string sukiTypeName = ((StringListParameter)(ParameterHandler.Instance.GetParameters(egParameterStrings.WARMUP_NAME).GetParameter(egParameterStrings.SUKI_TYPE))).Value;
        string sukiFileName = ((StringListParameter)(ParameterHandler.Instance.GetParameters(egParameterStrings.WARMUP_NAME).GetParameter(egParameterStrings.SUKI_FILELIST))).Value;
        Suki.SukiSchemaList.Instance.Reset();
        string sukiFileWithExtension = "Suki/" + sukiTypeName + "/" + sukiFileName;

        //  string sukiFileWithExtension = "Suki\\" + sukiType + "\\" + sukiFile;
        Debug.Log("gamecreatortomainmenu:sukiFile" + " +++++++++++++++"+sukiFileName);
        /*
        //we check if it is trying to take the default.suki from "Arms", in that case it will redirect it to the correct default.suki path
        if (string.Compare(sukiFile, "arms\\LElbowAngle.suki") == 0)
        {
            Debug.Log("default was arms/default that is wrong");
            // Suki.SukiSchemaList.Instance.AddFile(Application.streamingAssetsPath + "\\Suki\\default.suki.json");
        }
        else
        {
        */
        Debug.Log("---------------------  " + sukiFileName + " ----------------------------");
            if (string.IsNullOrEmpty(sukiFileName))
            {
                sukiFileWithExtension = "Suki/" + sukiTypeName + "/default.suki";

            }
            string sukiwithpath = Path.Combine(Application.streamingAssetsPath, sukiFileWithExtension);
            Debug.Log("GameCreatorToMainMenu:SaveSukiSelection:Full suki name = " + sukiwithpath);
            Suki.SukiSchemaList.Instance.AddFile(sukiwithpath);
        //}
    }
}
