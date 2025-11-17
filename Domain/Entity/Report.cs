namespace Work360.Domain.Entity;

public class Report
{
    public Guid ReportID { get; set; }

    public User User { get; set; }

    public Guid UserID { get; set; }
    public DateOnly dataInicio;
    public DateOnly dataFim;
    public int tarefasConcluidas;
    public int tarefasPendentes;
    public int reunioesRealizadas;
    public int minutosFocoTotal;
    public double percentualConclusao;
    public double riscoBurnout;
    public string tendenciaProdutividade;
    public string tendenciaublic;
    public string insiublic;
    public string recomendacublic;
    public string resumoGublic;
    public DateTime criadoEm = DateTime.Now;
}