// NOME DO ARQUIVO: ObjetoInterativoEditor.cs
// IMPORTANTE: ESTE ARQUIVO DEVE ESTAR DENTRO DE UMA PASTA CHAMADA 'Editor'

using UnityEngine;
using UnityEditor; // Namespace essencial para scripts de editor

// Atributos que dizem à Unity que esta classe é um editor customizado para o script 'ObjetoInterativo'
[CustomEditor(typeof(ObjetoInterativo))]
[CanEditMultipleObjects]
public class ObjetoInterativoEditor : Editor
{
    // Variáveis para guardar as propriedades do nosso script alvo
    SerializedProperty modoDeAtivacao;
    SerializedProperty modoDeUso;

    // Propriedades do modo 'PorDano'
    SerializedProperty vidaMaxima;
    SerializedProperty tipoDeAtaqueAceito_Dano;
    SerializedProperty corDeDano;
    SerializedProperty intensidadeTremor;
    SerializedProperty duracaoFeedbackDano;
    SerializedProperty efeitoDeQuebraPrefab;

    // Propriedades do modo 'PorHit'
    SerializedProperty tipoDeAtaqueAceito_Hit;

    // Propriedades do modo 'PorBotao'
    SerializedProperty promptVisual;

    // Propriedades do Feedback de Ativação
    SerializedProperty modoVisual;
    SerializedProperty spriteAtivo;
    SerializedProperty spriteInativo;
    SerializedProperty clipeAtivando;
    SerializedProperty clipeDesativando;
    SerializedProperty somAtivar;
    SerializedProperty somDesativar;

    // Propriedades dos Eventos
    SerializedProperty aoAtivar;
    SerializedProperty aoDesativar;

    // Função chamada quando o Inspector é habilitado
    void OnEnable()
    {
        // Linka nossas variáveis com as variáveis reais do script ObjetoInterativo
        modoDeAtivacao = serializedObject.FindProperty("modoDeAtivacao");
        modoDeUso = serializedObject.FindProperty("modoDeUso");

        vidaMaxima = serializedObject.FindProperty("vidaMaxima");
        tipoDeAtaqueAceito_Dano = serializedObject.FindProperty("tipoDeAtaqueAceito_Dano");
        corDeDano = serializedObject.FindProperty("corDeDano");
        intensidadeTremor = serializedObject.FindProperty("intensidadeTremor");
        duracaoFeedbackDano = serializedObject.FindProperty("duracaoFeedbackDano");
        efeitoDeQuebraPrefab = serializedObject.FindProperty("efeitoDeQuebraPrefab");

        tipoDeAtaqueAceito_Hit = serializedObject.FindProperty("tipoDeAtaqueAceito_Hit");

        promptVisual = serializedObject.FindProperty("promptVisual");

        modoVisual = serializedObject.FindProperty("modoVisual");
        spriteAtivo = serializedObject.FindProperty("spriteAtivo");
        spriteInativo = serializedObject.FindProperty("spriteInativo");
        clipeAtivando = serializedObject.FindProperty("clipeAtivando");
        clipeDesativando = serializedObject.FindProperty("clipeDesativando");
        somAtivar = serializedObject.FindProperty("somAtivar");
        somDesativar = serializedObject.FindProperty("somDesativar");

        aoAtivar = serializedObject.FindProperty("aoAtivar");
        aoDesativar = serializedObject.FindProperty("aoDesativar");
    }

    // Esta é a função principal que redesenha o Inspector
    public override void OnInspectorGUI()
    {
        // Puxa as últimas informações do objeto
        serializedObject.Update();

        // Desenha os campos principais que sempre aparecem
        EditorGUILayout.PropertyField(modoDeAtivacao);
        EditorGUILayout.PropertyField(modoDeUso);

        EditorGUILayout.Space(10); // Adiciona um espaço para organização

        // Pega os valores atuais dos enums para usar na lógica 'if'
        ModoDeAtivacao ativacaoAtual = (ModoDeAtivacao)modoDeAtivacao.enumValueIndex;
        ModoDeUso usoAtual = (ModoDeUso)modoDeUso.enumValueIndex;
        ModoFeedbackVisual visualAtual = (ModoFeedbackVisual)modoVisual.enumValueIndex;

        // --- LÓGICA CONDICIONAL PARA DESENHAR O INSPECTOR ---

        switch (ativacaoAtual)
        {
            case ModoDeAtivacao.PorDano:
                EditorGUILayout.LabelField("Opções para 'Por Dano'", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(vidaMaxima);
                EditorGUILayout.PropertyField(tipoDeAtaqueAceito_Dano);
                EditorGUILayout.PropertyField(corDeDano);
                EditorGUILayout.PropertyField(intensidadeTremor);
                EditorGUILayout.PropertyField(duracaoFeedbackDano);
                EditorGUILayout.PropertyField(efeitoDeQuebraPrefab);
                break;

            case ModoDeAtivacao.PorHit:
                EditorGUILayout.LabelField("Opções para 'Por Hit'", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(tipoDeAtaqueAceito_Hit);
                DesenharFeedbackDeAtivacao(usoAtual, visualAtual);
                break;

            case ModoDeAtivacao.PorBotao:
                EditorGUILayout.LabelField("Opções para 'Por Botão'", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(promptVisual);
                DesenharFeedbackDeAtivacao(usoAtual, visualAtual);
                break;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Ações (Eventos)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(aoAtivar);
        // Só desenha o evento 'aoDesativar' se o objeto for reativável
        if (usoAtual == ModoDeUso.Reativavel)
        {
            EditorGUILayout.PropertyField(aoDesativar);
        }

        // Aplica quaisquer mudanças feitas no Inspector de volta ao objeto
        serializedObject.ApplyModifiedProperties();
    }

    // Função auxiliar para não repetir o código do feedback
    // DENTRO DO SCRIPT: ObjetoInterativoEditor.cs

    // Função auxiliar para não repetir o código do feedback (VERSÃO CORRIGIDA)
    void DesenharFeedbackDeAtivacao(ModoDeUso usoAtual, ModoFeedbackVisual visualAtual)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Feedback de Ativação", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(modoVisual); // Mostra o dropdown para escolher o modo

        // --- INÍCIO DA CORREÇÃO ---
        // Agora, o switch verifica qual modo está selecionado e mostra os campos correspondentes.
        switch (visualAtual)
        {
            case ModoFeedbackVisual.TrocarSprite:
                EditorGUILayout.PropertyField(spriteAtivo, new GUIContent("Sprite Ativo"));
                // Só mostra o sprite inativo se o modo de uso for Reativável
                if (usoAtual == ModoDeUso.Reativavel)
                {
                    EditorGUILayout.PropertyField(spriteInativo, new GUIContent("Sprite Inativo"));
                }
                break;

            case ModoFeedbackVisual.TocarAnimacao:
                EditorGUILayout.PropertyField(clipeAtivando, new GUIContent("Clipe Ativando"));
                // Só mostra o clipe de desativação se o modo de uso for Reativável
                if (usoAtual == ModoDeUso.Reativavel)
                {
                    EditorGUILayout.PropertyField(clipeDesativando, new GUIContent("Clipe Desativando"));
                }
                break;
        }
        // --- FIM DA CORREÇÃO ---

        // A lógica para os sons continua a mesma, mas adicionamos labels para clareza
        EditorGUILayout.PropertyField(somAtivar, new GUIContent("Som Ativar"));
        if (usoAtual == ModoDeUso.Reativavel)
        {
            EditorGUILayout.PropertyField(somDesativar, new GUIContent("Som Desativar"));
        }
    }
}