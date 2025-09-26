using System.Collections;
using UnityEngine;
using TMPro;
using UnityEditor.PackageManager;
using UnityEditor.Search;

public class TutorialCutsceneManager : MonoBehaviour
{
    [Header("Refer�ncias da Cena")]
    public TextMeshProUGUI subtitleTextUI;
    public GameObject player;
    public MonoBehaviour playerController;
    public GameObject fadePanel;
    public AudioSource voiceAudioSource;
    public AudioSource sfxAudioSource;

    [Header("Configura��es da Cutscene")]
    public float textTypingSpeed = 50.0f;
    [Tooltip("O tempo de espera obrigat�rio entre o fim de um di�logo e o in�cio do pr�ximo.")]
    public float dialogueCooldown = 1.5f;

    [Header("Clipes de �udio (Opcionais)")]
    public AudioClip staticSound;
    public AudioClip droneExplosionSound;
    // ... (outros clipes de �udio)

    // Vari�veis internas
    private bool advanceDialogue = false;
    private Rigidbody2D playerRb; // Refer�ncia para o Rigidbody do jogador

    void Start()
    {
        if (subtitleTextUI != null) { subtitleTextUI.gameObject.SetActive(false); }
        else { Debug.LogError("ERRO: 'subtitleTextUI' n�o definido!"); this.enabled = false; return; }

        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>(); // Pega o componente Rigidbody
        }

        if (playerController != null) playerController.enabled = false;

        StartCoroutine(TutorialSequence());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) { advanceDialogue = true; }
    }

    // --- Corrotina Principal da Sequ�ncia ---
    private IEnumerator TutorialSequence()
    {
        fadePanel.SetActive(true);
        PlaySFX(staticSound, true);
        yield return new WaitForSeconds(1.5f);

        yield return PlayDialogue("Terminus", "...erro de sintaxe... a memória de um sorriso... quebra a lógica... apagar...", null);
        yield return new WaitForSeconds(dialogueCooldown); // NOVO: Cooldown

        fadePanel.SetActive(false);
        yield return new WaitForSeconds(1f);

        yield return PlayDialogue("Leo", "E ela vive! Eu disse para o Jonas que cutucar com a vara certa ia funcionar! Ei, V03, bem-vinda de volta ao caos. Consegue me ouvir bem? Pisque uma luz ou algo assim se a recepção estiver boa. Ou só se mova, isso também serve.", null);
        yield return new WaitForSeconds(dialogueCooldown); // NOVO: Cooldown

        // NOVO: Chama a rotina espec�fica do tutorial de movimento
        yield return MovementTutorialSection();
        yield return new WaitForSeconds(dialogueCooldown); // NOVO: Cooldown

        yield return PlayDialogue("Leo", "Isso! Seus servos estão perfeitos. Rápida, ágil... você não parece ter ferrugem de cemitério. Continue assim.", null);
        yield return new WaitForSeconds(dialogueCooldown); // NOVO: Cooldown

        yield return PlayDialogue("Terminus", "...relíquia quebrada... um eco de dor...", null);
        yield return new WaitForSeconds(dialogueCooldown); // NOVO: Cooldown

        yield return PlayDialogue("Leo", "Ah, não ligue para o fantasma na máquina. É a \"Estática\" da Zona, adora sussurrar besteiras no ouvido dos outros. Parece que a Ambit te deixou um presentinho de boas-vindas ali na frente. Um drone de segurança. Mostre a ele que você não está com humor para festas.", null);
        yield return new WaitForSeconds(dialogueCooldown); // NOVO: Cooldown

        // --- DESTRUI��O DO DRONE COM FADE ---
        fadePanel.SetActive(true); // NOVO: Tela escurece
        StopSFX();
        yield return PlayDialogue("", "[... sons de combate e uma explosão ao longe ...]", droneExplosionSound); // Note que o nome do personagem est� vazio
        fadePanel.SetActive(false); // NOVO: Tela clareia
        yield return new WaitForSeconds(dialogueCooldown); // NOVO: Cooldown

        yield return PlayDialogue("Leo", "É DISSO que eu tô falando! Desmontou o bicho como se fosse de papelão. Meu nome é Leo, aliás. A gente da Insurreição te tirou do seu sono de beleza. Acreditamos que você é o despertador que essa cidade precisa. Tem uma saída de ventilação no final desse corredor. Estou te esperando. Não se atrase.\r\n", null);

        // --- FIM DA CUTSCENE ---
        if (playerController != null) playerController.enabled = true;
        Debug.Log("Fim da cutscene do tutorial! Controles devolvidos.");

        this.enabled = false;
    }

    // --- Nova Corrotina para o Tutorial de Movimento ---
    private IEnumerator MovementTutorialSection()
    {
        subtitleTextUI.text = "[Pressione W, A, S e D para calibrar os servos]";
        subtitleTextUI.gameObject.SetActive(true);
        if (playerController != null) playerController.enabled = true;

        bool pressedW = false;
        bool pressedA = false;
        bool pressedS = false;
        bool pressedD = false;

        // Loop que s� termina quando todas as 4 teclas forem pressionadas
        while (!(pressedW && pressedA && pressedS && pressedD))
        {
            if (Input.GetKeyDown(KeyCode.W)) pressedW = true;
            if (Input.GetKeyDown(KeyCode.A)) pressedA = true;
            if (Input.GetKeyDown(KeyCode.S)) pressedS = true;
            if (Input.GetKeyDown(KeyCode.D)) pressedD = true;
            yield return null; // Espera o pr�ximo frame
        }

        // Assim que o loop termina, congela o jogador imediatamente
        if (playerController != null) playerController.enabled = false;
        if (playerRb != null) playerRb.linearVelocity = Vector2.zero; // For�a a parada do movimento

        subtitleTextUI.gameObject.SetActive(false);
    }

    // --- Corrotina de Di�logo (com a mesma l�gica de avan�o de antes) ---
    private IEnumerator PlayDialogue(string character, string text, AudioClip voiceClip)
    {
        // Constr�i o texto a ser exibido
        string fullText = (string.IsNullOrEmpty(character)) ? text : $"<b>{character}:</b> {text}";

        subtitleTextUI.text = "";
        subtitleTextUI.gameObject.SetActive(true);

        if (voiceClip != null) PlayVoice(voiceClip);

        // Loop de digita��o
        float t = 0; int charIndex = 0;
        while (charIndex < fullText.Length) { if (advanceDialogue) { advanceDialogue = false; break; } t += Time.deltaTime * textTypingSpeed; charIndex = Mathf.FloorToInt(t); charIndex = Mathf.Clamp(charIndex, 0, fullText.Length); subtitleTextUI.text = fullText.Substring(0, charIndex); yield return null; }

        subtitleTextUI.text = fullText;
        advanceDialogue = false;
        yield return new WaitUntil(() => advanceDialogue);

        advanceDialogue = false;
        if (voiceAudioSource != null) voiceAudioSource.Stop();

        subtitleTextUI.gameObject.SetActive(false);
    }

    // --- Fun��es Auxiliares de �udio (� prova de falhas) ---
    private void PlayVoice(AudioClip clip) { if (voiceAudioSource != null && clip != null) { voiceAudioSource.Stop(); voiceAudioSource.clip = clip; voiceAudioSource.Play(); } }
    private void PlaySFX(AudioClip clip, bool loop = false) { if (sfxAudioSource != null && clip != null) { sfxAudioSource.Stop(); sfxAudioSource.clip = clip; sfxAudioSource.loop = loop; sfxAudioSource.Play(); } }
    private void StopSFX() { if (sfxAudioSource != null) { sfxAudioSource.Stop(); } }
}