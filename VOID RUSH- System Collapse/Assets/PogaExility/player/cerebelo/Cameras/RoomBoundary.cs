using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Enum para clareza no Inspector sobre o modo da sala.
public enum CameraRoomMode
{
    Unlocked,           // Jogador pode alternar entre Auto e Manual
    LockedAutomatic,    // Força o modo Automático
    LockedManual        // Força o modo Manual
}

// Classe auxiliar para organizar as Zonas de Foco no Inspector.
[System.Serializable]
public class CameraFocusZone
{
    public string zoneName = "Nova Zona";
    public Collider2D framingCollider; // O "quadro" da câmera (verde, vermelho, etc.)
    [Tooltip("Opcional: área onde o jogador precisa estar para ativar esta zona.")]
    public Collider2D proximityTrigger;

    public enum ActivationKey { None, LookUp, LookDown }
    [Tooltip("Qual tecla ativa esta zona? 'None' a define como a zona padrão da sala.")]
    public ActivationKey activationKey = ActivationKey.None;

    [Tooltip("Para zonas de 'olhar', limita o quão longe a câmera vai (0 a 1). 0.65 = 65% do caminho.")]
    [Range(0f, 1f)]
    public float peekDistanceLimit = 0.65f;
}

[RequireComponent(typeof(Collider2D))]
public class RoomBoundary : MonoBehaviour
{
    // === SEÇÃO 1: MODO DE CÂMERA ===
    [Header("1. Modo da Câmera na Sala")]
    [Tooltip("Define como a câmera se comporta nesta sala.")]
    public CameraRoomMode roomMode = CameraRoomMode.Unlocked;

    // === SEÇÃO 2: CONFIGURAÇÕES GERAIS ===
    [Header("2. Configurações Gerais")]
    [Tooltip("O Collider2D que define os limites MÁXIMOS da câmera nesta sala para o Cinemachine Confiner.")]
    public Collider2D confinerBounds;

    // === SEÇÃO 3: ZONAS DE FOCO ===
    [Header("3. Zonas de Foco da Câmera")]
    public List<CameraFocusZone> focusZones;

    // === SEÇÃO 4: CONFIGURAÇÕES DE MODOS ===
    [Header("4. Configurações de Modo Automático")]
    [Tooltip("Fator de 'respiro' para o zoom. 1.1 = 10% de padding.")]
    public float automaticZoomPadding = 1.1f;

    [Header("5. Configurações de Modo Manual")]
    [Tooltip("O quão perto o jogador pode dar zoom in.")]
    public float manualMinZoom = 4f;
    [Tooltip("A velocidade do zoom manual com as teclas +/-.")]
    public float manualZoomSpeed = 3f;
    [Tooltip("O tempo em segundos para segurar a tecla antes de 'olhar' para cima/baixo.")]
    public float timeToActivatePeek = 1f;
    [Tooltip("A velocidade com que a câmera se move para a posição de 'olhar'.")]
    public float peekTransitionSpeed = 2f;

    // --- CÉREBRO E ESTADO GLOBAL (Estático) ---
    private static CinemachineConfiner2D activeConfiner;
    private static CinemachineCamera activeVirtualCamera;
    private static RoomBoundary currentActiveRoom;
    private static Coroutine activeTransitionCoroutine;
    public enum PlayerCameraPreference { Automatic, Manual }
    private static PlayerCameraPreference playerPreference = PlayerCameraPreference.Automatic;
    // --- FIM DO CÉREBRO ---

    // --- ESTADO LOCAL DA INSTÂNCIA ---
    private PlayerCameraPreference currentOperatingMode;
    private CameraFocusZone defaultZone;
    private bool isPeeking = false;
    private LensSettings prePeekLens; // Salva o estado da câmera antes de "olhar"
    private float peekTimer = 0f;
    private CameraFocusZone.ActivationKey currentPeekKey = CameraFocusZone.ActivationKey.None;
    // --- FIM DO ESTADO LOCAL ---

    #region Setup e Eventos de Trigger

