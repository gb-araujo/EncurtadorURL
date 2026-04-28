# EncurtadorURL

Aplicacao para encurtamento de URLs com backend em .NET 9, Redis como armazenamento key-value e frontend estatico. O projeto prioriza simplicidade, baixa latencia e deploy separado entre API e interface web.

## Links de producao

- Frontend: https://encurtador.gabrielaraujo.app/
- Backend: https://e.gabrielaraujo.app/
- Health check: https://e.gabrielaraujo.app/health

> Observacao: no plano gratuito do Render, a primeira requisicao pode demorar por causa de cold start.

## Tecnologias

| Componente | Tecnologia |
| --- | --- |
| Backend | .NET 9, C# |
| API | Carter / Minimal API |
| Armazenamento | Redis |
| Frontend | HTML, CSS e JavaScript |
| Container | Docker |
| Deploy | Render + Vercel |

## Como funciona

A API gera um identificador curto a partir da URL original e grava o relacionamento no Redis com TTL de 30 dias.

Fluxo de encurtamento:

1. O frontend envia a URL longa para a API.
2. A API calcula um hash SHA-256 deterministico.
3. O hash e convertido para Base64 URL-safe e reduzido para 8 caracteres.
4. A relacao `codigo -> URL original` e salva no Redis.
5. A API retorna a URL curta.

Fluxo de redirecionamento:

1. A API recebe o `chunk` da URL curta.
2. Busca a URL original no Redis.
3. Redireciona com HTTP `302`.
4. Retorna `404` caso o codigo nao exista ou tenha expirado.

## Requisitos

- .NET 9 SDK
- Redis local ou em container
- Docker opcional

## Rodando localmente

```bash
git clone https://github.com/gb-araujo/EncurtadorURL.git
cd EncurtadorURL
```

Suba o Redis com Docker:

```bash
docker run -d --name redis -p 6379:6379 redis:alpine
```

Configure a connection string em `appsettings.json`, `appsettings.Development.json` ou variavel de ambiente:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

Execute o backend:

```bash
dotnet run --project EncurtadorURL
```

A API ficara disponivel na URL exibida pelo terminal. Em desenvolvimento, o fallback usa `localhost:6379` para o Redis.

## Frontend

O frontend e estatico. Abra o `index.html` no navegador ou sirva os arquivos em qualquer host estatico.

## API Reference

| Metodo | Rota | Descricao |
| --- | --- | --- |
| `POST` | `/urls` | Cria ou retorna uma URL curta |
| `GET` | `/{chunk}` | Redireciona para a URL original |
| `GET` | `/health` | Verifica status da aplicacao |

## Limitacoes atuais

- URLs expiram apos 30 dias.
- O hash deterministico faz a mesma URL gerar o mesmo codigo.
- Redis precisa estar disponivel para criacao e redirecionamento de URLs.

## Melhorias futuras

- Painel para acompanhar quantidade de acessos.
- Codigo curto customizavel.
- Testes automatizados para API e regras de expiracao.
- Rate limiting para proteger o endpoint de criacao.
