# Relatório Acadêmico - Criptografia de Arquivos com AES

**Disciplina:** Segurança de Dados  
**Atividade:** Trabalho 1 - Criptografia Simétrica  
**Tecnologia:** C# (.NET 8)

---

## 1. Descrição do Cenário de Aplicação

O sistema desenvolvido é uma aplicação de console que permite ao usuário **proteger arquivos sensíveis** por meio de criptografia. O cenário típico de uso é o seguinte:

- Um usuário possui documentos (textos, planilhas, imagens, etc.) que deseja manter confidenciais.
- Ao criptografar, o conteúdo do arquivo é transformado em dados ilegíveis para quem não possui a senha.
- O arquivo criptografado pode ser armazenado em disco, enviado por e-mail ou em nuvem, sem expor o conteúdo original.
- Apenas quem conhece a senha pode descriptografar e recuperar o conteúdo original.

A aplicação é voltada para **proteção de dados em repouso** (data at rest), ou seja, arquivos armazenados em disco ou mídia.

---

## 2. Por que AES é Indicado

O **AES (Advanced Encryption Standard)** foi escolhido pelos seguintes motivos:

1. **Padrão internacional**: Adotado pelo NIST (EUA) em 2001 e amplamente utilizado em todo o mundo (TLS/SSL, Wi-Fi, bancos, etc.).

2. **Segurança comprovada**: Após décadas de análise, não há ataques práticos conhecidos contra o AES quando usado corretamente (chave adequada, IV único, modo apropriado).

3. **Simplicidade de uso**: É um algoritmo simétrico — a mesma chave cifra e decifra — o que facilita a implementação e o entendimento didático.

4. **Performance**: Eficiente em software e hardware, permitindo criptografia rápida mesmo em arquivos grandes.

5. **Disponibilidade**: Implementação nativa no .NET (`System.Security.Cryptography`), sem dependências externas.

Utilizamos **AES-256** (chave de 256 bits) em modo **CBC** (Cipher Block Chaining), que é uma combinação segura e amplamente recomendada.

---

## 3. Confidencialidade e Integridade Básica

### Confidencialidade

A confidencialidade é garantida porque:

- **Criptografia AES**: O conteúdo é transformado de forma que, sem a chave, é computacionalmente inviável recuperar o texto original.
- **Derivação de chave (PBKDF2)**: A senha do usuário não é usada diretamente como chave. Ela é combinada com um *salt* aleatório e processada por muitas iterações (100.000), gerando uma chave forte e resistente a ataques de dicionário e força bruta.
- **IV único**: Cada criptografia usa um vetor de inicialização aleatório, evitando que o mesmo conteúdo gere o mesmo cifrado (o que facilitaria ataques).

### Integridade Básica

A integridade é preservada de forma básica porque:

- **Estrutura fixa**: O arquivo .enc segue um formato definido (salt + IV + conteúdo cifrado). Alterações acidentais ou corrupção parcial tornam a descriptografia inviável.
- **Validação de tamanho**: Arquivos com tamanho insuficiente (menor que salt + IV) são rejeitados como inválidos.
- **Detecção de senha incorreta**: Se a senha estiver errada, a descriptografia falha com exceção criptográfica, indicando que os dados não foram recuperados corretamente.

*Nota:* Para integridade forte contra adulteração intencional, seria necessário adicionar um HMAC ou assinatura. Para o escopo didático deste trabalho, a abordagem atual é suficiente.

---

## 4. Resumo Técnico

| Componente | Função |
|------------|--------|
| **AES-256-CBC** | Criptografia simétrica do conteúdo |
| **PBKDF2** | Derivação da chave a partir da senha |
| **Salt** | Unicidade da chave por arquivo |
| **IV** | Unicidade do cifrado por operação |

O sistema atende aos requisitos de uma solução simples, segura e didática para proteção de arquivos locais.
