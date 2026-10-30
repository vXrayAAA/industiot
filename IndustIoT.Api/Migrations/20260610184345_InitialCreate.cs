using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IndustIoT.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Equipamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tag = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Fabricante = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Setor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LinhaProducao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PotenciaNominalWatts = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    LimiteConsumoKwh = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ComandosAtuacao",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipamentoId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SolicitadoPor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataHoraSolicitacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataHoraExecucao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComandosAtuacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComandosAtuacao_Equipamentos_EquipamentoId",
                        column: x => x.EquipamentoId,
                        principalTable: "Equipamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeiturasSensor",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipamentoId = table.Column<int>(type: "int", nullable: false),
                    ConsumoKwh = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    TemperaturaCelsius = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    VibracaoMmPorSegundo = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    CorrenteAmperes = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    DataHoraLeitura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataHoraRecebimento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeiturasSensor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeiturasSensor_Equipamentos_EquipamentoId",
                        column: x => x.EquipamentoId,
                        principalTable: "Equipamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alertas",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipamentoId = table.Column<int>(type: "int", nullable: false),
                    LeituraSensorId = table.Column<long>(type: "bigint", nullable: false),
                    ConsumoRegistradoKwh = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    LimiteConfiguradoKwh = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false),
                    Severidade = table.Column<int>(type: "int", nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DataHoraCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Resolvido = table.Column<bool>(type: "bit", nullable: false),
                    DataHoraResolucao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alertas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alertas_Equipamentos_EquipamentoId",
                        column: x => x.EquipamentoId,
                        principalTable: "Equipamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alertas_LeiturasSensor_LeituraSensorId",
                        column: x => x.LeituraSensorId,
                        principalTable: "LeiturasSensor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_EquipamentoId",
                table: "Alertas",
                column: "EquipamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_LeituraSensorId",
                table: "Alertas",
                column: "LeituraSensorId");

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_Resolvido_DataHoraCriacao",
                table: "Alertas",
                columns: new[] { "Resolvido", "DataHoraCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_ComandosAtuacao_EquipamentoId_DataHoraSolicitacao",
                table: "ComandosAtuacao",
                columns: new[] { "EquipamentoId", "DataHoraSolicitacao" });

            migrationBuilder.CreateIndex(
                name: "IX_Equipamentos_Setor",
                table: "Equipamentos",
                column: "Setor");

            migrationBuilder.CreateIndex(
                name: "IX_Equipamentos_Tag",
                table: "Equipamentos",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeiturasSensor_EquipamentoId_DataHoraLeitura",
                table: "LeiturasSensor",
                columns: new[] { "EquipamentoId", "DataHoraLeitura" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alertas");

            migrationBuilder.DropTable(
                name: "ComandosAtuacao");

            migrationBuilder.DropTable(
                name: "LeiturasSensor");

            migrationBuilder.DropTable(
                name: "Equipamentos");
        }
    }
}
