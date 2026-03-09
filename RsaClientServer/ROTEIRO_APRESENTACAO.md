# Roteiro de Apresentação (3 a 5 minutos)

## 1. Introdução (30 s)
- "Vou apresentar um sistema cliente-servidor didático que usa criptografia RSA para enviar mensagens de forma segura."
- "O servidor gera as chaves, o cliente criptografa a mensagem com a chave pública e envia; só o servidor consegue ler com a chave privada."

## 2. Demonstração prática (1 min 30 s)
- Abrir dois terminais.
- **Terminal 1**: `dotnet run --project ServerApp` — mostrar que o servidor gera as chaves e fica aguardando.
- **Terminal 2**: `dotnet run --project ClientApp` — conectar e digitar uma mensagem (ex.: "Segredo da disciplina").
- Mostrar no servidor: a mensagem criptografada (Base64) e a mensagem original descriptografada.
- Destacar: "A mensagem trafega cifrada; só o servidor vê o texto original."

## 3. Conceitos principais (1 min)
- **Chave pública**: pode ser enviada a qualquer um; serve para criptografar.
- **Chave privada**: fica só no servidor; serve para descriptografar.
- **Por que o cliente não lê?** Ele só tem a chave pública. Descriptografar exige a chave privada, que depende de um problema matemático difícil (fatoração).

## 4. Código (opcional, 30 s)
- Mostrar rapidamente: `RsaService` com `GenerateKeyPair`, `Encrypt` e `Decrypt`.
- Mencionar: "Usamos apenas bibliotecas padrão do .NET: `System.Security.Cryptography` e `System.Net.Sockets`."

## 5. Encerramento (30 s)
- "O projeto inclui testes unitários para o serviço RSA e um relatório explicando o cenário e o uso do RSA."
- Perguntas?
