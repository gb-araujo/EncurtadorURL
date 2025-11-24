# 🔗 EncurtadorURL

Aplicação para encurtamento de URLs, focada em **baixa latência** e simplicidade.  
O backend é construído com **.NET 9 (C#)** usando **Carter (Minimal API)** e **Redis** para armazenamento em memória.  
O Frontend é estático (HTML/JS) e pode ser hospedado separadamente.

---

## ✨ Tecnologias Principais

| Componente        | Tecnologia                       |
| ----------------- | -------------------------------- |
| **Backend**       | .NET 9 (C#)                      |
| **Roteamento**    | Carter (Minimal API)             |
| **Armazenamento** | Redis (Key-Value Store)          |
| **Deploy**        | Render (API) + Vercel (Frontend) |
| **Container**     | Docker                           |

---

## 🎯 Links de Produção

- **Frontend**: https://encurtador.gabrielaraujo.app/

- **Backend**: https://e.gabrielaraujo.app/
- **Health Check**: https://e.gabrielaraujo.app/health

⚠️ Observação: No plano gratuito do Render pode ocorrer lentidão na primeira requisição (**cold start**).

---

## 🧩 Arquitetura

A aplicação utiliza **hashing determinístico (SHA-256)** na URL original.  
Isso significa que a mesma URL sempre gera o mesmo código curto, tornando o processo **idempotente** e reduzindo processamento desnecessário.

### 📌 Fluxo de Encurtamento

1. Recebe URL longa do frontend
2. Calcula hash SHA-256
3. Converte para Base64 URL-safe (8 caracteres)
4. Armazena no Redis com TTL de 30 dias
5. Retorna a URL curta formatada

### 📌 Fluxo de Redirecionamento

1. Recebe o `chunk`
2. Busca no Redis
3. Redireciona com HTTP **302**
4. Se não existir, retorna **404**

API e Frontend são separados para permitir deploy independente e facilitar escalabilidade.

---

## 🚀 Rodando Localmente

### ✅ Pré-requisitos

- .NET 9 SDK
- Redis instalado ou em container
- Docker (opcional)

### ✅ Backend

\`\`\`bash

# Clone o repositório

git clone https://github.com/gb-araujo/EncurtadorURL

# Entre na pasta

cd EncurtadorURL

# Configure a conexão Redis em appsettings.json

"Redis": "localhost:6379"

# Execute

dotnet run
\`\`\`

O backend iniciará em:
\`\`\`
http://localhost:5000
\`\`\`

### Redis

Rodar com docker:

docker run -d --name redis -p 6379:6379 redis

### ✅ Frontend

Abra o \`index.html\` diretamente no navegador ou sirva em qualquer host estático.

---

## 📚 API Reference

| Rota         | Método | Descrição                       |
| ------------ | ------ | ------------------------------- |
| \`/urls\`    | POST   | Cria ou retorna a URL curta     |
| \`/{chunk}\` | GET    | Redireciona para a URL original |
| \`/health\`  | GET    | Verifica status da aplicação    |

## ⚠️ Limitações

- Plano gratuito do Render pode causar **lentidão inicial** (cold start)
- URLs expiram após 30 dias

---

## 🤝 Contribuições

Sugestões, issues e pull requests são bem-vindos.