    private void Awake()
    {
        // Garante que o collider principal seja um trigger.
        if (confinerBounds != null) confinerBounds.isTrigger = true;

        // Encontra a zona padrão (marcada como 'None')
        defaultZone = focusZones.FirstOrDefault(z => z.activationKey == CameraFocusZone.ActivationKey.None);
        if (defaultZone == null && focusZones.Count > 0)
        {
            defaultZone = focusZones[0]; // Se nenhuma for marcada, a primeira vira padrão.
            Debug.LogWarning($"A sala '{gameObject.name}' não tem uma Zona de Foco Padrão definida. Usando a primeira da lista: '{defaultZone.zoneName}'.");
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

    #region Input e Lógica de Update (O CORAÇÃO DO SCRIPT)

    // IMPORTANTE: Esta lógica de input deve ser movida para o seu PlayerController no futuro!
    // Deixei aqui por enquanto para que o sistema seja testável de forma independente.
    private void Update()
    {
        // O Update só executa para a sala que está ativa no momento.
        if (currentActiveRoom != this) return;

        // --- LÓGICA DE TROCA DE MODO (TECLA T) ---
        if (Input.GetKeyDown(KeyCode.T) && roomMode == CameraRoomMode.Unlocked && !isPeeking)
        {
            playerPreference = (playerPreference == PlayerCameraPreference.Automatic) ? PlayerCameraPreference.Manual : PlayerCameraPreference.Automatic;
            DetermineOperatingMode(); // Reavalia o modo da câmera
            Debug.Log($"Modo da Câmera alterado para: {currentOperatingMode}");
        }

        // --- LÓGICA DO "MODO BURACO" / "PEEK" (TECLAS W/S) ---
        // Detecta qual tecla de "olhar" está sendo pressionada.
        CameraFocusZone.ActivationKey intendedPeekKey = CameraFocusZone.ActivationKey.None;
        if (Input.GetKey(KeyCode.S)) intendedPeekKey = CameraFocusZone.ActivationKey.LookDown;
        else if (Input.GetKey(KeyCode.W)) intendedPeekKey = CameraFocusZone.ActivationKey.LookUp;

        // Se o jogador está segurando uma tecla de "olhar"
        if (intendedPeekKey != CameraFocusZone.ActivationKey.None)
        {
            // Se já não estamos "olhando", inicia o timer.
            if (!isPeeking)
            {
                peekTimer += Time.deltaTime;
                if (peekTimer >= timeToActivatePeek)
                {
                    StartPeek(intendedPeekKey);
                }
            }
        }
        else // Se o jogador soltou todas as teclas de "olhar"
        {
            peekTimer = 0f; // Reseta o timer.
            if (isPeeking)
            {
                StopPeek(); // Se estava "olhando", para de olhar.
            }
        }

        // Se estamos no "Modo Buraco", nenhuma outra lógica de input é processada.
        if (isPeeking) return;

        // --- LÓGICA DE ZOOM MANUAL (TECLAS +/-) ---
        if (currentOperatingMode == PlayerCameraPreference.Manual)
        {
            float zoomInput = Input.GetAxis("Mouse ScrollWheel"); // Roda do mouse é mais intuitiva que +/-
            if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus)) zoomInput = 0.1f;
            if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus)) zoomInput = -0.1f;

