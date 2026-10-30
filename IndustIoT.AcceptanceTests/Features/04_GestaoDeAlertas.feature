# language: pt-BR
@api @alertas @esg-governanca
Funcionalidade: Gestão de alertas de consumo
  Como operador da planta
  Quero listar os alertas por severidade e marcar como resolvidos os que já tratei
  Para manter um histórico auditável de cada desvio de consumo e de sua resolução

  Contexto:
    Dado que existe um equipamento cadastrado com limite de consumo de 50 kWh
    E que o sensor gerou um alerta de consumo para o equipamento

  @positivo @contrato
  Cenário: Listar alertas pendentes filtrando por severidade
    Quando eu listo os alertas não resolvidos com severidade "Critica"
    Então o status da resposta deve ser 200
    E o corpo da resposta deve respeitar o contrato "paged-alertas"
    E todos os itens devem ter o campo "severidade" igual a "Critica"
    E todos os itens devem ter o campo "resolvido" igual a "false"

  @positivo @contrato
  Cenário: Operador autenticado resolve um alerta
    Dado que estou autenticado como "operador"
    Quando eu resolvo o alerta gerado
    Então o status da resposta deve ser 200
    E o corpo da resposta deve respeitar o contrato "alerta"
    E o campo "resolvido" da resposta deve ser "true"
    E o alerta não deve mais aparecer entre os não resolvidos

  @negativo @contrato
  Cenário: Resolver um alerta que já foi resolvido
    Dado que estou autenticado como "gestor"
    E que o alerta gerado já foi resolvido
    Quando eu resolvo o alerta gerado
    Então o status da resposta deve ser 422
    E o corpo da resposta deve respeitar o contrato "problem-details"
    E o campo "title" da resposta deve ser "Regra de negócio violada"

  @negativo @seguranca
  Cenário: Resolver alerta sem estar autenticado
    Quando eu resolvo o alerta gerado sem token
    Então o status da resposta deve ser 401

  @negativo @contrato
  Cenário: Resolver um alerta inexistente
    Dado que estou autenticado como "gestor"
    Quando eu resolvo o alerta de id 999999999
    Então o status da resposta deve ser 404
    E o corpo da resposta deve respeitar o contrato "problem-details"
