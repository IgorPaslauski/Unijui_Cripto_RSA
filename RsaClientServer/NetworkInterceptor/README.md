# Ferramenta de Ataque — Interceptor de Comunicação

**Simulação de ataque** em que um invasor tenta interceptar e ler a comunicação entre cliente e servidor. O "hacker" posiciona-se entre as vítimas (ataque Man-in-the-Middle) para capturar todo o tráfego e tentar descriptografar as mensagens.

## Cenário

- **Atacante**: Roda o NetworkInterceptor. Não conhece o protocolo — analisa apenas os bytes na rede.
- **Vítimas**: Cliente e servidor que acreditam estar se comunicando diretamente.
- **Objetivo do ataque**: Capturar chave e mensagens para tentar ler o conteúdo.

## Como funciona

1. O atacante escuta na porta **5001**
2. A vítima (cliente) conecta no atacante pensando que é o servidor
3. O atacante conecta no servidor real e **relaya** o tráfego (invisível para as vítimas)
4. Enquanto encaminha, **captura** chave e mensagens cifradas
5. Tenta descriptografar com o que capturou → **falha** (só tem chave pública)

## Como rodar o ataque

### 1. Servidor (vítima) — porta 5000
```bash
dotnet run --project ServerApp
```

### 2. Atacante — porta 5001
```bash
dotnet run --project NetworkInterceptor
```
- IP do servidor alvo: Enter (127.0.0.1)
- Porta do servidor alvo: Enter (5000)

### 3. Cliente (vítima) — conecta no atacante
```bash
dotnet run --project ClientApp
```
- IP: **127.0.0.1**
- Porta: **5001**

### Fluxo do ataque
```
Cliente (vítima)  →  127.0.0.1:5001 (ATACANTE)  →  Servidor (vítima)
                            ↑
                    Captura todo o tráfego
                    Tenta ler as mensagens
                    FALHA — RSA protege
```

## Resultado didático

O atacante consegue capturar:
- A chave (pública) trocada
- As mensagens cifradas

Mas **não consegue ler** — a chave privada nunca sai do servidor. O RSA garante confidencialidade mesmo contra ataques Man-in-the-Middle.
