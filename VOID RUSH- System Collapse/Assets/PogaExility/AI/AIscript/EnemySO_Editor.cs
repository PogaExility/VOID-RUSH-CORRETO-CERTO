using UnityEditor;
using UnityEngine;

/// <summary>
/// Customiza a exibição do EnemySO no Inspector da Unity para melhor organização.
/// </summary>
[CustomEditor(typeof(EnemySO))]
public class EnemySO_Editor : Editor
{
    // --- PROPRIEDADES SERIALIZADAS ---
    // Atributos
    private SerializedProperty nomeInimigo;
    private SerializedProperty vidaBase;
    private SerializedProperty danoBase;
    private SerializedProperty velocidadeMovimentoBase;
    private SerializedProperty escalaBase;
    // Crescimento
    private SerializedProperty aumentoVidaPorNivel;
    private SerializedProperty aumentoDanoPorNivel;
    private SerializedProperty aumentoVelocidadePorNivel;
    private SerializedProperty aumentoEscalaPercentualPorNivel;
    // Detecção
    private SerializedProperty raioVisao;
    private SerializedProperty anguloVisao;
    private SerializedProperty quantidadeRaiosVisao;
    private SerializedProperty camadaAlvo;
    private SerializedProperty camadaObstaculos;


    private void OnEnable()
    {
        // Linka as propriedades com as variáveis do script EnemySO
        nomeInimigo = serializedObject.FindProperty("nomeInimigo");
        vidaBase = serializedObject.FindProperty("vidaBase");
        danoBase = serializedObject.FindProperty("danoBase");
        velocidadeMovimentoBase = serializedObject.FindProperty("velocidadeMovimentoBase");
        escalaBase = serializedObject.FindProperty("escalaBase");

        aumentoVidaPorNivel = serializedObject.FindProperty("aumentoVidaPorNivel");
        aumentoDanoPorNivel = serializedObject.FindProperty("aumentoDanoPorNivel");
        aumentoVelocidadePorNivel = serializedObject.FindProperty("aumentoVelocidadePorNivel");
        aumentoEscalaPercentualPorNivel = serializedObject.FindProperty("aumentoEscalaPercentualPorNivel");

        raioVisao = serializedObject.FindProperty("raioVisao");
        anguloVisao = serializedObject.FindProperty("anguloVisao");
        quantidadeRaiosVisao = serializedObject.FindProperty("quantidadeRaiosVisao");
        camadaAlvo = serializedObject.FindProperty("camadaAlvo");
        camadaObstaculos = serializedObject.FindProperty("camadaObstaculos");
    }

    public override void OnInspectorGUI()
    {
        // Atualiza o objeto serializado para pegar as últimas alterações
        serializedObject.Update();

        // Estilo para os títulos das seções
        GUIStyle boldStyle = new GUIStyle(EditorStyles.largeLabel)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 14
        };

        // --- Seção de Informações Básicas ---
        EditorGUILayout.LabelField("Informações Básicas", boldStyle);
        EditorGUILayout.PropertyField(nomeInimigo, new GUIContent("Nome do Inimigo", "Nome para identificar este tipo de inimigo no editor."));
        EditorGUILayout.Space(10); // Adiciona um espaço vertical

        // --- Seção de Atributos Base ---
        EditorGUILayout.LabelField("Atributos Base (Nível 1)", boldStyle);
        EditorGUILayout.PropertyField(vidaBase);
        EditorGUILayout.PropertyField(danoBase);
        EditorGUILayout.PropertyField(velocidadeMovimentoBase);
        EditorGUILayout.PropertyField(escalaBase);
        EditorGUILayout.Space(10);

        // --- Seção de Fatores de Crescimento ---
        EditorGUILayout.LabelField("Fatores de Crescimento por Nível", boldStyle);
        EditorGUILayout.PropertyField(aumentoVidaPorNivel);
        EditorGUILayout.PropertyField(aumentoDanoPorNivel);
        EditorGUILayout.PropertyField(aumentoVelocidadePorNivel);
        EditorGUILayout.PropertyField(aumentoEscalaPercentualPorNivel);
        EditorGUILayout.Space(10);

        // --- Seção de Detecção de Alvo ---
        EditorGUILayout.LabelField("Parâmetros de Detecção de Alvo", boldStyle);
        EditorGUILayout.PropertyField(raioVisao);
        EditorGUILayout.PropertyField(anguloVisao);
        EditorGUILayout.PropertyField(quantidadeRaiosVisao);
        EditorGUILayout.PropertyField(camadaAlvo);
        EditorGUILayout.PropertyField(camadaObstaculos);

        // Aplica todas as modificações feitas no Inspector
        serializedObject.ApplyModifiedProperties();
    }
}