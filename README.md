# 🔗 EncurtadorURL
<img width="837" height="581" alt="image" src="https://github.com/user-attachments/assets/846af5ea-4bf1-4997-bfbd-fd8f6670bf61" />


Projeto de encurtamento de URL de alta performance construído com **.NET 9 (C#)**, utilizando o framework **Carter** para o mapeamento de rotas minimalista (Minimal API) e **Redis** como banco de dados de chave-valor em memória para lookups instantâneos.

O Front-end é uma aplicação estática em HTML/JavaScript projetada para ser hospedada separadamente (Vercel) e consumir a API.

## ✨ Tecnologias Chave

| Componente        | Tecnologia                       |
| ----------------- | -------------------------------- |
| **Backend**       | .NET 9 (C#)                      |
| **Roteamento**    | Carter (Minimal API)             |
| **Armazenamento** | Redis (Key-Value Store)          |
| **Deploy**        | Render (API) + Vercel (Frontend) |
| **Container**     | Docker                           |

## 🎯 Links de Produção

- **Frontend**: [encurtador-omega.vercel.app](https://encurtador-omega.vercel.app)
- **Backend**: [encurtadorurl-c3lm.onrender.com](https://encurtadorurl-c3lm.onrender.com)
- **Health Check**: [encurtadorurl-c3lm.onrender.com/health](https://encurtadorurl-c3lm.onrender.com/health)

## ⚙️ Arquitetura

O projeto utiliza o **Hashing Determinístico (SHA-256)** na `LongUrl`. Isso garante que a mesma URL de entrada sempre produza o mesmo código curto (`chunk`), tornando o processo de criação de links **idempotente** e muito rápido.

### Fluxo de Encurtamento

1. **Recebe** URL longa do frontend
2. **Calcula** hash SHA-256
3. **Converte** para Base64 URL-safe (8 caracteres)
4. **Armazena** no Redis com TTL de 30 dias
5. **Retorna** URL curta formatada

### Fluxo de Redirecionamento

1. **Recebe** código curto (`chunk`)
2. **Busca** URL original no Redis
3. **Redireciona** com status 302 (Found)
4. **Cache** automático do Redis

## 📚 API Reference

| Rota       | Método | Descrição                          |
| ---------- | ------ | ---------------------------------- |
| `/urls/`   | `POST` | Cria ou retorna a URL curta        |
| `/{chunk}` | `GET`  | Redireciona para a URL longa (302) |
| `/health`  | `GET`  | Health check da aplicação          |
