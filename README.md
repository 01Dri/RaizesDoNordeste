# Rede Raízes do Nordeste — API Back-End

API RESTful desenvolvida para a rede de lanchonetes **Raízes do Nordeste** como solução completa para a disciplina de **Projeto Multidisciplinar (Trilha Back-End)** da **UNINTER (2026)**.

A aplicação atende múltiplos canais de atendimento (App, Totem, Balcão, Pickup e Web) com controle de autenticação JWT, gestão de estoque por unidade, programa de fidelização, fluxo de pedidos com validação de status e integração simulada com Gateway de Pagamento.

---

## 🚀 Requisitos e Tecnologias

- **Linguagem / Framework**: .NET 10 / C# 13
- **ORM / Persistência**: Entity Framework Core 10 (SQLite)
- **Segurança**: Authentication JWT Bearer + Refresh Tokens + Hashing de Senha BCrypt (Cost Factor 11)
- **Documentação de API**: OpenAPI / Scalar API Reference (Swagger)
- **Arquitetura**: Clean Architecture / DDD (Domain, Application, Data, API)

---

## 🛠️ Como Configurar e Executar

### 1. Pré-requisitos
- [.NET SDK 10.0](https://dotnet.microsoft.com/download) ou superior instalado.
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (opcional, apenas para execução via containers).

---

### 🐳 OPÇÃO 1: Executando com Docker Compose (API + Worker juntos)

Esta é a opção mais recomendada, pois sobe automaticamente em containers isolados tanto a **API Principal** quanto o **Worker de Pagamento (`UninterPayment.Worker`)** pré-conectados pela rede do Docker.

#### 1. Iniciar os Containers
A partir da raiz do repositório, execute:
```bash
docker-compose up --build
```

#### 2. Endereços dos Serviços
- **API Principal (`raizes-api`)**:
  - URL Base: `http://localhost:8080`
  - Documentação (Scalar/Swagger): `http://localhost:8080/scalar/v1`
  - JSON OpenAPI: `http://localhost:8080/openapi/v1.json`
- **Worker de Pagamento (`payment-worker`)**:
  - URL Base: `http://localhost:5200`
  - Health Check: `http://localhost:5200/health`

#### 3. Encerrar os Containers
Para parar a execução dos serviços em container:
```bash
docker-compose down
```

---

### 💻 OPÇÃO 2: Executando Individualmente via .NET CLI (`dotnet run`)

Para rodar os serviços individualmente na sua máquina local sem Docker:

#### 1. Aplicar Migrations e Gerar Banco do Zero
O banco SQLite (`app.db`) possui dados de Seed pré-configurados. Antes da primeira execução, aplique as migrations:
```bash
dotnet ef database update --project src/RaizesDoNordeste.Data/RaizesDoNordeste.Data.csproj --startup-project src/RaizesDoNordeste.API/RaizesDoNordeste.API.csproj
```

#### 2. Iniciar a API Principal (Terminal 1)
A partir da raiz do repositório, execute:
```bash
dotnet run --project src/RaizesDoNordeste.API/RaizesDoNordeste.API.csproj
```
- A API estará disponível em `https://localhost:7081` (ou `http://localhost:5269`).
- Documentação Scalar em: `https://localhost:7081/scalar/v1`.

#### 3. Iniciar o Worker de Pagamento (Terminal 2)
Em uma nova janela de terminal, execute:
```bash
dotnet run --project src/UninterPayment.Worker/UninterPayment.Worker.csproj
```
- O Worker iniciará escutando a fila de pagamentos Pix na porta `http://localhost:5200`.
- Endpoint de verificação (Healthcheck): `http://localhost:5200/health`.

---

## 🧪 Como Executar os Testes Automáticos e Postman

### Testes de Unidade e Integração (.NET Test)
```bash
dotnet test test/RaizesDoNordeste.Test/RaizesDoNordeste.Test.csproj
```

### Coleção de Testes do Postman
A coleção de testes contendo todos os cenários (positivos e negativos) está localizada na raiz do repositório:
- `RaizesDoNordeste.postman_collection.json`

**Ordem recomendada de execução no Postman:**
1. **Login do Desenvolvedor / Admin**: `GET /auth/desenvolvedor` ou `POST /auth/login` com `admin@raizesdonordeste.com` e `somehashedpassword`.
2. **Login do Cliente**: `POST /auth/login` com `cliente@raizesdonordeste.com` e `somehashedpassword`.
3. **Listar Unidades**: `GET /unidades` (Recupera o `restaurantId`).
4. **Criar Pedido (Multicanal)**: `POST /pedido` enviando `canalPedido` (`0=APP`, `1=TOTEM`, `2=BALCAO`, `3=PICKUP`, `4=WEB`).
5. **Solicitar Pagamento**: `POST /pagamento/pedido/{{orderId}}` e Webhook `POST /pagamento/webhook`.
6. **Atualizar Status do Pedido**: `PUT /pedido/status/{{orderId}}`.
7. **Testar Erros (401, 403, 404, 409)**: Cenários com token ausente, perfil sem permissão ou estoque insuficiente.

---

## 📌 Módulos e Endpoints da API

| Módulo | Método | Endpoint | Permissão | Descrição |
| :--- | :--- | :--- | :--- | :--- |
| **Auth** | `POST` | `/auth/login` | Público | Autenticação com e-mail, senha e unidade |
| **Auth** | `POST` | `/auth/refresh` | Público | Renovação de Access Token via Refresh Token |
| **Auth** | `POST` | `/auth/logout` | Autenticado | Revogação de Refresh Token |
| **Auth** | `GET` | `/auth/desenvolvedor` | Público (Dev) | Login rápido automático como Administrador |
| **Usuários** | `POST` | `/usuarios` | Público | Cadastro de novo usuário (`Customer`) |
| **Usuários** | `GET` | `/usuarios/perfil` | Autenticado | Consulta dados do perfil do usuário autenticado |
| **Unidades** | `GET` | `/unidades` | Admin/Owner | Lista restaurantes e unidades da rede |
| **Unidades** | `POST` | `/unidades` | Admin/Owner | Cadastra uma nova unidade/franquia |
| **Cardápio** | `POST` | `/cardapio` | Admin/Gerente/Owner | Cria um novo cardápio (menu) para uma unidade |
| **Cardápio** | `GET` | `/cardapio/usuario-atual` | Autenticado | Consulta o cardápio da unidade do usuário logado |
| **Produtos** | `POST` | `/produtos` | Admin/Gerente/Owner | Cadastra um novo produto no cardápio |
| **Produtos** | `GET` | `/produtos` | Autenticado | Consulta lista de produtos da unidade |
| **Produtos** | `GET` | `/produtos/{id}` | Autenticado | Obtém detalhes de um produto por ID |
| **Produtos** | `PUT` | `/produtos/{id}` | Admin/Gerente/Owner | Atualiza um produto |
| **Produtos** | `DELETE` | `/produtos/{id}` | Admin/Gerente/Owner | Remove um produto |
| **Produtos** | `POST` | `/produtos/{menuItemId}/ingredientes` | Admin/Gerente/Owner | Vincula ingrediente de estoque ao produto |
| **Estoque** | `POST` | `/estoque` | Admin/Gerente/Owner | Criação de estoque e insumos iniciais |
| **Estoque** | `POST` | `/estoque/movimentacao` | Admin/Gerente/Profissional | Registra entrada/saída/desperdício de insumos |
| **Estoque** | `GET` | `/estoque` | Admin/Gerente/Profissional | Consulta o estoque da unidade logada |
| **Estoque** | `GET` | `/estoque/unidade/{restaurantId}` | Admin/Gerente/Owner | Consulta estoque de unidade específica por GUID |
| **Pedidos** | `POST` | `/pedido` | Autenticado | Criação de pedido com validação de estoque e `canalPedido` |
| **Pedidos** | `GET` | `/pedido` | Admin/Profissional | Lista pedidos com filtro por `status` e `canalPedido` |
| **Pedidos** | `GET` | `/pedido/{id}` | Admin/Profissional | Detalhes de um pedido pelo seu `publicId` |
| **Pedidos** | `PUT` | `/pedido/status/{id}` | Admin/Profissional | Atualização do status do pedido |
| **Pagamento**| `POST` | `/pagamento/pedido/{orderId}` | Autenticado | Processamento mock de pagamento do pedido |
| **Pagamento**| `POST` | `/pagamento/webhook` | Assinatura Webhook | Confirmação assíncrona de pagamento |
| **Fidelidade**| `POST` | `/loyality` | Admin/Gerente | Registra adesão do cliente ao programa de pontos |
| **Fidelidade**| `DELETE` | `/loyality` | Autenticado | Cancela a adesão ao programa de pontos |
| **Worker Mock**| `POST` | `http://localhost:5200/payments` | Livre | Enfileira pagamento no Worker de Pagamento |
| **Worker Mock**| `GET` | `http://localhost:5200/health` | Livre | Healthcheck do serviço Worker de Pagamento |

---

## 🔒 Segurança e LGPD

- **Sanitização de Dados**: Senhas tratadas exclusivamente via hash BCrypt.
- **Minimização**: Respostas da API omitem dados sensíveis e credenciais internas.
- **Autorização por Roles**: Custom Attribute `[RolesAuthorize]` para restringir operações gerenciais.
- **Logs de Auditoria**: Registro estruturado de transações sensíveis.
