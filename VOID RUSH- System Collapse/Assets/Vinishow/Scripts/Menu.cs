using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Menu : MonoBehaviour
{
    public Button iniciarButton;
    public GameObject menuPanel,tituloPanel;
    IEnumerator TempoCarregar()
    {
        iniciarButton.interactable = false;

        yield return new WaitForSeconds(1f);
        menuPanel.SetActive(true);
        iniciarButton.interactable = true;
        tituloPanel.SetActive(false);  

    }
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(1);
    }


    public void Playgame()
    {
      StartCoroutine(TempoCarregar());

    } 

    public void QuitGame()
    {
        Application.Quit();
    }

}
