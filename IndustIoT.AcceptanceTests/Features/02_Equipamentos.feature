# language: pt-BR
@api @equipamentos @esg-ambiental
Funcionalidade: Cadastro e consulta de equipamentos monitorados
  Como gestor de eficiência energética
  Quero cadastrar os equipamentos da planta com um limite de consumo por leitura
  Para que a plataforma detecte desperdício de energia de cada máquina

  Regra: Cadastro exige TAG única no padrão de planta e limites positivos

    @positivo @contrato
    Cenário: Cadastrar um equipamento válido
      Quando eu cadastro um equipamento com nova TAG, potência de 55000 W e limite de 30 kWh
      Então o status da resposta deve ser 201
      E o corpo da resposta deve respeitar o contrato "equipamento"
      E o campo "status" da resposta deve ser "Ligado"
      E o campo "limiteConsumoKwh" da resposta deve ser "30"
      E o cabeçalho Location deve apontar para o equipamento criado

    @negativo @contrato @esg-governanca
    Cenário: Cadastrar equipamento com TAG duplicada viola a regra de unicidade
      Dado que existe um equipamento cadastrado com limite de consumo de 30 kWh
      Quando eu cadastro outro equipamento com a mesma TAG
      Então o status da resposta deve ser 422
      E o corpo da resposta deve respeitar o contrato "problem-details"
      E o campo "title" da resposta deve ser "Regra de negócio violada"

    @negativo @contrato
    Esquema do Cenário: Cadastro com dados inválidos é rejeitado pela validação
      Quando eu cadastro um equipamento com TAG "<tag>", nome "<nome>", potência de <potencia> W e limite de <limite> kWh
      Então o status da resposta deve ser 400
      E o corpo da resposta deve respeitar o contrato "validation-problem"
      E a resposta deve conter erro de validação para o campo "<campo>"

      Exemplos:
        | caso                  | tag      | nome       | potencia | limite | campo                |
        | TAG fora do padrão    | MOTOR1   | Motor      | 1000     | 10     | Tag                  |
        | nome vazio            | MTR-01   |            | 1000     | 10     | Nome                 |
        | potência zero         | MTR-02   | Motor      | 0        | 10     | PotenciaNominalWatts |
        | limite negativo       | MTR-03   | Motor      | 1000     | -5     | LimiteConsumoKwh     |

  Regra: Consultas são paginadas e retornam apenas o recurso pedido

    @positivo @contrato
    Cenário: Listar equipamentos de um setor com paginação
      Dado que existem 3 equipamentos cadastrados no mesmo setor
      Quando eu listo os equipamentos desse setor com página 1 e tamanho 2
      Então o status da resposta deve ser 200
      E o corpo da resposta deve respeitar o contrato "paged-equipamentos"
      E a lista "items" deve ter 2 elementos
      E o campo "totalCount" da resposta deve ser "3"
      E o campo "totalPages" da resposta deve ser "2"
      E o campo "hasNextPage" da resposta deve ser "true"

    @positivo @contrato
    Cenário: Consultar um equipamento existente pelo id
      Dado que existe um equipamento cadastrado com limite de consumo de 30 kWh
      Quando eu consulto esse equipamento
      Então o status da resposta deve ser 200
      E o corpo da resposta deve respeitar o contrato "equipamento"
      E o campo "tag" da resposta deve ser a TAG do equipamento

    @negativo @contrato
    Cenário: Consultar um equipamento inexistente
      Quando eu consulto o equipamento de id 999999
      Então o status da resposta deve ser 404
      E o corpo da resposta deve respeitar o contrato "problem-details"
      E o campo "title" da resposta deve ser "Recurso não encontrado"

  Regra: O histórico de consumo é a base da análise de eficiência energética

    @positivo @contrato
    Cenário: Consultar o histórico de consumo de um equipamento
      Dado que existe um equipamento cadastrado com limite de consumo de 30 kWh
      E que o sensor enviou as leituras de consumo:
        | minutosAtras | consumoKwh |
        | 30           | 10         |
        | 20           | 20         |
        | 10           | 25         |
      Quando eu consulto o histórico de consumo desse equipamento
      Então o status da resposta deve ser 200
      E o corpo da resposta deve respeitar o contrato "paged-consumo"
      E o campo "totalCount" da resposta deve ser "3"
      E o campo "items[0].consumoKwh" da resposta deve ser "25"
      E os itens devem estar ordenados da leitura mais recente para a mais antiga

    @negativo @contrato
    Cenário: Consultar o histórico de consumo de um equipamento inexistente
      Quando eu consulto o histórico de consumo do equipamento de id 999999
      Então o status da resposta deve ser 404
      E o corpo da resposta deve respeitar o contrato "problem-details"
