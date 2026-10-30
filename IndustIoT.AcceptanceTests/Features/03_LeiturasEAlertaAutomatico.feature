# language: pt-BR
@api @leituras @esg-ambiental
Funcionalidade: Recebimento de leituras IoT e alerta automático de consumo excessivo
  Como gestor de eficiência energética
  Quero que cada leitura acima do limite gere um alerta com severidade proporcional ao excesso
  Para agir rápido sobre desperdício de energia e reduzir a pegada de carbono da planta

  Contexto:
    Dado que existe um equipamento cadastrado com limite de consumo de 30 kWh

  Regra: Consumo até o limite não gera alerta

    @positivo @contrato
    Esquema do Cenário: Leitura dentro do limite é registrada sem alerta
      Quando o sensor envia uma leitura de <consumo> kWh para o equipamento
      Então o status da resposta deve ser 201
      E o corpo da resposta deve respeitar o contrato "leitura-registrada"
      E o campo "alertaGerado" da resposta deve ser "false"
      E o campo "alerta" da resposta deve ser nulo

      Exemplos:
        | caso                    | consumo |
        | bem abaixo do limite    | 12.5    |
        | exatamente no limite    | 30      |

  Regra: Consumo acima do limite gera alerta — até 20% Atencao, até 50% Alta, acima Critica

    @positivo @contrato @severidade
    Esquema do Cenário: Leitura acima do limite gera alerta com a severidade correta
      Quando o sensor envia uma leitura de <consumo> kWh para o equipamento
      Então o status da resposta deve ser 201
      E o corpo da resposta deve respeitar o contrato "leitura-registrada"
      E o campo "alertaGerado" da resposta deve ser "true"
      E o campo "alerta.severidade" da resposta deve ser "<severidade>"
      E o campo "alerta.excessoPercentual" da resposta deve ser "<excesso>"
      E o campo "alerta.limiteConfiguradoKwh" da resposta deve ser "30"
      E o campo "alerta.resolvido" da resposta deve ser "false"

      Exemplos:
        | caso                     | consumo | severidade | excesso |
        | excesso pequeno          | 33      | Atencao    | 10      |
        | fronteira de 20%         | 36      | Atencao    | 20      |
        | excesso médio            | 40      | Alta       | 33.3    |
        | fronteira de 50%         | 45      | Alta       | 50      |
        | desperdício grave        | 60      | Critica    | 100     |

    @positivo @esg-governanca
    Cenário: Alerta gerado fica disponível na listagem de alertas (rastreabilidade)
      Quando o sensor envia uma leitura de 60 kWh para o equipamento
      E eu listo os alertas não resolvidos com severidade "Critica"
      Então o status da resposta deve ser 200
      E a listagem de alertas deve conter o alerta gerado para o equipamento

  Regra: Leituras inválidas ou de equipamentos desconhecidos são rejeitadas (integridade dos dados)

    @negativo @contrato
    Cenário: Leitura para uma TAG não cadastrada
      Quando o sensor envia uma leitura de 10 kWh para a TAG "ZZZ-999"
      Então o status da resposta deve ser 404
      E o corpo da resposta deve respeitar o contrato "problem-details"
      E o campo "title" da resposta deve ser "Recurso não encontrado"

    @negativo @contrato @esg-governanca
    Esquema do Cenário: Leitura com dados implausíveis é rejeitada pela validação
      Quando o sensor envia uma leitura inválida com <problema>
      Então o status da resposta deve ser 400
      E o corpo da resposta deve respeitar o contrato "validation-problem"
      E a resposta deve conter erro de validação para o campo "<campo>"

      Exemplos:
        | problema                     | campo              |
        | consumo negativo             | ConsumoKwh         |
        | data no futuro               | DataHoraLeitura    |
        | temperatura de 900 graus     | TemperaturaCelsius |
        | TAG em minúsculas            | EquipamentoTag     |
