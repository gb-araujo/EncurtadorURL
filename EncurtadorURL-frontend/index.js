// Seleciona os elementos principais usando seus IDs
const urlInput = document.getElementById("url");
const submitButton = document.getElementById("submitButton");
const resultParagraph = document.getElementById("urlResult");

const resultContainer = document.getElementById("resultContainer");
const shortLink = document.getElementById("shortLink");
const copyButton = document.getElementById("copyButton");
const copyMessage = document.getElementById("message");

const API_URL = "https://encurtadorurl-c3lm.onrender.com/urls/";

// Handler do botão
submitButton.addEventListener("click", async () => {
  const longUrl = urlInput.value.trim();
  resultParagraph.textContent = "Enviando...";
  resultParagraph.style.color = "gray";

  // limpa o bloco quando reenvia
  resultContainer.style.display = "none";
  shortLink.textContent = "";
  shortLink.href = "#";

  if (!longUrl) {
    resultParagraph.textContent = "Por favor, insira uma URL válida.";
    resultParagraph.style.color = "red";
    return;
  }

  try {
    const response = await fetch(API_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      mode: "cors",
      body: JSON.stringify({ longUrl }),
    });

    // --- LEITURA DO BODY EM TEXTO PARA EVITAR ERROS DE JSON ---
    const text = await response.text();

    if (!text) {
      resultParagraph.textContent = "Erro: A API retornou resposta vazia.";
      resultParagraph.style.color = "red";
      console.error("Resposta vazia");
      return;
    }

    if (text.startsWith("<")) {
      resultParagraph.textContent =
        "Erro: A API retornou HTML ao invés de JSON. Verifique o servidor.";
      resultParagraph.style.color = "red";
      console.error("HTML recebido:", text);
      return;
    }

    let data;
    try {
      data = JSON.parse(text);
    } catch (err) {
      resultParagraph.textContent = "Erro: Resposta inválida da API.";
      resultParagraph.style.color = "red";
      console.error("JSON inválido:", text);
      return;
    }

    if (!response.ok) {
      resultParagraph.textContent = `Erro ${response.status}: Falha ao criar a URL.`;
      resultParagraph.style.color = "red";
      console.error("Erro da API:", data);
      return;
    }

    // --- SUCESSO ---
    const shortUrl = data.shortUrl;

    resultParagraph.textContent = "URL gerada com sucesso!";
    resultParagraph.style.color = "green";

    // Mostra o container com o link
    resultContainer.style.display = "flex";

    // Torna o link clicável
    shortLink.href = shortUrl;
    shortLink.textContent = shortUrl;

    console.log("API Response:", data);
  } catch (error) {
    resultParagraph.textContent =
      "Erro de Rede: Verifique se a API está rodando ou se o CORS está liberado.";
    resultParagraph.style.color = "red";
    console.error("Fetch Error:", error);
  }
});

if (!longUrl.startsWith("http://") && !longUrl.startsWith("https://")) {
  longUrl = "https://" + longUrl;
}

// Botão copiar
copyButton.addEventListener("click", async () => {
  const textToCopy = shortLink.textContent;

  try {
    await navigator.clipboard.writeText(textToCopy);

    copyMessage.style.opacity = 1;

    setTimeout(() => {
      copyMessage.style.opacity = 0;
    }, 1500);
  } catch (err) {
    console.error("Erro ao copiar:", err);
  }
});
