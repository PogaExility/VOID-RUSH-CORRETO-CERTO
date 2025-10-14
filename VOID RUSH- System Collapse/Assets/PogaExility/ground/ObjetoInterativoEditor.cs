// NOME DO ARQUIVO: ObjetoInterativoEditor.cs
// IMPORTANTE: ESTE ARQUIVO DEVE ESTAR DENTRO DE UMA PASTA CHAMADA 'Editor'

using UnityEngine;
using UnityEditor; // Namespace essencial para scripts de editor

// Atributos que dizem � Unity que esta classe � um editor customizado para o script 'ObjetoInterativo'
[CustomEditor(typeof(ObjetoInterativo))]
[CanEditMultipleObjects]
public class ObjetoInterativoEditor : Editor
{
    // Vari�veis para guardar as propriedades do nosso script alvo
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

    // Propriedades do Feedback de Ativa��o
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

    // Fun��o chamada quando o Inspector � habilitado
    void OnEnable()
    {
        // Linka nossas vari�veis com as vari�veis reais do script ObjetoInterativo
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

    // Esta � a fun��o principal que redesenha o Inspector
    public override void OnInspectorGUI()
    {
        // Puxa as �ltimas informa��es do objeto
        serializedObject.Update();

        // Desenha os campos principais que sempre aparecem
        EditorGUILayout.PropertyField(modoDeAtivacao);
        EditorGUILayout.PropertyField(modoDeUso);

        EditorGUILayout.Space(10); // Adiciona um espa�o para organiza��o

        // Pega os valores atuais dos enums para usar na l�gica 'if'
        ModoDeAtivacao ativacaoAtual = (ModoDeAtivacao)modoDeAtivacao.enumValueIndex;
        ModoDeUso usoAtual = (ModoDeUso)modoDeUso.enumValueIndex;
        ModoFeedbackVisual visualAtual = (ModoFeedbackVisual)modoVisual.enumValueIndex;

        // --- L�GICA CONDICIONAL PARA DESENHAR O INSPECTOR ---

        switch (ativacaoAtual)
        {
            case ModoDeAtivacao.PorDano:
                EditorGUILayout.LabelField("Op��es para 'Por Dano'", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(vidaMaxima);
                EditorGUILayout.PropertyField(tipoDeAtaqueAceito_Dano);
                EditorGUILayout.PropertyField(corDeDano);
                EditorGUILayout.PropertyField(intensidadeTremor);
                EditorGUILayout.PropertyField(duracaoFeedbackDano);
                EditorGUILayout.PropertyField(efeitoDeQuebraPrefab);
                break;

            case ModoDeAtivacao.PorHit:
                EditorGUILayout.LabelField("Op��es para 'Por Hit'", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(tipoDeAtaqueAceito_Hit);
                DesenharFeedbackDeAtivacao(usoAtual, visualAtual);
                break;

            case ModoDeAtivacao.PorBotao:
                EditorGUILayout.LabelField("Op��es para 'Por Bot�o'", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(promptVisual);
                DesenharFeedbackDeAtivacao(usoAtual, visualAtual);
                break;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("A��es (Eventos)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(aoAtivar);
        // S� desenha o evento 'aoDesativar' se o objeto for reativ�vel
        if (usoAtual == ModoDeUso.Reativavel)
        {
            EditorGUILayout.PropertyField(aoDesativar);
        }

        // Aplica quaisquer mudan�as feitas no Inspector de volta ao objeto
        serializedObject.ApplyModifiedProperties();
    }

    // Fun��o auxiliar para n�o repetir o c�digo do feedback
    // DENTRO DO SCRIPT: ObjetoInterativoEditor.cs

    // Fun��o auxiliar para n�o repetir o c�digo do feedback (VERS�O CORRIGIDA)
    void DesenharFeedbackDeAtivacao(ModoDeUso usoAtual, ModoFeedbackVisual visualAtual)
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Feedback de Ativa��o", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(modoVisual); // Mostra o dropdown para escolher o modo

        // --- IN�CIO DA CORRE��O ---
        // Agora, o switch verifica qual modo est� selecionado e mostra os campos correspondentes.
        switch (visualAtual)
        {
            case ModoFeedbackVisual.TrocarSprite:
                EditorGUILayout.PropertyField(spriteAtivo, new GUIContent("Sprite Ativo"));
                // S� mostra o sprite inativo se o modo de uso for Reativ�vel
                if (usoAtual == ModoDeUso.Reativavel)
                {
                    EditorGUILayout.PropertyField(spriteInativo, new GUIContent("Sprite Inativo"));
                }
                break;

            case ModoFeedbackVisual.TocarAnimacao:
                EditorGUILayout.PropertyField(clipeAtivando, new GUIContent("Clipe Ativando"));
                // S� mostra o clipe de desativa��o se o modo de uso for Reativ�vel
                if (usoAtual == ModoDeUso.Reativavel)
                {
                    EditorGUILayout.PropertyField(clipeDesativando, new GUIContent("Clipe Desativando"));
                }
                break;
        }
        // --- FIM DA CORRE��O ---

        // A l�gica para os sons continua a mesma, mas adicionamos labels para clareza
        EditorGUILayout.PropertyField(somAtivar, new GUIContent("Som Ativar"));
        if (usoAtual == ModoDeUso.Reativavel)
        {
            EditorGUILayout.PropertyField(somDesativar, new GUIContent("Som Desativar"));
        }
    }
}