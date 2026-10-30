# language: pt-BR
@api @comandos @esg-ambiental @esg-governanca
Funcionalidade: Desligamento remoto de equipamentos ociosos
  Como gestor da planta
  Quero desligar remotamente equipamentos ociosos ou com desperdício de energia
  Para economizar energia, com controle de acesso por perfil e trilha de auditoria de quem pediu

  Contexto:
    Dado que existe um equipamento cadastrado com limite de consumo de 30 kWh

  Regra: Somente o perfil Gestor pode desligar, e o comando registra quem o solicitou

    @positivo @contrato
    Cenário: Gestor desliga remotamente um equipamento ligado
      Dado que estou autenticado como "gestor"
      Quando eu envio o comando de desligamento com o motivo "Equipamento ocioso no turno da noite"
      Então o status da resposta deve ser 200
      E o corpo da resposta deve respeitar o contrato "comando"
      E o campo "tipo" da resposta deve ser "Desligamento"
      E o campo "status" da resposta deve ser "Confirmado"
      E o campo "solicitadoPor" da resposta deve ser o usuário logado
      E o campo "motivo" da resposta deve ser "Equipamento ocioso no turno da noite"
      E o equipamento deve ficar com status "Desligado"

    @negativo @contrato
    Cenário: Desligar um equipamento que já está desligado
      Dado que estou autenticado como "gestor"
      E que o equipamento já foi desligado remotamente
      Quando eu envio o comando de desligamento com o motivo "Segunda tentativa"
      Então o status da resposta deve ser 422
      E o corpo da resposta deve respeitar o contrato "problem-details"
      E o campo "detail" da resposta deve conter "já está desligado"

    @negativo @seguranca
    Cenário: Operador não tem permissão para desligar equipamentos
      Dado que estou autenticado como "operador"
      Quando eu envio o comando de desligamento com o motivo "Tentativa sem permissão"
      Então o status da resposta deve ser 403
      E o equipamento deve ficar com status "Ligado"

    @negativo @seguranca
    Cenário: Comando de desligamento sem token
      Quando eu envio o comando de desligamento sem token
      Então o status da resposta deve ser 401
      E o equipamento deve ficar com status "Ligado"

    @negativo @contrato
    Cenário: Desligar um equipamento inexistente
      Dado que estou autenticado como "gestor"
      Quando eu envio o comando de desligamento para o equipamento de id 999999
      Então o status da resposta deve ser 404
      E o corpo da resposta deve respeitar o contrato "problem-details"

    @negativo @contrato
    Cenário: Motivo do comando acima de 500 caracteres é rejeitado
      Dado que estou autenticado como "gestor"
      Quando eu envio o comando de desligamento com um motivo de 501 caracteres
      Então o status da resposta deve ser 400
      E o corpo da resposta deve respeitar o contrato "validation-problem"
      E a resposta deve conter erro de validação para o campo "Motivo"
      E o equipamento deve ficar com status "Ligado"
