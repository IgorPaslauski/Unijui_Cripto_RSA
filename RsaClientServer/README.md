# Sistema Cliente-Servidor com Criptografia RSA

Projeto didático para a disciplina de Segurança de Dados - Criptografia assimétrica com RSA.

## Estrutura do Projeto

```
RsaClientServer/
├── ServerApp/          # Aplicação servidor (PC1)
├── ClientApp/          # Aplicação cliente (PC2)
├── RsaCrypto/         # Biblioteca com serviço RSA
├── RsaCrypto.Tests/   # Testes unitários do RsaService
└── README.md
```

## Como Rodar

### Pré-requisitos
- .NET 8 ou .NET 9 SDK instalado

### Passo a passo

1. **Entre na pasta do projeto** (a pasta `RsaClientServer` está dentro de `Trab 1`):
   ```bash
   cd "Trab 1/RsaClientServer"
   ```

2. **Iniciar o servidor** (em um terminal):
   ```bash
   dotnet run --project ServerApp
   ```

3. **Iniciar o cliente** (em outro terminal, na mesma pasta RsaClientServer):
   ```bash
   dotnet run --project ClientApp
   ```

4. **No cliente**: Digite a mensagem quando solicitado e pressione Enter.

5. **No servidor**: A mensagem descriptografada será exibida na tela.

### Exemplo de saída

**Terminal do Servidor:**
```
=== SERVIDOR RSA - Segurança de Dados ===

[OK] Par de chaves RSA gerado (2048 bits)

[OK] Servidor escutando em 127.0.0.1:5000
[...] Aguardando conexão do cliente...

[OK] Cliente conectado!

[ENVIADO] Chave pública (Base64):
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA...

[RECEBIDO] Mensagem criptografada (Base64):
aB3xY9kL2mN...

========================================
[MENSAGEM ORIGINAL DESCRIPTOGRAFADA]:
Olá, esta é uma mensagem secreta!
========================================

Servidor encerrado. Pressione qualquer tecla para sair.
```

**Terminal do Cliente:**
```
=== CLIENTE RSA - Segurança de Dados ===

[...] Conectando ao servidor em 127.0.0.1:5000...
[OK] Conectado ao servidor!

[OK] Chave pública recebida do servidor.

Digite a mensagem a ser criptografada e enviada: Olá, esta é uma mensagem secreta!
[OK] Mensagem criptografada com sucesso.

[OK] Mensagem enviada ao servidor!

Pressione qualquer tecla para sair.
```

## Testar entre computadores diferentes

**No PC do servidor (PC1):**
1. Descubra o IP da máquina (no PowerShell: `ipconfig` — procure por "Endereço IPv4", ex: 192.168.1.100).
2. Inicie o servidor:
   ```bash
   dotnet run --project ServerApp
   ```
3. Libere a porta 5000 no firewall do Windows (se necessário):
   - Painel de Controle → Firewall do Windows → Configurações avançadas → Regras de entrada → Nova regra → Porta → TCP 5000.

**No PC do cliente (PC2):**
1. Inicie o cliente informando o IP do servidor:
   ```bash
   dotnet run --project ClientApp -- 192.168.1.100
   ```
   (Substitua `192.168.1.100` pelo IP real do PC1.)

**Teste local (mesmo PC):** use `dotnet run --project ClientApp` sem argumentos (conecta em 127.0.0.1).

## Gerar executáveis (.exe) para rodar sem instalar .NET

Para criar arquivos .exe que funcionam em qualquer PC Windows 64 bits **sem precisar instalar o .NET**:

1. Execute o script na pasta RsaClientServer:
   ```bash
   publicar.bat
   ```

2. Os executáveis serão gerados em:
   - `publish\ServerApp\ServerApp.exe`
   - `publish\ClientApp\ClientApp.exe`

3. Copie as pastas `publish\ServerApp` e `publish\ClientApp` para o PC desejado e execute os .exe diretamente (duplo clique ou pelo terminal).

**Para conectar a outro PC com o cliente:** execute pelo terminal e passe o IP:
   ```
   ClientApp.exe 192.168.1.100
   ```

## Executar os testes

```bash
dotnet test
```

## Limitações

- **Tamanho da mensagem**: RSA 2048 bits com PKCS#1 permite mensagens de até ~240 caracteres. Para mensagens maiores, seria necessário usar criptografia híbrida (RSA + AES).
- **Uso didático**: Este projeto é simplificado para fins educacionais. Em produção, considere TLS/SSL para a comunicação.
