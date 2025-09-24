using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Enum para clareza no Inspector sobre o modo da sala.
public enum CameraRoomMode
{
    Unlocked,           // Jogador pode alternar entre Auto e Manual
    LockedAutomatic,    // For�a o modo Autom�tico
    LockedManual        // For�a o modo Manual
}

// Classe auxiliar para organizar as Zonas de Foco no Inspector.
[System.Serializable]
public class CameraFocusZone
{
    public string zoneName = "Nova Zona";
    public Collider2D framingCollider; // O "quadro" da c�mera (verde, vermelho, etc.)
    [Tooltip("Opcional: �rea onde o jogador precisa estar para ativar esta zona.")]
    public Collider2D proximityTrigger;

    public enum ActivationKey { None, LookUp, LookDown }
    [Tooltip("Qual tecla ativa esta zona? 'None' a define como a zona padr�o da sala.")]
    public ActivationKey activationKey = ActivationKey.None;

    [Tooltip("Para zonas de 'olhar', limita o qu�o longe a c�mera vai (0 a 1). 0.65 = 65% do caminho.")]
    [Range(0f, 1f)]
    public float peekDistanceLimit = 0.65f;
}

[RequireComponent(typeof(Collider2D))]
public class RoomBoundary : MonoBehaviour
{
    // === SE��O 1: MODO DE C�MERA ===
    [Header("1. Modo da C�mera na Sala")]
    [Tooltip("Define como a c�mera se comporta nesta sala.")]
    public CameraRoomMode roomMode = CameraRoomMode.Unlocked;

    // === SE��O 2: CONFIGURA��ES GERAIS ===
    [Header("2. Configura��es Gerais")]
    [Tooltip("O Collider2D que define os limites M�XIMOS da c�mera nesta sala para o Cinemachine Confiner.")]
    public Collider2D confinerBounds;

    // === SE��O 3: ZONAS DE FOCO ===
    [Header("3. Zonas de Foco da C�mera")]
    public List<CameraFocusZone> focusZones;

    // === SE��O 4: CONFIGURA��ES DE MODOS ===
    [Header("4. Configura��es de Modo Autom�tico")]
    [Tooltip("Fator de 'respiro' para o zoom. 1.1 = 10% de padding.")]
    public float automaticZoomPadding = 1.1f;
    [Tooltip("A velocidade da transi��o de zoom ao entrar/sair de zonas.")]
    public float zoomTransitionSpeed = 2f;

    [Header("5. Configura��es de Modo Manual")]
    [Tooltip("O qu�o perto o jogador pode dar zoom in.")]
    public float manualMinZoom = 4f;
    [Tooltip("A velocidade do zoom manual com as teclas +/-.")]
    public float manualZoomSpeed = 3f;
    [Tooltip("O tempo em segundos para segurar a tecla antes de 'olhar' para cima/baixo.")]
    public float timeToActivatePeek = 1f;
    [Tooltip("A velocidade com que a c�mera se move para a posi��o de 'olhar'.")]
    public float peekTransitionSpeed = 2f;

    // --- C�REBRO E ESTADO GLOBAL (Est�tico) ---
    private static CinemachineConfiner2D activeConfiner;
    private static CinemachineCamera activeVirtualCamera;
    private static RoomBoundary currentActiveRoom;
    private static Coroutine activeTransitionCoroutine;
    public enum PlayerCameraPreference { Automatic, Manual }
    private static PlayerCameraPreference playerPreference = PlayerCameraPreference.Automatic;
    // --- FIM DO C�REBRO ---

    // --- ESTADO LOCAL DA INST�NCIA ---
    private PlayerCameraPreference currentOperatingMode;
    private CameraFocusZone defaultZone;
    private bool isPeeking = false;
    private LensSettings prePeekLens; // Salva o estado da c�mera antes de "olhar"
    private float peekTimer = 0f;
    private CameraFocusZone.ActivationKey currentPeekKey = CameraFocusZone.ActivationKey.None;
    // --- FIM DO ESTADO LOCAL ---

    #region Setup e Eventos de Trigger

