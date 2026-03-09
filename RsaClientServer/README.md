# Sistema Cliente-Servidor com Criptografia RSA

Projeto didático para a disciplina de Segurança de Dados — criptografia assimétrica com RSA 2048 bits.

## Como funciona

1. O **servidor** gera um par de chaves RSA (pública e privada).
2. O **cliente** conecta via TCP na porta 5000.
3. O servidor envia sua **chave pública** ao cliente.
4. O cliente criptografa as mensagens com a chave pública e envia ao servidor.
5. O servidor descriptografa com a chave privada e exibe o texto original.

O sistema suporta **múltiplos clientes** simultaneamente. Cada cliente mantém uma sessão e pode enviar várias mensagens até digitar `sair`.

## Estrutura do Projeto

```
RsaClientServer/
├── ServerApp/          # Aplicação servidor
├── ClientApp/          # Aplicação cliente
├── RsaCrypto/          # Biblioteca de criptografia RSA
├── RsaCrypto.Tests/    # Testes unitários do RsaService
├── publicar.bat        # Script para gerar executáveis standalone
└── RELATORIO_ACADEMICO.md
```

## Pré-requisitos

- .NET 8 ou .NET 9 SDK

## Como rodar

1. **Inicie o servidor** (em um terminal):
   ```bash
   cd RsaClientServer
   dotnet run --project ServerApp
   ```

2. **Inicie o cliente** (em outro terminal, na mesma pasta):
   ```bash
   dotnet run --project ClientApp
   ```

3. **No cliente**: Quando perguntado, digite o IP do servidor (ou Enter para localhost `127.0.0.1`).

4. **Envie mensagens**: Digite cada mensagem e pressione Enter. O servidor exibirá o texto descriptografado. Digite `sair` para encerrar a sessão.

### Exemplo de saída

**Servidor:**
```
=== SERVIDOR RSA - Segurança de Dados ===

[OK] Par de chaves RSA gerado (2048 bits)

[OK] Servidor escutando em 0.0.0.0:5000
[...] Aguardando conexões (Ctrl+C para encerrar)...

[Cliente 1] Conectado de 127.0.0.1:xxxxx
[Cliente 1] Chave pública enviada: MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...

[Cliente 1] Mensagem: Olá, esta é uma mensagem secreta!
[Cliente 1] Desconectado
```

**Cliente:**
```
=== CLIENTE RSA - Segurança de Dados ===

Digite o IP do servidor (Enter para localhost): 
[...] Conectando ao servidor em 127.0.0.1:5000...
[OK] Conectado ao servidor!

[OK] Chave pública recebida do servidor.

Digite suas mensagens (ou 'sair' para encerrar):

Mensagem: Olá, esta é uma mensagem secreta!
[OK] Mensagem enviada!
Mensagem: sair
Encerrando conexão...
```

## Testar entre computadores diferentes

**No PC do servidor (PC1):**
1. Descubra o IP da máquina (`ipconfig` — procure por "Endereço IPv4", ex: 192.168.1.100).
2. Inicie o servidor (opcional: informe o IP para fazer bind, ou use qualquer interface):
   ```bash
   dotnet run --project ServerApp
   ```
   Para escutar em um IP específico:
   ```bash
   dotnet run --project ServerApp -- 192.168.1.100
   ```
3. Libere a porta 5000 no firewall (se necessário).

**No PC do cliente (PC2):**
1. Execute o cliente e, quando solicitado, digite o IP do servidor (ex: `192.168.1.100`).
   ```bash
   dotnet run --project ClientApp
   ```
   O cliente pede o IP interativamente — não usa argumento de linha de comando.

## Gerar executáveis (.exe) sem .NET instalado

Execute na pasta `RsaClientServer`:
```bash
publicar.bat
```

Os executáveis serão gerados em:
- `publish\ServerApp\ServerApp.exe`
- `publish\ClientApp\ClientApp.exe`

Copie as pastas para qualquer PC Windows 64 bits e execute os .exe. O cliente perguntará o IP do servidor ao iniciar.

## Testes

```bash
dotnet test
```

## Limitações

- **Tamanho da mensagem**: Até ~240 caracteres (limite do RSA 2048 com PKCS#1). Para mensagens maiores, seria necessário criptografia híbrida (RSA + AES).
- **Uso didático**: Projeto simplificado para fins educacionais. Em produção, use TLS/SSL.
