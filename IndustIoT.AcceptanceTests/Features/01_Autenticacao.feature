# language: pt-BR
@api @auth @esg-governanca
Funcionalidade: Autenticação de usuários da plataforma IndústIoT
  Como operador ou gestor da planta industrial
  Quero me autenticar na API e receber um token JWT
  Para executar ações protegidas com rastreabilidade de quem as solicitou

  Regra: Somente credenciais válidas recebem token, com a role do usuário

    @positivo @contrato
    Esquema do Cenário: Login com credenciais válidas emite token JWT com a role correta
      Quando eu faço login como "<perfil>"
      Então o status da resposta deve ser 200
      E o corpo da resposta deve respeitar o contrato "login-response"
      E o campo "role" da resposta deve ser "<role>"
      E o token retornado deve ser um JWT com expiração futura

      Exemplos:
        | perfil   | role     |
        | gestor   | Gestor   |
        | operador | Operador |

    @negativo @contrato
    Cenário: Login com senha incorreta é recusado sem revelar qual campo errou
      Quando eu faço login com usuário "rodrigo.m" e senha "senha-errada"
      Então o status da resposta deve ser 401
      E o corpo da resposta deve respeitar o contrato "problem-details"
      E o campo "title" da resposta deve ser "Credenciais inválidas"
      E o campo "detail" da resposta deve ser "Usuário ou senha incorretos."
      E a resposta não deve conter o campo "token"

    @negativo
    Cenário: Login de usuário inexistente é recusado
      Quando eu faço login com usuário "invasor" e senha "qualquer"
      Então o status da resposta deve ser 401
      E o corpo da resposta deve respeitar o contrato "problem-details"
