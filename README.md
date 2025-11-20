🔗 EncurtadorURL: Aplicação de Encurtamento de URL

Este projeto é uma solução moderna e de alto desempenho para encurtamento de URLs, construída com .NET 9, utilizando a metodologia Minimal API e o framework Carter, com Redis para armazenamento ultra-rápido.

A arquitetura foi projetada para ser rápida e escalável, ideal para ambientes de produção.

✨ Tecnologias Utilizadas

Componente

Tecnologia

Propósito

Backend

.NET 9 (C#)

Servidor de aplicação de alta performance.

Roteamento

Carter

Mapeamento de rotas minimalista (Minimal API).

Banco de Dados

Redis

Armazenamento de chave-valor em memória para lookups instantâneos.

Deploy

Docker

Containerização para garantir portabilidade em ambientes como Render.

Front-end

HTML, CSS, JS

Interface do usuário simples, hospedada estaticamente.

🚀 Arquitetura e Fluxo

O projeto segue o princípio de Idempotência via Hashing Determinístico e utiliza o Redis como principal fonte de dados (Key-Value):

POST /urls/ (Criação):

O frontend envia a LongUrl (garantindo o protocolo https://).

A API calcula o hash SHA-256 da LongUrl e usa os primeiros 8 caracteres (Base64 URL-safe) como o chunk.

Se o chunk já existe no Redis (devido ao hash determinístico), o servidor retorna a URL curta existente.

Se não existe, o par (chunk -> LongUrl) é salvo no Redis com uma expiração (TTL) de 30 dias.

Retorna a URL curta para o usuário.

GET /{chunk} (Redirecionamento):

A API recebe o chunk (código curto).

Busca o chunk no Redis.

Se encontrado, retorna um Redirecionamento HTTP 302 (Temporário) para a LongUrl.

Se não encontrado (ou o link expirou), retorna 404 Not Found.

🛠️ Configuração e Execução (Local)

Pré-requisitos

.NET 9 SDK

Docker

Servidor Redis rodando localmente na porta 6379.

Passos

Clone o Repositório:

git clone https://github.com/gb-araujo/EncurtadorURL
cd EncurtadorURL


Rode o Servidor (Com o Redis Local):

O Program.cs está configurado para usar localhost:6379 se a variável de ambiente não estiver definida.

dotnet run --project EncurtadorURL/EncurtadorURL.csproj


Acesse o Front-end:

Acesse https://localhost:7014/ (a porta pode variar) no seu navegador. O front-end HTML será servido e estará pronto para interagir com a API.

☁️ Deploy em Produção (Render + Vercel + Redis Cloud)

O projeto está configurado para um ambiente de produção distribuído e seguro.

Variáveis de Ambiente

O projeto depende de uma única variável de ambiente para produção, que deve ser configurada na plataforma de hospedagem da API (Render):

Variável

Valor de Exemplo

Propósito

REDIS_CONNECTION_STRING

host:port,password=SUA_SENHA_FORTE

Credencial de conexão do Redis Cloud.

Configurações Chave

API (Backend): Hospedada no Render (via Dockerfile), lendo a REDIS_CONNECTION_STRING.

Front-end: Hospedado no Vercel (ou Render), chamando a API do Render na URL correta.

CORS: A política de CORS no Program.cs permite o acesso tanto da URL do Render quanto da URL final do Vercel (https://encurtador-omega.vercel.app).

Domínio: O UrlModule.cs está configurado para retornar URLs curtas usando a URL do Render (https://encurtadorurl-c3lm.onrender.com), que deve ser substituída pelo seu domínio personalizado (ex: https://curto.gabrielaraujo.app).

🔑 Segurança e Limitações

Idempotência: Garante que a mesma URL longa sempre gere o mesmo chunk.

Segredos: As credenciais do Redis são lidas exclusivamente via variáveis de ambiente, garantindo que o código-fonte permaneça seguro.

Limitação (Grátis): O Redis Cloud no plano gratuito não possui persistência (durabilidade None). Em caso de reinicialização do Redis, todos os links encurtados serão perdidos. Para resolver isso, é necessário fazer o upgrade do plano do Redis Cloud.
