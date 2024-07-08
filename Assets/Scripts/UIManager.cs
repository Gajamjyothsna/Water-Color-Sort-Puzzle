using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    #region Button Actions
    public void Level7ButtonClick()
    {
        SceneManager.LoadScene("Level7");
    }
    public void Level8ButtonClick()
    {
        SceneManager.LoadScene("Level8");
    }
    #endregion
}

