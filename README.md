# Unijui - Criptografia RSA

Repositório da disciplina **Segurança de Dados** — projetos de criptografia da UNIJUÍ.

## Projetos

| Pasta | Descrição |
|-------|-----------|
| **RsaClientServer/** | Sistema cliente-servidor com criptografia RSA 2048 bits. O servidor gera o par de chaves, distribui a chave pública aos clientes e descriptografa as mensagens recebidas. Suporta múltiplos clientes simultâneos. |
| **FileEncryption/** | Projeto de criptografia de arquivos. |

## Início rápido

### RsaClientServer
```bash
cd RsaClientServer
dotnet run --project ServerApp    # Terminal 1: inicia o servidor na porta 5000
dotnet run --project ClientApp    # Terminal 2: inicia o cliente e conecte ao servidor
```
Consulte o [README do RsaClientServer](RsaClientServer/README.md) para mais detalhes.

### FileEncryption
Entre na pasta `FileEncryption` e consulte o README local (se houver).