    private void Awake()
    {
        // Garante que o collider principal seja um trigger.
        if (confinerBounds != null) confinerBounds.isTrigger = true;

        // Encontra a zona padr�o (marcada como 'None')
        defaultZone = focusZones.FirstOrDefault(z => z.activationKey == CameraFocusZone.ActivationKey.None);
        if (defaultZone == null && focusZones.Count > 0)
        {
            defaultZone = focusZones[0]; // Se nenhuma for marcada, a primeira vira padr�o.
            Debug.LogWarning($"A sala '{gameObject.name}' n�o tem uma Zona de Foco Padr�o definida. Usando a primeira da lista: '{defaultZone.zoneName}'.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentActiveRoom != this)
        {
            EnterRoom();
        }
    }

    #endregion

    #region Input e L�gica de Update (O CORA��O DO SCRIPT)

    // --- FUN��O UPDATE CORRIGIDA ---
    private void Update()
    {
        // O Update s� executa para a sala que est� ativa no momento.
        if (currentActiveRoom != this) return;

        // --- L�gica de troca de modo (Tecla T) ---
        if (Input.GetKeyDown(KeyCode.T) && roomMode == CameraRoomMode.Unlocked && !isPeeking)
        {
            playerPreference = (playerPreference == PlayerCameraPreference.Automatic) ? PlayerCameraPreference.Manual : PlayerCameraPreference.Automatic;
            DetermineOperatingMode(); // Reavalia o modo da c�mera
            Debug.Log($"Modo da C�mera alterado para: {currentOperatingMode}");
        }

        // --- L�gica do "Modo Buraco" / "Peek" (Teclas W/S) ---
        CameraFocusZone.ActivationKey intendedPeekKey = CameraFocusZone.ActivationKey.None;
        if (Input.GetKey(KeyCode.S)) intendedPeekKey = CameraFocusZone.ActivationKey.LookDown;
        else if (Input.GetKey(KeyCode.W)) intendedPeekKey = CameraFocusZone.ActivationKey.LookUp;

        if (intendedPeekKey != CameraFocusZone.ActivationKey.None)
        {
            if (!isPeeking)
            {
                peekTimer += Time.deltaTime;
                if (peekTimer >= timeToActivatePeek)
                {
                    StartPeek(intendedPeekKey);
                }
            }
        }
        else
        {
            peekTimer = 0f;
            if (isPeeking)
            {
                StopPeek();
            }
        }

        if (isPeeking) return;

        // --- L�GICA DE ZOOM MANUAL (APENAS TECLAS +/-) ---
        if (currentOperatingMode == PlayerCameraPreference.Manual)
        {
            // Resetamos a vari�vel para garantir que n�o haja lixo de frames anteriores.
            float zoomInput = 0f;

            // Verificamos o input APENAS dos bot�es + e -.
            if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus))
            {
                zoomInput = -0.1f; // Diminuir o OrthographicSize = Zoom IN
            }
            if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
            {
                zoomInput = 0.1f; // Aumentar o OrthographicSize = Zoom OUT
            }

            // A l�gica de aplica��o do zoom s� roda se uma das teclas foi pressionada.
            if (zoomInput != 0 && activeVirtualCamera != null)
            {
                float currentSize = activeVirtualCamera.Lens.OrthographicSize;
                float maxZoom = CalculateOptimalLensForZone(defaultZone).OrthographicSize;
                float newSize = currentSize + zoomInput * manualZoomSpeed;
                activeVirtualCamera.Lens.OrthographicSize = Mathf.Clamp(newSize, manualMinZoom, maxZoom);
            }
        }
    }

    #endregion

    #region L�gica da C�mera e Transi��es

    // --- FUN��O EnterRoom - VERS�O CORRETA E SIMPLIFICADA ---
    private void EnterRoom()
    {
        currentActiveRoom = this;
        InitializeCameraReferences();
        Debug.Log($"Entrando na sala '{gameObject.name}'.");

        // Aplica o limite de movimento geral. Isso � a primeira prioridade.
        if (activeConfiner != null)
        {
            if (confinerBounds != null)
            {
                activeConfiner.BoundingShape2D = confinerBounds;
                Debug.Log($"Limite do Confiner definido para '{confinerBounds.name}'.");
            }
            else
            {
                Debug.LogError($"A sala '{gameObject.name}' nao tem um 'Confiner Bounds' definido no Inspector!", this);
            }
        }

        // Agora, determina o comportamento da c�mera.
        DetermineOperatingMode();
    }

