using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// NOVO: Garante que o GameObject sempre terá um AudioSource
[RequireComponent(typeof(AudioSource))]
public class NPC : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public string[] dialogue;
    private int index = 0;

    public float wordSpeed;
    public bool playerIsClose;

    // NOVO: Array para guardar os sons de cada diálogo.
    // O tamanho deve ser o mesmo do array 'dialogue'.
    public AudioClip[] dialogueAudio;

    // NOVO: Referência para o componente AudioSource.
    private AudioSource audioSource;


    void Start()
    {
        dialogueText.text = "";
        // NOVO: Pega o componente AudioSource no início do jogo.
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && playerIsClose)
        {
            if (!dialoguePanel.activeInHierarchy)
            {
                dialoguePanel.SetActive(true);
                StartCoroutine(Typing());
            }
            else if (dialogueText.text == dialogue[index])
            {
                NextLine();
            }

        }
        if (Input.GetKeyDown(KeyCode.F) && dialoguePanel.activeInHierarchy)
        {
            RemoveText();
        }
    }

    public void RemoveText()
    {
        dialogueText.text = "";
        index = 0;
        dialoguePanel.SetActive(false);
        // NOVO: Para o som quando o diálogo é fechado.
        audioSource.Stop();
    }

    IEnumerator Typing()
    {
        // --- INÍCIO DA MODIFICAÇÃO DE ÁUDIO ---
        // Para qualquer som que estivesse tocando antes.
        audioSource.Stop();

        // Verifica se existe um áudio para a linha atual.
        // A checagem "index < dialogueAudio.Length" evita erros se o array de audio for menor que o de diálogo.
        if (index < dialogueAudio.Length && dialogueAudio[index] != null)
        {
            audioSource.clip = dialogueAudio[index]; // Define o clipe de áudio atual.
            audioSource.loop = true; // O som ficará repetindo (ideal para sons de "fala"). Mude para 'false' se quiser que toque só uma vez.
            audioSource.Play(); // Toca o som.
        }
        // --- FIM DA MODIFICAÇÃO DE ÁUDIO ---

        foreach (char letter in dialogue[index].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(wordSpeed);
        }
    }

    public void NextLine()
    {
        if (index < dialogue.Length - 1)
        {
            index++;
            dialogueText.text = "";
            StartCoroutine(Typing());
        }
        else
        {
            RemoveText();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            RemoveText();
        }
    }
}