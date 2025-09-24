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
    [Tooltip("A velocidade da transição de zoom ao entrar/sair de zonas.")]
    public float zoomTransitionSpeed = 2f;

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

    // --- FUNÇÃO UPDATE CORRIGIDA ---
    private void Update()
    {
        // O Update só executa para a sala que está ativa no momento.
        if (currentActiveRoom != this) return;

        // --- LÓgica de troca de modo (Tecla T) ---
        if (Input.GetKeyDown(KeyCode.T) && roomMode == CameraRoomMode.Unlocked && !isPeeking)
        {
            playerPreference = (playerPreference == PlayerCameraPreference.Automatic) ? PlayerCameraPreference.Manual : PlayerCameraPreference.Automatic;
            DetermineOperatingMode(); // Reavalia o modo da câmera
            Debug.Log($"Modo da Câmera alterado para: {currentOperatingMode}");
        }

        // --- Lógica do "Modo Buraco" / "Peek" (Teclas W/S) ---
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

        // --- LÓGICA DE ZOOM MANUAL (APENAS TECLAS +/-) ---
        if (currentOperatingMode == PlayerCameraPreference.Manual)
        {
            // Resetamos a variável para garantir que não haja lixo de frames anteriores.
            float zoomInput = 0f;

            // Verificamos o input APENAS dos botões + e -.
            if (Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.KeypadPlus))
            {
                zoomInput = -0.1f; // Diminuir o OrthographicSize = Zoom IN
            }
            if (Input.GetKey(KeyCode.Minus) || Input.GetKey(KeyCode.KeypadMinus))
            {
                zoomInput = 0.1f; // Aumentar o OrthographicSize = Zoom OUT
            }

            // A lógica de aplicação do zoom só roda se uma das teclas foi pressionada.
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

    #region Lógica da Câmera e Transições
   
    private void EnterRoom()
    {
        currentActiveRoom = this;
        InitializeCameraReferences();
        Debug.Log($"Entrando na sala '{gameObject.name}'.");

        // Aplica o limite de movimento geral.
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

        // --- A LÓGICA CORRETA ---
        // Determina o modo, mas NÃO transiciona a câmera aqui...
        if (roomMode == CameraRoomMode.LockedAutomatic) currentOperatingMode = PlayerCameraPreference.Automatic;
        else if (roomMode == CameraRoomMode.LockedManual) currentOperatingMode = PlayerCameraPreference.Manual;
        else currentOperatingMode = playerPreference;
        Debug.Log($"Modo operacional definido para: {currentOperatingMode}.");

        // ...porque a transição para a zona padrão é a ação que deve acontecer.
        // Isso conserta TUDO: O zoom incorreto E o fato de a câmera não seguir o player.
        if (defaultZone != null)
        {
            TransitionToZone(defaultZone);
        }
        else
        {
            Debug.LogError($"A sala '{gameObject.name}' não tem uma Zona de Foco Padrão para iniciar a câmera!", this);
        }
    }

    // --- FUNÇÃO DetermineOperatingMode CORRIGIDA ---
    private void DetermineOperatingMode()
    {
        PlayerCameraPreference previousMode = currentOperatingMode;

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

        // CORREÇÃO: A transição para a zona padrão agora acontece sempre que o modo é determinado,
        // garantindo que a câmera se ajuste corretamente ao entrar em modo manual também.
        // E só faz a transição se o modo realmente mudou, para evitar saltos desnecessários.
        if (currentOperatingMode != previousMode || activeConfiner.BoundingShape2D == null)
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

    // --- FUNÇÃO CORRIGIDA ---
    private void TransitionToZone(CameraFocusZone zone, bool isPeek = false)
    {
        if (zone == null || zone.framingCollider == null || activeVirtualCamera == null) return;
        if (currentActiveRoom == null)
        {
            Debug.LogError("TransitionToZone foi chamada, mas não há uma sala ativa (currentActiveRoom is null)!");
            return;
        }

        if (activeTransitionCoroutine != null)
        {
            StopCoroutine(activeTransitionCoroutine);
        }

        // --- A CORREÇÃO PRINCIPAL ---
        // Acessamos as variáveis de velocidade diretamente da instância da sala ativa (currentActiveRoom).
        // Isso remove qualquer ambiguidade sobre de onde os valores estão vindo.
        float speed = isPeek ? currentActiveRoom.peekTransitionSpeed : currentActiveRoom.zoomTransitionSpeed;

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
    // --- CORROTINA DEFINITivamente CORRIGIDA ---
    private IEnumerator SmoothTransitionCoroutine(LensSettings targetLens, float speed)
    {
        // Pega o zoom inicial e o alvo
        float startSize = activeVirtualCamera.Lens.OrthographicSize;
        float targetSize = targetLens.OrthographicSize;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * speed;

            // Calcula o novo zoom interpolado
            float newSize = Mathf.Lerp(startSize, targetSize, t);

            // O jeito correto de aplicar: pega a lente, modifica, e reatribui.
            var lens = activeVirtualCamera.Lens;
            lens.OrthographicSize = newSize;
            activeVirtualCamera.Lens = lens;

            yield return null;
        }

        // Garante o valor final
        var finalLens = activeVirtualCamera.Lens;
        finalLens.OrthographicSize = targetLens.OrthographicSize;
        activeVirtualCamera.Lens = finalLens;

        activeTransitionCoroutine = null;
    }

    // Calcula o enquadramento perfeito para uma zona específica.
    // --- FUNÇÃO CORRIGIDA PARA RESPEITAR O LIMITE MÁXIMO ---
    private LensSettings CalculateOptimalLensForZone(CameraFocusZone zone)
    {
        if (zone.framingCollider == null) return activeVirtualCamera.Lens;
        // GARANTE que temos uma referência à sala ativa para pegar o confinerBounds.
        if (currentActiveRoom == null) return activeVirtualCamera.Lens;

        // --- CÁLCULO 1: O ZOOM IDEAL PARA A ZONA DE FOCO ---
        Bounds zoneBounds = zone.framingCollider.bounds;
        float screenRatio = (float)Screen.width / Screen.height;
        float requiredSizeX = (zoneBounds.size.x / screenRatio) / 2f;
        float requiredSizeY = zoneBounds.size.y / 2f;
        float zoneOptimalSize = Mathf.Max(requiredSizeX, requiredSizeY) * automaticZoomPadding;

        // --- CÁLCULO 2: O ZOOM MÁXIMO PERMITIDO PELO CONFINE ---
        float maxAllowedZoom = float.MaxValue; // Começa com um valor infinito.
        if (currentActiveRoom.confinerBounds != null)
        {
            Bounds confinerBounds = currentActiveRoom.confinerBounds.bounds;
            float maxRequiredSizeX = (confinerBounds.size.x / screenRatio) / 2f;
            float maxRequiredSizeY = confinerBounds.size.y / 2f;
            // O zoom máximo é o que enquadra o MAIOR limite, para garantir que nunca saia.
            maxAllowedZoom = Mathf.Max(maxRequiredSizeX, maxRequiredSizeY);
        }

        // --- DECISÃO FINAL ---
        // O tamanho final é o MENOR entre o que a zona quer e o que o limite permite.
        // Isso garante que o zoom nunca "vaze" para fora.
        float finalOptimalSize = Mathf.Min(zoneOptimalSize, maxAllowedZoom);

        // Retorna a configuração completa da lente com o zoom seguro.
        return new LensSettings
        {
            OrthographicSize = finalOptimalSize,
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