            if (zoomInput != 0)
            {
                float currentSize = activeVirtualCamera.Lens.OrthographicSize;
                // O zoom máximo é sempre o ideal para a zona atual.
                float maxZoom = CalculateOptimalLensForZone(defaultZone).OrthographicSize;

                // Aplica o zoom e o prende entre o mínimo manual e o máximo automático.
                float newSize = currentSize - zoomInput * manualZoomSpeed;
                activeVirtualCamera.Lens.OrthographicSize = Mathf.Clamp(newSize, manualMinZoom, maxZoom);
            }
        }
    }

    #endregion

    #region Lógica da Câmera e Transições

    private void EnterRoom()
    {
        Debug.Log($"Entrando na sala '{gameObject.name}'.");
        currentActiveRoom = this;
        InitializeCameraReferences();

        if (activeConfiner != null && confinerBounds != null)
        {
            activeConfiner.BoundingShape2D = confinerBounds;
        }

        DetermineOperatingMode();
    }

    private void DetermineOperatingMode()
    {
        // Decide qual modo a câmera vai usar baseado na configuração da sala e na preferência do jogador.
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

        // Força a câmera para o estado correto do modo ativado.
        if (currentOperatingMode == PlayerCameraPreference.Automatic)
        {
            TransitionToZone(defaultZone);
        }
    }

    private void StartPeek(CameraFocusZone.ActivationKey key)
    {
        // Encontra uma zona que corresponda à tecla e à proximidade do jogador.
        var targetZone = focusZones.FirstOrDefault(z => z.activationKey == key && (z.proximityTrigger == null || z.proximityTrigger.bounds.Contains(GameObject.FindGameObjectWithTag("Player").transform.position)));

        if (targetZone != null)
        {
            Debug.Log($"Iniciando 'Peek' para a zona '{targetZone.zoneName}'.");
            isPeeking = true;
            currentPeekKey = key;
            prePeekLens = activeVirtualCamera.Lens; // Salva o estado da lente

            // Inicia a transição para o enquadramento limitado do "buraco".
            TransitionToZone(targetZone, true);
        }
    }

    private void StopPeek()
    {
        Debug.Log("Parando 'Peek' e retornando ao estado anterior.");
        isPeeking = false;
        currentPeekKey = CameraFocusZone.ActivationKey.None;

        // Retorna a câmera para o enquadramento da zona padrão.
        TransitionToZone(defaultZone);
    }

    // O "motor" de transição principal.
    private void TransitionToZone(CameraFocusZone zone, bool isPeek = false)
    {
        if (zone == null || zone.framingCollider == null || activeVirtualCamera == null) return;

        if (activeTransitionCoroutine != null)
        {
            StopCoroutine(activeTransitionCoroutine);
        }

        float speed = isPeek ? peekTransitionSpeed : this.zoomTransitionSpeed;

        // Calcula a lente alvo. Se for um "peek", aplica o limite de visão.
        LensSettings targetLens = CalculateOptimalLensForZone(zone);
        if (isPeek)
        {
            // Interpola entre o zoom atual e o zoom do buraco para respeitar o limite de 65%.
            targetLens.OrthographicSize = Mathf.Lerp(activeVirtualCamera.Lens.OrthographicSize, targetLens.OrthographicSize, zone.peekDistanceLimit);
        }

        activeTransitionCoroutine = StartCoroutine(SmoothTransitionCoroutine(targetLens, speed));
    }

    // Corrotina que anima a lente da câmera.
    private IEnumerator SmoothTransitionCoroutine(LensSettings targetLens, float speed)
    {
        LensSettings startLens = activeVirtualCamera.Lens;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * speed;
            // Interpola todas as propriedades da lente, não só o zoom.
            activeVirtualCamera.Lens = LensSettings.Lerp(startLens, targetLens, t);
            yield return null;
        }

        activeVirtualCamera.Lens = targetLens; // Garante o valor final.
        activeTransitionCoroutine = null;
    }

    // Calcula o enquadramento perfeito para uma zona específica.
    private LensSettings CalculateOptimalLensForZone(CameraFocusZone zone)
    {
        if (zone.framingCollider == null) return activeVirtualCamera.Lens;

        Bounds bounds = zone.framingCollider.bounds;
        float screenRatio = (float)Screen.width / Screen.height;
        float requiredSizeX = (bounds.size.x / screenRatio) / 2f;
        float requiredSizeY = bounds.size.y / 2f;

        float optimalSize = Mathf.Max(requiredSizeX, requiredSizeY) * automaticZoomPadding;

        // A posição da lente deve ser o centro do collider de enquadramento.
        // A lógica do Confiner ainda segura a câmera, mas isso ajuda a focar.
        Vector3 targetPosition = bounds.center;

        // Retorna a configuração completa da lente.
        return new LensSettings
        {
            OrthographicSize = optimalSize,
            // A gente não vai mexer na posição diretamente, o Cinemachine faz isso, mas
            // teríamos a info aqui se precisássemos de um offset manual.
        };
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