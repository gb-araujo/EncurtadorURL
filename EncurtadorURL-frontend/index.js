// index.js — substitua o seu por este

// elementos
const urlInput = document.getElementById("url");
const submitButton = document.getElementById("submitButton");
const resultParagraph = document.getElementById("urlResult");

const resultContainer = document.getElementById("resultContainer");
const shortLink = document.getElementById("shortLink");
const copyButton = document.getElementById("copyButton");
const copyMessage = document.getElementById("message");

const API_ORIGIN = "https://encurtadorurl-c3lm.onrender.com";
const API_URL = API_ORIGIN + "/urls/";

// util: normaliza e valida
function normalizeUrl(input) {
  let u = input.trim();

  // remove espaços invs
  u = u.replace(/\s+/g, "");

  // se já tem protocolo ok, se não adiciona https://
  if (!/^https?:\/\//i.test(u)) {
    u = "https://" + u;
  }

  // tenta construir URL para validar
  try {
    const parsed = new URL(u);
    // opcional: restringir esquemas
    if (!["http:", "https:"].includes(parsed.protocol))
      throw new Error("Protocolo inválido");
    return parsed.toString();
  } catch (err) {
    return null;
  }
}

// evento enviar
submitButton.addEventListener("click", async () => {
  // desativa botão p/ evitar multi-submit
  submitButton.disabled = true;

  // pega e normaliza
  const raw = urlInput.value || "";
  resultParagraph.style.color = "gray";
  resultParagraph.textContent = "Enviando...";
  resultContainer.style.display = "none";
  shortLink.textContent = "";
  shortLink.href = "#";

  if (!raw.trim()) {
    resultParagraph.style.color = "red";
    resultParagraph.textContent = "Por favor, insira uma URL.";
    submitButton.disabled = false;
    return;
  }

  const longUrl = normalizeUrl(raw);
  console.log("INPUT RAW:", raw, " -> normalized:", longUrl);

  if (!longUrl) {
    resultParagraph.style.color = "red";
    resultParagraph.textContent = "Formato de URL inválido.";
    submitButton.disabled = false;
    return;
  }

  const payload = { longUrl };

  try {
    console.log("Enviando payload:", payload);

    const response = await fetch(API_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      mode: "cors",
      body: JSON.stringify(payload),
    });

    // sempre logue o status e o body cru pra debugar
    const text = await response.text();
    console.log("Status:", response.status, "Resposta bruta:", text);

    if (!text) {
      resultParagraph.style.color = "red";
      resultParagraph.textContent = "Erro: resposta vazia da API.";
      submitButton.disabled = false;
      return;
    }

    if (text.startsWith("<")) {
      resultParagraph.style.color = "red";
      resultParagraph.textContent =
        "Erro: API retornou HTML (verificar servidor).";
      submitButton.disabled = false;
      return;
    }

    let data;
    try {
      data = JSON.parse(text);
    } catch (e) {
      resultParagraph.style.color = "red";
      resultParagraph.textContent = "Erro: resposta JSON inválida.";
      console.error("JSON inválido:", text);
      submitButton.disabled = false;
      return;
    }

    if (!response.ok) {
      resultParagraph.style.color = "red";
      resultParagraph.textContent = `Erro ${response.status}: ${
        data?.message || "Falha ao criar URL."
      }`;
      console.error("Erro da API:", data);
      submitButton.disabled = false;
      return;
    }

    // pega shortUrl e garante que seja uma URL absoluta
    let returned = data.shortUrl || data.short || data.url || "";
    console.log("shortUrl bruto do backend:", returned);

    if (!returned) {
      resultParagraph.style.color = "red";
      resultParagraph.textContent = "API não retornou shortUrl.";
      submitButton.disabled = false;
      return;
    }

    // se backend devolver algo relativo (ex: "/aGB3blYR" ou "aGB3blYR"), transforma em absoluto
    let finalShort;
    try {
      if (
        /^\/|^[A-Za-z0-9_-]{4,}$/.test(returned) &&
        !/^https?:\/\//i.test(returned)
      ) {
        // tenta resolver com o origin do serviço
        finalShort = new URL(returned, API_ORIGIN).toString();
      } else {
        finalShort = new URL(returned).toString();
      }
    } catch (err) {
      // fallback: concatena
      finalShort =
        API_ORIGIN + (returned.startsWith("/") ? returned : "/" + returned);
    }

    // atualiza UI
    resultParagraph.style.color = "green";
    resultParagraph.textContent = "URL gerada com sucesso!";
    resultContainer.style.display = "flex";
    shortLink.href = finalShort;
    shortLink.textContent = finalShort;

    console.log("shortUrl final exibido:", finalShort);
  } catch (error) {
    console.error("Fetch error:", error);
    resultParagraph.style.color = "red";
    resultParagraph.textContent = "Erro de rede. Verifique console e CORS.";
  } finally {
    submitButton.disabled = false;
  }
});

// copiar
copyButton.addEventListener("click", async () => {
  const textToCopy = shortLink.textContent;
  if (!textToCopy) return;
  try {
    await navigator.clipboard.writeText(textToCopy);
    copyMessage.style.opacity = 1;
    setTimeout(() => (copyMessage.style.opacity = 0), 1500);
  } catch (err) {
    console.error("Erro ao copiar:", err);
  }
});

// permite Enter no input
urlInput.addEventListener("keydown", (ev) => {
  if (ev.key === "Enter") submitButton.click();
});
