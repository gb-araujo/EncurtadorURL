// Seleciona os elementos principais usando seus IDs
const urlInput = document.getElementById("url");
const submitButton = document.getElementById("submitButton");
const resultParagraph = document.getElementById("urlResult");

const API_URL = "https://encurtadorurl-c3lm.onrender.com/urls/";

// Handler do botão
submitButton.addEventListener("click", async () => {
  const longUrl = urlInput.value.trim();
  resultParagraph.textContent = "Enviando...";
  resultParagraph.style.color = "gray";

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
      body: JSON.stringify({ longUrl }), // camelCase funciona tanto quanto PascalCase no .NET
    });

    // --- LEITURA DO BODY EM TEXTO PARA EVITAR ERROS DE JSON ---
    const text = await response.text();

    // 1. Checa resposta vazia
    if (!text) {
      resultParagraph.textContent = "Erro: A API retornou resposta vazia.";
      resultParagraph.style.color = "red";
      console.error("Resposta vazia");
      return;
    }

    // 2. Backend retornou HTML (erro interno, rota errada ou CORS)
    if (text.startsWith("<")) {
      resultParagraph.textContent =
        "Erro: A API retornou HTML ao invés de JSON. Verifique o servidor.";
      resultParagraph.style.color = "red";
      console.error("HTML recebido:", text);
      return;
    }

    // 3. Tenta converter pra JSON
    let data;
    try {
      data = JSON.parse(text);
    } catch (err) {
      resultParagraph.textContent = "Erro: Resposta inválida da API.";
      resultParagraph.style.color = "red";
      console.error("JSON inválido:", text);
      return;
    }

    // 4. Se não foi 200
    if (!response.ok) {
      resultParagraph.textContent = `Erro ${response.status}: Falha ao criar a URL.`;
      resultParagraph.style.color = "red";
      console.error("Erro da API:", data);
      return;
    }

    // 5. Tudo certo
    resultParagraph.textContent = `URL Curta Criada: ${data.shortUrl}`;
    resultParagraph.style.color = "green";
    console.log("API Response:", data);
  } catch (error) {
    // 6. Provável erro de rede ou CORS bloqueado
    resultParagraph.textContent =
      "Erro de Rede: Verifique se a API está rodando ou se o CORS está liberado.";
    resultParagraph.style.color = "red";
    console.error("Fetch Error:", error);
  }
});