    // --- FUN��O DetermineOperatingMode - L�GICA FINAL ---
    private void DetermineOperatingMode()
    {
        // Decide qual modo a c�mera vai usar baseado na configura��o da sala e na prefer�ncia do jogador.
        if (roomMode == CameraRoomMode.LockedAutomatic)
        {
            currentOperatingMode = PlayerCameraPreference.Automatic;
        }
        else if (roomMode == CameraRoomMode.LockedManual)
        {
            currentOperatingMode = PlayerCameraPreference.Manual;
        }
        else // Unlocked
        {
            currentOperatingMode = playerPreference;
        }

        Debug.Log($"Modo operacional definido para: {currentOperatingMode}.");

        // Executa a a��o correta com base no modo determinado.
        if (currentOperatingMode == PlayerCameraPreference.Automatic)
        {
            // MODO AUTOM�TICO: Enquadra a Zona de Foco Padr�o.
            if (defaultZone != null)
            {
                TransitionToZone(defaultZone);
            }
            else
            {
                Debug.LogError($"A sala '{gameObject.name}' est� em Modo Autom�tico mas n�o tem uma Zona de Foco Padr�o!", this);
            }
        }
        else // MODO MANUAL
        {
            // MODO MANUAL: Come�a com o zoom m�ximo (enquadrando o confiner)
            // e permite que o jogador assuma o controle a partir da�.
            // Para isso, criamos uma "zona virtual" na hora.
            if (confinerBounds != null)
            {
                CameraFocusZone maxZoomZone = new CameraFocusZone { framingCollider = this.confinerBounds };
                TransitionToZone(maxZoomZone);
            }
        }
    }

    private void StartPeek(CameraFocusZone.ActivationKey key)
    {
        // Encontra uma zona que corresponda � tecla e � proximidade do jogador.
        var targetZone = focusZones.FirstOrDefault(z => z.activationKey == key && (z.proximityTrigger == null || z.proximityTrigger.bounds.Contains(GameObject.FindGameObjectWithTag("Player").transform.position)));

        if (targetZone != null)
        {
            Debug.Log($"Iniciando 'Peek' para a zona '{targetZone.zoneName}'.");
            isPeeking = true;
            currentPeekKey = key;
            prePeekLens = activeVirtualCamera.Lens; // Salva o estado da lente

            // Inicia a transi��o para o enquadramento limitado do "buraco".
            TransitionToZone(targetZone, true);
        }
    }

    private void StopPeek()
    {
        Debug.Log("Parando 'Peek' e retornando ao estado anterior.");
        isPeeking = false;
        currentPeekKey = CameraFocusZone.ActivationKey.None;

        // Retorna a c�mera para o enquadramento da zona padr�o.
        TransitionToZone(defaultZone);
    }

    // --- FUN��O CORRIGIDA ---
    private void TransitionToZone(CameraFocusZone zone, bool isPeek = false)
    {
        if (zone == null || zone.framingCollider == null || activeVirtualCamera == null) return;
        if (currentActiveRoom == null)
        {
            Debug.LogError("TransitionToZone foi chamada, mas n�o h� uma sala ativa (currentActiveRoom is null)!");
            return;
        }

        if (activeTransitionCoroutine != null)
        {
            StopCoroutine(activeTransitionCoroutine);
        }

        // --- A CORRE��O PRINCIPAL ---
        // Acessamos as vari�veis de velocidade diretamente da inst�ncia da sala ativa (currentActiveRoom).
        // Isso remove qualquer ambiguidade sobre de onde os valores est�o vindo.
        float speed = isPeek ? currentActiveRoom.peekTransitionSpeed : currentActiveRoom.zoomTransitionSpeed;

        // Calcula a lente alvo. Se for um "peek", aplica o limite de vis�o.
        LensSettings targetLens = CalculateOptimalLensForZone(zone);
        if (isPeek)
        {
            // Interpola entre o zoom atual e o zoom do buraco para respeitar o limite de 65%.
            targetLens.OrthographicSize = Mathf.Lerp(activeVirtualCamera.Lens.OrthographicSize, targetLens.OrthographicSize, zone.peekDistanceLimit);
        }

        activeTransitionCoroutine = StartCoroutine(SmoothTransitionCoroutine(targetLens, speed));
    }

