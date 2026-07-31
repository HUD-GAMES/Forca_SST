using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TelaCadastroPalavras : MonoBehaviour
{
    [Header("Componentes de UI")]
    public TMP_InputField inputNovaPalavra;
    public TextMeshProUGUI listaPalavrasTexto;
    public TextMeshProUGUI feedbackTexto;

    private List<string> bancoDePalavras = new List<string>();

    void OnEnable()
    {
        // Sempre que a tela de cadastro abrir, carrega a lista atualizada
        CarregarPalavras();
        AtualizarListaVisual();
    }

    void CarregarPalavras()
    {
        string palavrasSalvas = PlayerPrefs.GetString("BancoPalavrasForca", "");
        if (!string.IsNullOrEmpty(palavrasSalvas))
        {
            bancoDePalavras = new List<string>(palavrasSalvas.Split(','));
        }
        else
        {
            bancoDePalavras = new List<string> { "UNITY", "PROGRAMACAO", "DESENVOLVIMENTO", "JOGO" };
        }
    }

    void SalvarPalavras()
    {
        string resultado = string.Join(",", bancoDePalavras);
        PlayerPrefs.SetString("BancoPalavrasForca", resultado);
        PlayerPrefs.Save();
    }

    public void CadastrarPalavra()
    {
        string novaPalavra = inputNovaPalavra.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(novaPalavra))
        {
            feedbackTexto.text = "Digite uma palavra válida!";
            return;
        }

        if (bancoDePalavras.Contains(novaPalavra))
        {
            feedbackTexto.text = "Essa palavra já está cadastrada!";
            return;
        }

        bancoDePalavras.Add(novaPalavra);
        SalvarPalavras();

        inputNovaPalavra.text = ""; // Limpa o campo
        feedbackTexto.text = $"'{novaPalavra}' cadastrada com sucesso!";
        AtualizarListaVisual();
    }

    public void ExcluirPalavra()
    {
        string palavraParaRemover = inputNovaPalavra.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(palavraParaRemover))
        {
            feedbackTexto.text = "Digite a palavra que deseja excluir!";
            return;
        }

        if (bancoDePalavras.Contains(palavraParaRemover))
        {
            bancoDePalavras.Remove(palavraParaRemover);
            SalvarPalavras();

            inputNovaPalavra.text = ""; // Limpa o campo
            feedbackTexto.text = $"'{palavraParaRemover}' foi removida!";
            AtualizarListaVisual();
        }
        else
        {
            feedbackTexto.text = "Palavra não encontrada no banco!";
        }
    }

    void AtualizarListaVisual()
    {
        if (bancoDePalavras.Count == 0)
        {
            listaPalavrasTexto.text = "Nenhuma palavra cadastrada.";
            return;
        }

        // Junta todas as palavras quebrando linha para exibi-las como uma lista na tela
        listaPalavrasTexto.text = "Palavras atuais:\n" + string.Join("\n", bancoDePalavras);
    }
}