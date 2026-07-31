using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class GerenciadorForca : MonoBehaviour
{
    [Header("Componentes de UI")]
    public TextMeshProUGUI palavraTexto;
    public TMP_InputField inputLetra;
    public TextMeshProUGUI statusTexto;
    public TextMeshProUGUI errosTexto;
    public GameObject painelDoJogo;
    public GameObject Buttom_NewGame;

    [Header("Componentes de UI - Introdução")]
    public GameObject painelIntroducao; // Arraste o novo painel de intro aqui
    public TextMeshProUGUI textoIntroducao; // O texto que vai mostrar a mensagem aleatória

    [Header("Visual do Operador (EPIs)")]
    // Arraste os objetos de imagem na ordem dos erros (0 erros, 1 erro, 2 erros...)
    public List<GameObject> estadosDoOperador;

    [Header("Configurações do Jogo")]
    public int maxErros = 6;

    private List<string> bancoDePalavras = new List<string>();
    private string palavraSecreta;
    private List<char> letrasDescobertas = new List<char>();
    private List<char> letrasTentadas = new List<char>();
    private int errosAtuais = 0;
    private bool jogoAtivo = true;

    private List<string> mensagensSeguranca = new List<string>
    {
        "ATENÇÃO OPERADOR!\n\nUma manutenção crítica na caldeira principal precisa ser realizada. Para liberar a ordem de serviço e garantir que você está com os EPIs em dia, descubra a senha de segurança!",
        "ALERTA DE SEGURANÇA!\n\nOcorreu um vazamento de pressão na linha 3. Você foi escalado para conter a área, mas o painel eletrônico travou. Descubra a senha para acessar as ferramentas corretas!",
        "ORDEM DE SERVIÇO DIÁRIA:\n\nHoje é dia de inspeção de risco na área de usinagem. Nenhum operador entra sem autorização. Decifre a palavra-passe para validar seus equipamentos!",
        "ATENÇÃO SGI:\n\nUm novo carregamento de produtos químicos chegou ao pátio. Para operar a empilhadeira com segurança, valide o checklist encontrando a senha do sistema!"
    };

    void Start()
    {
        CarregarPalavras();
        MostrarTelaIntroducao(); // O jogo agora começa mostrando a introdução!
    }

    public void MostrarTelaIntroducao()
    {
        jogoAtivo = false;

        // Ativa a tela de intro e desativa a tela da forca temporariamente
        if (painelIntroducao != null) painelIntroducao.SetActive(true);
        if (painelDoJogo != null) painelDoJogo.SetActive(false);

        // Sorteia uma mensagem aleatória do nosso banco
        if (textoIntroducao != null && mensagensSeguranca.Count > 0)
        {
            int indiceAleatorio = Random.Range(0, mensagensSeguranca.Count);
            textoIntroducao.text = mensagensSeguranca[indiceAleatorio];
        }
    }

    // Essa função será chamada pelo botão "Iniciar Trabalho" na tela de introdução
    public void FecharIntroducaoEIniciar()
    {
        if (painelIntroducao != null) painelIntroducao.SetActive(false);
        if (painelDoJogo != null) painelDoJogo.SetActive(true);

        IniciarNovoJogo();
    }

    void CarregarPalavras()
    {
        bancoDePalavras.Clear();

        // Tenta buscar a string com as palavras salvas
        string palavrasSalvas = PlayerPrefs.GetString("BancoPalavrasForca", "");

        // Se estiver vazio, define as palavras padrão de segurança industrial
        if (string.IsNullOrWhiteSpace(palavrasSalvas) || palavrasSalvas.Length < 2)
        {
            bancoDePalavras = new List<string> { "CAPACETE", "OCULOS", "LUVAS", "PROTETOR", "BOTINA" };
            SalvarPalavrasNoPlayerPrefs();
        }
        else
        {
            // Separa a string pelas vírgulas e joga na lista
            string[] palavrasSeparadas = palavrasSalvas.Split(',');
            foreach (string palavra in palavrasSeparadas)
            {
                if (!string.IsNullOrWhiteSpace(palavra))
                {
                    bancoDePalavras.Add(palavra.Trim().ToUpper());
                }
            }
        }
    }

    void SalvarPalavrasNoPlayerPrefs()
    {
        // Transforma a lista em uma única string separada por vírgulas (Ex: "EPI,CIPA,SEGURANCA")
        string resultado = string.Join(",", bancoDePalavras);
        PlayerPrefs.SetString("BancoPalavrasForca", resultado);
        PlayerPrefs.Save();
    }

    // --------------------------------------------

    public void IniciarNovoJogo()
    {
        CarregarPalavras();
        Buttom_NewGame.SetActive(false);

        if (bancoDePalavras.Count == 0)
        {
            if (statusTexto != null) statusTexto.text = "Nenhuma palavra cadastrada!";
            return;
        }

        palavraSecreta = bancoDePalavras[Random.Range(0, bancoDePalavras.Count)].ToUpper();

        letrasDescobertas.Clear();
        letrasTentadas.Clear();

        // --- ADICIONE ESTA LINHA AQUI ---
        // Faz o jogo já considerar o espaço como "descoberto" automaticamente
        letrasDescobertas.Add(' ');

        errosAtuais = 0;
        jogoAtivo = true;

        if (statusTexto != null) statusTexto.text = "Digite uma letra e clique em Tentar!";

        AtualizarUI();
        AtualizarVisualOperador();
    }

    public void TentarLetra()
    {
        if (!jogoAtivo || string.IsNullOrEmpty(inputLetra.text)) return;

        char letraTentada = inputLetra.text.ToUpper()[0];
        inputLetra.text = "";
        inputLetra.ActivateInputField();

        if (letrasTentadas.Contains(letraTentada))
        {
            statusTexto.text = $"Você já tentou a letra {letraTentada}!";
            return;
        }

        letrasTentadas.Add(letraTentada);

        if (palavraSecreta.Contains(letraTentada.ToString()))
        {
            letrasDescobertas.Add(letraTentada);
            statusTexto.text = $"Boa! A palavra tem a letra {letraTentada}.";
        }
        else
        {
            errosAtuais++;
            statusTexto.text = $"Errou! A palavra não tem a letra {letraTentada}.";
            AtualizarVisualOperador();
        }

        AtualizarUI();
        VerificarFimDeJogo();
    }

    void AtualizarVisualOperador()
    {
        if (estadosDoOperador == null || estadosDoOperador.Count == 0) return;

        for (int i = 0; i < estadosDoOperador.Count; i++)
        {
            // Ativa o estado que condiz com o número de erros e desativa os outros
            if (i == errosAtuais)
            {
                estadosDoOperador[i].SetActive(true);
            }
            else
            {
                estadosDoOperador[i].SetActive(false);
            }
        }
    }

    void Update()
    {
        // Atalho opcional: se o jogador apertar Enter no teclado, aciona o botão de tentar
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            TentarLetra();
        }
    }

    void AtualizarUI()
    {
        string exibicaoPalavra = "";
        bool ganhou = true;

        foreach (char letra in palavraSecreta)
        {
            if (letra == ' ')
            {
                // Como o espaço é um caractere invisível, colocamos 3 espaços (ou um " | ") 
                // para o jogador notar visualmente que há uma separação de palavras
                exibicaoPalavra += "   ";
            }
            else if (letrasDescobertas.Contains(letra))
            {
                exibicaoPalavra += letra + " ";
            }
            else
            {
                exibicaoPalavra += "_ ";
                ganhou = false; // Se ainda tem letras normais escondidas, ele não ganhou
            }
        }

        if (palavraTexto != null) palavraTexto.text = exibicaoPalavra.Trim();
        if (errosTexto != null) errosTexto.text = $"Erros: {errosAtuais} / {maxErros}";

        if (ganhou && jogoAtivo)
        {
            statusTexto.text = "Parabéns! Você salvou o operador e liberou o trabalho!";
            jogoAtivo = false;
            Buttom_NewGame.SetActive(true);
        }
    }

    void VerificarFimDeJogo()
    {
        if (errosAtuais >= maxErros)
        {
            statusTexto.text = $"Acidente de trabalho! A palavra era: {palavraSecreta}";
            jogoAtivo = false;
            Buttom_NewGame.SetActive(true);

        }
    }

    public void FecharJogo()
    {
        Application.Quit();
    }
}

