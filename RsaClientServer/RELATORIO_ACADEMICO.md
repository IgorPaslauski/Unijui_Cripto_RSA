# Relatório Acadêmico - Sistema Cliente-Servidor com RSA

## 1. Descrição do Cenário de Aplicação

O sistema desenvolvido simula um cenário de comunicação segura entre dois computadores (PC1 e PC2) em uma rede local. O **servidor (PC1)** deseja receber mensagens confidenciais de **clientes (PC2)** de forma que, mesmo que a comunicação seja interceptada, apenas o servidor consiga ler o conteúdo original.

O fluxo é o seguinte:
1. O servidor gera um par de chaves RSA (pública e privada).
2. O cliente conecta ao servidor via TCP.
3. O servidor envia sua chave pública ao cliente.
4. O cliente criptografa a mensagem com a chave pública e envia ao servidor.
5. O servidor descriptografa com a chave privada e exibe a mensagem.

## 2. Por que RSA é Indicado Nesse Caso?

O **RSA** (Rivest-Shamir-Adleman) é um algoritmo de **criptografia assimétrica** amplamente utilizado para:

- **Confidencialidade**: Garantir que apenas o destinatário (quem tem a chave privada) leia a mensagem.
- **Troca de chaves sem canal seguro prévio**: A chave pública pode ser enviada em texto aberto; não há necessidade de um canal secreto para combiná-la.
- **Simplicidade didática**: O conceito de par de chaves (pública/privada) é fácil de entender e demonstrar.

Neste cenário, o servidor é o único que deve ler as mensagens. O RSA permite que qualquer cliente criptografe dados usando a chave pública, mas apenas o servidor, com a chave privada, consiga descriptografar.

## 3. Como a Troca de Chaves Funciona

1. **Geração**: O servidor gera um par de chaves RSA (2048 bits). A chave pública contém o módulo (n) e o expoente público (e); a chave privada contém informações adicionais (p, q, d, etc.) que permitem a descriptografia.

2. **Distribuição**: A chave pública é enviada ao cliente via TCP. Não há problema em enviá-la em texto aberto, pois ela serve apenas para criptografar.

3. **Uso pelo cliente**: O cliente usa a chave pública para criptografar a mensagem. O resultado é um bloco de bytes ilegível.

4. **Transmissão**: A mensagem criptografada é enviada ao servidor. Mesmo que um atacante intercepte, ele não consegue recuperar o texto original sem a chave privada.

5. **Descriptografia**: O servidor usa a chave privada para descriptografar e obter a mensagem original.

## 4. Por que o Cliente Não Consegue Ler a Mensagem sem a Chave Privada?

O RSA é baseado em um problema matemático difícil: a **fatoração de números grandes**. A chave pública contém o módulo *n* (produto de dois primos grandes *p* e *q*) e o expoente *e*. A chave privada contém o expoente *d*, que depende de *p* e *q*.

- **Criptografar**: usa *n* e *e* (chave pública) → qualquer um pode fazer.
- **Descriptografar**: usa *n* e *d* (chave privada) → requer conhecer *d*, que só pode ser calculado conhecendo *p* e *q*.

Como fatorar *n* em *p* e *q* é computacionalmente inviável para números suficientemente grandes (ex.: 2048 bits), o cliente não consegue obter a chave privada a partir da chave pública. Portanto, ele pode criptografar, mas não descriptografar.

## 5. Conclusão

O sistema demonstra de forma prática os conceitos de criptografia assimétrica e a aplicação do RSA em um cenário cliente-servidor. A separação entre chave pública (para criptografar) e chave privada (para descriptografar) garante a confidencialidade das mensagens enviadas ao servidor.
