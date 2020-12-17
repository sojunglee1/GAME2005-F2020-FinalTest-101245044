using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManager : MonoBehaviour
{
    public void GoToMain()
    {
      UnityEngine.SceneManagement.SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }

    void Start ()
    {
        Cursor.visible = true;
    }
    private void Update() {
         if (Input.GetKeyDown(KeyCode.P))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Start", LoadSceneMode.Single);
        }
    }




}
