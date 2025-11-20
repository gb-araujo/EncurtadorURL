// elementos
const urlInput = document.getElementById("url");
const submitButton = document.getElementById("submitButton");
const resultParagraph = document.getElementById("urlResult");

const resultContainer = document.getElementById("resultContainer");
const shortLink = document.getElementById("shortLink");
const copyButton = document.getElementById("copyButton");
const copyMessage = document.getElementById("message");

// --- URL DINÂMICA DA API ---
const isLocal =
  window.location.hostname === "localhost" ||
  window.location.hostname === "127.0.0.1";

// Configuração por ambiente
const API_CONFIG = {
  development: "https://localhost:7000",
  production: "https://encurtadorurl-c3lm.onrender.com", // ← SEU BACKEND NO RENDER
};

const getApiBase = () => {
  if (isLocal) return API_CONFIG.development;

  // Se estiver no Vercel (frontend), usa o backend no Render
  if (window.location.hostname.includes("vercel.app")) {
    return API_CONFIG.production;
  }

  // Fallback
  return window.location.origin;
};

const API_BASE = getApiBase();
const API_URL = `${API_BASE}/urls/`;
console.log("🔗 API Base:", API_BASE); // Para debug
// --- FIM DA URL DINÂMICA ---

// util: normaliza e valida
function normalizeUrl(input) {
  let u = input.trim();
  u = u.replace(/\s+/g, "");

  if (!/^https?:\/\//i.test(u)) {
    u = "https://" + u;
  }

  try {
    const parsed = new URL(u);
    if (!["http:", "https:"].includes(parsed.protocol))
      throw new Error("Protocolo inválido");
    return parsed.toString();
  } catch (err) {
    return null;
  }
}

// evento enviar
submitButton.addEventListener("click", async () => {
  submitButton.disabled = true;
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

  const payload = { LongUrl: longUrl };

  try {
    console.log("Enviando payload:", payload);
    console.log("API URL:", API_URL);

    const response = await fetch(API_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });

    const text = await response.text();
    console.log("Status:", response.status, "Resposta bruta:", text);

    if (!text) {
      resultParagraph.style.color = "red";
      resultParagraph.textContent = "Erro: resposta vazia da API.";
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

    let returned = data.shortUrl || "";
    console.log("shortUrl bruto do backend:", returned);

    if (!returned) {
      resultParagraph.style.color = "red";
      resultParagraph.textContent = "API não retornou shortUrl.";
      submitButton.disabled = false;
      return;
    }

    // Usa a URL retornada pelo backend diretamente
    resultParagraph.style.color = "green";
    resultParagraph.textContent = "URL gerada com sucesso!";
    resultContainer.style.display = "flex";
    shortLink.href = returned;
    shortLink.textContent = returned;

    console.log("shortUrl final exibido:", returned);
  } catch (error) {
    console.error("Fetch error:", error);
    resultParagraph.style.color = "red";
    resultParagraph.textContent = "Erro de rede. Verifique console.";
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