    // Corrotina que anima a lente da c�mera.
    // --- CORROTINA DEFINITivamente CORRIGIDA ---
    private IEnumerator SmoothTransitionCoroutine(LensSettings targetLens, float speed)
    {
        float startSize = activeVirtualCamera.Lens.OrthographicSize;
        float targetSize = targetLens.OrthographicSize;
        float t = 0;

        // Armazena a refer�ncia da sala que INICIOU esta corrotina.
        RoomBoundary originatingRoom = this;

        // O LOOP AGORA TEM UMA CONDI��O DE SEGURAN�A.
        while (t < 1 && currentActiveRoom == originatingRoom)
        {
            t += Time.deltaTime * speed;
            float newSize = Mathf.Lerp(startSize, targetSize, t);

            var lens = activeVirtualCamera.Lens;
            lens.OrthographicSize = newSize;
            activeVirtualCamera.Lens = lens;

            yield return null;
        }

        // S� FINALIZA se a corrotina terminou naturalmente, na mesma sala.
        if (t >= 1 && currentActiveRoom == originatingRoom)
        {
            var finalLens = activeVirtualCamera.Lens;
            finalLens.OrthographicSize = targetLens.OrthographicSize;
            activeVirtualCamera.Lens = finalLens;
        }

        // Limpa a refer�ncia da corrotina.
        activeTransitionCoroutine = null;
    }

    private LensSettings CalculateOptimalLensForZone(CameraFocusZone zone)
    {
        if (zone == null || zone.framingCollider == null || currentActiveRoom == null || currentActiveRoom.confinerBounds == null)
        {
            // Se faltar alguma informa��o essencial, retorna a lente atual para evitar erros.
            return activeVirtualCamera.Lens;
        }

        // --- C�LCULO DO TETO ABSOLUTO (BASEADO NO CONFINE) ---
        Bounds confinerBounds = currentActiveRoom.confinerBounds.bounds;
        float screenRatio = (float)Screen.width / Screen.height;
        float maxRequiredSizeX = (confinerBounds.size.x / screenRatio) / 2f;
        float maxRequiredSizeY = confinerBounds.size.y / 2f;
        float maxAllowedZoom = Mathf.Max(maxRequiredSizeX, maxRequiredSizeY);

        // --- C�LCULO DO ZOOM DESEJADO (BASEADO NA ZONA DE FOCO) ---
        Bounds zoneBounds = zone.framingCollider.bounds;
        float zoneRequiredSizeX = (zoneBounds.size.x / screenRatio) / 2f;
        float zoneRequiredSizeY = zoneBounds.size.y / 2f;
        float zoneOptimalSize = Mathf.Max(zoneRequiredSizeX, zoneRequiredSizeY);

        // --- DECIS�O FINAL E APLICA��O DA REGRA ---
        // Garante que o zoom da zona NUNCA seja maior que o zoom m�ximo permitido.
        // E aplica o padding de forma segura.
        float finalSize = Mathf.Min(zoneOptimalSize, maxAllowedZoom) * automaticZoomPadding;

        return new LensSettings { OrthographicSize = finalSize };
    }
    private static void InitializeCameraReferences()
    {
        if (activeVirtualCamera == null)
        {
            GameObject vcamObject = GameObject.FindGameObjectWithTag("VirtualCamera");
            if (vcamObject != null)
            {
                activeVirtualCamera = vcamObject.GetComponent<CinemachineCamera>();
                activeConfiner = vcamObject.GetComponent<CinemachineConfiner2D>();
            }
        }
    }

    #endregion
}